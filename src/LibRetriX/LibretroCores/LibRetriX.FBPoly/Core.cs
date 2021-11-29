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

    namespace FBPoly
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
                new FileDependency("pgm.zip", "IGS PolyGame Master BIOS", "581cc172db39bb5007642405adf25b6e")
            };
        }
    }
}
