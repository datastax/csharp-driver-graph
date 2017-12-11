//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dse.Geometry;
using Dse.Graph.Serialization;
using Gremlin.Net.Process.Remote;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure.IO.GraphSON;
using Newtonsoft.Json.Linq;

namespace Dse.Graph
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

        private static readonly GraphSONReader Reader = new GraphSONReader(CustomSerializers
            .Where(s => s.FullTypeName != null).ToDictionary(s => s.FullTypeName, s => (IGraphSONDeserializer) s));

        internal static readonly GraphSONWriter Writer = new GraphSONWriter(
            CustomSerializers.GroupBy(s => s.Type).Select(g => g.First())
                             .ToDictionary(s => s.Type, s => (IGraphSONSerializer) s));
        
        private const string GraphLanguage = "bytecode-json";
        
        private readonly GraphOptions _graphOptions;
        private readonly IDseSession _session;

        internal DseRemoteConnection(IDseSession session, GraphOptions graphOptions)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _graphOptions = graphOptions;
        }

        public async Task<ITraversal<TStart, TEnd>> SubmitAsync<TStart, TEnd>(Bytecode bytecode)
        {
            var query = Writer.WriteObject(bytecode);
            var graphStatement = CreateStatement(query, _graphOptions);
            var rs = await _session.ExecuteGraphAsync(graphStatement);
            var graphTraversal = new GraphTraversal<TStart, TEnd>
            {
                Traversers = GetTraversers(rs)
            };
            return graphTraversal;
        }

        internal static IGraphStatement CreateStatement(string query, GraphOptions graphOptions)
        {
            var graphStatement = new SimpleGraphStatement(query).SetGraphLanguage(GraphLanguage);
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
            return Reader.ToObject(json);
        }
    }
}
