using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Gremlin.Net.Structure;
using NUnit.Framework;

namespace Dse.Graph.Test.Integration
{
    [TestFixture]
    public class TraversalIntegrationTest : BaseIntegrationTest
    {
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
    }
}
