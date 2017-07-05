//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using Gremlin.Net.Process.Traversal;

namespace Dse.Graph
{
    /// <summary>
    /// Utility class for interacting with DataStax Enterprise Graph and Apache TinkerPop.
    /// </summary>
    public static class DseGraph
    {
        /// <summary>
        /// Creates a new Apache TinkerPop's <c>GraphTraversalSource</c> instance, that can be used to build 
        /// <c>GraphTraversal</c> instances.
        /// </summary>
        /// <param name="session">
        /// The session instance, representing a pool of connections connected to the DSE cluster.
        /// </param>
        /// <param name="graphOptions">
        /// Configurations to use for this traversal source. When provided, the options on this object will override
        /// the ones defined when building the <see cref="IDseCluster"/> instance.
        /// </param>
        public static GraphTraversalSource Traversal(IDseSession session, GraphOptions graphOptions = null)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            return new GraphTraversalSource().WithRemote(new DseRemoteConnection(session, graphOptions));
        }

        /// <summary>
        /// Creates an initialized <see cref="IGraphStatement"/> from a <c>GraphTraversal</c> to use directly with a 
        /// <see cref="IDseSession"/>.
        /// </summary>
        public static IGraphStatement StatementFromTraversal(ITraversal traversal)
        {
            if (traversal == null)
            {
                throw new ArgumentNullException(nameof(traversal));
            }
            var query = DseRemoteConnection.Writer.WriteObject(traversal.Bytecode);
            return DseRemoteConnection.CreateStatement(query, null);
        }
    }
}
