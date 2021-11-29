using LibRetriX.RetroBindings;
using System;
using System.IO;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U72HM1";
    }

    namespace FBAlpha
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
                core.Initialize(true);
                return core;
            }

            private static readonly FileDependency[] Dependencies =
            {
                new FileDependency("neogeo.zip", "NeoGeo BIOS collection", "93adcaa22d652417cbc3927d46b11806"),
                new FileDependency("bubsys.zip", "Bubble System (Optional)", "f81298afd68a1a24a49a1a2d9f087964"),
                new FileDependency("cchip.zip", "C-Chip Internal ROM (Optional)", "df6f8a3d83c028a5cb9f2f2be60773f3"),
                new FileDependency("coleco.zip", "ColecoVision System (Optional)", "140f47a7fe0cc6f7ff9634c4bb6014b4"),
                new FileDependency("decocass.zip", "DECO Cassette System (Optional)", "b7e1189b341bf6a8e270017c096d21b0"),
                new FileDependency("fdsbios.zip", "FDS System BIOS (Optional)", "d5e59c76bd6fb0668c79ecfa934cbc66"),
                new FileDependency("isgsm.zip", "ISG Selection Master 2006 (Optional)", "4a56d56e2219c5e2b006b66a4263c01c"),
                new FileDependency("midssio.zip", "Midway SSIO Sound Board (Optional)", "5904b0de768d1d506e766aa7e18994c1"),
                new FileDependency("neocdz.zip", "Neo Geo CDZ System (Optional)", "eed0134ebf619aebb81bdc4f53b1084e"),
                new FileDependency("nmk004.zip", "NMK004 Internal (Optional)", "bfacf1a68792d5348f93cf724d2f1dda"),
                new FileDependency("skns.zip", "Super Kaneko Nova System (Optional)", "3f956c4e7008804cb47cbde49bd5b908"),
                new FileDependency("spec128.zip", "ZX Spectrum 128 (Optional)", "b1b94f4e4cd645515fd42e6e61836f35"),
                new FileDependency("spectrum.zip", "ZX Spectrum (Optional)", "c5f6e525ec21b8b3bef52e9f8416be24"),
                new FileDependency("ym2608.zip", "YM2608 Internal (Optional)", "79ae0d2bb1901b7e606b6dc339b79a97"),
                new FileDependency("msx.zip", "MSX1 System (Optional)", "a317e6b4"),
            };
        }
    }
}
