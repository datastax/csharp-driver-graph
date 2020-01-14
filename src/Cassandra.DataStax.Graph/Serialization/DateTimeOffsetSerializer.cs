﻿//
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
using Newtonsoft.Json.Linq;

namespace Cassandra.DataStax.Graph.Serialization
{
    internal class DateTimeOffsetSerializer : TypeSerializer<DateTimeOffset>
    {
        public DateTimeOffsetSerializer(string prefix, string typeName)
            : base(prefix, typeName, DateTimeOffsetSerializer.ParseDateTimeOffset, DateTimeOffsetSerializer.ToUnixEpoch)
        {
        }

        private static object ToUnixEpoch(DateTimeOffset value)
        {
            var ticks = (value - DateTimeOffsetSerializer.UnixStart).Ticks;
            return ticks / TimeSpan.TicksPerMillisecond;
        }

        private static readonly DateTimeOffset UnixStart = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        private static DateTimeOffset ParseDateTimeOffset(JToken token)
        {
            if (token.Type == JTokenType.Integer)
            {
                var milliseconds = token.ToObject<int>();
                return DateTimeOffsetSerializer.UnixStart.AddTicks(TimeSpan.TicksPerMillisecond * milliseconds);   
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