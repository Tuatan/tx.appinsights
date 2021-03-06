﻿namespace Tx.ApplicationInsights
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;

    using Newtonsoft.Json;

    internal static class JsonFileReader
    {
        internal static IEnumerable<AppInsightsEnvelope> ReadAsEnumerable(params string[] folders)
        {
            var blobs = folders
                .SelectMany(ListFolder);

            return blobs
                .OrderBy(i => i.Start)
                .Select(i => i.Name)
                .SelectMany(ReadFileSafe)
                .Where(i => i != null);
        }

        public static IObservable<AppInsightsEnvelope> Read(params string[] folders)
        {
            return ReadAsEnumerable(folders)
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
                // Add EventSource based tracing to track these errors
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
                // Add EventSource based tracing to track these errors
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

        private static IEnumerable<BlobInfo> ListFolder(string folderName)
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

                    // Add EventSource based tracing to track these errors
                    return (BlobInfo)null;
                })
                .Where(i => i != null);
        }
    }
}