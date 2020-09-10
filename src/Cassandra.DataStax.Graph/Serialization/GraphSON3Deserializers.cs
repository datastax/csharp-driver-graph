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

using System.Collections.Generic;
using Cassandra.DataStax.Graph.Internal;
using Cassandra.DataStax.Graph.Serialization.Tinkerpop.Structure.IO.GraphSON;

namespace Cassandra.DataStax.Graph.Serialization
{
    internal static class GraphSON3Deserializers
    {
        /// <summary>
        /// Contains the <see cref="IGraphSONDeserializer" /> instances by their type identifier. 
        /// </summary>
        public static readonly IReadOnlyDictionary<string, IGraphSONDeserializer> CustomDeserializers;

        private static readonly IReadOnlyDictionary<string, IGraphSONDeserializer> GraphSON3SpecificCustomDeserializers
            = new Dictionary<string, IGraphSONDeserializer>()
            {
                { "g:Path", new Path3Deserializer() }
            };

        static GraphSON3Deserializers()
        {
            var dictionary = new Dictionary<string, IGraphSONDeserializer>();

            foreach (var kvp in GraphSON2Deserializers.CustomDeserializers)
            {
                dictionary[kvp.Key] = kvp.Value;
            }
            
            foreach (var kvp in GraphSON3Deserializers.GraphSON3SpecificCustomDeserializers)
            {
                dictionary[kvp.Key] = kvp.Value;
            }

            GraphSON3Deserializers.CustomDeserializers = dictionary;
        }
    }
}