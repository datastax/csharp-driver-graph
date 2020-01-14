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

using System.Threading.Tasks;
using Cassandra.DataStax.Graph.Test.Integration.TestClusterManagement;
using Gremlin.Net.Process.Traversal;
using NUnit.Framework;

namespace Cassandra.DataStax.Graph.Test.Integration
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