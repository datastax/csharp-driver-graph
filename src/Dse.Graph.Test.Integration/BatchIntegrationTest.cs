//
//  Copyright DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System.Threading.Tasks;
using Dse.Graph.Test.Integration.TestClusterManagement;
using Gremlin.Net.Process.Traversal;
using NUnit.Framework;

namespace Dse.Graph.Test.Integration
{
    [TestDseVersion(6, 0)]
    public class BatchIntegrationTest : BaseIntegrationTest
    {
        [Test]
        public void Should_Execute_Batch_Of_Traversals()
        {
            var g = DseGraph.Traversal(Session);
            var batch = DseGraph.Batch(new GraphOptions().SetWriteConsistencyLevel(ConsistencyLevel.LocalQuorum));
            batch
                .Add(g.AddV("character").Property("name", "Matias").Property("age", 12).Property("tag", "batch1"))
                .Add(g.AddV("character").Property("name", "Olivia").Property("age", 8).Property("tag", "batch1"));

            Session.ExecuteGraph(batch);

            var list = g.V().Has("character", "tag", "batch1").Values<string>("name").ToList();
            CollectionAssert.AreEquivalent(new [] { "Matias", "Olivia"}, list);
        }

        [Test]
        public async Task Should_Execute_Batch_Of_Traversals_Asynchronously_With_Edges()
        {
            var g = DseGraph.Traversal(Session);
            var batch = DseGraph.Batch(new GraphOptions().SetWriteConsistencyLevel(ConsistencyLevel.LocalQuorum));
            batch
                .Add(g.AddV("character").Property("name", "Matias").Property("age", 12).Property("tag", "batch2"))
                .Add(g.AddV("character").Property("name", "Olivia").Property("age", 8).Property("tag", "batch2"))
                .Add(g.V().Has("name", "Matias").Has("tag", "batch2").AddE("knows").To(
                        __.V().Has("name", "Olivia").Has("tag", "batch2")));

            await Session.ExecuteGraphAsync(batch);

            var characters = g.V().Has("character", "tag", "batch2").Values<string>("name").ToList();
            CollectionAssert.AreEquivalent(new [] { "Matias", "Olivia"}, characters);

            var knowsOut = g.V().Has("name", "Matias").Has("tag", "batch2")
                                .Out("knows").Values<string>("name").ToList();
            Assert.AreEqual(new [] { "Olivia" }, knowsOut);
        }
    }
}