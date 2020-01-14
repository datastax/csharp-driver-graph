﻿//
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

#if NETCORE
using Microsoft.DotNet.InternalAbstractions;
#endif

namespace Cassandra.DataStax.Graph.Test.Integration
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
