//
//  Copyright DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gremlin.Net.Process.Traversal;
using NUnit.Framework;

namespace Dse.Graph.Test.Integration
{
    public class StatementIntegrationTest : BaseIntegrationTest
    {
        [Test]
        public async Task Should_Create_A_Query_From_A_Traversal()
        {
            var g = DseGraph.Traversal(Session);
            var statement = DseGraph.StatementFromTraversal(g.V().HasLabel("person").Has("name", P.Eq("marko")));
            var rs = await Session.ExecuteGraphAsync(statement);
            var rsSync = Session.ExecuteGraph(statement);
            // The result should be DSE driver vertices
            VerifyGraphResultSet(rs);
            VerifyGraphResultSet(rsSync);
        }

        [Test]
        public async Task Should_Execute_A_Traversal()
        {
            var g = DseGraph.Traversal(Session);
            var rs = await Session.ExecuteGraphAsync(g.V().HasLabel("person").Has("name", P.Eq("marko")));
            var rsSync = Session.ExecuteGraph(g.V().HasLabel("person").Has("name", P.Eq("marko")));
            // The result should be DSE driver vertices
            VerifyGraphResultSet(rs);
            VerifyGraphResultSet(rsSync);
        }

        private static void VerifyGraphResultSet(GraphResultSet rs)
        {
            var v = rs.First().ToVertex();
            Assert.AreEqual("person", v.Label);
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
            var knowsStatement = DseGraph.StatementFromTraversal(g.E().HasLabel("knows").Has("weight", 0.5f));
            var knowsStatementResultSet = Session.ExecuteGraph(knowsStatement);
            var knowsResultArray = knowsStatementResultSet.ToArray();
            Assert.AreEqual(1, knowsResultArray.Length);
            var knowsEdge = knowsResultArray[0].ToEdge();
            var getKnowsByIdStatement = DseGraph.StatementFromTraversal(g.E(knowsEdge.Id));
            var knowsByIdResult = Session.ExecuteGraph(getKnowsByIdStatement);
            var knowsByIdArray = knowsByIdResult.ToArray();
            Assert.AreEqual(1, knowsByIdArray.Length);
            Assert.AreEqual("knows", knowsByIdArray[0].ToEdge().Label);
            Assert.AreEqual(0.5f, knowsByIdArray[0].ToEdge().Properties["weight"].Get<float>("value"));
        }

        [Test]
        public void Should_Be_Able_To_Create_Vertex_With_Multicardinality_Property()
        {
            var g = DseGraph.Traversal(Session);
            var direwolves = new[] {"Ghost", "Nymeria", "Shaggydog", "Grey Wind", "Lady"};
            var addVertext = g.AddV("multi_v");
            foreach (var t in direwolves)
            {
                addVertext = addVertext.Property("multi_prop", t);
            }
            var stmt = DseGraph.StatementFromTraversal(addVertext);
            Session.ExecuteGraph(stmt);
            VerifyMultiCardinalityProperty(Session, g, direwolves);
            g.V().HasLabel("multi_v").Drop().Next();
        }

        [Test]
        public void Should_Be_Able_To_Create_Vertex_With_MetaProperties()
        {
            var g = DseGraph.Traversal(Session);
            var stmt = DseGraph.StatementFromTraversal(g.AddV("meta_v")
                .Property("meta_prop", "What can kill a dragon", "sub_prop", "Qyburn's scorpion", "sub_prop2", "Another dragon"));
            Session.ExecuteGraph(stmt);
            VerifyMetaProperties(Session, g, "What can kill a dragon", "Qyburn's scorpion", "Another dragon");
            var dropstmt = DseGraph.StatementFromTraversal(g.V().HasLabel("meta_v").Drop());
            Session.ExecuteGraph(dropstmt);
        }

        [Test]
        public void Should_Make_OLAP_Query_Using_Statement()
        {
            var g = DseGraph.Traversal(Session, new GraphOptions().SetSourceAnalytics());
            var statement = DseGraph.StatementFromTraversal(g.V().HasLabel("person")
                .PeerPressure().By("cluster")).SetGraphSourceAnalytics();
            var rs = Session.ExecuteGraph(statement);
            var result = rs.ToArray();
            Assert.AreEqual(4, result.Length);
        }

        public static void VerifyMetaProperties(IDseSession session, GraphTraversalSource g, string meta, string sub1, string sub2)
        {
            var stmt = DseGraph.StatementFromTraversal(g.V().HasLabel("meta_v"));
            var walkers = session.ExecuteGraph(stmt);
            var result = walkers.FirstOrDefault();
            Assert.IsNotNull(result);
            var nightKing = result.ToVertex();
            Assert.AreEqual("meta_v", nightKing.Label);
            var properties = nightKing.Properties["meta_prop"].ToArray();
            Assert.AreEqual(1, properties.Length);
            Assert.AreEqual(meta, properties[0].Get<string>("value"));
            var subProperties = properties[0].Get<IDictionary<string, string>>("properties");
            Assert.AreEqual(sub1, subProperties["sub_prop"]);
            Assert.AreEqual(sub2, subProperties["sub_prop2"]);
        }

        public static void VerifyMultiCardinalityProperty(IDseSession session, GraphTraversalSource g, string[] values)
        {
            var stmt = DseGraph.StatementFromTraversal(g.V().HasLabel("multi_v"));
            var vertex = session.ExecuteGraph(stmt);
            var result = vertex.FirstOrDefault();
            Assert.IsNotNull(result);
            var multiPropertyVertex = result.ToVertex();
            Assert.AreEqual("multi_v", multiPropertyVertex.Label);
            var properties = multiPropertyVertex.Properties["multi_prop"].ToArray();
            Assert.AreEqual(values.Length, properties.Length);
            for (var i = 0; i < values.Length; i++)
            {
                Assert.AreEqual(values[i], properties[i].Get<string>("value"));
            }
        }
    }
}