namespace Tx.ApplicationInsights.TelemetryType
{
    using Tx.ApplicationInsights.InternalData;

    public class ExceptionEvent : BaseEvent
    {
        public ExceptionEventData[] basicException;
    }
}