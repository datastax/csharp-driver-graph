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
using Cassandra.Mapping.TypeConversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cassandra.DataStax.Graph.Test.Integration
{
    public static class TestHelper
    {
        public static bool IsWin
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                        return true;
                }
                return false;
            }
        }

        public static string GetTestClusterNameBasedOnTime()
        {
            return "test_" + (DateTimeOffset.UtcNow.Ticks / TimeSpan.TicksPerSecond);
        }
        
        public static void FillEdgeProperties(ISession session, IEdge edge)
        {
            var castedEdge = (Edge) edge;

            if (castedEdge.Properties.Count != 0)
            {
                return;
            }

            var rs = session.ExecuteGraph(
                new SimpleGraphStatement("g.E(edge_id).properties()", new { edge_id = castedEdge.Id })
                    .SetGraphLanguage("gremlin-groovy"));
            var propertiesList = rs.Select(p => new Tuple<GraphNode, IProperty>(p, p.To<IProperty>())).ToList();
            foreach (var prop in propertiesList)
            {
                castedEdge.Properties.Add(prop.Item2.Name, prop.Item1);
            }
        }
        
        public static void FillVertexProperties(ISession session, IVertex vertex)
        {
            var castedVertex = (Vertex) vertex;

            if (castedVertex.Properties.Count != 0)
            {
                return;
            }

            var rs = session.ExecuteGraph(
                new SimpleGraphStatement("g.V(vertex_id).properties().group().by(key)", new { vertex_id = castedVertex.Id })
                    .SetGraphLanguage("gremlin-groovy")).Single().To<IDictionary<string, GraphNode>>();
            
            foreach (var propertiesByName in rs)
            {
                // "properties" is a map where the key is the vertexproperty name
                // and the value is a json array of vertex properties with that name

                castedVertex.Properties.Add(
                    propertiesByName.Key,
                    propertiesByName.Value.Get<GraphNode>("@value"));
            }
        }
    }
}
