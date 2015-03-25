namespace Tx.ApplicationInsights
{
    using System;

    internal class AppInsightsEnvelope
    {
        public string Property;

        public DateTimeOffset Timestamp;

        public string Json;

        public int PropertyJsonStart;
        public int InternalJsonStart;
        public int ContextJsonStart;

        public int PropertyJsonLength;
        public int InternalJsonLength;
        public int ContextJsonLength;
    }
}