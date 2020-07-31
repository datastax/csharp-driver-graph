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
using Cassandra.DataStax.Graph.Test.Integration.TestClusterManagement;
using NUnit.Framework;

namespace Cassandra.DataStax.Graph.Test.Integration
{
    [SetUpFixture]
    public class CommonFixtureSetup
    {
        private const string GraphName = BaseIntegrationTest.DefaultGraphName;

        protected const string ClassicSchemaGremlinQuery =
            "schema.propertyKey('name').Text().ifNotExists().create();\n" +
            "schema.propertyKey('tag').Text().ifNotExists().create();\n" +
            "schema.propertyKey('age').Int().ifNotExists().create();\n" +
            "schema.propertyKey('lang').Text().ifNotExists().create();\n" +
            "schema.propertyKey('weight').Float().ifNotExists().create();\n" +
            "schema.propertyKey('multi_prop').Text().multiple().ifNotExists().create()\n" +
            "schema.propertyKey('sub_prop').Text().ifNotExists().create()\n" +
            "schema.propertyKey('sub_prop2').Text().ifNotExists().create()\n" +
            "schema.propertyKey('meta_prop').Text().properties('sub_prop', 'sub_prop2').ifNotExists().create()\n" +
            "schema.vertexLabel('multi_v').properties('multi_prop').ifNotExists().create()\n" +
            "schema.vertexLabel('meta_v').properties('meta_prop').ifNotExists().create()\n" +
            "schema.vertexLabel('person').properties('name', 'age').ifNotExists().create();\n" +
            "schema.vertexLabel('software').properties('name', 'lang').ifNotExists().create();\n" +
            "schema.vertexLabel('character').properties('name', 'age', 'tag').ifNotExists().create()\n" +
            "schema.edgeLabel('created').properties('weight').connection('person', 'software').ifNotExists().create();\n" +
            "schema.edgeLabel('knows').properties('weight').connection('person', 'person').ifNotExists().create();\n" +
            "schema.edgeLabel('knows').properties('weight').connection('character', 'character').ifNotExists().create();\n";
        
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

        protected const string MakeStrict = "schema.config().option('graph.schema_mode').set('production');";
        protected const string AllowScans = "schema.config().option('graph.allow_scan').set('true');";

        [OneTimeSetUp]
        public void SetupTestSuite()
        {
            //this method is executed once BEFORE all the fixtures are started
            TestContext.Progress.WriteLine("Will start dse cluster");
            TestClusterManager.CreateNew(1, new TestClusterOptions
            {
                Workloads = new[] { "graph", "spark" }
            });
            TestContext.Progress.WriteLine("DSE started");
            using (var cluster = Cluster.Builder().AddContactPoint(TestClusterManager.InitialContactPoint).Build())
            {
                var session = cluster.Connect();
                CreateDefaultGraph(session);
            }

            TestContext.Progress.WriteLine("Graph created");
        }

        private void CreateDefaultGraph(ISession session)
        {
            try
            {
                session.ExecuteGraph(new SimpleGraphStatement($"system.graph('{CommonFixtureSetup.GraphName}').drop()"));
            }
            catch
            {
                // ignored
            }
            session.ExecuteGraph(new SimpleGraphStatement($"system.graph('{CommonFixtureSetup.GraphName}')" +
                                                          (TestClusterManager.DseVersion < new Version(6, 8) ? string.Empty : ".engine(Classic)") +
                                                          ".create()"));
            session.ExecuteGraph(new SimpleGraphStatement(CommonFixtureSetup.MakeStrict).SetGraphName(CommonFixtureSetup.GraphName));
            session.ExecuteGraph(new SimpleGraphStatement(CommonFixtureSetup.AllowScans).SetGraphName(CommonFixtureSetup.GraphName));
            session.ExecuteGraph(new SimpleGraphStatement(CommonFixtureSetup.ClassicSchemaGremlinQuery).SetGraphName(CommonFixtureSetup.GraphName));
            session.ExecuteGraph(new SimpleGraphStatement(CommonFixtureSetup.ClassicLoadGremlinQuery).SetGraphName(CommonFixtureSetup.GraphName));
        }

        [OneTimeTearDown]
        public void TearDownTestSuite()
        {
            // this method is executed once after all the fixtures have completed execution
            TestClusterManager.TryRemove();
        }
    }
}
