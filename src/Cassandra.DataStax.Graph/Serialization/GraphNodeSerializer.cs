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
using System.Collections.Generic;
using System.Linq;
using Gremlin.Net.Structure.IO.GraphSON;
using Newtonsoft.Json.Linq;

namespace Cassandra.DataStax.Graph.Serialization
{
    internal class GraphNodeSerializer : ITypeSerializer
    {
        public Dictionary<string, dynamic> Dictify(dynamic objectData, GraphSONWriter writer)
        {
            GraphNode graphNode = objectData;
            JToken token = graphNode.GetRaw();
            if (!(token is JObject obj))
            {
                throw new NotSupportedException("Can not serialize a GraphNode that doesn't represent an object");
            }
            return JObjectToGraphSON(obj);
        }

        private Dictionary<string, dynamic> JObjectToGraphSON(JObject obj)
        {
            var result = new Dictionary<string, dynamic>();
            foreach (var kv in obj)
            {
                result.Add(kv.Key, ToGraphSON(kv.Value));
            }
            return result;
        }

        private object ToGraphSON(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return JObjectToGraphSON((JObject) token);
                case JTokenType.Array:
                {
                    var array = (JArray) token;
                    return array.Select(ToGraphSON).ToArray();
                }
                case JTokenType.Boolean:
                    return token.ToObject<bool>();
                case JTokenType.Integer:
                    return token.ToObject<int>();
                case JTokenType.Float:
                    return token.ToObject<double>();
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return null;
                default:
                    return token.ToObject<string>();
            }
        }

        public dynamic Objectify(JToken graphsonObject, GraphSONReader reader)
        {
            throw new NotSupportedException("GraphNodeSerializer should not be using for deserialization");
        }

        public string FullTypeName => null;

        public Type Type => typeof(GraphNode);
    }
}