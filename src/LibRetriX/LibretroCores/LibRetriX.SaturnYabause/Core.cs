using LibRetriX.RetroBindings;
using LibRetriX.RetroBindings.Tools;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U11XT24";
    }

    namespace BeetleSaturnYabause
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
                new FileDependency("saturn_bios.bin", "Saturn BIOS", "af5828fdff51384f99b3c4926be27762"),
                new FileDependency("sega_101.bin", "Saturn JP BIOS", "85ec9ca47d8f6807718151cbcca8b964"),
                new FileDependency("mpr-17933.bin", "Saturn US.mdEU BIOS (Optional)", "3240872c70984b6cbfda1586cab68dbe")
            };

        }
    }
}
