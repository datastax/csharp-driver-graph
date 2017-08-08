﻿//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections;
using System.Linq;
using Dse.Geometry;
using Dse.Graph.Test.Integration.TestClusterManagement;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure;
using NUnit.Framework;

namespace Dse.Graph.Test.Integration
{
    public class TraversalIntegrationTest : BaseIntegrationTest
    {
        private static readonly Tuple<string, object>[] PropertyItems =
        {
            Tuple.Create<string, object>("Float", 3.1415f),
            Tuple.Create<string, object>("Point", new Point(0, 1))
        };
        
        [OneTimeSetUp]
        public void FixtureSetup()
        {
            foreach (var type in PropertyItems.Select(i => i.Item1).Distinct())
            {
                var typeDef = $"{type}()";
                if (TestClusterManager.DseVersion >= new Version(5, 1))
                switch (type)
                {
                    case "Point":
                        typeDef = $"{type}().withBounds(-40, -40, 40, 40)";
                        break;
                    case "Linestring":
                    case "Polygon":
                        typeDef = $"{type}().withGeoBounds()";
                        break;
                }
                
                var schemaQuery = $"schema.propertyKey(propertyName).{typeDef}.ifNotExists().create();\n" +
                                  "schema.vertexLabel(vertexLabel).properties(propertyName).ifNotExists().create();";
                var statement = new SimpleGraphStatement(schemaQuery,
                    new {vertexLabel = $"vertex{type}", propertyName = $"prop{type}"});
                Session.ExecuteGraph(statement);
            }
        }
        
        [Test]
        public void Should_Handle_Zero_Results()
        {
            var g = DseGraph.Traversal(Session);
            CollectionAssert.IsEmpty(g.V().HasLabel("not_existent_label").ToList());
        }

        [Test]
        public void Should_Parse_Vertices()
        {
            var g = DseGraph.Traversal(Session);
            var vertices = g.V().HasLabel("person").ToList();
            Assert.Greater(vertices.Count, 0);
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
            Assert.Greater(names.Count, 0);
            CollectionAssert.Contains(names, "peter");
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
        [TestCase("Point")]
        public void Should_Handle_Types(string type)
        {
            var g = DseGraph.Traversal(Session);
            foreach (var value in PropertyItems.Where(i => i.Item1 == type).Select(i => i.Item2))
            {
                var vertexLabel = $"vertex{type}";
                var propertyName = $"prop{type}";
                g.AddV(vertexLabel).Property(propertyName, value).Next();
                var result = g.V().HasLabel(vertexLabel).Has(propertyName, value).Next();
//                var parameters = new { vertexLabel = vertexLabel, propertyName = propertyName, val = value };
//                session.ExecuteGraph(new SimpleGraphStatement("g.addV(label, vertexLabel, propertyName, val)", parameters));
//
//                var rs =
//                    session.ExecuteGraph(
//                        new SimpleGraphStatement("g.V().hasLabel(vertexLabel).has(propertyName, val).next()", parameters));
            }
        }
    }
}
