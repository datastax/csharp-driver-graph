//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using Dse.Graph.Test.Integration.TestClusterManagement;
using NUnit.Framework;

namespace Dse.Graph.Test.Integration
{
    [SetUpFixture]
    public class CommonFixtureSetup
    {
        private const string GraphName = BaseIntegrationTest.DefaultGraphName;

        protected const string ClassicSchemaGremlinQuery =
            "schema.propertyKey('name').Text().ifNotExists().create();\n" +
            "schema.propertyKey('age').Int().ifNotExists().create();\n" +
            "schema.propertyKey('lang').Text().ifNotExists().create();\n" +
            "schema.propertyKey('weight').Float().ifNotExists().create();\n" +
            "schema.vertexLabel('person').properties('name', 'age').ifNotExists().create();\n" +
            "schema.vertexLabel('software').properties('name', 'lang').ifNotExists().create();\n" +
            "schema.edgeLabel('created').properties('weight').connection('person', 'software').ifNotExists().create();\n" +
            "schema.edgeLabel('knows').properties('weight').connection('person', 'person').ifNotExists().create();\n";
        
        protected const string ClassicLoadGremlinQuery =
            "Vertex marko = graph.addVertex(label, 'person', 'name', 'marko', 'age', 29);\n" +
            "Vertex vadas = graph.addVertex(label, 'person', 'name', 'vadas', 'age', 27);\n" +
            "Vertex lop = graph.addVertex(label, 'software', 'name', 'lop', 'lang', 'java');\n" +
            "Vertex josh = graph.addVertex(label, 'person', 'name', 'josh', 'age', 32);\n" +
            "Vertex ripple = graph.addVertex(label, 'software', 'name', 'ripple', 'lang', 'java');\n" +
            "Vertex peter = graph.addVertex(label, 'person', 'name', 'peter', 'age', 35);\n" +
            "marko.addEdge('knows', vadas, 'weight', 0.5f);\n" +
            "marko.addEdge('knows', josh, 'weight', 1.0f);\n" +
            "marko.addEdge('created', lop, 'weight', 0.4f);\n" +
            "josh.addEdge('created', ripple, 'weight', 1.0f);\n" +
            "josh.addEdge('created', lop, 'weight', 0.4f);\n" +
            "peter.addEdge('created', lop, 'weight', 0.2f);";

        protected const string MakeStrict = "schema.config().option(\"graph.schema_mode\").set(\"production\");";
        protected const string AllowScans = "schema.config().option(\"graph.allow_scan\").set(\"true\");";

        [OneTimeSetUp]
        public void SetupTestSuite()
        {
            // this method is executed once BEFORE all the fixtures are started
            TestClusterManager.CreateNew(1, new TestClusterOptions
            {
                Workloads = new [] { "graph" }
            });
            using (var cluster = DseCluster.Builder().AddContactPoint(TestClusterManager.InitialContactPoint).Build())
            {
                var session = cluster.Connect();
                CreateDefaultGraph(session);
            }

        }

        private void CreateDefaultGraph(IDseSession session)
        {
            session.ExecuteGraph(new SimpleGraphStatement($"system.graph('{GraphName}').ifNotExists().create()"));
            session.ExecuteGraph(new SimpleGraphStatement(MakeStrict).SetGraphName(GraphName));
            session.ExecuteGraph(new SimpleGraphStatement(AllowScans).SetGraphName(GraphName));
            session.ExecuteGraph(new SimpleGraphStatement(ClassicSchemaGremlinQuery).SetGraphName(GraphName));
            session.ExecuteGraph(new SimpleGraphStatement(ClassicLoadGremlinQuery).SetGraphName(GraphName));
        }

        [OneTimeTearDown]
        public void TearDownTestSuite()
        {
            // this method is executed once after all the fixtures have completed execution
            TestClusterManager.TryRemove();
        }
    }
}
