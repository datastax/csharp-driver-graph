//
//  Copyright DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System.Threading.Tasks;
using NUnit.Framework;
using Moq;

namespace Dse.Graph.Test.Unit
{
    [TestFixture]
    public class TraversalUnitTest
    {
        private delegate void WithMockDelegate(IDseSession session, ref IGraphStatement statement);
        
        [Test]
        public void Should_Set_The_Graph_Language()
        {
            WithMock((IDseSession session, ref IGraphStatement statement) =>
            {
                var g = DseGraph.Traversal(session);
                g.V().ToList();
                Assert.NotNull(statement);
                Assert.AreEqual("bytecode-json", statement.GraphLanguage); 
            });
        }

        [Test]
        public void Should_Use_The_Graph_Options()
        {
            WithMock((IDseSession session, ref IGraphStatement statement) =>
            {
                var options = new GraphOptions()
                    .SetName("my_graph")
                    .SetSource("my_source")
                    .SetReadTimeoutMillis(100)
                    .SetReadConsistencyLevel(ConsistencyLevel.EachQuorum)
                    .SetWriteConsistencyLevel(ConsistencyLevel.LocalQuorum);
                var g = DseGraph.Traversal(session, options);
                g.V().ToList();
                Assert.NotNull(statement);
                Assert.AreEqual("bytecode-json", statement.GraphLanguage);
                Assert.AreEqual(options.Name, statement.GraphName);
                Assert.AreEqual(options.Source, statement.GraphSource);
                Assert.AreEqual(options.ReadTimeoutMillis, statement.ReadTimeoutMillis);
                Assert.AreEqual(options.ReadConsistencyLevel, statement.GraphReadConsistencyLevel);
                Assert.AreEqual(options.WriteConsistencyLevel, statement.GraphWriteConsistencyLevel);
            });
        }

        [Test]
        public void Extension_Method_Should_Build_Traversal_And_Execute()
        {
            WithMock((IDseSession session, ref IGraphStatement statement) =>
            {
                var g = DseGraph.Traversal(session);
                session.ExecuteGraph(g.V());
                Assert.NotNull(statement);
                Assert.IsInstanceOf<SimpleGraphStatement>(statement);
                var s = (SimpleGraphStatement) statement;
                StringAssert.Contains("g:Bytecode", s.Query);
            });
        }

        [Test]
        public void Extension_Async_Method_Should_Build_Traversal_And_Execute()
        {
            WithMock((IDseSession session, ref IGraphStatement statement) =>
            {
                var g = DseGraph.Traversal(session);
                session.ExecuteGraphAsync(g.V());
                Assert.NotNull(statement);
                Assert.IsInstanceOf<SimpleGraphStatement>(statement);
                var s = (SimpleGraphStatement) statement;
                StringAssert.Contains("g:Bytecode", s.Query);
            });
        }

        private static void WithMock(WithMockDelegate handler)
        {
            IGraphStatement statement = null;
            var sessionMock = new Mock<IDseSession>();
            sessionMock.Setup(s => s.ExecuteGraphAsync(It.IsAny<IGraphStatement>()))
                .ReturnsAsync(new GraphResultSet(new RowSet()))
                .Callback<IGraphStatement>(stmt =>
                {
                    statement = stmt;
                });
            sessionMock.Setup(s => s.ExecuteGraph(It.IsAny<IGraphStatement>()))
                .Returns(new GraphResultSet(new RowSet()))
                .Callback<IGraphStatement>(stmt =>
                {
                    statement = stmt;
                });
            handler(sessionMock.Object, ref statement);
        }
    }
}
