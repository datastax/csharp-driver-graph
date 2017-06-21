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
using Gremlin.Net.Process.Remote;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure.IO.GraphSON;
using Newtonsoft.Json.Linq;

namespace Dse.Graph
{
    internal class DseRemoteConnection : IRemoteConnection
    {
        private readonly IDseSession _session;
        private readonly GraphSONReader _reader;
        private readonly GraphSONWriter _writer;
        private const string GraphLanguage = "bytecode-json";

        internal DseRemoteConnection(IDseSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _reader = new GraphSONReader();
            _writer = new GraphSONWriter();
        }

        public async Task<ITraversal<TStart, TEnd>> SubmitAsync<TStart, TEnd>(Bytecode bytecode)
        {
            var query = _writer.WriteObject(bytecode);
            var graphStatement = new SimpleGraphStatement(query).SetGraphLanguage(GraphLanguage);
            var rs = await _session.ExecuteGraphAsync(graphStatement);
            var graphTraversal = new GraphTraversal<TStart, TEnd>
            {
                Traversers = GetTraversers(rs)
            };
            return graphTraversal;
        }

        private IEnumerable<Traverser> GetTraversers(GraphResultSet rs)
        {
            foreach (var graphNode in rs)
            {
                yield return new Traverser(_reader.ToObject(JToken.Parse(graphNode.ToString())));
            }
        }
    }
}
