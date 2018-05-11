//
//  Copyright DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
#if NETCORE
using Microsoft.DotNet.InternalAbstractions;
#endif

namespace Dse.Graph.Test.Integration
{
    public static class TestHelper
    {
        public static bool IsWin
        {
            get
            {
#if !NETCORE
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                        return true;
                }
                return false;
#else
                return RuntimeEnvironment.OperatingSystemPlatform == Platform.Windows;
#endif
            }
        }

        public static string GetTestClusterNameBasedOnTime()
        {
            return "test_" + (DateTimeOffset.UtcNow.Ticks / TimeSpan.TicksPerSecond);
        }
    }
}
