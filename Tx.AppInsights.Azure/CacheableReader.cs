namespace Tx.ApplicationInsights.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    internal class CacheableReader
    {
        public static IObservable<AppInsightsEnvelope> Read(
            string connectionString,
            string containerName,
            string localFolder)
        {
            return ReadAsEnumerable(connectionString, containerName, localFolder)
                .ToObservable();
        }

        public static IEnumerable<AppInsightsEnvelope> ReadAsEnumerable(
            string connectionString,
            string containerName,
            string localFolder)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(containerName);

            var blobs = List(container).GetAwaiter().GetResult();

            return blobs
                .OrderBy(i => i.Start)
                .Select(i => Cache(i, container, localFolder))
                .SelectMany(JsonFileReader.ReadFileSafe)
                .Where(i => i != null);
        }

        private static string Cache(
            BlobInfo blobInfo,
            CloudBlobContainer container,
            string localFolder)
        {
            var fullName = Path.Combine(localFolder, blobInfo.Name);

            if (!File.Exists(fullName))
            {
                var folder = localFolder;
                var parts = blobInfo.Name.Split('/');

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    folder = Path.Combine(folder, parts[i]);

                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                }

                var blockBlob2 = container.GetBlockBlobReference(blobInfo.Name);

                using (var memoryStream = new MemoryStream())
                {
                    blockBlob2.DownloadToStream(memoryStream);
                    var text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());

                    File.WriteAllText(fullName, text);
                }
            }

            return fullName;
        }

        private async static Task<BlobInfo[]> List(CloudBlobContainer container)
        {
            var result = (await ListBlobsSegments(container))
                .Select(i =>
                {
                    var date = i.Segments[4].Substring(0, i.Segments[4].Length - 1);
                    var time = i.Segments[5].Substring(0, i.Segments[5].Length - 1);

                    var dt = date + " " + time;

                    var start = DateTime.ParseExact(
                        dt,
                        "yyyy-MM-dd HH",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

                    return new BlobInfo
                    {
                        Name = string.Join(string.Empty, i.Segments.Skip(2)),
                        Start = start,
                        End = start.AddHours(1).AddTicks(-1)
                    };
                })
                .ToArray();

            return result;
        }

        private async static Task<List<Uri>> ListBlobsSegments(CloudBlobContainer container)
        {
            var resultSegment = await container.ListBlobsSegmentedAsync(
                    string.Empty,
                    true,
                    BlobListingDetails.All,
                    10,
                    null,
                    null,
                    null);

            var result = resultSegment.Results
                .Select(blobItem => blobItem.StorageUri.PrimaryUri)
                .ToList();

            var continuationToken = resultSegment.ContinuationToken;

            while (continuationToken != null)
            {
                resultSegment = await container.ListBlobsSegmentedAsync(
                        string.Empty,
                        true,
                        BlobListingDetails.All,
                        10,
                        continuationToken,
                        null,
                        null);

                result.AddRange(resultSegment.Results.Select(blobItem => blobItem.StorageUri.PrimaryUri));

                continuationToken = resultSegment.ContinuationToken;
            }

            return result;
        }
    }
}
