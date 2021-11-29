using LibRetriX.RetroBindings;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U11XT5";
    }

    namespace Handy
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
                new FileDependency("lynxboot.img", "Lynx Boot Image", "fcd403db69f54290b51035d82f835e7b"),
            };

        }
    }
}

