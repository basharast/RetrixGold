using LibRetriX.RetroBindings;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U11XT13";
    }

    namespace Atari7800
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
                var core = new LibretroCore(Dependencies);
                core.Initialize();
                return core;
            }

            private static readonly FileDependency[] Dependencies =
            {
                new FileDependency("7800 BIOS (U).rom", "7800 BIOS (U) (Optional)", "0763f1ffb006ddbe32e52d497ee848ae"),
                new FileDependency("7800 BIOS (E).rom", "7800 BIOS (E) (Optional)", "397bb566584be7b9764e7a68974c4263"),
            };


        }
    }
}
