//
//  Copyright (C) 2016 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

namespace Dse.Graph.Test.Integration.TestClusterManagement
{
    public class LocalCcmProcessExecuter : CcmProcessExecuter
    {
        public const string CcmCommandPath = "/usr/local/bin/ccm";
        public static readonly LocalCcmProcessExecuter Instance = new LocalCcmProcessExecuter();

        private LocalCcmProcessExecuter()
        {
            
        }

        protected override string GetExecutable(ref string args)
        {
            var executable = CcmCommandPath;

            if (!TestHelper.IsWin)
            {
                return executable;
            }
            executable = "cmd.exe";
            args = "/c ccm " + args;
            return executable;
        }
    }
}
