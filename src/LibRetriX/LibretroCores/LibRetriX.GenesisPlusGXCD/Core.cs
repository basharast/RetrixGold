﻿using LibRetriX.RetroBindings;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U79KY1";
    }

    namespace GenesisPlusGXCD
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
                new FileDependency("BIOS_CD_E.bin", "Mega-CD (Model 1 1.00 Europe) BIOS", "e66fa1dc5820d254611fdcdba0662372"),
                new FileDependency("BIOS_CD_J.bin", "Mega-CD (Model 1 1.00 Japan) BIOS", "278a9397d192149e84e820ac621a8edd"),
                new FileDependency("BIOS_CD_U.bin", "Mega-CD (Model 1 1.00 USA) BIOS", "2efd74e3232ff260e371b99f84024f7f"),
                new FileDependency("sk.bin", "Sonic & Knuckles ROM (Optional)", "4ea493ea4e9f6c9ebfccbdb15110367e"),
                new FileDependency("sk2chip.bin", "Sonic & Knuckles UPMEM (Optional)", "b4e76e416b887f4e7413ba76fa735f16"),
                new FileDependency("areplay.bin", "Action Replay (Optional)", "a0028b3043f9d59ceeb03da5b073b30d"),
            };
        }
    }
}
