using LibRetriX.RetroBindings;
using LibRetriX.RetroBindings.Tools;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = nameof(PPSSPP);
    }

    namespace PPSSPP
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
                var core = new LibretroCore();
                core.Initialize();
                return core;
            }

            /*private static readonly FileDependency[] Dependencies =
            {
                new FileDependency("scph5500.bin", "PlayStation (v3.0 09/09/96 J) BIOS", "8dd7d5296a650fac7319bce665a6a53c"),
                new FileDependency("scph5501.bin", "PlayStation (v3.0 11/18/96 A) BIOS", "490f666e1afb15b7362b406ed1cea246"),
                new FileDependency("scph5502.bin", "PlayStation (v3.0 01/06/97 E) BIOS", "32736f17079d0b2b7024407c39bd3050"),
            };

            private static readonly Tuple<string, uint>[] Options =
            {
                Tuple.Create("beetle_psx_frame_duping", 1U),
                Tuple.Create("beetle_psx_analog_calibration", 1U)
            };*/
        }
    }
}
