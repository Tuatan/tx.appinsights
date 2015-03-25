namespace Tx.ApplicationInsights
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;

    using Newtonsoft.Json;

    internal class JsonFileReader
    {
        public IObservable<AppInsightsEnvelope> Read(params string[] folders)
        {
            var blobs = folders
                .SelectMany(this.ListFolder);

            return blobs
                .OrderBy(i => i.Start)
                .Select(i => i.Name)
                .SelectMany(ReadFileSafe)
                .Where(i => i != null)
                .ToObservable();
        }

        internal static IEnumerable<AppInsightsEnvelope> ReadFileSafe(string fileName)
        {
            try
            {
                return ReadFile(fileName);
            }
            catch (Exception e)
            {
                return Enumerable.Empty<AppInsightsEnvelope>();
            }
        }

        internal static IEnumerable<AppInsightsEnvelope> ReadFile(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                while (reader.Peek() >= 0)
                {
                    yield return ReadLineSafe(reader.ReadLine());
                }
            }
        }

        internal static AppInsightsEnvelope ReadLineSafe(string i)
        {
            try
            {
                return ReadLine(i);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static AppInsightsEnvelope ReadLine(string line)
        {
            var result = new AppInsightsEnvelope
            {
                Json = line,
            };

            using (var r = new StringReader(line))
            {
                var reader = new JsonTextReader(r);

                reader.Read();

                reader.Read();
                result.Property = reader.Value.ToString();
                var l = reader.LinePosition;
                reader.Skip();

                result.PropertyJsonStart = l;
                result.PropertyJsonLength = reader.LinePosition - l;

                reader.Read();
                l = reader.LinePosition;
                reader.Skip();

                result.InternalJsonStart = l;
                result.InternalJsonLength = reader.LinePosition - l;

                reader.Read();
                l = reader.LinePosition;

                result.ContextJsonStart = l;
                result.ContextJsonLength = line.Length - l - 1;

                reader.Read();

                reader.Read();
                reader.Skip();
                reader.Read();
                reader.Read();
                reader.Read();

                reader.Read();

                var occurance = reader.Value.ToString();

                result.Timestamp = DateTimeOffset.Parse(occurance);
            }

            return result;
        }

        private IEnumerable<BlobInfo> ListFolder(string folderName)
        {
            return Directory.EnumerateFiles(
                    folderName,
                    "*.blob",
                    SearchOption.AllDirectories)
                .Select(i =>
                {
                    var parts = i.Split('\\');

                    return new
                    {
                        Parts = parts,
                        Name = i
                    };
                })
                .Where(i => i.Parts.Length > 2)
                .Select(i =>
                {
                    var dateTime = i.Parts[i.Parts.Length - 3] + " " + i.Parts[i.Parts.Length - 2];
                    DateTime start;
                    if (DateTime.TryParseExact(
                                    dateTime,
                                    "yyyy-MM-dd HH",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                                    out start))
                    {
                        return new BlobInfo
                        {
                            Name = i.Name,
                            Start = start,
                            End = start.AddHours(1).AddTicks(-1)
                        };
                    }

                    return (BlobInfo)null;
                })
                .Where(i => i != null);
        }
    }
}