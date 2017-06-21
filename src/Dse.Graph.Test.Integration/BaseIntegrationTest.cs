using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Dse.Graph.Test.Integration
{
    [TestFixture]
    public abstract class BaseIntegrationTest
    {
        protected const string ContactPoint = "172.16.56.1";
        public const string DefaultGraphName = "graph1";

        protected IDseCluster Cluster { get; set; }

        protected IDseSession Session { get; set; }

        [OneTimeSetUp]
        public void BaseOneTimeSetUp()
        {
            Cluster = DseCluster.Builder()
                .AddContactPoint(ContactPoint)
                .WithGraphOptions(new GraphOptions().SetName(DefaultGraphName))
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
