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
using System.Threading.Tasks;
using Cassandra.DataStax.Graph.Test.Integration.TestClusterManagement;
using NUnit.Framework;

namespace Cassandra.DataStax.Graph.Test.Integration
{
    [SetUpFixture]
    public class CommonFixtureSetup
    {
        private const string ClassicGraphName = BaseIntegrationTest.DefaultClassicGraphName;
        private const string CoreGraphName = BaseIntegrationTest.DefaultCoreGraphName;

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
        
        protected const string CoreSchemaGremlinQuery =
            "schema.type('meta_prop_type').property('sub_prop', Text).property('sub_prop2', Text).create()\n" +
            "schema.vertexLabel('meta_v').partitionBy('pk', UUID).property('meta_prop', typeOf('meta_prop_type')).create()\n" +
            "schema.vertexLabel('multi_v').partitionBy('pk', UUID).property('multi_prop', listOf(Text)).create()\n" +
            "schema.vertexLabel('collection_v').partitionBy('pk', Text).property('sub_prop', listOf(Text)).property('sub_prop2', listOf(Text)).create()\n" +
            "schema.vertexLabel('tuple_v').partitionBy('pk', UUID).property('tuple_prop', tupleOf(Text, Int)).create()\n" +
            "schema.vertexLabel('person').partitionBy('name', Text).property('age', Int).create();\n" +
            "schema.vertexLabel('software').partitionBy('name', Text).property('lang', Text).create();\n" +
            "schema.vertexLabel('character').partitionBy('name', Text).property('age', Int).property('tag', Text).create()\n" +
            "schema.edgeLabel('created').from('person').to('software').property('weight', Float).create();\n" +
            "schema.edgeLabel('knows').from('person').to('person').property('weight', Float).create();\n" +
            "schema.edgeLabel('knows').from('character').to('character').property('weight', Float).create();\n";
        
        protected const string CoreLoadGremlinQuery =
            "g.addV('person').property('name', 'marko').property('age', 29).as('marko')" +
            ".addV('person').property('name', 'vadas').property('age', 27).as('vadas')" +
            ".addV('software').property('name', 'lop').property('lang', 'java').as('lop')" +
            ".addV('person').property('name', 'josh').property('age', 32).as('josh')" +
            ".addV('software').property('name', 'ripple').property('lang', 'java').as('ripple')" +
            ".addV('person').property('name', 'peter').property('age', 35).as('peter')" +
            ".addE('knows').property('weight', 0.5f).from('marko').to('vadas')" +
            ".addE('knows').property('weight', 1.0f).from('marko').to('josh')" +
            ".addE('created').property('weight', 0.4f).from('marko').to('lop')" +
            ".addE('created').property('weight', 1.0f).from('josh').to('ripple')" +
            ".addE('created').property('weight', 0.4f).from('josh').to('lop')" +
            ".addE('created').property('weight', 0.2f).from('peter').to('lop')";

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
                CreateDefaultGraph(session, CommonFixtureSetup.ClassicGraphName);
                if (TestClusterManager.DseVersion >= new Version(6, 8))
                {
                    CreateCoreGraph(session, CommonFixtureSetup.CoreGraphName);
                }
            }

            TestContext.Progress.WriteLine("Graph created");
        }

        private void CreateDefaultGraph(ISession session, string name)
        {
            try
            {
                session.ExecuteGraph(new SimpleGraphStatement($"system.graph('{name}').drop()"));
            }
            catch
            {
                // ignored
            }
            session.ExecuteGraph(new SimpleGraphStatement($"system.graph('{name}')" +
                                                          (TestClusterManager.DseVersion < new Version(6, 8) ? string.Empty : ".engine(Classic)") +
                                                          ".create()"));
            session.ExecuteGraph(new SimpleGraphStatement(CommonFixtureSetup.MakeStrict).SetGraphName(name));
            session.ExecuteGraph(new SimpleGraphStatement(CommonFixtureSetup.AllowScans).SetGraphName(name));
            session.ExecuteGraph(new SimpleGraphStatement(CommonFixtureSetup.ClassicSchemaGremlinQuery).SetGraphName(name));
            session.ExecuteGraph(new SimpleGraphStatement(CommonFixtureSetup.ClassicLoadGremlinQuery).SetGraphName(name));
        }
        
        /// <summary>
        /// Creates a core graph using the current session
        /// </summary>
        private void CreateCoreGraph(ISession session, string name)
        {
            session.ExecuteGraph(new SimpleGraphStatement($"system.graph('{name}').ifExists().drop()").SetSystemQuery());
            WaitUntilKeyspaceMetadataRefresh(session, name, false);
            session.ExecuteGraph(new SimpleGraphStatement($"system.graph('{name}').ifNotExists().create()").SetSystemQuery());
            WaitUntilKeyspaceMetadataRefresh(session, name, true);
            session.ExecuteGraph(new SimpleGraphStatement(CoreSchemaGremlinQuery).SetGraphName(name));
            session.ExecuteGraph(new SimpleGraphStatement(CoreLoadGremlinQuery).SetGraphName(name));
        }
        
        private void WaitUntilKeyspaceMetadataRefresh(ISession session, string graphName, bool exists)
        {
            var now = DateTime.UtcNow;
            while ((DateTime.UtcNow - now).TotalMilliseconds <= 30000)
            {
                var keyspaceExists = session.Cluster.Metadata.RefreshSchema(graphName);
                if (keyspaceExists == exists)
                {
                    return;
                }

                if (exists)
                {
                    var ks = session.Cluster.Metadata.GetKeyspace(graphName);
                    if (ks?.GraphEngine == "Core")
                    {
                        return;
                    }
                }

                Task.Delay(500).GetAwaiter().GetResult();
            }
            
            Assert.Fail("Keyspace metadata does not have the correct graph engine.");
        }

        [OneTimeTearDown]
        public void TearDownTestSuite()
        {
            // this method is executed once after all the fixtures have completed execution
            TestClusterManager.TryRemove();
        }
    }
}
