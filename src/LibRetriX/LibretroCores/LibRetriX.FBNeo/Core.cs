using LibRetriX.RetroBindings;
using System;
using System.IO;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U72HM1";
    }

    namespace FBNeo
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
                core.Initialize(true);
                return core;
            }

            private static readonly FileDependency[] Dependencies =
            {
                new FileDependency("neogeo.zip", "NeoGeo BIOS collection", "93adcaa22d652417cbc3927d46b11806")
            };
        }
    }
}
