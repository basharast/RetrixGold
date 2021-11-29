using LibRetriX.RetroBindings;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U42PC1";
    }

    namespace MelonDS
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

            public static ICore InitCore()
            {
                var core = new LibretroCore(Dependencies, Options);
                core.Initialize();
                return core;
            }

            private static readonly FileDependency[] Dependencies =
            {
                new FileDependency("bios7.bin", "ARM7 BIOS (Optional)", "df692a80a5b1bc90728bc3dfc76cd948"),
                new FileDependency("bios9.bin", "ARM9 BIOS (Optional)", "a392174eb3e572fed6447e956bde4b25"),
                new FileDependency("firmware.bin", "Firmware (Optional)", "b10f39a8a5a573753406f9da2e7232c8"),
            };

            private static readonly Tuple<string, uint>[] Options =
            {
                Tuple.Create("desmume_num_cores", 1U),
                Tuple.Create("desmume_frameskip", 2U),
                Tuple.Create("desmume_gfx_linehack", 1U),
                Tuple.Create("desmume_load_to_memory", 0U),
                Tuple.Create("desmume_pointer_type", 0U),
                Tuple.Create("desmume_pointer_device_l", 2U),
                Tuple.Create("desmume_mouse_speed", 7U),
                Tuple.Create("desmume_pointer_colour", 0U),
                Tuple.Create("desmume_gfx_edgemark", 1U),
                Tuple.Create("desmume_use_external_bios", 0U),
                Tuple.Create("desmume_advanced_timing", 1U)
            };
        }
    }
}
