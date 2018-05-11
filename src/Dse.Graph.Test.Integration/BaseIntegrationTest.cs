//
//  Copyright DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using Dse.Graph.Test.Integration.TestClusterManagement;
using NUnit.Framework;

namespace Dse.Graph.Test.Integration
{
    [TestFixture]
    public abstract class BaseIntegrationTest
    {
        public const string DefaultGraphName = "graph1";

        protected IDseCluster Cluster { get; set; }

        protected IDseSession Session { get; set; }

        [OneTimeSetUp]
        public void BaseOneTimeSetUp()
        {   
            var graphOptions = new GraphOptions().SetName(DefaultGraphName);
            if (TestClusterManager.DseVersion < new Version(5, 1))
            {
                graphOptions.SetLanguage("bytecode-json");
            }
            Cluster = DseCluster.Builder()
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
