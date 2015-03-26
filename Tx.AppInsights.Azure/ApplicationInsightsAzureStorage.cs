namespace Tx.ApplicationInsights.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Tx.ApplicationInsights.InternalData;

    public static class ApplicationInsightsAzureStorage
    {
        private static readonly PartitionableTypeMap TypeMap = new PartitionableTypeMap();

        public static IEnumerable<T> ReadFromFiles<T>(
            string connectionString,
            string containerName,
            string localFolder) where T : BaseEvent
        {
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

            var typeKey = TypeMap.GetTypeKey(typeof(T));

            var transform = TypeMap.GetTransform(typeof(T));

            if (string.IsNullOrEmpty(typeKey) || transform == null)
            {
                throw new NotSupportedException(typeof(T).FullName + " is unsupported type.");
            }

            return CacheableReader.ReadAsEnumerable(connectionString, containerName, localFolder)
                .Where(i => TypeMap.Comparer.Equals(TypeMap.GetInputKey(i), typeKey))
                .Select(i => (T)transform(i));
        }
    }
}
