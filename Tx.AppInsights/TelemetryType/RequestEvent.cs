namespace Tx.ApplicationInsights.TelemetryType
{
    using Tx.ApplicationInsights.InternalData;

    public class RequestEvent : BaseEvent
    {
        public RequestEventData[] Request;
    }
}
