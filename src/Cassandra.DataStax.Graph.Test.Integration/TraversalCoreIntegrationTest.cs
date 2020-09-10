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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cassandra.DataStax.Graph.Test.Integration.TestClusterManagement;
using Cassandra.Geometry;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure;
using NUnit.Framework;

namespace Cassandra.DataStax.Graph.Test.Integration
{
    [TestDseVersion(6, 8)]
    public class TraversalCoreIntegrationTest : BaseIntegrationTest
    {
        private static readonly Tuple<string, object>[] PropertyItems =
        {
            Tuple.Create<string, object>("Float", 3.1415f),
            Tuple.Create<string, object>("Duration", new Duration(0, 0, 123000000L)),
            Tuple.Create<string, object>("Date", new LocalDate(1999, 12, 31)),
            Tuple.Create<string, object>("Time", new LocalTime(12, 50, 45, 0)),
            Tuple.Create<string, object>("Timestamp", DateTimeOffset.Parse("2017-08-09 17:21:29+00:00")),
            Tuple.Create<string, object>("Blob", new byte[]{ 1, 2, 3 }),
            Tuple.Create<string, object>("Point", new Point(0, 1)),
            Tuple.Create<string, object>("LineString", 
                new LineString(new Point(0, 0), new Point(0, 1), new Point(1, 1))),
            Tuple.Create<string, object>("Polygon", 
                new Polygon(new Point(-10, 10), new Point(10, 0), new Point(10, 10), new Point(-10, 10)))
        };

        public TraversalCoreIntegrationTest() : base(BaseIntegrationTest.DefaultCoreGraphName)
        {
        }

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            foreach (var type in TraversalCoreIntegrationTest.PropertyItems.Select(i => i.Item1).Distinct())
            {
                var typeDef = $"{type}";
                var schemaQuery = $"schema.vertexLabel(vertexLabel).partitionBy('pk', UUID).property(propertyName, {typeDef}).create();";
                var statement = new SimpleGraphStatement(schemaQuery,
                    new {vertexLabel = $"vertex{type}", propertyName = $"prop{type}"});
                Session.ExecuteGraph(statement.SetGraphLanguage("gremlin-groovy"));
            }
        }
        
        [Test]
        public void Should_Handle_Zero_Results()
        {
            var g = DseGraph.Traversal(Session);
            CollectionAssert.IsEmpty(g.V().HasLabel("person").Has("name", Guid.NewGuid().ToString()).ToList());
        }

        [Test]
        public void Should_Parse_Vertices()
        {
            var g = DseGraph.Traversal(Session);
            var vertices = g.V().HasLabel("person").ToList();
            Assert.GreaterOrEqual(vertices.Count, 4);
            foreach (var vertex in vertices)
            {
                Assert.AreEqual("person", vertex.Label);
            }
        }

        [Test]
        public void Should_Parse_Property_Values()
        {
            var g = DseGraph.Traversal(Session);
            var names = g.V().HasLabel("person").Values<string>("name").ToList();
            Assert.GreaterOrEqual(names.Count, 4);
            CollectionAssert.Contains(names, "peter");
            CollectionAssert.Contains(names, "josh");
            CollectionAssert.Contains(names, "vadas");
            CollectionAssert.Contains(names, "marko");
        }

        [Test]
        public void Should_Parse_Property_Map()
        {
            var g = DseGraph.Traversal(Session);
            var propertyMap = g.V().HasLabel("person").PropertyMap<object>("age").ToList();
            Assert.Greater(propertyMap.Count, 0);
            var firstAge = (IEnumerable) propertyMap.First()["age"];
            var enumerator = firstAge.GetEnumerator();
            enumerator.MoveNext();
            var firstKv = (VertexProperty)enumerator.Current;
            Assert.IsInstanceOf<int>(firstKv.Value);
        }

        [TestCase("Float")]
        [TestCase("Duration")]
        [TestCase("Timestamp")]
        [TestCase("Blob")]
        [TestCase("Point")]
        [TestCase("Linestring")]
        [TestCase("Polygon")]
        public void Should_Handle_Types(string type)
        {
            var g = DseGraph.Traversal(Session);
            foreach (var value in TraversalCoreIntegrationTest.PropertyItems.Where(i => i.Item1 == type).Select(i => i.Item2))
            {
                var vertexLabel = $"vertex{type}";
                var propertyName = $"prop{type}";
                var pk = Guid.NewGuid();
                g.AddV(vertexLabel).Property("pk", pk).Property(propertyName, value).Next();
                var result = g.With("allow-filtering").V().HasLabel(vertexLabel).Has("pk", pk).Has(propertyName, value)
                    .Values<object>(propertyName).Next();
                Assert.AreEqual(value, result);
            }
        }

        [TestCase("Date")]
        [TestCase("Time"), TestDseVersion(5, 1)]
        public void Should_Handle_Date_Time_Types(string type)
        {
            Should_Handle_Types(type);
        }

        [Test]
        public void Should_Use_Vertex_Id_As_Parameter()
        {
            var g = DseGraph.Traversal(Session);
            var vertices = g.V().HasLabel("person").Has("name", "marko").ToList();
            Assert.AreEqual(1, vertices.Count);
            var markoVertex = vertices.First();
            var markoVertexByIdResult = g.V(markoVertex.Id).ToList();
            Assert.AreEqual(1, markoVertexByIdResult.Count);
            Assert.AreEqual("person", markoVertexByIdResult.First().Label);
        }

        [Test]
        public void Should_Use_Edge_Id_As_Parameter()
        {
            var g = DseGraph.Traversal(Session);
            var edges = g.With("allow-filtering").E().HasLabel("knows").Has("weight", 0.5f).ToList();
            Assert.AreEqual(1, edges.Count);
            var knowEdge = edges.First();
            var knowEdgeByIdResult = g.E(knowEdge.Id).ToList();
            Assert.AreEqual(1, knowEdgeByIdResult.Count);
            Assert.AreEqual("knows", knowEdgeByIdResult.First().Label);
        }

        [Test]
        public void Should_Be_Able_To_Query_With_Predicate()
        {
            var g = DseGraph.Traversal(Session);
            var edges = g.With("allow-filtering").E().HasLabel("knows").Has("weight", P.Gte(0.5f)).ToList();
            TraversalCoreIntegrationTest.VerifyEdgeResults(edges, 2, "knows");

            edges = g.With("allow-filtering").E().HasLabel("knows").Has("weight", P.Between(0.4f, 0.6f)).ToList();
            TraversalCoreIntegrationTest.VerifyEdgeResults(edges, 1, "knows");

            edges = g.With("allow-filtering").E().HasLabel("knows").Has("weight", P.Eq(0.5f)).ToList();
            TraversalCoreIntegrationTest.VerifyEdgeResults(edges, 1, "knows");

            edges = g.With("allow-filtering").E().HasLabel("knows").Has("weight", P.Eq(0.5f)).ToList();
            TraversalCoreIntegrationTest.VerifyEdgeResults(edges, 1, "knows");

            var vertices = g.With("allow-filtering").V().HasLabel("person").Has("name", P.Eq("marko")).ToList();
            TraversalCoreIntegrationTest.VerifyVertexResults(vertices, 1, "person");

            vertices = g.With("allow-filtering").V().HasLabel("person").Has("name", P.Neq("marko")).ToList();
            TraversalCoreIntegrationTest.VerifyVertexResults(vertices, 3, "person");

            vertices = g.With("allow-filtering").V().HasLabel("person").Has("name", P.Within("marko", "josh", "john")).ToList();
            TraversalCoreIntegrationTest.VerifyVertexResults(vertices, 2, "person");
        }

        private static void VerifyEdgeResults(IList<Gremlin.Net.Structure.Edge> edges, int count, string label)
        {
            Assert.AreEqual(count, edges.Count);
            foreach (var edge in edges)
            {
                Assert.AreEqual(label, edge.Label);
            }
        }

        private static void VerifyVertexResults(IList<Gremlin.Net.Structure.Vertex> vertices, int count, string label)
        {
            Assert.AreEqual(count, vertices.Count);
            foreach (var vertex in vertices)
            {
                Assert.AreEqual(label, vertex.Label);
            }
        }

        [Test]
        public void Should_Retrieve_Path_With_Labels()
        {
            var g = DseGraph.Traversal(Session);
            var result = g.V().HasLabel("person").Has("name", "marko").As("a")
                .OutE("knows").As("b")
                .InV().As("c", "d")
                .OutE("created").As("e", "f", "g")
                .InV().As("h")
                .Path().ToList();

            Assert.AreEqual(2, result.Count);
            foreach (var path in result)
            {
                var pathObjs = path.Objects;
                Assert.AreEqual(5, pathObjs.Count);
                var marko = (Gremlin.Net.Structure.Vertex)pathObjs[0];
                var knows = (Gremlin.Net.Structure.Edge)pathObjs[1];
                var josh = (Gremlin.Net.Structure.Vertex)pathObjs[2];
                var created = (Gremlin.Net.Structure.Edge)pathObjs[3];
                var software = (Gremlin.Net.Structure.Vertex)pathObjs[4];

                Assert.AreEqual(marko.Label, "person");
                Assert.AreEqual(knows.Label, "knows");
                Assert.AreEqual(josh.Label, "person");
                Assert.AreEqual(created.Label, "created");
                Assert.AreEqual(software.Label, "software");
            }
        }

        [Test]
        public void Should_Be_Able_To_Create_And_Drop_Vertex()
        {
            const string name = "john snow";
            var g = DseGraph.Traversal(Session);
            g.AddV("character").Property("name", name).Property("age", 20).Next();
            var knowsNothing = g.V().HasLabel("character").Has("name", name).ToList();
            Assert.AreEqual(1, knowsNothing.Count);
            g.With("force-vertex-deletion").V().HasLabel("character").Has("name", P.Eq(name)).Drop().ToList();
            knowsNothing = g.V().HasLabel("character").Has("name", P.Eq(name)).ToList();
            Assert.AreEqual(0, knowsNothing.Count);
        }

        [Test]
        public void Should_Be_Able_To_Create_Vertex_With_Multicardinality_Property()
        {
            var g = DseGraph.Traversal(Session);
            g.AddV("multi_v")
                .Property("pk", Guid.NewGuid())
                .Property("multi_prop", new List<string> { "Hold" , "the", "door"})
                .Next();
            StatementCoreIntegrationTest.VerifyMultiCardinalityProperty(Session, g, new []{ "Hold" , "the", "door"});
            g.V().HasLabel("multi_v").Drop().Next();
        }

        [Test]
        public void Should_Be_Able_To_Create_Vertex_With_CollectionProperties()
        {
            Session.UserDefinedTypes.Define(UdtMap.For<MetaProp>("meta_prop_type", GraphName)
                .Map(u => u.SubProp, "sub_prop").Map(u => u.SubProp2, "sub_prop2"));
            var g = DseGraph.Traversal(Session);
            g.AddV("collection_v")
                .Property("pk", "White Walkers")
                .Property("sub_prop", new List<string> { "Dragonglass" })
                .Property("sub_prop2", new List<string> { "Valyrian steel" })
                .Next();
            StatementCoreIntegrationTest.VerifyCollectionProperties(Session, g, "White Walkers", "Dragonglass", "Valyrian steel");
            g.V().HasLabel("collection_v").Drop().Next();
        }

        [Test]
        public void Should_Be_Able_To_Create_Vertex_With_MetaProperties()
        {
            var g = DseGraph.Traversal(Session);
            var guid = Guid.NewGuid();
            var metaProp = new MetaProp { SubProp = "Dragonglass", SubProp2 = "Valyrian steel" };
            g.AddV("meta_v")
                .Property("pk", guid)
                .Property("meta_prop", metaProp)
                .Next();

            StatementCoreIntegrationTest.VerifyMetaProperties(Session, g, guid, metaProp);
            g.V().HasLabel("meta_v").Drop().Next();
        }
        
        [Test]
        public void Should_Be_Able_To_Create_Vertex_With_TupleProperties()
        {
            var g = DseGraph.Traversal(Session);
            var tuple = new Tuple<string, int>("test 123", 123);
            var guid = Guid.NewGuid();
            g.AddV("tuple_v")
                .Property("pk", guid)
                .Property("tuple_prop", tuple)
                .Next();

            StatementCoreIntegrationTest.VerifyProperty(Session, g, "tuple_v", guid, "tuple_prop", tuple);
            g.V().HasLabel("tuple_v").Drop().Next();
        }

        [Test]
        public void Should_Handle_Result_With_Mixed_Object()
        {
            var g = DseGraph.Traversal(Session);
            var result = g.With("allow-filtering").V().HasLabel("software")
                .As("a", "b", "c")
                .Select<object>("a", "b", "c")
                .By("name")
                .By("lang")
                .By(__.In("created").Values<string>("name").Fold()).ToList();

            Assert.AreEqual(2, result.Count);
            var lop = result.FirstOrDefault(software => software["a"].Equals("lop"));
            var ripple = result.FirstOrDefault(software => software["a"].Equals("ripple"));
            Assert.IsNotNull(lop);
            Assert.IsNotNull(ripple);
            Assert.AreEqual("java", lop["b"]);
            Assert.AreEqual("java", ripple["b"]);
            var lopDevs = (lop["c"] as IEnumerable).Cast<string>();
            var rippleDevs = (ripple["c"] as IEnumerable).Cast<string>();
            Assert.AreEqual(3, lopDevs.Count());
            Assert.AreEqual(1, rippleDevs.Count());
            Assert.True(lopDevs.Contains("marko"));
            Assert.True(lopDevs.Contains("josh"));
            Assert.True(lopDevs.Contains("peter"));
        }

        [Test]
        public void Should_Execute_Trasversal_With_Enums()
        {
            var g = DseGraph.Traversal(Session);
            var enums = g.With("allow-filtering").V().HasLabel("person").Has("age").Order().By("age", Order.Decr).ToList();
            Assert.AreEqual(4, enums.Count);
        }

        [Test]
        public void Should_Make_OLAP_Query_Using_Traversal()
        {
            var g = DseGraph.Traversal(Session, new GraphOptions().SetSourceAnalytics());
            var result = g.V().HasLabel("person")
                .PeerPressure().By("cluster").ToList();
            Assert.AreEqual(4, result.Count);
        }

        [Test]
        public void Should_Override_Cluster_Options()
        {
            var g = DseGraph.Traversal(Session, new GraphOptions().SetName("non_existing_graph"));
            // Its supposed to be InvalidQueryException but DSE 6.0 introduced a bug: DSP-15783
            if (TestClusterManager.DseVersion < new Version(6, 0))
            {
                Assert.Throws<InvalidQueryException>(() => g.V().HasLabel("person").ToList());
            }
        }
    }
}
