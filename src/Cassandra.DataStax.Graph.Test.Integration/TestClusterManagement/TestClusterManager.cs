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
using System.Diagnostics;

namespace Cassandra.DataStax.Graph.Test.Integration.TestClusterManagement
{
    /// <summary>
    /// Test Helper class for keeping track of multiple CCM (Cassandra Cluster Manager) instances
    /// </summary>
    public class TestClusterManager
    {
        public static ITestCluster LastInstance { get; private set; }
        public const string DefaultKeyspaceName = "test_cluster_keyspace";
        private static ICcmProcessExecuter _executor;

        private static readonly Version Version2Dot0 = new Version(2, 0);
        private static readonly Version Version2Dot1 = new Version(2, 1);
        private static readonly Version Version2Dot2 = new Version(2, 2);
        private static readonly Version Version3Dot0 = new Version(3, 0);
        private static readonly Version Version3Dot10 = new Version(3, 10);
        private static readonly Version Version4Dot6 = new Version(4, 6);
        private static readonly Version Version4Dot7 = new Version(4, 7);
        private static readonly Version Version4Dot8 = new Version(4, 8);
        private static readonly Version Version5Dot0 = new Version(5, 0);
        private static readonly Version Version5Dot1 = new Version(5, 1);

        /// <summary>
        /// Gets the Cassandra version used for this test run
        /// </summary>
        public static Version CassandraVersion
        {
            get
            {
                var dseVersion = TestClusterManager.DseVersion;
                if (dseVersion < TestClusterManager.Version4Dot7)
                {
                    // C* 2.0
                    return TestClusterManager.Version2Dot0;
                }
                if (dseVersion < TestClusterManager.Version5Dot0)
                {
                    // C* 2.1
                    return TestClusterManager.Version2Dot1;
                }
                if (dseVersion < TestClusterManager.Version5Dot1)
                {
                    // C* 3.0
                    return TestClusterManager.Version3Dot0;
                }
                // C* 3.10
                return TestClusterManager.Version3Dot10;
            }
        }

        /// <summary>
        /// Gets the IP prefix for the DSE instances
        /// </summary>
        public static string IpPrefix
        {
            get { return Environment.GetEnvironmentVariable("DSE_INITIAL_IPPREFIX") ?? "127.0.0."; }
        }

        /// <summary>
        /// Gets the path to DSE source code
        /// </summary>
        public static string DsePath
        {
            get { return Environment.GetEnvironmentVariable("DSE_PATH"); }
        }

        public static string InitialContactPoint
        {
            get { return TestClusterManager.IpPrefix + "1"; }
        }

        public static string DseVersionString
        {
            get { return Environment.GetEnvironmentVariable("DSE_VERSION") ?? "5.0.0"; }
        }

        public static Version DseVersion
        {
            get { return new Version(TestClusterManager.DseVersionString); }
        }

        /// <summary>
        /// Get the ccm executor instance (local or remote)
        /// </summary>
        public static ICcmProcessExecuter Executor
        {
            get
            {
                if (TestClusterManager._executor != null)
                {
                    return TestClusterManager._executor;
                }
                var dseRemote = bool.Parse(Environment.GetEnvironmentVariable("DSE_IN_REMOTE_SERVER") ?? "true");
                if (!dseRemote)
                {
                    TestClusterManager._executor = LocalCcmProcessExecuter.Instance;
                }
                else
                {
                    var remoteDseServer = Environment.GetEnvironmentVariable("DSE_SERVER_IP") ?? "127.0.0.1";
                    var remoteDseServerUser = Environment.GetEnvironmentVariable("DSE_SERVER_USER") ?? "vagrant";
                    var remoteDseServerPassword = Environment.GetEnvironmentVariable("DSE_SERVER_PWD") ?? "vagrant";
                    var remoteDseServerPort = int.Parse(Environment.GetEnvironmentVariable("DSE_SERVER_PORT") ?? "2222");
                    var remoteDseServerUserPrivateKey = Environment.GetEnvironmentVariable("DSE_SERVER_PRIVATE_KEY");
                    TestClusterManager._executor = new RemoteCcmProcessExecuter(remoteDseServer, remoteDseServerUser, remoteDseServerPassword,
                        remoteDseServerPort, remoteDseServerUserPrivateKey);
                }
                return TestClusterManager._executor;
            }
        }

        public static Version GetDseVersion(Version cassandraVersion)
        {
            if (cassandraVersion < TestClusterManager.Version2Dot1)
            {
                // C* 2.0 => DSE 4.6
                return TestClusterManager.Version4Dot6;
            }
            if (cassandraVersion < TestClusterManager.Version2Dot2)
            {
                // C* 2.1 => DSE 4.8
                return TestClusterManager.Version4Dot8;
            }
            if (cassandraVersion < TestClusterManager.Version3Dot10)
            {
                // C* 3.0 => DSE 5.0
                return TestClusterManager.Version5Dot0;
            }
            // DSE 5.1
            return TestClusterManager.Version5Dot1;
        }

        /// <summary>
        /// Creates a new test cluster
        /// </summary>
        public static ITestCluster CreateNew(int nodeLength = 1, TestClusterOptions options = null, bool startCluster = true)
        {
            TestClusterManager.TryRemove();
            options = options ?? new TestClusterOptions();
            var testCluster = new CcmCluster(
                TestHelper.GetTestClusterNameBasedOnTime(), 
                TestClusterManager.IpPrefix, 
                TestClusterManager.DsePath, 
                TestClusterManager.Executor,
                TestClusterManager.DefaultKeyspaceName,
                TestClusterManager.DseVersionString);
            testCluster.Create(nodeLength, options);
            if (startCluster)
            {
                testCluster.Start(options.JvmArgs);   
            }
            TestClusterManager.LastInstance = testCluster;
            return testCluster;
        }

        /// <summary>
        /// Deprecated, use <see cref="TestClusterManager.CreateNew"/> method instead
        /// </summary>
        public static ITestCluster GetNonShareableTestCluster(int dc1NodeCount, int dc2NodeCount, int maxTries = 1, bool startCluster = true, bool initClient = true)
        {
            return TestClusterManager.GetTestCluster(dc1NodeCount, dc2NodeCount, false, maxTries, startCluster, initClient);
        }

        /// <summary>
        /// Deprecated, use <see cref="TestClusterManager.CreateNew"/> method instead
        /// </summary>
        public static ITestCluster GetNonShareableTestCluster(int dc1NodeCount, int maxTries = 1, bool startCluster = true, bool initClient = true)
        {
            if (startCluster == false)
                initClient = false;

            return TestClusterManager.GetTestCluster(dc1NodeCount, 0, false, maxTries, startCluster, initClient);
        }

        /// <summary>
        /// Deprecated, use <see cref="TestClusterManager.CreateNew"/> method instead
        /// </summary>
        public static ITestCluster GetTestCluster(int dc1NodeCount, int dc2NodeCount, bool shareable = true, int maxTries = 1, bool startCluster = true, bool initClient = true, int currentRetryCount = 0, string[] jvmArgs = null, bool useSsl = false)
        {
            var testCluster = TestClusterManager.CreateNew(
                dc1NodeCount,
                new TestClusterOptions
                {
                    Dc2NodeLength = dc2NodeCount,
                    UseSsl = useSsl,
                    JvmArgs = jvmArgs
                },
                startCluster);
            if (initClient)
            {
                testCluster.InitClient();
            }
            return testCluster;
        }

        /// <summary>
        /// Deprecated, use <see cref="TestClusterManager.CreateNew"/> method instead
        /// </summary>
        public ITestCluster GetTestCluster(int dc1NodeCount, int maxTries = 1, bool startCluster = true, bool initClient = true)
        {
            return TestClusterManager.GetTestCluster(dc1NodeCount, 0, true, maxTries, startCluster, initClient);
        }

        /// <summary>
        /// Removes the current ccm cluster, without throwing exceptions if it fails
        /// </summary>
        public static void TryRemove()
        {
            try
            {
                TestClusterManager.Executor.ExecuteCcm("remove");
            }
            catch (Exception ex)
            {
                if (Diagnostics.CassandraTraceSwitch.Level == TraceLevel.Verbose)
                {
                    Trace.TraceError("ccm test cluster could not be removed: {0}", ex);   
                }
            }
        }
    }
}
