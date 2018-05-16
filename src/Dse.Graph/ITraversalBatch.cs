//
//  Copyright DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Generic;
using Gremlin.Net.Process.Traversal;

namespace Dse.Graph
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
        /// a <see cref="IDseSession"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Throws InvalidOperationException when the batch does not contain any traversals.
        /// </exception>
        IGraphStatement ToGraphStatement();
    }
}