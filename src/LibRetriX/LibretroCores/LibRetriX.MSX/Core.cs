using LibRetriX.RetroBindings;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U77WQ1";
    }

    namespace MSX
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
                new FileDependency("MSX.ROM", " MSX BIOS", "364a1a579fe5cb8dba54519bcfcdac0d"),
                new FileDependency("MSX2.ROM", " MSX2 BIOS", "ec3a01c91f24fbddcbcab0ad301bc9ef"),
                new FileDependency("MSX2EXT.ROM", "MSX2 ExtROM", "2183c2aff17cf4297bdb496de78c2e8a"),
                new FileDependency("MSX2P.ROM", "MSX2+ BIOS", "847cc025ffae665487940ff2639540e5"),
                new FileDependency("MSX2PEXT.ROM", "MSX2+ ExtROM", "7c8243c71d8f143b2531f01afa6a05dc"),
                new FileDependency("DISK.ROM", "DiskROM/BDOS (Optional)", "80dcd1ad1a4cf65d64b7ba10504e8190"),
                new FileDependency("FMPAC.ROM", "FMPAC BIOS (Optional)", "6f69cc8b5ed761b03afd78000dfb0e19"),
                new FileDependency("MSXDOS2.ROM", "MSX-DOS 2 (Optional)", "6418d091cd6907bbcf940324339e43bb"),
                new FileDependency("PAINTER.ROM", "Yamaha Painter (Optional)", "403cdea1cbd2bb24fae506941f8f655e"),
                new FileDependency("KANJI.ROM", "Kanji Font (Optional)", "febe8782b466d7c3b16de6d104826b34"),
            };


        }
    }
}
