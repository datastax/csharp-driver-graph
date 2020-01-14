//
//      Copyright (C) DataStax Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//

using System;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Cassandra.DataStax.Graph.Serialization
{
    internal class DurationSerializer : TypeSerializer<Duration>
    {
        private const long NanosPerMicro = 1000L;
        private const long NanosPerMilli = 1000L * DurationSerializer.NanosPerMicro;
        private const long NanosPerSecond = 1000L * DurationSerializer.NanosPerMilli;
        private const long NanosPerMinute = 60L * DurationSerializer.NanosPerSecond;
        private const long NanosPerHour = 60L * DurationSerializer.NanosPerMinute;
        
        public DurationSerializer() : base("gx", "Duration", DurationSerializer.Read, DurationSerializer.ToJavaDurationString)
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
            var remainder = nanos%DurationSerializer.NanosPerHour;
            long hours = Math.Abs(value.Days)*hoursPerDay + nanos/DurationSerializer.NanosPerHour;
            if (hours > 0L)
            {
                builder.Append(hours).Append("H");
            }
            remainder = DurationSerializer.Append(builder, remainder, DurationSerializer.NanosPerMinute, "M");
            if (remainder > 0L)
            {
                var seconds = Convert.ToDecimal(remainder) / DurationSerializer.NanosPerSecond;
                builder.Append(string.Format(CultureInfo.InvariantCulture, "{0:0.#########}", seconds)).Append("S");
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