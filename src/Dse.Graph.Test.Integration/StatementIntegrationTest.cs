//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System.Threading.Tasks;
using NUnit.Framework;

namespace Dse.Graph.Test.Integration
{
    public class StatementIntegrationTest : BaseIntegrationTest
    {
        [Test]
        public async Task Should_Create_A_Query_From_A_Traversal()
        {
            var g = DseGraph.Traversal(Session);
            var statement = DseGraph.StatementFromTraversal(g.V().HasLabel("person"));
            var rs = await Session.ExecuteGraphAsync(statement);
            // The result should be DSE driver vertices
            foreach (var node in rs)
            {
                var v = node.ToVertex();
                Assert.AreEqual("person", v.Label);
            }
        }
    }
}