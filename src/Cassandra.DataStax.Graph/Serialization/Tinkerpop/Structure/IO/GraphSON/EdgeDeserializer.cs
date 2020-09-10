﻿#region License

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

#endregion

using Cassandra.DataStax.Graph.Internal;
using Newtonsoft.Json.Linq;

namespace Cassandra.DataStax.Graph.Serialization.Tinkerpop.Structure.IO.GraphSON
{
    internal class EdgeDeserializer : IGraphSONDeserializer
    {
        public dynamic Objectify(JToken graphsonObject, IGraphSONReader reader)
        {
            var outVId = reader.ToObject(graphsonObject["outV"]);
            var outVLabel = (string) (graphsonObject["outVLabel"] ?? Gremlin.Net.Structure.Vertex.DefaultLabel);
            var outV = new Gremlin.Net.Structure.Vertex(outVId, outVLabel);
            var inVId = reader.ToObject(graphsonObject["inV"]);
            var inVLabel = (string) (graphsonObject["inVLabel"] ?? Gremlin.Net.Structure.Vertex.DefaultLabel);
            var inV = new Gremlin.Net.Structure.Vertex(inVId, inVLabel);
            var edgeId = reader.ToObject(graphsonObject["id"]);
            var edgeLabel = (string) graphsonObject["label"] ?? "edge";
            return new Gremlin.Net.Structure.Edge(edgeId, outV, edgeLabel, inV);
        }
    }
}