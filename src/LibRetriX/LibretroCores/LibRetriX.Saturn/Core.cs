using LibRetriX.RetroBindings;
using LibRetriX.RetroBindings.Tools;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U38JA1";
    }

    namespace BeetleSaturn
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
                new FileDependency("sega_101.bin", "Saturn JP BIOS", "85ec9ca47d8f6807718151cbcca8b964"),
                new FileDependency("mpr-17933.bin", "Saturn US.mdEU BIOS", "3240872c70984b6cbfda1586cab68dbe")
            };

            /*
             * ,
                new FileDependency("mpr-18811-mx.ic1", "The King of Fighters 95 ROM", "255113ba943c92a54facd25a10fd780c"),
                new FileDependency("mpr-19367-mx.ic1", "Hikari no Kyojin Densetsu ROM", "1cd19988d1d72a3e7caa0b73234c96b4"),
            private static readonly Tuple<string, uint>[] Options =
            {
                Tuple.Create("beetle_psx_frame_duping", 1U),
                Tuple.Create("beetle_psx_analog_calibration", 1U)
            };
            */
        }
    }
}
