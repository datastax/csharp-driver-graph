using System;
using Newtonsoft.Json.Linq;

namespace Dse.Graph.Serialization
{
    internal class DateTimeOffsetSerializer : TypeSerializer<DateTimeOffset>
    {
        public DateTimeOffsetSerializer(string prefix, string typeName)
            : base(prefix, typeName, ParseDateTimeOffset, ToUnixEpoch)
        {
        }

        private static object ToUnixEpoch(DateTimeOffset value)
        {
            var ticks = (value - UnixStart).Ticks;
            return ticks / TimeSpan.TicksPerMillisecond;
        }

        private static readonly DateTimeOffset UnixStart = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        private static DateTimeOffset ParseDateTimeOffset(JToken token)
        {
            if (token.Type == JTokenType.Integer)
            {
                var milliseconds = token.ToObject<int>();
                return UnixStart.AddTicks(TimeSpan.TicksPerMillisecond * milliseconds);   
            }
            if (!(token is JValue))
            {
                throw new NotSupportedException($"Invalid format: {token}");
            }
            // The Json.NET library parses DateTime instances before hand
            return new DateTimeOffset(token.Value<DateTime>(), TimeSpan.Zero);
        }
    }
}