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
    [TestFixture]
    public abstract class BaseIntegrationTest
    {
        public const string DefaultGraphName = "graph1";

        protected ICluster Cluster { get; set; }

        protected ISession Session { get; set; }

        [OneTimeSetUp]
        public void BaseOneTimeSetUp()
        {   
            var graphOptions = new GraphOptions().SetName(BaseIntegrationTest.DefaultGraphName);
            if (TestClusterManager.DseVersion < new Version(5, 1))
            {
                graphOptions.SetLanguage("bytecode-json");
            }
            Cluster = Cassandra.Cluster.Builder()
                .AddContactPoint(TestClusterManager.InitialContactPoint)
                .WithGraphOptions(graphOptions)
                .Build();
            Session = Cluster.Connect();
        }

        [OneTimeTearDown]
        public void BaseOneTimeTearDown()
        {
            Cluster.Shutdown();
        }
    }
}
