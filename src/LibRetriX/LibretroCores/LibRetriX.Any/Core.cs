using LibRetriX.RetroBindings;
using LibRetriX.RetroBindings.Tools;
using System;
using System.IO;
using System.Threading;

namespace LibRetriX
{
    internal static class NativeDllInfo
    {
        public static string DllName = nameof(AnyCore);
    }

    namespace AnyCore
    {
        
        public  class Core
        {
            
            string AnyCoreName;
            private readonly FileDependency[] Dependencies = null;
            public Core(FileDependency[] BOISFiles)
            {
                Dependencies = BOISFiles;
            }
            public  ICore GetCoreInstance(string CoreDllName)
            {
                //NativeDllInfo.DllName = CoreDllName;
                AnyCoreName = CoreDllName;
                
                Lazy<ICore> core = new Lazy<ICore>(InitCore, LazyThreadSafetyMode.ExecutionAndPublication);
                return core.Value;
            }
            private  ICore InitCore()
            {
                var core = new LibretroCore(Dependencies, null ,null , AnyCoreName);
                core.Initialize();
                return core;
            }

        }
    }
}
