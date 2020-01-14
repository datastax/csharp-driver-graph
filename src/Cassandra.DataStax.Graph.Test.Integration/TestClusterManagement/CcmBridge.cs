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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Cassandra.DataStax.Graph.Test.Integration.TestClusterManagement
{
    public class CcmBridge : IDisposable
    {
        public DirectoryInfo CcmDir { get; private set; }
        public const int DefaultCmdTimeout = 90 * 1000;
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string IpPrefix { get; private set; }
        public ICcmProcessExecuter CcmProcessExecuter { get; set; }
        private readonly string _dseInstallPath;

        public CcmBridge(string name, string ipPrefix, string dsePath, string version, ICcmProcessExecuter executor)
        {
            Name = name;
            IpPrefix = ipPrefix;
            CcmDir = Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName()));
            CcmProcessExecuter = executor;
            _dseInstallPath = dsePath;
            Version = version;
        }

        public void Dispose()
        {
        }

        public void Create(bool useSsl)
        {
            if (string.IsNullOrEmpty(_dseInstallPath))
            {
                ExecuteCcm(string.Format(
                    "create {0} --dse -v {1}", Name, Version));
            }
            else
            {
                ExecuteCcm(string.Format(
                    "create {0} --install-dir={1}", Name, _dseInstallPath));
            }
        }

        protected string GetHomePath()
        {
            var home = Environment.GetEnvironmentVariable("USERPROFILE");
            if (!string.IsNullOrEmpty(home))
            {
                return home;
            }
            home = Environment.GetEnvironmentVariable("HOME");
            if (string.IsNullOrEmpty(home))
            {
                throw new NotSupportedException("HOME or USERPROFILE are not defined");
            }
            return home;
        }

        public void Start(string[] jvmArgs)
        {
            var parameters = new List<string>
            {
                "start",
                "--wait-for-binary-proto"
            };
            if (TestHelper.IsWin && CcmProcessExecuter is LocalCcmProcessExecuter)
            {
                parameters.Add("--quiet-windows");
            }
            if (jvmArgs != null)
            {
                foreach (var arg in jvmArgs)
                {
                    parameters.Add("--jvm_arg");
                    parameters.Add(arg);
                }
            }
            ExecuteCcm(string.Join(" ", parameters));
        }

        public void Populate(int dc1NodeLength, int dc2NodeLength, bool useVNodes)
        {
            var parameters = new List<string>
            {
                "populate",
                "-n",
                dc1NodeLength + (dc2NodeLength > 0 ? ":" + dc2NodeLength : null),
                "-i",
                IpPrefix
            };
            if (useVNodes)
            {
                parameters.Add("--vnodes");
            }
            ExecuteCcm(string.Join(" ", parameters));
        }

        public void SwitchToThis()
        {
            string switchCmd = "switch " + Name;
            ExecuteCcm(switchCmd, CcmBridge.DefaultCmdTimeout, false);
        }

        public void List()
        {
            ExecuteCcm("list");
        }

        public void Stop()
        {
            ExecuteCcm("stop");
        }

        public void StopForce()
        {
            ExecuteCcm("stop --not-gently");
        }

        public void Start(int n, string additionalArgs = null)
        {
            string quietWindows = null;
            if (TestHelper.IsWin && CcmProcessExecuter is LocalCcmProcessExecuter)
            {
                quietWindows = "--quiet-windows";
            }
            ExecuteCcm(string.Format("node{0} start --wait-for-binary-proto {1} {2}", n, additionalArgs, quietWindows));
        }

        public void Stop(int n)
        {
            ExecuteCcm(string.Format("node{0} stop", n));
        }

        public void StopForce(int n)
        {
            ExecuteCcm(string.Format("node{0} stop --not-gently", n));
        }

        public void Remove()
        {
            ExecuteCcm("remove");
        }

        public void Remove(int nodeId)
        {
            ExecuteCcm(string.Format("node{0} remove", nodeId));
        }

        public void BootstrapNode(int n, bool start = true)
        {
            BootstrapNode(n, null, start);
        }

        public void BootstrapNode(int n, string dc, bool start = true)
        {
            ExecuteCcm(string.Format("add node{0} -i {1}{2} -j {3} -b -s {4} --dse", n, IpPrefix, n, 7000 + 100 * n, dc != null ? "-d " + dc : null));
            if (start)
            {
                Start(n);
            }
        }

        public void DecommissionNode(int n)
        {
            ExecuteCcm(string.Format("node{0} decommission", n));
        }

        public ProcessOutput ExecuteCcm(string args, int timeout = CcmBridge.DefaultCmdTimeout, bool throwOnProcessError = true)
        {
            return CcmProcessExecuter.ExecuteCcm(args, timeout, throwOnProcessError);
        }

        public void UpdateConfig(params string[] configs)
        {
            if (configs == null)
            {
                return;
            }
            foreach (var c in configs)
            {
                ExecuteCcm(string.Format("updateconf \"{0}\"", c));
            }
        }

        public void UpdateDseConfig(params string[] configs)
        {
            if (configs == null)
            {
                return;
            }
            foreach (var c in configs)
            {
                ExecuteCcm(string.Format("updatedseconf \"{0}\"", c));
            }
        }

        public void SetNodeWorkloads(int nodeId, string[] workloads)
        {
            ExecuteCcm(string.Format("node{0} setworkload {1}", nodeId, string.Join(",", workloads)));
        }

        /// <summary>
        /// Sets the workloads for all nodes.
        /// </summary>
        public void SetWorkloads(int nodeLength, string[] workloads)
        {
            if (workloads == null || workloads.Length == 0)
            {
                return;
            }
            for (var nodeId = 1; nodeId <= nodeLength; nodeId++)
            {
                SetNodeWorkloads(nodeId, workloads);
            }
        }

        /// <summary>
        /// Spawns a new process (platform independent)
        /// </summary>
        public static ProcessOutput ExecuteProcess(string processName, string args, int timeout = CcmBridge.DefaultCmdTimeout)
        {
            var output = new ProcessOutput();
            using (var process = new Process())
            {
                process.StartInfo.FileName = processName;
                process.StartInfo.Arguments = args;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                using (var outputWaitHandle = new AutoResetEvent(false))
                using (var errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            try
                            {
                                outputWaitHandle.Set();
                            }
                            catch
                            {
                                //probably is already disposed
                            }
                        }
                        else
                        {
                            output.OutputText.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            try
                            {
                                errorWaitHandle.Set();
                            }
                            catch
                            {
                                //probably is already disposed
                            }
                        }
                        else
                        {
                            output.OutputText.AppendLine(e.Data);
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(timeout) &&
                        outputWaitHandle.WaitOne(timeout) &&
                        errorWaitHandle.WaitOne(timeout))
                    {
                        // Process completed.
                        output.ExitCode = process.ExitCode;
                    }
                    else
                    {
                        // Timed out.
                        output.ExitCode = -1;
                    }
                }
            }
            return output;
        }
    }
}
