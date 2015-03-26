namespace Tx.ApplicationInsights
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Tx.ApplicationInsights.InternalData;

    public static class ApplicationInsightsFileStorage
    {
        private static readonly PartitionableTypeMap TypeMap = new PartitionableTypeMap();

        public static IEnumerable<T> ReadFromFiles<T>(params string[] folders) where T : BaseEvent
        {
            if (folders == null)
            {
                throw new ArgumentNullException("folders");
            }

            var typeKey = TypeMap.GetTypeKey(typeof(T));

            var transform = TypeMap.GetTransform(typeof(T));

            if (string.IsNullOrEmpty(typeKey) || transform == null)
            {
                throw new NotSupportedException(typeof(T).FullName + " is unsupported type.");
            }

            return JsonFileReader.ReadAsEnumerable(folders)
                .Where(i => TypeMap.Comparer.Equals(TypeMap.GetInputKey(i), typeKey))
                .Select(i => (T)transform(i));
        }
    }
}
