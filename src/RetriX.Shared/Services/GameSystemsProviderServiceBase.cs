using Acr.UserDialogs;
using Newtonsoft.Json;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Models;
using RetriX.Shared.StreamProviders;
using RetriX.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RetriX.Shared.Services
{
    public abstract class GameSystemsProviderServiceBase : IGameSystemsProviderService
    {
        
        public abstract Task<List<GameSystemViewModel>> GenerateSystemsList(IFileSystem fileSystem);

        private IFileSystem FileSystem { get; set; }
        private IPlatformService PlatformService { get; set; }

        private IList<GameSystemViewModel> systems { get; set; }
        public IList<GameSystemViewModel> Systems => systems;

        public GameSystemsProviderServiceBase(IFileSystem fileSystem)
        {
            LoadAllCores(fileSystem);
        }

        public async void LoadAllCores(IFileSystem fileSystem)
        {
            try
            {
                FileSystem = fileSystem;
                systems = await GenerateSystemsList(FileSystem);

            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public async Task<IReadOnlyList<GameSystemViewModel>> GetCompatibleSystems(IFileInfo file)
        {
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
                    IEnumerable<string> entries;
                    using (var provider = new ArchiveStreamProvider($"test{Path.DirectorySeparatorChar}", file, PlatformService))
                    {
                        entries = await provider.ListEntriesAsync();
                    }

                    await Task.Run(() =>
                    {
                        try
                        {
                            var entriesExtensions = new HashSet<string>(entries.Select(d => Path.GetExtension(d.Replace("#", "")).ToLower()));
                            foreach (var i in Systems)
                            {
                                foreach (var j in entriesExtensions)
                                {
                                    if (i.SupportedExtensions != null && i.SupportedExtensions.Contains(j))
                                    {
                                        output.Add(i);
                                    }
                                }
                            }

                            //One extension in archive and one compatible core found. Skip adding systems natively supporting archives.
                            if (entriesExtensions.Count == 1 && output.Any())
                            {
                                shouldAddNativelySupportingSystems = false;
                            }
                        }catch(Exception ex)
                        {

                        }
                    });
                }

                if (shouldAddNativelySupportingSystems)
                {
                    var nativelySupportingSystems = Systems.Where(d => d.SupportedExtensions.Contains(Path.GetExtension(file.Name.Replace("#", "")).ToLower())).ToArray();
                    foreach (var i in nativelySupportingSystems)
                    {
                        output.Add(i);
                    }
                }

                return output.OrderBy(d => d.TempName).ToList();
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return null;
            }
        }

        
            public async Task<Tuple<GameLaunchEnvironment, GameLaunchEnvironment.GenerateResult>> GenerateGameLaunchEnvironmentAsync(GameSystemViewModel system, IFileInfo file, IDirectoryInfo rootFolder,bool IgnoreRoot,IPlatformService platformService, EventHandler StatusHandler=null)
           {
            
            try
            {
                PlatformService = platformService;
                bool rootNeeded = system.CheckRootFolderRequired(file) && rootFolder == null && !IgnoreRoot;

                /*var dependenciesMet = await system.CheckDependenciesMetAsync();
                if (!dependenciesMet)
                {
                    return Tuple.Create(default(GameLaunchEnvironment), GameLaunchEnvironment.GenerateResult.DependenciesUnmet);
                }*/

                if (rootNeeded)
                {
                    return Tuple.Create(default(GameLaunchEnvironment), GameLaunchEnvironment.GenerateResult.RootFolderRequired);
                }

                var vfsRomPath = "ROM";
                var vfsSystemPath = "System";
                var vfsSavePath = "Save";

                var core = system.Core;

                string virtualMainFilePath = null;
                var provider = default(StreamProviders.IStreamProvider);
                if (ArchiveStreamProvider.SupportedExtensions.Contains(Path.GetExtension(file.Name).ToLower()) && core.NativeArchiveSupport == false)
                {
                    var archiveProvider = new ArchiveStreamProvider(vfsRomPath, file, platformService);
                    provider = archiveProvider;
                    var entries = await provider.ListEntriesAsync();
                    virtualMainFilePath = entries.FirstOrDefault(d => system.SupportedExtensions.Contains(Path.GetExtension(d.Replace("#","")).ToLower()));
                    if (string.IsNullOrEmpty(virtualMainFilePath))
                    {
                        return Tuple.Create(default(GameLaunchEnvironment), GameLaunchEnvironment.GenerateResult.NoMainFileFound);
                    }
                }
                else
                {
                    string FileName = "";
                    if (system.Core.Name == "FinalBurn Neo" || system.Core.Name == "FB Alpha")
                    {
                        PlatformService?.PlayNotificationSound("notice.mp3");
                        ConfirmConfig confirSmartRename = new ConfirmConfig();
                        confirSmartRename.SetTitle("Smart Rename?");
                        confirSmartRename.SetMessage("Arcade games require custom name to work, would you like to use Retrix smart rename\n\nSelect 'No' if the game already renamed\n\nSelect Yes in case:\n1-You changed the game name\n2-Playing SubSystem game");
                        confirSmartRename.UseYesNo();
                        bool SmartRenameGame = await UserDialogs.Instance.ConfirmAsync(confirSmartRename);
                        if (SmartRenameGame) {
                            SetStatus("Smart Rename..", StatusHandler);
                            await Task.Delay(500);
                              var GameDataObject = await ArcadeSmartNameResolver(file, StatusHandler);
                          FileName = GameDataObject.GameName;
                          core.RetroGameType = GameDataObject.GameType;
                            //GC.Collect();
                            //GC.WaitForPendingFinalizers();
                        }
                        else
                        {
                            FileName = file.Name;
                        }
                    }
                    else
                    {
                        FileName = file.Name;
                    }
                    virtualMainFilePath = Path.Combine(vfsRomPath, FileName);
                    provider = new SingleFileStreamProvider(virtualMainFilePath, file);
                    if (rootFolder != null)
                    {
                        virtualMainFilePath = file.FullName.Substring(rootFolder.FullName.Length + 1);
                        virtualMainFilePath = Path.Combine(vfsRomPath, virtualMainFilePath);
                        provider = new FolderStreamProvider(vfsRomPath, rootFolder);
                    }
                }
                SetStatus("Preparing game..", StatusHandler);
                var systemFolder = await system.GetSystemDirectoryAsync();
                var systemProvider = new FolderStreamProvider(vfsSystemPath, systemFolder);
                core.SystemRootPath = vfsSystemPath;
                var saveFolder = await system.GetSaveDirectoryAsync();
                var saveProvider = new FolderStreamProvider(vfsSavePath, saveFolder);
                core.SaveRootPath = vfsSavePath;

                provider = new CombinedStreamProvider(new HashSet<StreamProviders.IStreamProvider>() { provider, systemProvider, saveProvider });

                return Tuple.Create(new GameLaunchEnvironment(core, provider, virtualMainFilePath, file.FullName, system.TempName, rootNeeded), GameLaunchEnvironment.GenerateResult.Success);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return null;
            }
        }

        #region Old Resolver
        Dictionary<string, string> dictionaryList = null;
        async Task<string> MAMERomNameResolver(IFileInfo sourceFile, GameSystemViewModel system)
        {
            try { 
            var md5 = await CryptographyService.ComputeMD5AsyncDirect(sourceFile);
            if (md5 != null) {
                
                var ROMSMAME = await FileSystem.InstallLocation.GetFileAsync("roms.mame");
                if (ROMSMAME != null)
                {
                    if (dictionaryList == null)
                    {
                            Encoding unicode = Encoding.UTF8;
                    byte[] result;
                    using (var outStream = await ROMSMAME.OpenAsync(FileAccess.Read))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            outStream.CopyTo(memoryStream);
                            result = memoryStream.ToArray();
                        }
                        await outStream.FlushAsync();
                    }
                    string ActionsFileContent = unicode.GetString(result);
                    ActionsFileContent = ActionsFileContent.Replace(Environment.NewLine, String.Empty);
                    dictionaryList = JsonConvert.DeserializeObject<Dictionary<string, string>>(ActionsFileContent);
                    }
                    string md5Check = md5.ToLowerInvariant();
                    foreach (string dictionaryItem in dictionaryList.Keys)
                    {
                        if (dictionaryItem.Contains(md5Check))
                        {
                            string fullName = dictionaryList[dictionaryItem] + Path.GetExtension(sourceFile.Name);
                            return fullName;
                        }
                    }
                    
                }
                    
            }
            }catch(Exception e)
            {
                if(PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }

            return sourceFile.Name;
        }
        #endregion

        bool HandlerErrorCatched = false;
        void SetStatus(string statusMessage,EventHandler eventHandler)
        {
            if (HandlerErrorCatched)
            {
                return;
            }
            try
            {
                if(eventHandler != null)
                {
                    eventHandler.Invoke(this, new StatusMessageArgs(statusMessage));
                }
            }catch(Exception e)
            {
                HandlerErrorCatched = true;
                PlatformService?.ShowErrorMessage(e);
            }
        }

        async Task<GameData> ArcadeSmartNameResolver(IFileInfo sourceFile, EventHandler StatusHandler = null)
        {
            GameData gameData = new GameData();
            gameData.GameName = sourceFile.Name;
            gameData.GameType = 0U;
            try
            {
                using (var outStream = await sourceFile.OpenAsync(FileAccess.Read))
                {
                    var parentArchive = new ZipArchive(outStream);
                    if (parentArchive.Entries.Count > 1)
                    {
                        /*ZipArchiveEntry entry1 = parentArchive.Entries.OrderByDescending(key => key.Length).ElementAt(0);
                        ZipArchiveEntry entry2 = parentArchive.Entries.OrderByDescending(key => key.Length).ElementAt(1);
                        var entry1CRC32 = await GetFileCRC32(entry1);
                        var entry2CRC32 = await GetFileCRC32(entry2);*/
                        var filesList = new List<string>();
                        foreach(var EntryItem in parentArchive.Entries)
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
                                    await Task.Delay(25);
                                    var CRC32 = await GetFileCRC32(EntryItem);
                                    filesList.Add(CRC32);
                                    break;
                            }
                        }
                        var CRC32Array = filesList.ToArray();
                        foreach (string XMLLibrary in GamesDatabases)
                        {
                            SetStatus($"Searching in {await GetConsoleNameByDatabase(XMLLibrary)}..", StatusHandler);
                            await Task.Delay(70);
                            var OriginalFileName = await GetFileOriginalName(CRC32Array, sourceFile, XMLLibrary);
                            if (OriginalFileName.Length > 0)
                            {
                                parentArchive.Dispose();
                                outStream.Dispose();
                                var GameType = await GetSubSystemType(XMLLibrary);
                                SetStatus($"Found {OriginalFileName}..", StatusHandler);
                                await Task.Delay(500);
                                gameData.GameName = OriginalFileName;
                                gameData.GameType = GameType;
                                return gameData;
                            }
                        }
                    }
                    else { 
                      ZipArchiveEntry entry = parentArchive.Entries.OrderByDescending(key => key.Length).ElementAt(0);
                      var CRC32 = await GetFileCRC32(entry);
                      foreach (string XMLLibrary in GamesDatabases)
                      {
                        SetStatus($"Searching in {await GetConsoleNameByDatabase(XMLLibrary)}..", StatusHandler);
                        await Task.Delay(70);
                        var OriginalFileName = await GetFileOriginalName(CRC32, sourceFile, XMLLibrary);
                            if (OriginalFileName.Length > 0)
                            {
                                parentArchive.Dispose();
                                outStream.Dispose();
                                var GameType = await GetSubSystemType(XMLLibrary);
                            SetStatus($"Found {OriginalFileName}..", StatusHandler);
                            await Task.Delay(500);
                            gameData.GameName = OriginalFileName;
                                gameData.GameType = GameType;
                                return gameData;
                            }
                      }
                    }

                }
            }catch(Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
            return gameData;
        }
        async Task<string> GetFileOriginalName(string CRC32, IFileInfo sourceFile, string XMLLibrary)
        {
            string OriginalName = "";
            try { 
            XDocument xmlContents = null;
            var OriginalFileExtension = Path.GetExtension(sourceFile.Name);
            
            xmlContents = XDocument.Load($@"{FileSystem.InstallLocation}\dats\{XMLLibrary} ");

                foreach (var node in xmlContents.Element("datafile").Descendants("game"))
                {
                    if (node is XElement)
                    {
                        foreach (var subNode in node.Descendants("rom"))
                        {
                            if(subNode.HasAttributes && subNode.Attribute("crc")!=null && subNode.Attribute("crc").Value.Equals(CRC32))
                            {
                                OriginalName = $"{node.Attribute("name").Value}{OriginalFileExtension}";
                                return OriginalName;
                            }
                        }
                    }
                }
            }catch(Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
            return OriginalName;
        }
        async Task<string> GetFileOriginalName(string[] CRC32, IFileInfo sourceFile, string XMLLibrary)
        {
            string OriginalName = "";
            try
            {
                XDocument xmlContents = null;
                var OriginalFileExtension = Path.GetExtension(sourceFile.Name);

                xmlContents = XDocument.Load($@"{FileSystem.InstallLocation}\dats\{XMLLibrary} ");
                Dictionary<string[], bool> CRC32Dictionary = new Dictionary<string[], bool>();
                foreach(var CRC32Element in CRC32)
                {
                    CRC32Dictionary.Add(new string[] {Path.GetRandomFileName(), CRC32Element }, false);
                }
                foreach (var node in xmlContents.Element("datafile").Descendants("game"))
                {
                    if (node is XElement)
                    {
                        bool foundMatch = false;
                        foreach(var CRC32Item in CRC32Dictionary.Keys.ToArray()) {
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
                PlatformService?.ShowErrorMessage(e);
            }
            return OriginalName;
        }
        private bool CheckDictionaryBool(Dictionary<string[], bool> dictionary)
        {
            foreach(var DictionaryValue in dictionary.Values)
            {
                if (!DictionaryValue)
                {
                    return false;
                }
            }
            return true;
        }
        async Task<string> GetFileCRC32(ZipArchiveEntry sourceFile)
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
        
        async Task<uint> GetSubSystemType(string DatabaseName)
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

        async Task<string> GetConsoleNameByDatabase(string DatabaseName)
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
}
