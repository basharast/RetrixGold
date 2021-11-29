using LibRetriX.RetroBindings;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U79KY1";
    }

    namespace GenesisPlusGXMegaDrive
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
                new FileDependency("bios_MD.bin", "MegaDrive TMSS (Optional)", "45e298905a08f9cfb38fd504cd6dbc84"),
                new FileDependency("ggenie.bin", "Game Genie ROM (Optional)", "e8af7fe115a75c849f6aab3701e7799b"),
            };
        }
    }
}
