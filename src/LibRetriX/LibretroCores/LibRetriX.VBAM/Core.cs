using LibRetriX.RetroBindings;
using LibRetriX.RetroBindings.Tools;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U82RO1";
    }

    namespace VBAM
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
                var core = new LibretroCore(Dependencies, Options, Converter.GenerateDeviceSubclass(Constants.RETRO_DEVICE_JOYPAD, 0));
                core.Initialize();
                return core;
            }

            private static readonly FileDependency[] Dependencies =
            {
                new FileDependency("gb_bios.bin", "Game Boy BIOS (Optional)", "32fbbd84168d3482956eb3c5051637f5"),
            };
            private static readonly Tuple<string, uint>[] Options =
            {
                Tuple.Create("vbam_gbHardware", 1U),
                Tuple.Create("vbam_showborders", 2U),
            };
        }
    }
}
