using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DotNet.InternalAbstractions;

namespace Dse.Graph.Test.Integration
{
    public static class TestHelper
    {
        public static bool IsWin => RuntimeEnvironment.OperatingSystemPlatform == Platform.Windows;

        public static string GetTestClusterNameBasedOnTime()
        {
            return "test_" + (DateTimeOffset.UtcNow.Ticks / TimeSpan.TicksPerSecond);
        }
    }
}
