using LibRetriX.RetroBindings;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U11XT3";
    }

    namespace FreeChaf
    {
        public static class Core
        {
            public static ICore Instance(string LocalStateLocation = "")
            {
                string TestLocation = $"{LocalStateLocation}\\PureCore\\{NativeDllInfo.DllName}";
                if (System.IO.File.Exists($"{TestLocation}.dll"))
                {
                    NativeDllInfo.DllName = TestLocation;
                }
                ICore core = InitCore();
                return core;
            }

            private static ICore InitCore()
            {
                var core = new LibretroCore(Dependencies);
                core.Initialize();
                return core;
            }

            private static readonly FileDependency[] Dependencies =
            {
                new FileDependency("sl31253.bin", "ChannelF BIOS (PSU 1)", "ac9804d4c0e9d07e33472e3726ed15c3"),
                new FileDependency("sl31254.bin", "ChannelF BIOS (PSU 2)", "da98f4bb3242ab80d76629021bb27585"),
                new FileDependency("sl90025.bin", "ChannelF II BIOS (PSU 1)", "95d339631d867c8f1d15a5f2ec26069d"),
            };
        }
    }
}
