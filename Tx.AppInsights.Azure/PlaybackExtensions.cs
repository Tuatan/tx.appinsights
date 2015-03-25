namespace System.Reactive
{
    using System;
    using System.IO;
    using Tx.ApplicationInsights;
    using Tx.ApplicationInsights.Azure;

    public static class PlaybackExtensions
    {
        public static void AddApplicationInsightsStorage(
            this IPlaybackConfiguration playback,
            string connectionString,
            string containerName,
            string localFolder)
        {
            if (playback == null)
            {
                throw new ArgumentNullException("playback");
            }

            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (containerName == null)
            {
                throw new ArgumentNullException("containerName");
            }

            if (localFolder == null)
            {
                throw new ArgumentNullException("localFolder");
            }

            if (!Directory.Exists(localFolder))
            {
                throw new ArgumentException("localFolder");
            }

            playback.AddInput(
                () => new CacheableReader().Read(connectionString, containerName, localFolder),
                typeof(PartitionableTypeMap));
        }
    }
}
