using LibRetriX.RetroBindings;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U11XT19";
    }

    namespace TGBDual
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
                //var core = new LibretroCore(Dependencies);
                var core = new LibretroCore();
                core.Initialize();
                return core;
            }

            /*private static readonly FileDependency[] Dependencies =
            {
                new FileDependency("disksys.rom", "Famicom Disk System BIOS (Optional)", "ca30b50f880eb660a320674ed365ef7a"),
                new FileDependency("NstDatabase.xml", "Nestopia UE Database file", "NstDatabase.xml"),
            };*/
        }
    }
}
