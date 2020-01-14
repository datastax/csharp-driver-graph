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
using System.Threading.Tasks;
using Cassandra.DataStax.Graph.Serialization;
using Cassandra.Geometry;
using Gremlin.Net.Process.Remote;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure.IO.GraphSON;
using Newtonsoft.Json.Linq;

namespace Cassandra.DataStax.Graph
{
    internal class DseRemoteConnection : IRemoteConnection
    {
        private static readonly ITypeSerializer[] CustomSerializers =
        {
            new DurationSerializer(),
            new TypeSerializer<LocalDate>("gx", "LocalDate", LocalDate.Parse),
            new TypeSerializer<LocalTime>("gx", "LocalTime", LocalTime.Parse),
            new TypeSerializer<Point>("dse", "Point", Point.Parse),
            new TypeSerializer<LineString>("dse", "LineString", LineString.Parse),
            new TypeSerializer<Polygon>("dse", "Polygon", Polygon.Parse),
            new ByteArraySerializer(),
            new DateTimeOffsetSerializer("g", "Timestamp"),
            new DateTimeOffsetSerializer("gx", "Instant"),
            new GraphNodeSerializer()
        };

        private static readonly GraphSONReader Reader = new GraphSONReader(DseRemoteConnection.CustomSerializers
            .Where(s => s.FullTypeName != null).ToDictionary(s => s.FullTypeName, s => (IGraphSONDeserializer) s));

        internal static readonly GraphSONWriter Writer = new GraphSONWriter(
            DseRemoteConnection.CustomSerializers.GroupBy(s => s.Type).Select(g => g.First())
                             .ToDictionary(s => s.Type, s => (IGraphSONSerializer) s));
        
        private const string GraphLanguage = "bytecode-json";
        
        private readonly GraphOptions _graphOptions;
        private readonly ISession _session;

        internal DseRemoteConnection(ISession session, GraphOptions graphOptions)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _graphOptions = graphOptions;
        }

        public async Task<ITraversal<TStart, TEnd>> SubmitAsync<TStart, TEnd>(Bytecode bytecode)
        {
            var query = DseRemoteConnection.Writer.WriteObject(bytecode);
            var graphStatement = DseRemoteConnection.CreateStatement(query, _graphOptions);
            var rs = await _session.ExecuteGraphAsync(graphStatement);
            var graphTraversal = new GraphTraversal<TStart, TEnd>
            {
                Traversers = GetTraversers(rs)
            };
            return graphTraversal;
        }

        internal static IGraphStatement CreateStatement(string query, GraphOptions graphOptions)
        {
            var graphStatement = new SimpleGraphStatement(query).SetGraphLanguage(DseRemoteConnection.GraphLanguage);
            if (graphOptions == null)
            {
                return graphStatement;
            }
            graphStatement
                .SetGraphSource(graphOptions.Source)
                .SetGraphName(graphOptions.Name)
                .SetReadTimeoutMillis(graphOptions.ReadTimeoutMillis);
            if (graphOptions.ReadConsistencyLevel != null)
            {
                graphStatement.SetGraphReadConsistencyLevel(graphOptions.ReadConsistencyLevel.Value);
            }
            if (graphOptions.WriteConsistencyLevel != null)
            {
                graphStatement.SetGraphWriteConsistencyLevel(graphOptions.WriteConsistencyLevel.Value);
            }
            return graphStatement;
        }

        private IEnumerable<Traverser> GetTraversers(GraphResultSet rs)
        {
            foreach (var graphNode in rs)
            {
                yield return new Traverser(AdaptGraphNode(graphNode));
            }
        }

        private object AdaptGraphNode(GraphNode graphNode)
        {
            // Avoid using conversion to JToken TINKERPOP-1696 is implemented
            var json = (JToken) graphNode.GetRaw();
            return DseRemoteConnection.Reader.ToObject(json);
        }
    }
}
