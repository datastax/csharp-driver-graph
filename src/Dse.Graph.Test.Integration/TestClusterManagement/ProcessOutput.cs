using System;
using System.Collections.Generic;
using System.Text;

namespace Dse.Graph.Test.Integration.TestClusterManagement
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
