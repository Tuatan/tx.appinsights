namespace Tx.ApplicationInsights.TelemetryType
{
    using Tx.ApplicationInsights.InternalData;

    public class TraceEvent : BaseEvent
    {
        public TraceEventData[] message;
    }
}