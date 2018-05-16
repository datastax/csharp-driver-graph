//
//  Copyright DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gremlin.Net.Process.Traversal;

namespace Dse.Graph
{
    internal class TraversalBatch : ITraversalBatch
    {
        private readonly IList<ITraversal> _list = new List<ITraversal>();
        private readonly GraphOptions _options;

        public int Count => _list.Count;

        internal TraversalBatch(GraphOptions options)
        {
            _options = options;
        }

        public IEnumerator<ITraversal> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public ITraversalBatch Add(ITraversal traversal)
        {
            _list.Add(traversal);
            return this;
        }

        public IGraphStatement ToGraphStatement()
        {
            if (_list.Count == 0)
            {
                throw new InvalidOperationException("Batch does not contain traversals");
            }
            var query = DseRemoteConnection.Writer.WriteObject(_list.Select(t => t.Bytecode).ToArray());
            return DseRemoteConnection.CreateStatement(query, _options);
        }
    }
}