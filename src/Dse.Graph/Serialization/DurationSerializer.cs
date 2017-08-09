//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Dse.Graph.Serialization
{
    internal class DurationSerializer : TypeSerializer<Duration>
    {
        private const long NanosPerMicro = 1000L;
        private const long NanosPerMilli = 1000L * NanosPerMicro;
        private const long NanosPerSecond = 1000L * NanosPerMilli;
        private const long NanosPerMinute = 60L * NanosPerSecond;
        private const long NanosPerHour = 60L * NanosPerMinute;
        
        public DurationSerializer() : base("gx", "Duration", Read, ToJavaDurationString)
        {
            
        }

        private static Duration Read(JToken token)
        {
            return Duration.Parse(token.ToString());
        }
        
        private static object ToJavaDurationString(Duration value)
        {
            if (value.Equals(Duration.Zero))
            {
                return "PT0S";
            }
            if (value.Months != 0L)
            {
                throw new ArgumentOutOfRangeException(string.Format(
                    "Duration {0} can not be represented in java.time.Duration (seconds and nanoseconds)", value));
            }
            var builder = new StringBuilder();
            if (value.Days < 0 || value.Nanoseconds < 0)
            {
                builder.Append('-');
            }
            builder.Append("PT");
            // No leap seconds considered
            const long hoursPerDay = 24L;
            var nanos = Math.Abs(value.Nanoseconds);
            var remainder = nanos%NanosPerHour;
            long hours = Math.Abs(value.Days)*hoursPerDay + nanos/NanosPerHour;
            if (hours > 0L)
            {
                builder.Append(hours).Append("H");
            }
            remainder = Append(builder, remainder, NanosPerMinute, "M");
            if (remainder > 0L)
            {
                var seconds = Convert.ToDecimal(remainder) / NanosPerSecond;
                builder.Append(string.Format("{0:0.#########}", seconds)).Append("S");
            }
            return builder.ToString();
        }
        
        private static long Append(StringBuilder builder, long dividend, long divisor, string unit)
        {
            if (dividend == 0L || dividend < divisor)
            {
                return dividend;
            }
            builder.Append(dividend/divisor).Append(unit);
            return dividend % divisor;
        }
    }
}