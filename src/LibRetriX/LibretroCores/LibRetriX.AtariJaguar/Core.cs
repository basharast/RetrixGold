using LibRetriX.RetroBindings;
using LibRetriX.RetroBindings.Tools;
using System;
using System.IO;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U46UB1";
    }

    namespace AtariJaguar
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
                //, Options, Converter.GenerateDeviceSubclass(Constants.RETRO_DEVICE_ANALOG, 0)
                var core = new LibretroCore(Dependencies, Options);
                core.Initialize();
                return core;
            }

            private static readonly FileDependency[] Dependencies =
            {
                new FileDependency("jagboot.zip", "BIOS Loading (Optional)", "2c35353f766c4448632df176c320f3a9"),
            };

            private static readonly Tuple<string, uint>[] Options =
            {
                Tuple.Create("virtualjaguar_usefastblitter", 1U),
            };

        }
    }
}
