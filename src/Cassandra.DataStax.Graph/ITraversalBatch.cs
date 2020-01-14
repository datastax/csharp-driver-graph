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
using Gremlin.Net.Process.Traversal;

namespace Cassandra.DataStax.Graph
{
    /// <summary>
    /// Represents a group of traversals to allow executing multiple DSE Graph mutation operations inside the
    /// same transaction.
    /// </summary>
    /// <inheritdoc />
    public interface ITraversalBatch : IReadOnlyCollection<ITraversal>
    {
        /// <summary>
        /// Adds a new traversal to this batch.
        /// </summary>
        /// <returns>This instance.</returns>
        ITraversalBatch Add(ITraversal traversal);

        /// <summary>
        /// Converts this batch into a <see cref="IGraphStatement"/> that can be executed using
        /// a <see cref="ISession"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Throws InvalidOperationException when the batch does not contain any traversals.
        /// </exception>
        IGraphStatement ToGraphStatement();
    }
}