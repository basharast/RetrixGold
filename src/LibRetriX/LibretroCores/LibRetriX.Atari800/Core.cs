using LibRetriX.RetroBindings;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U11XT23";
    }

    namespace Atari800
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
                new FileDependency("5200.rom", "5200 BIOS", "281f20ea4320404ec820fb7ec0693b38"),
                new FileDependency("ATARIXL.ROM", "Atari XL/XE OS", "06daac977823773a3eea3422fd26a703"),
                new FileDependency("ATARIBAS.ROM", "BASIC interpreter", "0bac0c6a50104045d902df4503a4c30b"),
                new FileDependency("ATARIOSA.ROM", "Atari 400/800 PAL", "eb1f32f5d9f382db1bbfb8d7f9cb343a"),
                new FileDependency("ATARIOSB.ROM", "BIOS for Atari 400/800 NTSC", "a3e8d617c95d08031fe1b20d541434b2"),
            };
        }
    }
}
