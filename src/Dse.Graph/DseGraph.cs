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

        /// <summary>
        /// Create a <see cref="ITraversalBatch"/> instance to perform batch mutations in DSE Graph.
        /// <para>
        /// Mutations made to the DSE Graph may all or none succeed. In case of an issue during one of the mutations,
        /// the execution of the batch will fail and none of the mutations will have been applied.
        /// </para>
        /// <para>
        /// A <see cref="ITraversalBatch"/> can be directly executed via
        /// <see cref="ExecuteGraph(IDseSession, ITraversalBatch)"/> extension method.
        /// </para>
        /// </summary>
        public static ITraversalBatch Batch(GraphOptions options = null)
        {
            return new TraversalBatch(options);
        }

        /// <summary>
        /// Asynchronously executes a batch of traversals.
        /// </summary>
        /// <param name="session">The session to extend.</param>
        /// <param name="batch">The batch of traversals to execute.</param>
        /// <seealso cref="Batch"/>
        public static Task<GraphResultSet> ExecuteGraphAsync(this IDseSession session, ITraversalBatch batch)
        {
            return session.ExecuteGraphAsync(batch.ToGraphStatement());
        }

        /// <summary>
        /// Executes a batch of traversals.
        /// </summary>
        /// <param name="session">The session to extend.</param>
        /// <param name="batch">The batch of traversals to execute.</param>
        /// <seealso cref="Batch"/>
        public static GraphResultSet ExecuteGraph(this IDseSession session, ITraversalBatch batch)
        {
            return session.ExecuteGraph(batch.ToGraphStatement());
        }
    }
}
