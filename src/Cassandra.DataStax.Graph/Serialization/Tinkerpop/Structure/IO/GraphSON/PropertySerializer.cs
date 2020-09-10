#region License

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

#endregion License

using System.Collections.Generic;
using Cassandra.DataStax.Graph.Internal;
using Gremlin.Net.Structure.IO.GraphSON;
using IGraphSONSerializer = Cassandra.DataStax.Graph.Internal.IGraphSONSerializer;

namespace Cassandra.DataStax.Graph.Serialization.Tinkerpop.Structure.IO.GraphSON
{
    internal class PropertySerializer : IGraphSONSerializer
    {
        public Dictionary<string, dynamic> Dictify(dynamic objectData, IGraphSONWriter writer)
        {
            Gremlin.Net.Structure.Property property = objectData;
            var elementDict = CreateElementDict(property.Element, writer);
            var valueDict = new Dictionary<string, dynamic>
            {
                {"key", property.Key},
                {"value", writer.ToDict(property.Value)},
                {"element", elementDict}
            };
            return GraphSONUtil.ToTypedValue(nameof(Gremlin.Net.Structure.Property), valueDict);
        }

        private dynamic CreateElementDict(Gremlin.Net.Structure.Element element, IGraphSONWriter writer)
        {
            if (element == null)
                return null;
            var serializedElement = writer.ToDict(element);
            Dictionary<string, dynamic> elementDict = serializedElement;
            if (elementDict.ContainsKey(GraphSONTokens.ValueKey))
            {
                var elementValueSerialized = elementDict[GraphSONTokens.ValueKey];
                Dictionary<string, dynamic> elementValueDict = elementValueSerialized;
                if (elementValueDict != null)
                {
                    elementValueDict.Remove("outVLabel");
                    elementValueDict.Remove("inVLabel");
                    elementValueDict.Remove("properties");
                    elementValueDict.Remove("value");
                }
            }
            return serializedElement;
        }
    }
}