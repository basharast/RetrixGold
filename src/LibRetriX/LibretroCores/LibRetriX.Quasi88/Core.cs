using LibRetriX.RetroBindings;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U11XT14";
    }

    namespace Quasi88
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
                new FileDependency("n88.rom", "-", "4f984e04a99d56c4cfe36115415d6eb8"),
                new FileDependency("n88n.rom", "PC-8000 series emulation", "2ff07b8769367321128e03924af668a0"),
                new FileDependency("disk.rom", "Loading disk images", "793f86784e5608352a5d7f03f03e0858"),
                new FileDependency("n88knj1.rom", "Viewing kanji", "d81c6d5d7ad1a4bbbd6ae22a01257603"),
                new FileDependency("n88_0.rom", "-", "d675a2ca186c6efcd6277b835de4c7e5"),
                new FileDependency("n88_1.rom", "-", "e844534dfe5744b381444dbe61ef1b66"),
                new FileDependency("n88_2.rom", "-", "6548fa45061274dee1ea8ae1e9e93910"),
                new FileDependency("n88_3.rom", "-", "fc4b76a402ba501e6ba6de4b3e8b4273"),
            };
        }
    }
}
