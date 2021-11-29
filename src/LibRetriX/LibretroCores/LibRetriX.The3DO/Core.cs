using LibRetriX.RetroBindings;
using LibRetriX.RetroBindings.Tools;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U11XT8";
    }

    namespace The3DO
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
                new FileDependency("panafz1.bin", "Panasonic FZ-1", "f47264dd47fe30f73ab3c010015c155b"),
                new FileDependency("panafz10.bin", "Panasonic FZ-10 (Optional)", "51f2f43ae2f3508a14d9f56597e2d3ce"),
                new FileDependency("panafz10-norsa.bin", "Panasonic FZ-10 [RSA] (Optional)", "1477bda80dc33731a65468c1f5bcbee9"),
                new FileDependency("panafz10e-anvil.bin", "Panasonic FZ-10-E [Anvil] (Optional)", "a48e6746bd7edec0f40cff078f0bb19f"),
                new FileDependency("panafz10e-anvil-norsa.bin", "FZ-10-E [Anvil,RSA] (Optional)", "cf11bbb5a16d7af9875cca9de9a15e09"),
                new FileDependency("panafz1j.bin", "Panasonic FZ-1J (Optional)", "a496cfdded3da562759be3561317b605"),
                new FileDependency("panafz1j-norsa.bin", "Panasonic FZ-1J [RSA] (Optional)", "f6c71de7470d16abe4f71b1444883dc8"),
                new FileDependency("goldstar.bin", "Goldstar GDO-101M (Optional)", "8639fd5e549bd6238cfee79e3e749114"),
                new FileDependency("sanyotry.bin", "Sanyo IMP-21J TRY (Optional)", "35fa1a1ebaaeea286dc5cd15487c13ea"),
                new FileDependency("3do_arcade_saot.bin", "Shootout At Old Tucson (Optional)", "8970fc987ab89a7f64da9f8a8c4333ff"),
                new FileDependency("panafz1-kanji.bin", "Panasonic FZ-1 Kanji (Optional)", "b8dc97f778a6245c58e064b0312e8281"),
                new FileDependency("panafz10ja-anvil-kanji.bin", "FZ-10JA Kanji (Optional)", "428577250f43edc902ea239c50d2240d"),
                new FileDependency("panafz1j-kanji.bin", "Panasonic FZ-1J Kanji (Optional)", "c23fb5d5e6bb1c240d02cf968972be37"),
            };


        }
    }
}
