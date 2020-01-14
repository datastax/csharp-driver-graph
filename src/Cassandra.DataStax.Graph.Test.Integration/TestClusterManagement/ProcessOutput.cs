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
using System.Text;

namespace Cassandra.DataStax.Graph.Test.Integration.TestClusterManagement
{
    /// <summary>
    /// Represents a result from executing an external process.
    /// </summary>
    public class ProcessOutput
    {
        public int ExitCode { get; set; }

        public StringBuilder OutputText { get; set; }

        public ProcessOutput()
        {
            OutputText = new StringBuilder();
            ExitCode = Int32.MinValue;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                "Exit Code: " + ExitCode + Environment.NewLine +
                "Output Text: " + OutputText + Environment.NewLine;
        }
    }
}
