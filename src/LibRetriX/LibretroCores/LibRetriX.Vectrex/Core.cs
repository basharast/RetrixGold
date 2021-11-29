using LibRetriX.RetroBindings;
using LibRetriX.RetroBindings.Tools;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U85JV1";
    }

    namespace Vectrex
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
                var core = new LibretroCore(null, Options);
                core.Initialize();
                return core;
            }

            private static readonly Tuple<string, uint>[] Options =
            {
                Tuple.Create("vecx_res_multi", 0U),
            };
        }
    }
}
