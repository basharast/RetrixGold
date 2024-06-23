using RetriX.Shared.Models;
using RetriX.Shared.StreamProviders;
using RetriX.Shared.ViewModels;
using RetriX.UWP;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.Storage;

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

namespace RetriX.Shared.Services
{
    public class GameSystemsProviderServiceBase
    {
        public async Task<IReadOnlyList<GameSystemViewModel>> GetCompatibleSystems(GameFile gfile)
        {
            var file = gfile?.file;
            try
            {
                if (file == null)
                {
                    return new GameSystemViewModel[0];
                }

                var output = new HashSet<GameSystemViewModel>();
                bool shouldAddNativelySupportingSystems = true;
                if (ArchiveStreamProvider.SupportedExtensions.Contains(Path.GetExtension(file.Name.Replace("#", "")).ToLower()))
                {
                    var provider = new ArchiveStreamProvider(file);
                    var entries = await provider.ListEntriesAsync();

                    await Task.Run(() =>
                    {
                        try
                        {
                            var entriesExtensions = new HashSet<string>(entries.Select(d => Path.GetExtension(d.Replace("#", "")).ToLower()));
                            foreach (var i in PlatformService.Consoles)
                            {
                                foreach (var j in entriesExtensions)
                                {
                                    if (i.SupportedExtensions != null && (i.SupportedExtensions.Contains(j) || i.SupportedExtensions.Contains(j.ToUpper())))
                                    {
                                        output.Add(i);
                                    }
                                }
                            }

                            //Once extension in archive and one compatible core found. Skip adding systems natively supporting archives.
                            if (entriesExtensions.Count == 1 && output.Any())
                            {
                                shouldAddNativelySupportingSystems = false;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                    provider.Dispose();
                    entries = null;
                }

                if (shouldAddNativelySupportingSystems)
                {
                    var nativelySupportingSystems = PlatformService.Consoles.Where(d => d.SupportedExtensions.Contains(Path.GetExtension(file.Name.Replace("#", "")).ToLower())).ToArray();
                    foreach (var i in nativelySupportingSystems)
                    {
                        output.Add(i);
                    }
                }

                return output.OrderBy(d => d.TempName).ToList();
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                return null;
            }
        }


        //This function created for Start core without content
        public async Task<Tuple<GameLaunchEnvironment, GameLaunchEnvironment.GenerateResult>> GenerateGameLaunchEnvironmentAsync(GameSystemViewModel system, StorageFolder rootFolder, bool IgnoreRoot, EventHandler StatusHandler = null)
        {
            try
            {
                bool rootNeeded = false;
                system.Core.ignoreGamesFolderSelection = true;

                var core = system.Core;

                string virtualMainFilePath = null;

                SetStatus("Preparing game..", StatusHandler);
                var systemFolder = await system.GetSystemDirectoryAsync();

                var vfsSystemPath = systemFolder.Path;
                core.SystemRootPath = vfsSystemPath;
                core.systemRootFolder = systemFolder;

                var saveFolder = await system.GetSaveDirectoryAsync();

                var vfsSavePath = saveFolder.Path;
                core.SaveRootPath = vfsSavePath;
                core.savesRootFolder = saveFolder;

                var vfsContentPath = await system.GetGamesDirectoryAsync();
                core.SystemGamesPath = vfsContentPath.Path;

                //Generate required files for the core
                await GenerateRequiredFiles(core.Name.ToLower(), saveFolder, systemFolder, rootFolder, null);

                core.EnabledDebugLog = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("EnabledDebugLogsList", false);
                core.EnabledLogFile = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("LogFileState", false);

                return Tuple.Create(new GameLaunchEnvironment(core, virtualMainFilePath, null, system.TempName, rootNeeded), GameLaunchEnvironment.GenerateResult.Success);
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                return null;
            }
        }

        public async Task<Tuple<GameLaunchEnvironment, GameLaunchEnvironment.GenerateResult>> GenerateGameLaunchEnvironmentAsync(GameSystemViewModel system, GameFile gfile, StorageFolder rootFolder, bool IgnoreRoot, EventHandler StatusHandler = null, bool arcadeSmartCheck = false, bool singleFile = false, StorageFile sFile = null, bool customStart = false)
        {
            //Important: I dropped all stream providers part, new VFS functions will handle files requests directly at EmulationService.cs
            var file = gfile?.file;
            try
            {
                bool rootNeeded = system.CheckRootFolderRequired(file) && rootFolder == null && !IgnoreRoot;

                if (rootNeeded)
                {
                    return Tuple.Create(default(GameLaunchEnvironment), GameLaunchEnvironment.GenerateResult.RootFolderRequired);
                }


                //arcadeData will be used only in case the core is Arcade
                var arcadeData = "";

                bool ignoreGamesFolderSelection = customStart || singleFile;
                system.Core.ignoreGamesFolderSelection = ignoreGamesFolderSelection;

                var core = system.Core;

                string virtualMainFilePath = null;

                var vfsContentPath = await system.GetGamesDirectoryAsync();
                core.SystemGamesPath = vfsContentPath.Path;

                //Entry point will help to direct the core to the first file will be requested from the extracted files
                string entryPoint = "";
                bool archiveSupport = core.NativeArchiveSupport;
                var fileExt = Path.GetExtension(file.Name).ToLower();
                if (ArchiveStreamProvider.SupportedExtensionsNonZip.Contains(fileExt) && !core.NativeArchiveNonZipSupport)
                {
                    archiveSupport = false;
                }
                if (ArchiveStreamProvider.SupportedExtensions.Contains(fileExt) && !archiveSupport)
                {
                    //Archive handling will be in EmulationService.cs as VFS part
                    //This here will only to verify if the file contains supported content or not
                    //Then set entryPoint
                    var archiveProvider = new ArchiveStreamProvider(file);
                    var entries = await archiveProvider.ListEntriesAsync();
                    var virtualTestPath = entries.FirstOrDefault(d => system.SupportedExtensions.Contains(Path.GetExtension(d.Replace("#", "")).ToLower()));

                    //Give another try with uppercase
                    if (string.IsNullOrEmpty(virtualTestPath))
                    {
                        virtualTestPath = entries.FirstOrDefault(d => system.SupportedExtensions.Contains(Path.GetExtension(d.Replace("#", "")).ToUpper()));
                    }
                    if (string.IsNullOrEmpty(virtualTestPath))
                    {
                        return Tuple.Create(default(GameLaunchEnvironment), GameLaunchEnvironment.GenerateResult.NoMainFileFound);
                    }

                    entryPoint = Path.Combine(vfsContentPath.Path, virtualTestPath);

                    archiveProvider.Dispose();
                    archiveProvider = null;
                    entries = null;
                    //GC.Collect();
                    //GC.WaitForPendingFinalizers();
                }

                {
                    string FileName = "";
                    #region Arcade Name Resolver
                    if (arcadeSmartCheck)
                    {
                        CRC32Cache.Clear();
                        PlatformService.PlayNotificationSound("notice");
                        ConfirmConfig confirSmartRename = new ConfirmConfig();
                        confirSmartRename.SetTitle("Smart Rename?");
                        confirSmartRename.SetMessage("Arcade games require custom name to work, would you like to use smart rename\n\nSelect 'No' if the game already renamed\n\nSelect Yes in case:\n1-You changed the game name\n2-Playing SubSystem game\n3-After rename.. confirm the name 'in-game' from (Advaced->Confirm Game)");
                        confirSmartRename.UseYesNo();
                        bool SmartRenameGame = await UserDialogs.Instance.ConfirmAsync(confirSmartRename);
                        if (SmartRenameGame)
                        {
                            SetStatus("Smart Rename..", StatusHandler);
                            await Task.Delay(500);

                            bool cacheFound = false;
                            //Get File MD5
                            string md5 = file.Path;
                            try
                            {
                                md5 = await CryptographyService.ComputeMD5AsyncDirect(file);
                                var testValue = Plugin.Settings.CrossSettings.Current.GetValueOrDefault($"k{md5}", "");
                                if (testValue != null && testValue.Length > 0)
                                {
                                    cacheFound = true;
                                    var data = testValue.Split('|');
                                    FileName = data[0];
                                    try
                                    {
                                        core.RetroGameType = uint.Parse(data[1]);
                                    }
                                    catch (Exception exp)
                                    {
                                        PlatformService.ShowErrorMessage(exp);
                                    }
                                    arcadeData = $"{md5}|{FileName}|{core.RetroGameType}";
                                    SetStatus($"Found In cache..\n({FileName})", StatusHandler);
                                    await Task.Delay(500);
                                }
                            }
                            catch (Exception ex)
                            {
                                PlatformService.ShowErrorMessage(ex);
                            }

                            if (!cacheFound)
                            {
                                var GameDataObject = await ArcadeSmartNameResolver(file, StatusHandler);
                                FileName = GameDataObject.GameName;
                                core.RetroGameType = GameDataObject.GameType;
                                arcadeData = $"{md5}|{FileName}|{core.RetroGameType}";
                                //GC.Collect();
                                //GC.WaitForPendingFinalizers();
                            }
                        }
                        else
                        {
                            FileName = file.Name;
                        }
                    }
                    else
                    #endregion
                    {
                        FileName = file.Name;
                    }
                    virtualMainFilePath = file.Path;
                }
                SetStatus("Preparing game..", StatusHandler);
                var systemFolder = await system.GetSystemDirectoryAsync();

                var vfsSystemPath = systemFolder.Path;
                core.SystemRootPath = vfsSystemPath;
                core.systemRootFolder = systemFolder;

                var saveFolder = await system.GetSaveDirectoryAsync();

                var vfsSavePath = saveFolder.Path;
                core.SaveRootPath = vfsSavePath;
                core.savesRootFolder = saveFolder;

                //Generate required files for the core
                await GenerateRequiredFiles(core.Name.ToLower(), saveFolder, systemFolder, rootFolder, file);


                core.EnabledDebugLog = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("EnabledDebugLogsList", false);
                core.EnabledLogFile = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("LogFileState", false);

                //Reset emulation game service
                PlatformService.emulationService = null;
                PlatformService.emulationService = new EmulationService();

                //I used the `gfile.fullPath` not `file.Path` in case there is specific/old path sent with the file
                //The first start will generate recent entry with the current path
                //I used the path as ID (will be fixed in future)
                //If a backup restored on other device it will be a problem to use `file.Path`
                //(because the same game will not match if it's not in the same location)
                //using `gfile.fullPath` is safe for now / don't change it until creating new way for ID
                return Tuple.Create(new GameLaunchEnvironment(core, virtualMainFilePath, gfile.fullPath, system.TempName, rootNeeded, arcadeData, entryPoint, singleFile, sFile, customStart), GameLaunchEnvironment.GenerateResult.Success);
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                return null;
            }
        }
        private async Task GenerateRequiredFiles(string coreName, StorageFolder saveFolder, StorageFolder systemFolder, StorageFolder rootFolder, StorageFile file)
        {
            try
            {
                //Use core name in lower case
                switch (coreName)
                {
                    case "desmume":
                        //DeSmuME required .sav, .dct, .dsv for each game
                        //for some reason it's not creating these files (when not exists)
                        //I have to create these files from here before start
                        {
                            var gameName = Path.GetFileNameWithoutExtension(file.Name);
                            string[] requiredExts = new string[] { "sav", "dct", "dsv" };
                            foreach (var requiredExt in requiredExts)
                            {
                                StorageFile extFile = null;
                                try
                                {
                                    extFile = (StorageFile)await saveFolder.TryGetItemAsync($"{gameName}.{requiredExt}");
                                }
                                catch (Exception ex)
                                {

                                }
                                if (extFile == null)
                                {
                                    extFile = await saveFolder.CreateFileAsync($"{gameName}.{requiredExt}");
                                }
                            }
                        }
                        break;

                    case "beetle saturn":
                        {
                            var gameName = Path.GetFileNameWithoutExtension(file.Name);
                            string[] requiredExts = new string[] { "smpc" };
                            foreach (var requiredExt in requiredExts)
                            {
                                StorageFile extFile = null;
                                try
                                {
                                    extFile = (StorageFile)await saveFolder.TryGetItemAsync($"{gameName}.{requiredExt}");
                                }
                                catch (Exception ex)
                                {

                                }
                                if (extFile == null)
                                {
                                    extFile = await saveFolder.CreateFileAsync($"{gameName}.{requiredExt}");
                                }
                            }
                        }
                        break;

                    case "beetle psx":
                        {
                            var gameName = Path.GetFileNameWithoutExtension(file.Name);
                            string[] requiredExts = new string[] { "mcr" };
                            foreach (var requiredExt in requiredExts)
                            {
                                StorageFile extFile = null;
                                try
                                {
                                    extFile = (StorageFile)await saveFolder.TryGetItemAsync($"{gameName}.{requiredExt}");
                                }
                                catch (Exception ex)
                                {

                                }
                                if (extFile == null)
                                {
                                    extFile = await saveFolder.CreateFileAsync($"{gameName}.{requiredExt}");
                                }
                                var saveSlots = 10;
                                for (int i = 0; i < saveSlots; i++)
                                {
                                    try
                                    {
                                        var testFile = (StorageFile)await saveFolder.TryGetItemAsync($"{gameName}.{i}.{requiredExt}");
                                        if (testFile == null)
                                        {
                                            extFile = await saveFolder.CreateFileAsync($"{gameName}.{i}.{requiredExt}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }
                        }
                        break;

                    case "vice x64":
                        {
                            string[] requiredFiles = new string[] { @"vice\vice.rtc", @"vice\fliplist-C64.vfl", @"vice\vicerc", @"vice\vicerc-dump-C64" };
                            foreach (var requiredFile in requiredFiles)
                            {
                                StorageFile testFile = null;
                                try
                                {
                                    testFile = (StorageFile)await systemFolder.TryGetItemAsync(requiredFile);
                                }
                                catch (Exception ex)
                                {

                                }
                                if (testFile == null)
                                {
                                    testFile = await systemFolder.CreateFileAsync(requiredFile);
                                }
                            }
                        }
                        break;

                    case "fmsx":
                        {
                            string[] requiredFiles = new string[] { @"FMPAC.sav" };
                            foreach (var requiredFile in requiredFiles)
                            {
                                StorageFile testFile = null;
                                try
                                {
                                    testFile = (StorageFile)await systemFolder.TryGetItemAsync(requiredFile);
                                }
                                catch (Exception ex)
                                {

                                }
                                if (testFile == null)
                                {
                                    testFile = await systemFolder.CreateFileAsync(requiredFile);
                                }
                            }
                        }
                        break;

                    case "neocd":
                        {
                            string[] requiredFiles = new string[] { @"neocd\neocd.srm" };
                            foreach (var requiredFile in requiredFiles)
                            {
                                StorageFile testFile = null;
                                try
                                {
                                    testFile = (StorageFile)await systemFolder.TryGetItemAsync(requiredFile);
                                }
                                catch (Exception ex)
                                {

                                }
                                if (testFile == null)
                                {
                                    testFile = await systemFolder.CreateFileAsync(requiredFile);
                                }
                            }
                        }
                        break;

                    case "tyrquake":
                        {
                            string[] requiredFiles = new string[] { @"quake\s0" };
                            foreach (var requiredFile in requiredFiles)
                            {
                                var saveSlots = 12;
                                StorageFile extFile = null;
                                for (int i = 0; i < saveSlots; i++)
                                {
                                    try
                                    {
                                        var testFile = (StorageFile)await saveFolder.TryGetItemAsync($@"quake\s{i}");
                                        if (testFile == null)
                                        {
                                            extFile = await saveFolder.CreateFileAsync($@"quake\s{i}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    try
                                    {
                                        var testFile = (StorageFile)await saveFolder.TryGetItemAsync($@"quake\s{i}.sav");
                                        if (testFile == null)
                                        {
                                            extFile = await saveFolder.CreateFileAsync($@"quake\s{i}.sav");
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }
                        }
                        break;

                    case "opera":
                        {
                            string[] requiredFiles = new string[] { @"3DO.nvram" };
                            foreach (var requiredFile in requiredFiles)
                            {
                                StorageFile testFile = null;
                                try
                                {
                                    testFile = (StorageFile)await systemFolder.TryGetItemAsync(requiredFile);
                                }
                                catch (Exception ex)
                                {

                                }
                                if (testFile == null)
                                {
                                    testFile = await systemFolder.CreateFileAsync(requiredFile);
                                }
                                try
                                {
                                    testFile = (StorageFile)await saveFolder.TryGetItemAsync(requiredFile);
                                }
                                catch (Exception ex)
                                {

                                }
                                if (testFile == null)
                                {
                                    testFile = await saveFolder.CreateFileAsync(requiredFile);
                                }
                            }

                            var saveSlots = 12;
                            StorageFile extFile = null;
                            for (int i = 0; i < saveSlots; i++)
                            {

                                try
                                {
                                    var testFile = (StorageFile)await saveFolder.TryGetItemAsync($@"opera\shared\nvram.{i}.srm");
                                    if (testFile == null)
                                    {
                                        extFile = await saveFolder.CreateFileAsync($@"opera\shared\nvram.{i}.srm");
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                        break;
                }

            }
            catch (Exception e)
            {

            }
        }
        bool HandlerErrorCatched = false;
        void SetStatus(string statusMessage, EventHandler eventHandler)
        {
            if (HandlerErrorCatched)
            {
                return;
            }
            try
            {
                if (eventHandler != null)
                {
                    eventHandler.Invoke(this, new StatusMessageArgs(statusMessage));
                }
            }
            catch (Exception e)
            {
                HandlerErrorCatched = true;
                PlatformService.ShowErrorMessage(e);
            }
        }

        #region Arcade Name Resolver
        Dictionary<string, string> CRC32Cache = new Dictionary<string, string>();
        async Task<GameData> ArcadeSmartNameResolver(StorageFile sourceFile, EventHandler StatusHandler = null)
        {
            GameData gameData = new GameData();
            gameData.GameName = sourceFile.Name;
            gameData.GameType = 0U;
            try
            {
                using (var outStream = (await sourceFile.OpenAsync(FileAccessMode.Read)).AsStream())
                {
                    var parentArchive = new ZipArchive(outStream);
                    if (parentArchive.Entries.Count > 1)
                    {
                        var filesList = new List<string>();
                        foreach (var EntryItem in parentArchive.Entries)
                        {
                            var FileExtention = Path.GetExtension(EntryItem.FullName);
                            switch (FileExtention)
                            {
                                case ".htm":
                                case ".html":
                                case ".txt":
                                case ".doc":
                                case ".docx":
                                case ".lnk":
                                    //Ignore
                                    break;
                                default:
                                    SetStatus($"Check {EntryItem.Name}..", StatusHandler);
                                    await Task.Delay(1);
                                    var CRC32 = "";
                                    try
                                    {
                                        var testCRC32 = "";
                                        if (CRC32Cache.TryGetValue(EntryItem.Name, out testCRC32))
                                        {
                                            CRC32 = testCRC32;
                                        }
                                        else
                                        {
                                            CRC32 = GetFileCRC32(EntryItem);
                                            CRC32Cache.Add(EntryItem.Name, CRC32);
                                        }
                                    }
                                    catch (Exception exx)
                                    {
                                        CRC32 = GetFileCRC32(EntryItem);
                                    }
                                    filesList.Add(CRC32);
                                    break;
                            }
                        }
                        var CRC32Array = filesList.ToArray();
                        foreach (string XMLLibrary in GamesDatabases)
                        {
                            SetStatus($"Searching in {GetConsoleNameByDatabase(XMLLibrary)}..", StatusHandler);
                            await Task.Delay(5);
                            var OriginalFileName = GetFileOriginalName(CRC32Array, sourceFile, XMLLibrary);
                            if (OriginalFileName.Length > 0)
                            {
                                parentArchive.Dispose();
                                outStream.Dispose();
                                var GameType = GetSubSystemType(XMLLibrary);
                                SetStatus($"Found {OriginalFileName}..", StatusHandler);
                                await Task.Delay(500);
                                gameData.GameName = OriginalFileName;
                                gameData.GameType = GameType;
                                return gameData;
                            }
                        }
                    }
                    else
                    {
                        ZipArchiveEntry entry = parentArchive.Entries.OrderByDescending(key => key.Length).ElementAt(0);
                        var CRC32 = "";
                        try
                        {
                            var testCRC32 = "";
                            if (CRC32Cache.TryGetValue(entry.Name, out testCRC32))
                            {
                                CRC32 = testCRC32;
                            }
                            else
                            {
                                CRC32 = GetFileCRC32(entry);
                                CRC32Cache.Add(entry.Name, CRC32);
                            }
                        }
                        catch (Exception exx)
                        {
                            CRC32 = GetFileCRC32(entry);
                        }
                        foreach (string XMLLibrary in GamesDatabases)
                        {
                            SetStatus($"Searching in {GetConsoleNameByDatabase(XMLLibrary)}..", StatusHandler);
                            await Task.Delay(70);
                            var OriginalFileName = GetFileOriginalName(CRC32, sourceFile, XMLLibrary);
                            if (OriginalFileName.Length > 0)
                            {
                                parentArchive.Dispose();
                                outStream.Dispose();
                                var GameType = GetSubSystemType(XMLLibrary);
                                SetStatus($"Found {OriginalFileName}..", StatusHandler);
                                await Task.Delay(500);
                                gameData.GameName = OriginalFileName;
                                gameData.GameType = GameType;
                                return gameData;
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            return gameData;
        }
        string GetFileOriginalName(string CRC32, StorageFile sourceFile, string XMLLibrary)
        {
            string OriginalName = "";
            try
            {
                XDocument xmlContents = null;
                var OriginalFileExtension = Path.GetExtension(sourceFile.Name);

                xmlContents = XDocument.Load($@"{Package.Current.InstalledLocation.Path}\dats\{XMLLibrary} ");

                foreach (var node in xmlContents.Element("datafile").Descendants("game"))
                {
                    if (node is XElement)
                    {
                        foreach (var subNode in node.Descendants("rom"))
                        {
                            if (subNode.HasAttributes && subNode.Attribute("crc") != null && subNode.Attribute("crc").Value.Equals(CRC32))
                            {
                                OriginalName = $"{node.Attribute("name").Value}{OriginalFileExtension}";
                                return OriginalName;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            return OriginalName;
        }
        string GetFileOriginalName(string[] CRC32, StorageFile sourceFile, string XMLLibrary)
        {
            string OriginalName = "";
            try
            {
                XDocument xmlContents = null;
                var OriginalFileExtension = Path.GetExtension(sourceFile.Name);

                xmlContents = XDocument.Load($@"{Package.Current.InstalledLocation.Path}\dats\{XMLLibrary} ");
                Dictionary<string[], bool> CRC32Dictionary = new Dictionary<string[], bool>();
                foreach (var CRC32Element in CRC32)
                {
                    CRC32Dictionary.Add(new string[] { Path.GetRandomFileName(), CRC32Element }, false);
                }
                foreach (var node in xmlContents.Element("datafile").Descendants("game"))
                {
                    if (node is XElement)
                    {
                        bool foundMatch = false;
                        foreach (var CRC32Item in CRC32Dictionary.Keys.ToArray())
                        {
                            string currentCRC32 = CRC32Item[1];
                            CRC32Dictionary[CRC32Item] = false;
                            foreach (var subNode in node.Descendants("rom"))
                            {
                                if (subNode.HasAttributes && subNode.Attribute("crc") != null && subNode.Attribute("crc").Value.Equals(currentCRC32))
                                {
                                    OriginalName = $"{node.Attribute("name").Value}{OriginalFileExtension}";
                                    CRC32Dictionary[CRC32Item] = true;
                                    break;
                                }
                            }
                        }
                        if (CheckDictionaryBool(CRC32Dictionary))
                        {
                            return OriginalName;
                        }
                        else
                        {
                            OriginalName = "";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            return OriginalName;
        }
        private bool CheckDictionaryBool(Dictionary<string[], bool> dictionary)
        {
            foreach (var DictionaryValue in dictionary.Values)
            {
                if (!DictionaryValue)
                {
                    return false;
                }
            }
            return true;
        }
        string GetFileCRC32(ZipArchiveEntry sourceFile)
        {
            CRC32 crc32 = new CRC32();
            String hash = String.Empty;

            using (var outStream = sourceFile.Open())
                foreach (byte b in crc32.ComputeHash(outStream)) hash += b.ToString("x2").ToLower();
            return hash;
        }

        string[] GamesDatabases = new string[] {
        "FinalBurn Neo (ClrMame Pro XML, Arcade only).dat",
        "FinalBurn Neo (ClrMame Pro XML, Neogeo only).dat",
        "FinalBurn Neo (ClrMame Pro XML, ColecoVision only).dat",
        "FinalBurn Neo (ClrMame Pro XML, Game Gear only).dat",
        "FinalBurn Neo (ClrMame Pro XML, Megadrive only).dat",
        "FinalBurn Neo (ClrMame Pro XML, MSX 1 Games only).dat",
        "FinalBurn Neo (ClrMame Pro XML, PC-Engine only).dat",
        "FinalBurn Neo (ClrMame Pro XML, Sega SG-1000 only).dat",
        "FinalBurn Neo (ClrMame Pro XML, SuprGrafx only).dat",
        "FinalBurn Neo (ClrMame Pro XML, Master System only).dat",
        "FinalBurn Neo (ClrMame Pro XML, TurboGrafx16 only).dat",
        "FinalBurn Neo (ClrMame Pro XML, ZX Spectrum Games only).dat",
        "FinalBurn Neo (ClrMame Pro XML, NES Games only).dat",
        "FinalBurn Neo (ClrMame Pro XML, FDS Games only).dat"
        };

        uint GetSubSystemType(string DatabaseName)
        {
            uint SystemName = 0U;
            switch (DatabaseName)
            {
                case "FinalBurn Neo (ClrMame Pro XML, ColecoVision only).dat":
                    SystemName = 1;
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, Game Gear only).dat":
                    SystemName = 2;
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, Megadrive only).dat":
                    SystemName = 3;
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, MSX 1 Games only).dat":
                    SystemName = 4;
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, PC-Engine only).dat":
                    SystemName = 5;
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, Sega SG-1000 only).dat":
                    SystemName = 6;
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, SuprGrafx only).dat":
                    SystemName = 7;
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, Master System only).dat":
                    SystemName = 8;
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, TurboGrafx16 only).dat":
                    SystemName = 9;
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, ZX Spectrum Games only).dat":
                    SystemName = 10;
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, NES Games only).dat":
                    SystemName = 11;
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, FDS Games only).dat":
                    SystemName = 12;
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, Neogeo only).dat":
                    SystemName = 13;
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, NeoGeo Pocket Games only).dat":
                    SystemName = 14;
                    break;
            }
            return SystemName;
        }

        string GetConsoleNameByDatabase(string DatabaseName)
        {
            string SystemName = "Arcade";
            switch (DatabaseName)
            {
                case "FinalBurn Neo (ClrMame Pro XML, ColecoVision only).dat":
                    SystemName = "ColecoVision";
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, Game Gear only).dat":
                    SystemName = "Game Gear";
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, Megadrive only).dat":
                    SystemName = "Megadrive";
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, MSX 1 Games only).dat":
                    SystemName = "MSX 1";
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, PC-Engine only).dat":
                    SystemName = "PC-Engine";
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, Sega SG-1000 only).dat":
                    SystemName = "SG-1000";
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, SuprGrafx only).dat":
                    SystemName = "SuprGrafx";
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, Master System only).dat":
                    SystemName = "Master System";
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, TurboGrafx16 only).dat":
                    SystemName = "TurboGrafx16";
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, ZX Spectrum Games only).dat":
                    SystemName = "Spectrum";
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, NES Games only).dat":
                    SystemName = "NES";
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, FDS Games only).dat":
                    SystemName = "FDS";
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, Neogeo only).dat":
                    SystemName = "Neogeo";
                    break;
                case "FinalBurn Neo (ClrMame Pro XML, NeoGeo Pocket Games only).dat":
                    SystemName = "NeoGeo Pocket";
                    break;
            }
            return SystemName;
        }
        #endregion

    }

    public class GameData
    {
        public string GameName;
        public uint GameType;
    }
    public class CRC32
    {
        private readonly uint[] ChecksumTable;
        private readonly uint Polynomial = 0xEDB88320;

        public CRC32()
        {
            ChecksumTable = new uint[0x100];

            for (uint index = 0; index < 0x100; ++index)
            {
                uint item = index;
                for (int bit = 0; bit < 8; ++bit)
                    item = ((item & 1) != 0) ? (Polynomial ^ (item >> 1)) : (item >> 1);
                ChecksumTable[index] = item;
            }
        }

        public byte[] ComputeHash(Stream stream)
        {
            uint result = 0xFFFFFFFF;

            int current;
            while ((current = stream.ReadByte()) != -1)
                result = ChecksumTable[(result & 0xFF) ^ (byte)current] ^ (result >> 8);

            byte[] hash = BitConverter.GetBytes(~result);
            Array.Reverse(hash);
            return hash;
        }

        public byte[] ComputeHash(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
                return ComputeHash(stream);
        }
    }

    public class StatusMessageArgs : EventArgs
    {
        public string StatusMessage { get; set; }

        public StatusMessageArgs(string statusMessage)
        {
            this.StatusMessage = statusMessage;
        }
    }
    public class GameFile
    {
        //Q: Why not using file.Path?
        //A: Recent games use path as unique id..
        //   when you restore backup on other device, the location will not be the same
        //   so I need to keep the same id (old path) or there will duplicated results
        //   it can be solved by using md5 or crc as id but it will affect on the old backups
        //   I will change it to md5 or crc in future
        public string fullPath { get; set; }
        public StorageFile file;
        public GameFile(StorageFile file, string path = "")
        {
            fullPath = path;
            this.file = file;
            if (fullPath == null || fullPath.Length == 0)
            {
                fullPath = file?.Path;
            }
        }
    }
}
