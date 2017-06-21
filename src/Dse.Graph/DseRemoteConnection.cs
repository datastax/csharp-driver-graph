//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System.Threading.Tasks;
using Gremlin.Net.Process.Remote;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure.IO.GraphSON;

namespace Dse.Graph
{
    internal class DseRemoteConnection : IRemoteConnection
    {
        private readonly IDseSession _session;
        private readonly GraphSONReader _graphSONReader;
        private readonly GraphSONWriter _graphSONWriter;

        internal DseRemoteConnection(IDseSession session)
        {
            _session = session;
            _graphSONReader = new GraphSONReader();
            _graphSONWriter = new GraphSONWriter();
        }

        public async Task<ITraversal<TStart, TEnd>> SubmitAsync<TStart, TEnd>(Bytecode bytecode)
        {
            var query = _graphSONWriter.WriteObject(bytecode);
            var graphStatement = new SimpleGraphStatement(query);
            var rs = await _session.ExecuteGraphAsync(graphStatement);
            var graphTraversal = new GraphTraversal<TStart, TEnd>();
            return graphTraversal;
        }
    }
}
