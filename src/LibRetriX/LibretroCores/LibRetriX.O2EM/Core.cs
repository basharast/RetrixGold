using LibRetriX.RetroBindings;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U11XT7";
    }

    namespace O2EM
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
                new FileDependency("o2rom.bin", "Odyssey2 BIOS - G7000", "562d5ebf9e030a40d6fabfc2f33139fd"),
                new FileDependency("c52.bin", "Videopac+ French BIOS - G7000", "f1071cdb0b6b10dde94d3bc8a6146387"),
                new FileDependency("g7400.bin", "Videopac+ European BIOS - G7400", "c500ff71236068e0dc0d0603d265ae76"),
                new FileDependency("jopac.bin", "Videopac+ French BIOS - G7400", "279008e4a0db2dc5f1c048853b033828"),
            };
        }
    }
}
