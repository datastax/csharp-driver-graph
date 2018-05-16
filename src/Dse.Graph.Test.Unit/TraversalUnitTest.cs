//
//  Copyright DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
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
                CompareGraphOptionsOnStatement(options, statement);
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

        [Test]
        public void TraversalBatch_Should_Use_GraphOptions()
        {
            WithMock((IDseSession session, ref IGraphStatement statement) =>
            {
                var g = DseGraph.Traversal(session);
                var options = new GraphOptions().SetName("name1")
                    .SetReadConsistencyLevel(ConsistencyLevel.LocalQuorum)
                    .SetWriteConsistencyLevel(ConsistencyLevel.Quorum)
                    .SetReadTimeoutMillis(123499);
                var batch = DseGraph.Batch(options);
                batch
                    .Add(g.AddV("person").Property("name", "Matias").Property("age", 12))
                    .Add(g.AddV("person").Property("name", "Olivia").Property("age", 8));
                session.ExecuteGraph(batch);
                Assert.NotNull(statement);
                Assert.IsInstanceOf<SimpleGraphStatement>(statement);
                var query = ((SimpleGraphStatement) statement).Query;
                Assert.AreEqual("bytecode-json", statement.GraphLanguage);
                CompareGraphOptionsOnStatement(options, statement);
                StringAssert.Contains(
                    "{\"@type\":\"g:Bytecode\",\"@value\":{\"step\":[[\"addV\",\"person\"],[\"property\",\"name\",\"Matias\"],[\"property\",\"age\",{\"@type\":\"g:Int32\",\"@value\":12}]]}}",
                    query);
                StringAssert.Contains(
                    "{\"@type\":\"g:Bytecode\",\"@value\":{\"step\":[[\"addV\",\"person\"],[\"property\",\"name\",\"Olivia\"],[\"property\",\"age\",{\"@type\":\"g:Int32\",\"@value\":8}]]}}",
                    query);
            });
        }

        [Test]
        public void TraversalBatch_Should_Throw_When_No_Traversal_Was_Provided()
        {
            var batch = DseGraph.Batch();
            Assert.Throws<InvalidOperationException>(() => batch.ToGraphStatement());
        }

        private static void CompareGraphOptionsOnStatement(GraphOptions options, IGraphStatement statement)
        {
            Assert.AreEqual(options.Name, statement.GraphName);
            Assert.AreEqual(options.Source, statement.GraphSource);
            Assert.AreEqual(options.ReadTimeoutMillis, statement.ReadTimeoutMillis);
            Assert.AreEqual(options.ReadConsistencyLevel, statement.GraphReadConsistencyLevel);
            Assert.AreEqual(options.WriteConsistencyLevel, statement.GraphWriteConsistencyLevel);
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
