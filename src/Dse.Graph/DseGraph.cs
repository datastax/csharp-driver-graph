//
//  Copyright DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Threading.Tasks;
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
        /// Configurations to use for this traversal source. When provided, the options on this instance will override
        /// the ones defined when building the <see cref="IDseCluster"/> instance for <c>GraphTraversal</c>
        /// execution methods like <c>ToList()</c>.
        /// <para>
        /// These options won't have any effect when using the method <see cref="StatementFromTraversal(ITraversal)"/>. 
        /// </para>
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
        /// <para>
        /// Note that the <c>IGraphStatement</c> will use the default <c>GraphOptions</c> at cluster level and not the
        /// ones defined on the <c>GraphTraversalSource</c>. 
        /// </para>
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

        /// <summary>
        /// Creates a <see cref="IGraphStatement"/> from the given traversal and executes it using the provided session.
        /// </summary>
        /// <param name="session">The session instance used to execute the statement.</param>
        /// <param name="traversal">The traversal to create the statement to execute.</param>
        public static GraphResultSet ExecuteGraph(this IDseSession session, ITraversal traversal)
        {
            return session.ExecuteGraph(StatementFromTraversal(traversal));
        }

        /// <summary>
        /// Creates a <see cref="IGraphStatement"/> from the given traversal and asynchronously executes it using the
        /// provided session.
        /// </summary>
        /// <param name="session">The session instance used to execute the statement.</param>
        /// <param name="traversal">The traversal to create the statement to execute.</param>
        public static Task<GraphResultSet> ExecuteGraphAsync(this IDseSession session, ITraversal traversal)
        {
            return session.ExecuteGraphAsync(StatementFromTraversal(traversal));
        }
    }
}
