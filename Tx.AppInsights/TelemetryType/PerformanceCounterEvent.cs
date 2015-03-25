namespace Tx.ApplicationInsights.TelemetryType
{
    using Tx.ApplicationInsights.InternalData;

    public class PerformanceCounterEvent : BaseEvent
    {
        public PerformanceCounterEventData[] performanceCounter;
    }
}
