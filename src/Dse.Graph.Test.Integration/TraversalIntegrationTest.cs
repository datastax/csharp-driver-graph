using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
