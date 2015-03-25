namespace Tx.ApplicationInsights.TelemetryType
{
    using Tx.ApplicationInsights.InternalData;

    public class CustomEvent : BaseEvent
    {
        public CustomEventData[] @event;
    }
}