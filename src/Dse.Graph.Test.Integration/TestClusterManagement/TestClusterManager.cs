using System;
using System.Collections.Generic;
using System.Text;

namespace Dse.Graph.Test.Integration.TestClusterManagement
{
    internal static class TestClusterManager
    {
        /// <summary>
        /// Gets the IP prefix for the DSE instances
        /// </summary>
        public static string IpPrefix => Environment.GetEnvironmentVariable("DSE_INITIAL_IPPREFIX") ?? "127.0.0.";

        /// <summary>
        /// Gets the path to DSE source code
        /// </summary>
        public static string DsePath => Environment.GetEnvironmentVariable("DSE_PATH");

        public static string InitialContactPoint => IpPrefix + "1";

        public static void TryRemove()
        {
            //TODO
        }
    }
}
