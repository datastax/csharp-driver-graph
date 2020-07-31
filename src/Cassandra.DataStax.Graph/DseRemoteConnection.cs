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
using System.Threading.Tasks;

using Cassandra.DataStax.Graph.Serialization;

using Gremlin.Net.Process.Remote;
using Gremlin.Net.Process.Traversal;

namespace Cassandra.DataStax.Graph
{
    internal class DseRemoteConnection : IRemoteConnection
    {
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
            var graphStatement = DseRemoteConnection.CreateStatement(bytecode, _graphOptions, true);
            var rs = await _session.ExecuteGraphAsync(graphStatement).ConfigureAwait(false);
            var graphTraversal = new GraphTraversal<TStart, TEnd>
            {
                Traversers = GetTraversers<TEnd>(rs)
            };
            return graphTraversal;
        }

        internal static IGraphStatement CreateStatement(
            object queryBytecode, GraphOptions graphOptions, bool customDeserializers)
        {
            IGraphStatement graphStatement;
            if (customDeserializers)
            {
                graphStatement = new FluentGraphStatement(
                    queryBytecode, GraphSONUtils.CustomSerializers, GraphSONUtils.CustomDeserializers);
            }
            else
            {
                graphStatement = new FluentGraphStatement(queryBytecode, GraphSONUtils.CustomSerializers);
            }

            graphStatement = graphStatement.SetGraphLanguage(DseRemoteConnection.GraphLanguage);

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

        private IEnumerable<Gremlin.Net.Process.Traversal.Traverser> GetTraversers<TEnd>(GraphResultSet rs)
        {
            foreach (var graphNode in rs)
            {
                yield return new Gremlin.Net.Process.Traversal.Traverser(graphNode.To<TEnd>());
            }
        }
    }
}