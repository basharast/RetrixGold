using LibRetriX.RetroBindings;
using Windows.Storage;

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

namespace RetriX.Shared.Models
{
    public class GameLaunchEnvironment
    {
        public enum GenerateResult { Success, RootFolderRequired, DependenciesUnmet, NoMainFileFound };

        public LibretroCore Core { get; }
        public string MainFilePath { get; }
        public string MainFileStoredPath { get; }
        public string SystemName="";
        public string ArcadeData = "";
        public string EntryPoint = "";
        public bool RootNeeded=false;
        public bool isSingleFile = false;
        public bool customStart = false;
        public StorageFile singleFile = null;
        public GameLaunchEnvironment(LibretroCore core, string mainFilePath)
        {
            Core = core;
            MainFilePath = mainFilePath;
        }
        public GameLaunchEnvironment(LibretroCore core, string mainFilePath, string mainFileStoredPath, string systemName, bool rootNeeded, string arcadeData = "", string entryPoint = "", bool isSingle = false, StorageFile file = null, bool cStart = false)
        {
            Core = core;
            MainFilePath = mainFilePath;
            SystemName = systemName;
            RootNeeded = rootNeeded;
            MainFileStoredPath = mainFileStoredPath;
            ArcadeData = arcadeData;
            EntryPoint = entryPoint;
            isSingleFile = isSingle;
            singleFile = file;
            customStart = cStart;
        }
    }
}
