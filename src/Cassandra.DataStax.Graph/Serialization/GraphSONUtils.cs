//
//       Copyright (C) DataStax Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using Cassandra.DataStax.Graph.Internal;

namespace Cassandra.DataStax.Graph.Serialization
{
    internal static class GraphSONUtils
    {
        public static IReadOnlyDictionary<GraphProtocol, IReadOnlyDictionary<Type, IGraphSONSerializer>> CustomSerializers { get; }

        public static IReadOnlyDictionary<GraphProtocol, IReadOnlyDictionary<string, IGraphSONDeserializer>> CustomDeserializers { get; }

        static GraphSONUtils()
        {
            GraphSONUtils.CustomSerializers = new Dictionary<GraphProtocol, IReadOnlyDictionary<Type, IGraphSONSerializer>>
            {
                { GraphProtocol.GraphSON2, GraphSON2Serializers.CustomSerializers},
                { GraphProtocol.GraphSON3, GraphSON3Serializers.CustomSerializers}
            };

            GraphSONUtils.CustomDeserializers = new Dictionary<GraphProtocol, IReadOnlyDictionary<string, IGraphSONDeserializer>>
            {
                { GraphProtocol.GraphSON2, GraphSON2Deserializers.CustomDeserializers},
                { GraphProtocol.GraphSON3, GraphSON3Deserializers.CustomDeserializers}
            };
        }
    }
}