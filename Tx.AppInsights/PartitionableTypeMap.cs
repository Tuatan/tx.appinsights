namespace Tx.ApplicationInsights
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reactive;

    using Newtonsoft.Json;

    using Tx.ApplicationInsights.InternalData;
    using Tx.ApplicationInsights.TelemetryType;

    internal class PartitionableTypeMap : IPartitionableTypeMap<AppInsightsEnvelope, string>
    {
        private readonly Dictionary<Type, Func<AppInsightsEnvelope, object>> map = new Dictionary<Type, Func<AppInsightsEnvelope, object>>();
        private readonly Dictionary<Type, string> map2 = new Dictionary<Type, string>();

        public PartitionableTypeMap()
        {
            this.map2.Add(typeof(TraceEvent), "message");
            this.map2.Add(typeof(CustomEvent), "event");
            this.map2.Add(typeof(ExceptionEvent), "basicException");
            this.map2.Add(typeof(RequestEvent), "request");
            this.map2.Add(typeof(PerformanceCounterEvent), "performanceCounter");

            this.map.Add(typeof(TraceEvent), e => Safe(e, ParseTrace));
            this.map.Add(typeof(CustomEvent), e => Safe(e, ParseCustomEvent));
            this.map.Add(typeof(ExceptionEvent), e => Safe(e, ParseException));
            this.map.Add(typeof(RequestEvent), e => Safe(e, this.ParseRequest));
            this.map.Add(typeof(PerformanceCounterEvent), e => Safe(e, this.ParsePerformanceCounter));
        }

        private static object Safe(AppInsightsEnvelope envelope, Func<AppInsightsEnvelope, object> transformer)
        {
            object result = null;

            try
            {
                result = transformer(envelope);
            }
            catch (Exception)
            {
                // Add EventSource based tracing to track these errors
            }

            return result;
        }

        private PerformanceCounterEvent ParsePerformanceCounter(AppInsightsEnvelope envelope)
        {
            var result = ParseBase<PerformanceCounterEvent>(envelope);

            var json = envelope.Json.Substring(envelope.PropertyJsonStart, envelope.PropertyJsonLength);

            using (var stringReader = new StringReader(json))
            {
                var reader = new JsonTextReader(stringReader);

                var counters = new List<PerformanceCounterEventData>();

                reader.Read();
		        reader.Read();

                while (reader.TokenType != JsonToken.EndArray)
                {
                    reader.Read();

                    var n = reader.Value.ToString();

                    reader.Read();
                    reader.Read();
                    reader.Read();

                    var v = reader.Value.ToString();

                    reader.Read();
                    reader.Read();
                    reader.Read();

                    var categoryName = reader.Value.ToString();

                    reader.Read();
                    reader.Read();

                    var instanceName = reader.Value.ToString();

                    reader.Read();
                    reader.Read();

                    double value;
                    Double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out value);

                    counters.Add(new PerformanceCounterEventData
                    {
                        CategoryName = categoryName,
                        InstanceName = instanceName,
                        Values = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                        {
                            { n, value },   
                        }
                    });
                }

                result.PerformanceCounter = counters.ToArray();
            }

            return result;
        }

        private static TEvent ParseEvent<TEvent, TEventData>(AppInsightsEnvelope envelope, Action<TEvent, TEventData> setter) where TEvent : BaseEvent, new()
        {
            var result = ParseBase<TEvent>(envelope);

            var json = envelope.Json.Substring(envelope.PropertyJsonStart, envelope.PropertyJsonLength);

            setter(result, JsonConvert.DeserializeObject<TEventData>(json));

            return result;
        }

        private RequestEvent ParseRequest(AppInsightsEnvelope envelope)
        {
            var result = ParseEvent<RequestEvent, RequestEventData[]>(
                envelope,
                (@event, datas) => @event.Request = datas);

            return result;
        }

        private static ExceptionEvent ParseException(AppInsightsEnvelope envelope)
        {
            var result = ParseEvent<ExceptionEvent, ExceptionEventData[]>(
                envelope,
                (@event, datas) => @event.BasicException = datas);

            return result;
        }

        private static TraceEvent ParseTrace(AppInsightsEnvelope envelope)
        {
            var result = ParseEvent<TraceEvent, TraceEventData[]>(
                envelope, 
                (@event, datas) => @event.Message = datas);

            return result;
        }

        private static CustomEvent ParseCustomEvent(AppInsightsEnvelope envelope)
        {
            var result = ParseEvent<CustomEvent, CustomEventData[]>(
                envelope,
                (@event, datas) => @event.Event = datas);

            return result;
        }

        private static T ParseBase<T>(AppInsightsEnvelope envelope) where T : BaseEvent, new()
        {
            var result = new T();

            var json = envelope.Json.Substring(envelope.ContextJsonStart, envelope.ContextJsonLength);

            result.Context = JsonConvert.DeserializeObject<Context>(json);

            return result;
        }

        public Func<AppInsightsEnvelope, object> GetTransform(Type outputType)
        {
            Func<AppInsightsEnvelope, object> key;

            if (!this.map.TryGetValue(outputType, out key))
            {
                key = envelope => null;
            }

            return key;
        }

        public Func<AppInsightsEnvelope, DateTimeOffset> TimeFunction
        {
            get
            {
                return e => e.Timestamp;
            }
        }

        public string GetTypeKey(Type outputType)
        {
            string key;

            this.map2.TryGetValue(outputType, out key);

            return key;
        }

        public string GetInputKey(AppInsightsEnvelope evt)
        {
            return evt.Property;
        }

        public IEqualityComparer<string> Comparer
        {
            get
            {
                return StringComparer.OrdinalIgnoreCase;
            }
        }
    }
}