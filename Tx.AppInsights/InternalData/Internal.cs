namespace Tx.ApplicationInsights.InternalData
{
    using System.Collections.Generic;

    public class Internal
    {
        public InternalData Data;
    }

    public class RequestEventData
    {
        public string Id;

        public string Name;

        public int ResponseCode;

        public bool Success;

        public int Count;

        public string Url;

        public DurationMetricData DurationMetric;

        public UrlData UrlData;
    }

    public class UrlData
    {
        public int Port;

        public string Protocol;

        public string Host;

        public string Base;

        public string HashTag;
    }

    public class DurationMetricData
    {
        public double Value;

        public double Count;
    }

    public class PerformanceCounterEventData
    {
        public string CategoryName;

        public string InstanceName;

        public IDictionary<string, double> Values;
    }
}