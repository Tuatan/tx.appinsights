namespace System.Reactive
{
    using System;
    using Tx.ApplicationInsights;

    public static class PlaybackExtensions
    {
        public static void AddApplicationInsightsFiles(
            this IPlaybackConfiguration playback,
            params string[] folders)
        {
            if (folders == null)
            {
                throw new ArgumentNullException("folders");
            }

            playback.AddInput(
                () => JsonFileReader.Read(folders),
                typeof(PartitionableTypeMap));
        }
    }
}
