using LibRetriX.RetroBindings;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U79KY1";
    }

    namespace GenesisPlusGXMasterSystem
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
                new FileDependency("bios_E.sms", "MasterSystem EU (Optional)", "840481177270d5642a14ca71ee72844c"),
                new FileDependency("bios_U.sms", "MasterSystem US (Optional)", "840481177270d5642a14ca71ee72844c"),
                new FileDependency("bios_J.sms", "MasterSystem JP (Optional)", "24a519c53f67b00640d0048ef7089105"),
            };
        }
    }
}
