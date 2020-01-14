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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gremlin.Net.Process.Traversal;

namespace Cassandra.DataStax.Graph
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