using LibRetriX.RetroBindings;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U62BA1";
    }

    namespace Gambatte
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
                new FileDependency("gb_bios.bin", "Game Boy BIOS (Optional)", "32fbbd84168d3482956eb3c5051637f5"),
                new FileDependency("gbc_bios.bin", "Game Boy Color BIOS (Optional)", "dbfce9db9deaa2567f6a84fde55f9680"),
            };
        }
    }
}
