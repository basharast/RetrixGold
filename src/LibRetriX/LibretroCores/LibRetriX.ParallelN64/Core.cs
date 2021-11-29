using LibRetriX.RetroBindings;
using System;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = "_U51VG1";
    }

    namespace ParallelN64
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
                var core = new LibretroCore(null, Options);
                core.Initialize();
                return core;
            }
            private static readonly Tuple<string, uint>[] Options =
            {
                Tuple.Create("parallel-n64-audio-buffer-size", 1U),
                Tuple.Create("parallel-n64-gfxplugin-accuracy", 3U),
                Tuple.Create("parallel-n64-send_allist_to_hle_rsp", 1U),
                Tuple.Create("parallel-n64-cpucore", 0U),
                Tuple.Create("parallel-n64-skipframes", 1U),
                Tuple.Create("parallel-n64-screensize", 9U),
                Tuple.Create("parallel-n64-angrylion-multithread", 1U),
                Tuple.Create("parallel-n64-angrylion-vioverlay", 4U),
                Tuple.Create("parallel-n64-dithering", 1U),
                Tuple.Create("parallel-n64-gfxplugin", 0U)
            };
        }
    }
}
