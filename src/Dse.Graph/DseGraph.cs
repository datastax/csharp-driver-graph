//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using Gremlin.Net.Process.Traversal;

namespace Dse.Graph
{
    /// <summary>
    /// Utility class for interacting with DataStax Enterprise Graph and Apache TinkerPop.
    /// </summary>
    public class DseGraph
    {
        /// <summary>
        /// Creates a new <c>GraphTraversalSource</c> instance.
        /// </summary>
        /// <param name="session">
        /// The session instance, representing a pool of connections connected to the DSE cluster
        /// </param>
        public GraphTraversalSource Traversal(IDseSession session)
        {
            return new GraphTraversalSource().WithRemote(new DseRemoteConnection(session));
        }
    }
}
