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

    namespace VBAMAdvance
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
                new FileDependency("gba_bios.bin", "Game Boy Advance BIOS (Optional)", "a860e8c0b6d573d191e4ec7db1b1e4f6"),
            };
        }
    }
}
