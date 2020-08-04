﻿//
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
using System.Linq;
using System.Threading.Tasks;
using Gremlin.Net.Process.Traversal;
using NUnit.Framework;

namespace Cassandra.DataStax.Graph.Test.Integration
{
    public class StatementCoreIntegrationTest : BaseIntegrationTest
    {
        public StatementCoreIntegrationTest() : base(BaseIntegrationTest.DefaultCoreGraphName)
        {
        }

        [Test]
        public async Task Should_Create_A_Query_From_A_Traversal()
        {
            var g = DseGraph.Traversal(Session);
            var statement = DseGraph.StatementFromTraversal(g.V().HasLabel("person").Has("name", P.Eq("marko")));
            var rs = await Session.ExecuteGraphAsync(statement);
            var rsSync = Session.ExecuteGraph(statement);
            // The result should be DSE driver vertices
            StatementCoreIntegrationTest.VerifyGraphResultSet(Session, rs);
            StatementCoreIntegrationTest.VerifyGraphResultSet(Session, rsSync);
        }

        [Test]
        public async Task Should_Execute_A_Traversal()
        {
            var g = DseGraph.Traversal(Session);
            var rs = await Session.ExecuteGraphAsync(g.V().HasLabel("person").Has("name", P.Eq("marko")));
            var rsSync = Session.ExecuteGraph(g.V().HasLabel("person").Has("name", P.Eq("marko")));
            // The result should be DSE driver vertices
            StatementCoreIntegrationTest.VerifyGraphResultSet(Session, rs);
            StatementCoreIntegrationTest.VerifyGraphResultSet(Session, rsSync);
        }

        private static void VerifyGraphResultSet(ISession session, GraphResultSet rs)
        {
            var v = rs.First().ToVertex();
            Assert.AreEqual("person", v.Label);
            TestHelper.FillVertexProperties(session, v);
            Assert.AreEqual("marko", v.GetProperty("name").Value.ToString());
            Assert.AreEqual(29, v.GetProperty("age").Value.ToInt32());
        }

        [Test]
        public void Should_Use_Vertex_Id_As_Parameter()
        {
            var g = DseGraph.Traversal(Session);
            var statement = DseGraph.StatementFromTraversal(g.V().HasLabel("person").Has("name", "marko"));
            var rs = Session.ExecuteGraph(statement);
            var markoArray = rs.ToArray();
            Assert.AreEqual(1, markoArray.Length);
            var markoVertex = markoArray[0].ToVertex();
            var getStmt = DseGraph.StatementFromTraversal(g.V(markoVertex.Id));
            var getvertexById = Session.ExecuteGraph(getStmt);
            var getByIdArray = getvertexById.ToArray();
            Assert.AreEqual(1, getByIdArray.Length);
            Assert.AreEqual("person", getByIdArray[0].ToVertex().Label);
        }

        [Test]
        public void Should_Use_Edge_Id_As_Parameter()
        {
            var g = DseGraph.Traversal(Session);
            var knowsStatement = DseGraph.StatementFromTraversal(g.With("allow-filtering").E().HasLabel("knows").Has("weight", 0.5f));
            var knowsStatementResultSet = Session.ExecuteGraph(knowsStatement);
            var knowsResultArray = knowsStatementResultSet.ToArray();
            Assert.AreEqual(1, knowsResultArray.Length);
            var knowsEdge = knowsResultArray[0].ToEdge();
            var getKnowsByIdStatement = DseGraph.StatementFromTraversal(g.E(knowsEdge.Id));
            var knowsByIdResult = Session.ExecuteGraph(getKnowsByIdStatement);
            var knowsByIdArray = knowsByIdResult.ToArray();
            Assert.AreEqual(1, knowsByIdArray.Length);
            var edge = knowsByIdArray[0].ToEdge();
            Assert.AreEqual("knows", edge.Label);
            TestHelper.FillEdgeProperties(Session, edge);
            Assert.AreEqual(0.5f, edge.Properties["weight"].Get<float>("value"));
        }

        [Test]
        public void Should_Be_Able_To_Create_Vertex_With_Multicardinality_Property()
        {
            var g = DseGraph.Traversal(Session);
            var direwolves = new[] {"Ghost", "Nymeria", "Shaggydog", "Grey Wind", "Lady"};
            var stmt = DseGraph.StatementFromTraversal(
                g.AddV("multi_v")
                    .Property("pk", Guid.NewGuid())
                    .Property("multi_prop", new object[] { direwolves }));
            Session.ExecuteGraph(stmt);
            StatementCoreIntegrationTest.VerifyMultiCardinalityProperty(Session, g, direwolves);
            g.V().HasLabel("multi_v").Drop().Next();
        }

        [Test]
        public void Should_Be_Able_To_Create_Vertex_With_CollectionProperties()
        {
            var g = DseGraph.Traversal(Session);
            var stmt = DseGraph.StatementFromTraversal(g.AddV("meta_v")
                .Property("meta_prop", "What can kill a dragon")
                .Property("sub_prop", new List<string> { "Qyburn's scorpion" })
                .Property("sub_prop2", new List<string> { "Another dragon" }));
            Session.ExecuteGraph(stmt);
            StatementCoreIntegrationTest.VerifyCollectionProperties(Session, g, "What can kill a dragon", "Qyburn's scorpion", "Another dragon");
            var dropstmt = DseGraph.StatementFromTraversal(g.V().HasLabel("meta_v").Drop());
            Session.ExecuteGraph(dropstmt);
        }

        // TODO GRAPH (UDT)
        //[Test]
        //public void Should_Be_Able_To_Create_Vertex_With_MetaProperties()
        //{
        //    var g = DseGraph.Traversal(Session);
        //    var stmt = DseGraph.StatementFromTraversal(g.AddV("meta_v")
        //        .Property("meta_prop", "What can kill a dragon", "sub_prop", "Qyburn's scorpion", "sub_prop2", "Another dragon"));
        //    Session.ExecuteGraph(stmt);
        //    StatementCoreIntegrationTest.VerifyMetaProperties(Session, g, "What can kill a dragon", "Qyburn's scorpion", "Another dragon");
        //    var dropstmt = DseGraph.StatementFromTraversal(g.V().HasLabel("meta_v").Drop());
        //    Session.ExecuteGraph(dropstmt);
        //}

        [Test]
        public void Should_Make_OLAP_Query_Using_Statement()
        {
            var g = DseGraph.Traversal(Session, new GraphOptions().SetSourceAnalytics());
            var statement = DseGraph.StatementFromTraversal(g.V().HasLabel("person")
                .PeerPressure().By("cluster")).SetGraphSourceAnalytics();
            try
            {
                var rs = Session.ExecuteGraph(statement);
                var result = rs.ToArray();
                Assert.AreEqual(4, result.Length);
            }
            catch (InvalidQueryException ex)
            {
                Assert.Inconclusive("Graph OLAP tests are not stable. Exception: " + ex);
            }
        }

        public static void VerifyCollectionProperties(ISession session, GraphTraversalSource g, string meta, string sub1, string sub2)
        {
            var stmt = DseGraph.StatementFromTraversal(g.V().HasLabel("meta_v"));
            var walkers = session.ExecuteGraph(stmt);
            var result = walkers.FirstOrDefault();
            Assert.IsNotNull(result);
            var nightKing = result.ToVertex();
            Assert.AreEqual("meta_v", nightKing.Label);
            TestHelper.FillVertexProperties(session, nightKing);
            var metaProp = nightKing.GetProperty("meta_prop").Value.To<string>();
            Assert.AreEqual(meta, metaProp);
            CollectionAssert.AreEqual(new [] { sub1 }, nightKing.GetProperty("sub_prop").Value.To<IEnumerable<string>>());
            CollectionAssert.AreEqual(new [] { sub2 }, nightKing.GetProperty("sub_prop2").Value.To<IEnumerable<string>>());
        }

        // TODO GRAPH UDT
        //public static void VerifyMetaProperties(ISession session, GraphTraversalSource g, string meta, string sub1, string sub2)
        //{
        //    var stmt = DseGraph.StatementFromTraversal(g.V().HasLabel("meta_v"));
        //    var walkers = session.ExecuteGraph(stmt);
        //    var result = walkers.FirstOrDefault();
        //    Assert.IsNotNull(result);
        //    var nightKing = result.ToVertex();
        //    Assert.AreEqual("meta_v", nightKing.Label);
        //    var properties = nightKing.Properties["meta_prop"].ToArray();
        //    Assert.AreEqual(1, properties.Length);
        //    Assert.AreEqual(meta, properties[0].Get<string>("value"));
        //    var subProperties = properties[0].Get<IDictionary<string, string>>("properties");
        //    Assert.AreEqual(sub1, subProperties["sub_prop"]);
        //    Assert.AreEqual(sub2, subProperties["sub_prop2"]);
        //}

        public static void VerifyMultiCardinalityProperty(ISession session, GraphTraversalSource g, string[] values)
        {
            var stmt = DseGraph.StatementFromTraversal(g.V().HasLabel("multi_v"));
            var vertex = session.ExecuteGraph(stmt);
            var result = vertex.FirstOrDefault();
            Assert.IsNotNull(result);
            var multiPropertyVertex = result.ToVertex();
            Assert.AreEqual("multi_v", multiPropertyVertex.Label);
            TestHelper.FillVertexProperties(session, multiPropertyVertex);
            var properties = multiPropertyVertex.GetProperty("multi_prop").Value.To<IList<string>>();
            Assert.AreEqual(values.Length, properties.Count);
            CollectionAssert.AreEqual(values, properties);
        }
    }
}