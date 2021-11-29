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

    namespace VBAMMicro
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
                var core = new LibretroCore(Dependencies, null, Converter.GenerateDeviceSubclass(Constants.RETRO_DEVICE_JOYPAD, 0));
                core.Initialize();
                return core;
            }

            private static readonly FileDependency[] Dependencies =
            {
                new FileDependency("gb_bios.bin", "Game Boy BIOS (Optional)", "32fbbd84168d3482956eb3c5051637f5"),
                new FileDependency("gbc_bios.bin", "Game Boy Color BIOS (Optional)", "dbfce9db9deaa2567f6a84fde55f9680"),
                new FileDependency("gba_bios.bin", "Game Boy Advance BIOS (Optional)", "a860e8c0b6d573d191e4ec7db1b1e4f6"),
            };
        }
    }
}
