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

        private readonly JsonSerializer serializer = new JsonSerializer();

        public PartitionableTypeMap()
        {
            this.map2.Add(typeof(TraceEvent), "message");
            this.map2.Add(typeof(CustomEvent), "event");
            this.map2.Add(typeof(ExceptionEvent), "basicException");
            this.map2.Add(typeof(RequestEvent), "request");
            this.map2.Add(typeof(PerformanceCounterEvent), "performanceCounter");

            this.map.Add(typeof(TraceEvent), this.ParseTrace);
            this.map.Add(typeof(CustomEvent), this.ParseCustomEvent);
            this.map.Add(typeof(ExceptionEvent), this.ParseException);
            this.map.Add(typeof(RequestEvent), this.ParseRequest);
            this.map.Add(typeof(PerformanceCounterEvent), this.ParsePerformanceCounter);
        }

        private PerformanceCounterEvent ParsePerformanceCounter(AppInsightsEnvelope envelope)
        {
            var result = this.ParseBase<PerformanceCounterEvent>(envelope);

            var json = envelope.Json.Substring(envelope.PropertyJsonStart, envelope.PropertyJsonLength);

            using (var stringReader = new StringReader(json))
            {
                var reader = new JsonTextReader(stringReader);

                //result.performanceCounter = this.serializer.Deserialize<PerformanceCounterEventData[]>(reader);

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

                result.performanceCounter = counters.ToArray();
            }

            return result;
        }

        private RequestEvent ParseRequest(AppInsightsEnvelope envelope)
        {
            var result = this.ParseBase<RequestEvent>(envelope);

            using (var stringReader = new StringReader(
                envelope.Json.Substring(envelope.PropertyJsonStart, envelope.PropertyJsonLength)))
            {
                var reader = new JsonTextReader(stringReader);

                result.request = this.serializer.Deserialize<RequestEventData[]>(reader);
            }

            return result;
        }

        private ExceptionEvent ParseException(AppInsightsEnvelope envelope)
        {
            var result = this.ParseBase<ExceptionEvent>(envelope);

            using (var stringReader = new StringReader(
                envelope.Json.Substring(envelope.PropertyJsonStart, envelope.PropertyJsonLength)))
            {
                var reader = new JsonTextReader(stringReader);

                result.basicException = this.serializer.Deserialize<ExceptionEventData[]>(reader);
            }

            return result;
        }

        private TraceEvent ParseTrace(AppInsightsEnvelope envelope)
        {
            var result = this.ParseBase<TraceEvent>(envelope);

            using (var stringReader = new StringReader(
                envelope.Json.Substring(envelope.PropertyJsonStart, envelope.PropertyJsonLength)))
            {
                var reader = new JsonTextReader(stringReader);

                result.message = this.serializer.Deserialize<TraceEventData[]>(reader);
            }

            return result;
        }

        private CustomEvent ParseCustomEvent(AppInsightsEnvelope envelope)
        {
            var result = this.ParseBase<CustomEvent>(envelope);

            using (var stringReader = new StringReader(
                envelope.Json.Substring(envelope.PropertyJsonStart, envelope.PropertyJsonLength)))
            {
                var reader = new JsonTextReader(stringReader);

                result.@event = this.serializer.Deserialize<CustomEventData[]>(reader);
            }

            return result;
        }

        private T ParseBase<T>(AppInsightsEnvelope envelope) where T : BaseEvent, new()
        {
            var result = new T();

            //using (var stringReader = new StringReader(envelope.Json.Substring(envelope.InternalJsonStart, envelope.InternalJsonLength)))
            //{
            //    var reader = new JsonTextReader(stringReader);

            //    result.@internal = this.serializer.Deserialize<Internal>(reader);
            //}

            using (var stringReader = new StringReader(
                envelope.Json.Substring(envelope.ContextJsonStart, envelope.ContextJsonLength)))
            {
                var reader = new JsonTextReader(stringReader);

                result.Context = this.serializer.Deserialize<Context>(reader);
            }

            return result;
        }

        public Func<AppInsightsEnvelope, object> GetTransform(Type outputType)
        {
            Func<AppInsightsEnvelope, object> key;

            if (!this.map.TryGetValue(outputType, out key))
            {
                key = (envelope) => null;
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