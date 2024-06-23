using LibRetriX;
using LibRetriX.RetroBindings;
using RetriX.Shared.Components;
using RetriX.Shared.StreamProviders;
using RetriX.Shared.ViewModels;
using RetriX.UWP;
using RetriX.UWP.Services;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.Xaml;
using static RetriX.UWP.Services.PlatformService;

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
    //The changes for R,L names related to SEGA keys, it was easier for me to map the buttons
    public enum InjectedInputTypes
    {
        DeviceIdJoypadB = 0,
        DeviceIdJoypadY = 1,
        DeviceIdJoypadSelect = 2,
        DeviceIdJoypadStart = 3,
        DeviceIdJoypadUp = 4,
        DeviceIdJoypadDown = 5,
        DeviceIdJoypadLeft = 6,
        DeviceIdJoypadRight = 7,
        DeviceIdJoypadA = 8,
        DeviceIdJoypadX = 9,
        DeviceIdJoypadC = 10,//L1
        DeviceIdJoypadZ = 11, //R1
        DeviceIdJoypadL = 12, //L2
        DeviceIdJoypadR = 13, //R2
        DeviceIdJoypadL2 = 14, //L3
        DeviceIdJoypadR2 = 15, //R3
        DeviceIdPointerPressed = 38,
        DeviceIdPointerX = 36,
        DeviceIdPointerY = 37,
        DeviceIdMouseLeft = 22,
        DeviceIdMouseRight = 23,
        DeviceIdMouseX = 20,
        DeviceIdMouseY = 21
    };

    public class EmulationService
    {
        private static readonly IReadOnlyDictionary<InjectedInputTypes, InputTypes> InjectedInputMapping = new Dictionary<InjectedInputTypes, InputTypes>
        {
            { InjectedInputTypes.DeviceIdJoypadA, InputTypes.DeviceIdJoypadA },
            { InjectedInputTypes.DeviceIdJoypadB, InputTypes.DeviceIdJoypadB },
            { InjectedInputTypes.DeviceIdJoypadDown, InputTypes.DeviceIdJoypadDown },
            { InjectedInputTypes.DeviceIdJoypadLeft, InputTypes.DeviceIdJoypadLeft },
            { InjectedInputTypes.DeviceIdJoypadRight, InputTypes.DeviceIdJoypadRight },
            { InjectedInputTypes.DeviceIdJoypadSelect, InputTypes.DeviceIdJoypadSelect },
            { InjectedInputTypes.DeviceIdJoypadStart, InputTypes.DeviceIdJoypadStart },
            { InjectedInputTypes.DeviceIdJoypadUp, InputTypes.DeviceIdJoypadUp },
            { InjectedInputTypes.DeviceIdJoypadX, InputTypes.DeviceIdJoypadX },
            { InjectedInputTypes.DeviceIdJoypadY, InputTypes.DeviceIdJoypadY },
            { InjectedInputTypes.DeviceIdJoypadC, InputTypes.DeviceIdJoypadL },
            { InjectedInputTypes.DeviceIdJoypadZ, InputTypes.DeviceIdJoypadR },
            { InjectedInputTypes.DeviceIdJoypadL, InputTypes.DeviceIdJoypadL2 },
            { InjectedInputTypes.DeviceIdJoypadR, InputTypes.DeviceIdJoypadR2 },
            { InjectedInputTypes.DeviceIdJoypadL2, InputTypes.DeviceIdJoypadL3 },
            { InjectedInputTypes.DeviceIdJoypadR2, InputTypes.DeviceIdJoypadR3 },
            { InjectedInputTypes.DeviceIdPointerPressed, InputTypes.DeviceIdPointerPressed },
            { InjectedInputTypes.DeviceIdPointerX, InputTypes.DeviceIdPointerX },
            { InjectedInputTypes.DeviceIdPointerY, InputTypes.DeviceIdPointerY },
            { InjectedInputTypes.DeviceIdMouseRight, InputTypes.DeviceIdMouseRight },
            { InjectedInputTypes.DeviceIdMouseLeft, InputTypes.DeviceIdMouseLeft },
            { InjectedInputTypes.DeviceIdMouseX, InputTypes.DeviceIdMouseX },
            { InjectedInputTypes.DeviceIdMouseY, InputTypes.DeviceIdMouseY }
        };

        public EventHandler StopGameHandler;

        public bool CorePaused { get; set; } = false;
        public string SystemName { get; set; }
        private bool StartStopOperationInProgress { get; set; } = false;

        private Func<bool> RequestedFrameAction { get; set; }
        private TaskCompletionSource<bool> RequestedRunFrameThreadActionTCS { get; set; }

        public LibretroCore currentCore;

        private LibretroCore CurrentCore
        {
            get => currentCore;
            set
            {
                if (currentCore == value)
                {
                    return;
                }

                currentCore = value;

                if (currentCore != null)
                {
                    currentCore.GeometryChanged = videoService.GeometryChanged;
                    currentCore.PixelFormatChanged = videoService.PixelFormatChanged;
                    currentCore.RenderVideoFrame = videoService.RenderVideoFrame;
                    currentCore.TimingsChanged = videoService.TimingsChanged;
                    currentCore.RotationChanged = videoService.RotationChanged;
                    currentCore.TimingsChanged = audioService.TimingChanged;
                    currentCore.RenderAudioFrames = audioService.RenderAudioFrames;
                    currentCore.PollInput = inputService.PollInput;
                    currentCore.GetInputState = inputService.GetInputState;
                    currentCore.OpenFileStream = OnCoreOpenFileStream;
                    currentCore.CloseFileStream = OnCoreCloseFileStream;
                    currentCore.DeleteHandler = VFSDeleteHandler;
                    currentCore.RenameHandler = VFSRenameHandler;
                    currentCore.TruncateHandler = VFSTruncateHandler;
                    currentCore.StatHandler = VFSStatHandler;
                    currentCore.MkdirHandler = VFSMkdirHandler;
                    currentCore.OpendirHandler = VFSOpendirHandler;
                    currentCore.ReaddirHandler = VFSReaddirHandler;
                    currentCore.DirentGetNameHandler = VFSDirentGetNameHandler;
                    currentCore.DirentIsDirHandler = VFSDirentIsDirHandler;
                    currentCore.ClosedirHandler = VFSClosedirHandler;
                    currentCore.StopGame = StopGame;
                }
            }
        }

        public EventHandler GameStarted;
        public EventHandler GameStopped;
        public EventHandler GameLoaded;
        public EventHandler<Exception> GameRuntimeExceptionOccurred;
        public string MainFilePath = "";
        public string MainFileRealPath = "";


        public EmulationService()
        {
            videoService.RequestRunCoreFrame += OnRunFrameRequested;
        }

        public bool isGameLoaded()
        {
            if (CurrentCore != null)
            {
                return !CurrentCore.FailedToLoadGame;
            }
            else
            {
                return false;
            }
        }

        public string EntryPoint { get; set; }
        bool isSingleFile = false;
        StorageFile SingleStorageFile = null;
        public async Task<string> getFileCRC(StorageFile storageFile)
        {
            var fileCRC = Path.GetRandomFileName();
            try
            {
                var checksum = new CRC32Tool();
                using (var sfileItem = await storageFile.OpenAsync(FileAccessMode.Read))
                {
                    fileCRC = await checksum.ComputeHash(sfileItem.AsStream());
                }
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
            }

            return fileCRC;
        }
        public async Task<long> getFileSize(StorageFile storageFile)
        {
            long fileSize = (long)new Random().Next();
            try
            {
                BasicProperties fileAttr = null;
                try
                {
                    fileAttr = await storageFile.GetBasicPropertiesAsync();
                }
                catch (Exception ex)
                {
                    var state = await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, () =>
                    {
                        try
                        {
                            fileAttr = storageFile.GetBasicPropertiesAsync().AsTask().Result;
                        }
                        catch (Exception ecx)
                        {

                        }
                    });
                }

                if (fileAttr != null)
                {
                    fileSize = (long)fileAttr.Size;
                }
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
            }

            return fileSize;
        }
        bool gameIsLoading = false;
        public async Task<bool> StartGameAsync(LibretroCore core, string mainFilePath, string mainFileRealPath, string entryPoint, bool singleFile = false, StorageFile singleStorageFile = null)
        {
            try
            {
                if (StartStopOperationInProgress)
                {
                    return false;
                }
                try
                {
                    if (PlatformService.PreventGCAlways) GC.TryStartNoGCRegion(176400, true);
                }
                catch (Exception ea)
                {
                }
                gameIsLoading = true;
                isSingleFile = singleFile;
                SingleStorageFile = singleStorageFile;

                try
                {
                    await core.InsertLog(-1, "Generating files");

                    //Reset log at startup
                    //await FileIO.WriteTextAsync(logfileLocation, $"");
                    string DateText = DateTime.Now.ToString();
                    WriteLog($"**************{DateText}***************");
                }
                catch (Exception ex)
                {

                }

                StartStopOperationInProgress = true;

                EntryPoint = entryPoint;
                gamesFolderFinal = null;

                await inputService.InitAsync();
                await audioService.InitAsync();
                await videoService.InitAsync();


                MainFilePath = mainFilePath;
                MainFileRealPath = mainFileRealPath;
                CorePaused = false;
                //await Task.Delay(1000);

                var expectedName = $"{core.Name}_{SystemName}";

                CoresOptions TargetSystem = null;
                if (MainFilePath != null && MainFilePath.Length > 0)
                {
                    var GameName = Path.GetFileNameWithoutExtension(MainFileRealPath);
                    TargetSystem = await GameSystemSelectionViewModel.OptionsRetrieveAsync(core.Name, SystemName, core.IsNewCore, GameName);
                }
                if (TargetSystem == null)
                {
                    if (!GameSystemSelectionViewModel.SystemsOptions.TryGetValue(expectedName, out TargetSystem))
                    {
                        if (!core.IsNewCore)
                        {
                            GameSystemSelectionViewModel.SystemsOptions.TryGetValue(SystemName, out TargetSystem);
                        }
                    }
                }

                if (TargetSystem != null)
                {
                    foreach (var optionItem in TargetSystem.OptionsList.Keys)
                    {
                        var optionObject = TargetSystem.OptionsList[optionItem];
                        CoreOption coreOption;
                        if (core.Options.TryGetValue(optionObject.OptionsKey, out coreOption))
                        {
                            core.Options[optionObject.OptionsKey].SelectedValueIx = optionObject.SelectedIndex;
                        }
                    }
                }

                CurrentCore = core;

                await Task.Run(() =>
                {
                    VFSResolver(core).Wait();
                });

                var loadSuccessful = false;

                try
                {
                    await saveStateService.SetSaveStatesFolder(currentCore.savesRootFolder);
                    await saveStateService.SetGameId(GetGameID());
                    //Entry point for archived games
                    var gameMainPath = EntryPoint.Length > 0 ? EntryPoint : MainFileRealPath;
                    if (EntryPoint.Length > 0)
                    {
                        WriteLog($"Game entry point {EntryPoint}");
                    }

                    //First chance
                    loadSuccessful = await RequestFrameActionAsync(() =>
                    {
                        try
                        {
                            return CurrentCore.LoadGame(gameMainPath);
                        }
                        catch (Exception ex)
                        {
                            return false;
                        }
                    });

                    if (core.FailedToLoadGame && core.VFSSupport && !core.RequestNoGame)
                    {
                        //If load failed it could be because of something went wrong in the VFS
                        //We should re-load without VFS support as second chance
                        if (MainFileRealPath != null && MainFileRealPath.Contains(ApplicationData.Current.LocalFolder.Path))
                        {
                            //If the game inside local folder then there is issue, disabling VFS will not help
                            loadSuccessful = false;
                        }
                        else
                        {
                            var secondChanceState = PlatformService.AutoResolveVFSIssues || await PlatformService.AskForSecondChance();
                            if (secondChanceState)
                            {
                                //Second chance
                                core.FailedToLoadGame = false;
                                await Task.Run(() =>
                                {
                                    //Call resolver without VFS
                                    VFSResolver(core, true).Wait();
                                });
                                gameMainPath = EntryPoint.Length > 0 ? EntryPoint : MainFileRealPath;
                                loadSuccessful = await RequestFrameActionAsync(() =>
                                {
                                    try
                                    {
                                        return CurrentCore.LoadGame(gameMainPath, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        return false;
                                    }
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message);
                }


                if (!loadSuccessful)
                {
                    await StopGameAsyncInternal();
                    StartStopOperationInProgress = false;
                    gameIsLoading = false;
                    try
                    {
                        if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                    }
                    catch (Exception ea)
                    {
                    }
                    return loadSuccessful;
                }

                await Task.Delay(PlatformService.isMobile ? 1000 : 500);
                await PauseGameAsync();
                await Task.Delay(PlatformService.isMobile ? 500 : 100);
                await ResumeGameAsync();

                GameStarted?.Invoke(this, EventArgs.Empty);
                GameLoaded?.Invoke(this, EventArgs.Empty);
                StartStopOperationInProgress = false;
                gameIsLoading = false;
                try
                {
                    if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                }
                catch (Exception ea)
                {
                }
                return loadSuccessful;
            }
            catch (Exception e)
            {
                try
                {
                    if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                }
                catch (Exception ea)
                {
                }
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
                gameIsLoading = false;
                return false;
            }
        }

        public async Task VFSResolver(LibretroCore core, bool disabledVFS = false)
        {
            try
            {
                if (PlatformService.PreventGCAlways) GC.TryStartNoGCRegion(176400, true);
            }
            catch (Exception ea)
            {
            }
            try
            {
                var archiveSupport = core.NativeArchiveSupport;
                var archiveNonZipRequestWithNoSupport = false;
                var vfsSupport = core.VFSSupport;
                if (disabledVFS)
                {
                    vfsSupport = false;
                }
                if (MainFileRealPath != null)
                {
                    var fileExt = Path.GetExtension(MainFileRealPath).ToLower();
                    if (ArchiveStreamProvider.SupportedExtensionsNonZip.Contains(fileExt) && !currentCore.NativeArchiveNonZipSupport)
                    {
                        archiveNonZipRequestWithNoSupport = true;
                    }
                }
                if (EntryPoint.Length > 0 && !archiveSupport && !vfsSupport)
                {
                    //When there is no archive support and VFS is not supported also 
                    //The entry point will not work
                    //extarct call and resolve the link here is required
                    try
                    {
                        WriteLog("Archived file requested, core doesn't support ZIP and VFS (resolving the link..)");
                        StorageFolder tempLocation = await CheckTempDir(EntryPoint);
                        if (tempLocation != null)
                        {
                            var fileName = Path.GetFileName(EntryPoint);
                            var testTempFile = (StorageFile)await gameTempDir.TryGetItemAsync(fileName);
                            if (testTempFile == null)
                            {
                                //Expected root folder and the files not directly compressed
                                QueryOptions queryOptions = new QueryOptions();
                                queryOptions.FolderDepth = FolderDepth.Shallow;
                                if (PlatformService.UseWindowsIndexer)
                                {
                                    queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
                                }
                                var folders = gameTempDir.CreateFolderQueryWithOptions(queryOptions);
                                var totalFolders = await folders.GetFoldersAsync();
                                if (totalFolders != null && totalFolders.Count > 0)
                                {
                                    testTempFile = (StorageFile)await totalFolders[0].TryGetItemAsync(fileName);
                                }
                            }
                            if (testTempFile != null)
                            {
                                WriteLog($"Original entry point: {EntryPoint}");
                                EntryPoint = testTempFile.Path;
                                WriteLog($"Entry point resolved to temp folder");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex.Message);
                    }
                }
                else if (EntryPoint.Length > 0 && !archiveSupport && vfsSupport)
                {
                    //This will resolve the case when there is no archive support 
                    //Emulation core will not be able to get entry point even
                    //extarct call and resolve the link here is required
                    try
                    {
                        WriteLog("Archived file requested, core doesn't support ZIP (resolving the link..)");
                        StorageFolder tempLocation = await CheckTempDir(EntryPoint);
                        if (tempLocation != null)
                        {
                            var fileName = Path.GetFileName(EntryPoint);
                            var testTempFile = (StorageFile)await gameTempDir.TryGetItemAsync(fileName);
                            if (testTempFile == null)
                            {
                                //Expected root folder and the files not directly compressed
                                QueryOptions queryOptions = new QueryOptions();
                                queryOptions.FolderDepth = FolderDepth.Shallow;
                                if (PlatformService.UseWindowsIndexer)
                                {
                                    queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
                                }
                                var folders = gameTempDir.CreateFolderQueryWithOptions(queryOptions);
                                var totalFolders = await folders.GetFoldersAsync();
                                if (totalFolders != null && totalFolders.Count > 0)
                                {
                                    testTempFile = (StorageFile)await totalFolders[0].TryGetItemAsync(fileName);
                                }
                            }
                            if (testTempFile != null)
                            {
                                WriteLog($"Original entry point: {EntryPoint}");
                                EntryPoint = testTempFile.Path;
                                WriteLog($"Entry point resolved to temp folder");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex.Message);
                    }
                }
                else if (!vfsSupport && !archiveNonZipRequestWithNoSupport && MainFileRealPath != null && !MainFileRealPath.Contains(ApplicationData.Current.LocalFolder.Path))
                {
                    //We should copy the game to temp folder
                    WriteLog("Core doesn't support VFS (resolving the link..)");
                    try
                    {
                        var coreLocalDir = ApplicationData.Current.LocalFolder;
                        var coreInstallDir = Package.Current.InstalledLocation;
                        var coreGamesDir = await GetGamesFolder();
                        var coreCustomSavesDir = await PlatformService.GetCustomFolder(currentCore.Name, "saves");
                        var coreCustomSystemDir = await PlatformService.GetCustomFolder(currentCore.Name, "system");

                        //Custom saves/system added recently
                        //for easy handling I will just change the coreGamesFolder to the target custom folder if there is match
                        if (coreCustomSavesDir != null && MainFileRealPath.Contains(coreCustomSavesDir.Path))
                        {
                            //Request requires file from custom saves folder
                            coreGamesDir = coreCustomSavesDir;
                            WriteLog($"Request from custom saves folder");
                            WriteLog($"Saves folder forwared to: {coreCustomSavesDir.Path}");
                        }
                        else if (coreCustomSystemDir != null && MainFileRealPath.Contains(coreCustomSystemDir.Path))
                        {
                            //Request requires file from custom system folder
                            coreGamesDir = coreCustomSystemDir;
                            WriteLog($"Request from custom system folder");
                            WriteLog($"System folder forwared to: {coreCustomSystemDir.Path}");
                        }

                        var fileShortPath = MainFileRealPath.Replace($"{coreGamesDir.Path}{(coreGamesDir.Path.EndsWith("\\") ? "" : "\\")}", "");
                        fileShortPath = ResolvePath(fileShortPath);
                        WriteLog(fileShortPath);

                        var file = (isSingleFile ? SingleStorageFile : (StorageFile)await coreGamesDir.TryGetItemAsync(fileShortPath));
                        WriteLog($"Copy target file to temp folder");
                        var tempFolder = await coreLocalDir.TryGetItemAsync("Temporary");
                        if (tempFolder != null)
                        {
                            var testFile = (StorageFile)await ((StorageFolder)tempFolder).TryGetItemAsync(file.Name);
                            long sourceSize = 1;
                            long targetSize = 0;
                            try
                            {
                                sourceSize = await getFileSize(file);
                            }
                            catch (Exception ex)
                            {

                            }
                            if (testFile != null)
                            {
                                targetSize = await getFileSize(testFile);
                            }

                            //The file could be exists but not complete so I have to copy again
                            if (testFile == null || sourceSize != targetSize)
                            {
                                StorageFile dest = null;
                                var state = false;

                                //In some cases if the file very small like 1k it will fail with 'CopyFileWithProgress'
                                //so it's better to handle the small files with 'CopyAsync'
                                if (sourceSize < 1000000)
                                {
                                    dest = await file.CopyAsync((IStorageFolder)tempFolder, file.Name, NameCollisionOption.ReplaceExisting);
                                    if (dest != null)
                                    {
                                        state = true;
                                    }
                                }
                                else
                                {
                                    dest = await ((StorageFolder)tempFolder).CreateFileAsync(file.Name, CreationCollisionOption.ReplaceExisting);
                                    state = await CopyFileWithProgress(file, dest);
                                    //The new file should be verified, in some cases it will be 0 byte
                                    //so if it already failed but not reported I should use the old method
                                    var testSize = await getFileSize(dest);
                                    if (testSize == 0)
                                    {
                                        state = false;
                                        dest = await file.CopyAsync((IStorageFolder)tempFolder, file.Name, NameCollisionOption.ReplaceExisting);
                                        if (dest != null)
                                        {
                                            state = true;
                                        }
                                    }
                                }

                                if (state)
                                {
                                    EntryPoint = dest.Path;
                                }
                                else
                                {
                                    WriteLog($"Failed to copy file");
                                }
                            }
                            else
                            {
                                WriteLog($"File already copied");
                                EntryPoint = testFile.Path;
                            }
                            WriteLog($"New entry point: {EntryPoint}");
                            WriteLog($"Entry point resolved to temp folder");
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex.Message);
                    }
                }
                else if (archiveNonZipRequestWithNoSupport)
                {
                    //This case happens when core support archives but requests 7z,rar..etc
                    //this file should be extracted to the temp
                    try
                    {
                        WriteLog("Archived file (7z,rar,tar) requested, core doesn't support this kind of archives");
                        StorageFolder tempLocation = await CheckTempDir(EntryPoint);
                        if (tempLocation != null)
                        {
                            var fileName = Path.GetFileName(EntryPoint);
                            var testTempFile = (StorageFile)await gameTempDir.TryGetItemAsync(fileName);
                            if (testTempFile == null)
                            {
                                //Expected root folder and the files not directly compressed
                                QueryOptions queryOptions = new QueryOptions();
                                queryOptions.FolderDepth = FolderDepth.Shallow;
                                if (PlatformService.UseWindowsIndexer)
                                {
                                    queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
                                }
                                var folders = gameTempDir.CreateFolderQueryWithOptions(queryOptions);
                                var totalFolders = await folders.GetFoldersAsync();
                                if (totalFolders != null && totalFolders.Count > 0)
                                {
                                    testTempFile = (StorageFile)await totalFolders[0].TryGetItemAsync(fileName);
                                }
                            }
                            if (testTempFile != null)
                            {
                                WriteLog($"Original entry point: {EntryPoint}");
                                EntryPoint = testTempFile.Path;
                                WriteLog($"Entry point resolved to temp folder");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
            try
            {
                if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
            }
            catch (Exception ea)
            {
            }
        }
        public async Task<bool> CopyFileWithProgress(StorageFile src, StorageFile dest)
        {
            try
            {
                CachedFileManager.DeferUpdates(dest);
                using (var destStream = await dest.OpenAsync(FileAccessMode.ReadWrite))
                using (var srcStream = await src.OpenAsync(FileAccessMode.Read))
                {
                    await TransferTo(srcStream.AsStream(), destStream.AsStream(), src);
                    try
                    {

                        if (PlatformService.PreventGCAlways)
                        {
                            destStream.Dispose();
                            srcStream.Dispose();
                            GC.SuppressFinalize(destStream);
                            GC.SuppressFinalize(srcStream);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(dest);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<long> TransferTo(Stream source, Stream destination, StorageFile file)
        {
            try
            {
                if (PlatformService.PreventGCAlways) GC.TryStartNoGCRegion(176400, true);
            }
            catch (Exception ea)
            {
            }
            byte[] array = GetTransferByteArray();
            try
            {
                var fileSize = (long)(await file.GetBasicPropertiesAsync()).Size;
                var iterations = 0;
                long total = 0;
                while (ReadTransferBlock(source, array, out int count))
                {
                    total += count;
                    await destination.WriteAsync(array, 0, count);
                    iterations++;
                    FramebufferConverter.currentFileEntry = file.Name;
                    var totalCopied = (total * 1d) / (fileSize * 1d) * 100;
                    FramebufferConverter.currentFileProgress = totalCopied;
                }
                try
                {
                    if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                }
                catch (Exception ea)
                {
                }
                return total;
            }
            finally
            {
                try
                {
                    if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                }
                catch (Exception ea)
                {
                }
                ArrayPool<byte>.Shared.Return(array);
            }
        }
        private byte[] GetTransferByteArray()
        {
            return ArrayPool<byte>.Shared.Rent(81920);
        }
        private bool ReadTransferBlock(Stream source, byte[] array, out int count)
        {
            return (count = source.Read(array, 0, array.Length)) != 0;
        }

        public void UpdateCoreOption(string optionName, uint optionValue)
        {
            if (CurrentCore != null)
            {
                CurrentCore.Options[optionName].SelectedValueIx = optionValue;
                CurrentCore.UpdateCoreOptions();
            }
        }
        public void UpdateCoreDebugState(bool debugState)
        {
            if (currentCore != null)
            {
                currentCore.EnabledDebugLog = debugState;
            }
        }
        public void UpdateCoreDebugFile(bool debugState)
        {
            if (currentCore != null)
            {
                currentCore.EnabledLogFile = debugState;
            }
        }

        public async Task<StorageFile> GetLogFileLocation()
        {
            if (currentCore != null)
            {
                return await currentCore.GetLogFile();
            }
            return null;
        }
        public async Task<StorageFile> GetLogVFSFileLocation()
        {
            if (currentCore != null)
            {
                return await currentCore.GetLogFile(true);
            }
            return null;
        }
        public uint GetCoreOptionValue(string optionName)
        {
            if (CurrentCore != null)
            {
                return CurrentCore.Options[optionName].SelectedValueIx;
            }
            else
            {
                return 0;
            }
        }

        public ObservableCollection<CoreLogItem> GetCoreLogsList()
        {
            if (CurrentCore != null)
            {
                return CurrentCore.GetLogsList();
            }
            else
            {
                return null;
            }
        }

        public IDictionary<string, CoreOption> GetCoreOptions()
        {
            if (CurrentCore != null)
            {
                return CurrentCore.Options;
            }
            else
            {
                return null;
            }
        }
        public Task ResetGameAsync()
        {
            try
            {
                return RequestFrameActionAsync(() =>
                {
                    CurrentCore?.Reset();
                });
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
                return null;
            }
        }

        public async Task StopGameAsync()
        {
            try
            {
                if (StartStopOperationInProgress)
                {
                    return;
                }

                StartStopOperationInProgress = true;
                await StopGameAsyncInternal();
                GameStopped?.Invoke(this, EventArgs.Empty);
                StartStopOperationInProgress = false;
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
                return;
            }
        }

        private async Task StopGameAsyncInternal()
        {
            try
            {
                await RequestFrameActionAsync(() =>
                {
                    CurrentCore?.UnloadGame();
                    CurrentCore.FreeLibretroCore();
                    CurrentCore = null;
                });

                saveStateService.SetGameId(null);

                await inputService.DeinitAsync();
                await audioService.DeinitAsync();
                await videoService.DeinitAsync();
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
        }

        public Task PauseGameAsync()
        {
            try
            {
                return SetCorePaused(true);
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
                return null;
            }
        }

        public Task ResumeGameAsync()
        {
            try
            {
                return SetCorePaused(false);
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
                return null;
            }
        }

        private async Task SetCorePaused(bool value)
        {
            try
            {
                if (value)
                {
                    await Task.Run(() => audioService.Stop());
                }

                CorePaused = value;
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
        }

        public bool IsNewCore()
        {
            try
            {
                return currentCore.IsNewCore;
            }
            catch (Exception e)
            {
                return true;
            }
        }
        public bool IsCoreReady()
        {
            return currentCore != null;
        }
        public string GetGameID()
        {
            if (MainFilePath == null)
            {
                //When core started without content, there will be no main game path
                //Let's set the id to core's name md5
                return currentCore.Name.MD5();
            }
            try
            {
                string gameID = PlatformService.GetGameIDByLocation(currentCore.Name, SystemName, MainFilePath, currentCore.IsNewCore);
                return gameID;
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
                return null;
            }
        }
        public string GetGameName()
        {
            return MainFilePath;
        }
        public async Task<bool> SaveGameStateAsync(uint slotID, bool showMessage = true)
        {
            try
            {
                try
                {
                    if (PlatformService.PreventGCAlways) GC.TryStartNoGCRegion(176400, true);
                }
                catch (Exception ea)
                {
                }
                var success = false;
                var file = await saveStateService.GetStreamForSlotAsync(slotID, FileAccessMode.ReadWrite);
                if (file != null)
                {
                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        if (stream == null)
                        {
                            return success;
                        }

                        success = await RequestFrameActionAsync(() =>
                        {
                            if (CurrentCore == null)
                            {
                                return false;
                            }

                            return CurrentCore.SaveState(stream.AsStream());
                        });

                        await stream.FlushAsync();
                        try
                        {
                            if (PlatformService.PreventGCAlways)
                            {
                                stream.Dispose();
                                GC.SuppressFinalize(stream);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                if (success)
                {
                    if (showMessage)
                    {
                        PlatformService.PlayNotificationSound("save-state");
                    }
                }
                else
                {
                    if (file != null)
                    {
                        await file.DeleteAsync();
                    }
                }
                try
                {
                    if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                }
                catch (Exception ea)
                {
                }
                return success;
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
                return false;
            }
        }


        public async Task<bool> LoadGameStateAsync(uint slotID)
        {
            try
            {
                var success = false;
                var file = await saveStateService.GetStreamForSlotAsync(slotID, FileAccessMode.Read);
                if (file != null)
                {
                    using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        if (stream == null)
                        {
                            return false;
                        }

                        success = await RequestFrameActionAsync(() =>
                        {
                            if (CurrentCore == null)
                            {
                                return false;
                            }

                            return CurrentCore.LoadState(stream.AsStream());
                        });
                    }
                }

                return success;
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
                return false;
            }
        }

        public void SetFPSCounterState(bool FPSCounterState)
        {
            try
            {
                if (CurrentCore != null)
                {
                    CurrentCore.ShowFPSCounter = FPSCounterState;
                }
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
        }

        public int GetFPSCounterValue()
        {
            try
            {
                if (CurrentCore != null)
                {
                    return CurrentCore.FrameRate;
                }
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
            return 0;
        }
        public void SetVideoOnlyState(bool VideoOnlyState)
        {
            try
            {
                if (CurrentCore != null)
                {
                    CurrentCore.VideoOnly = VideoOnlyState;
                }
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
        }

        public void SetAudioOnlyState(bool AudioOnlyState)
        {
            try
            {
                if (CurrentCore != null)
                {
                    CurrentCore.AudioOnly = AudioOnlyState;
                }
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
        }

        public void SetSkipFramesState(bool SkipFramesState)
        {
            try
            {
                if (CurrentCore != null)
                {
                    CurrentCore.NativeSkipFrames = SkipFramesState;
                }
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
        }
        public void SetSkipFramesRandomState(bool SkipFramesState)
        {
            try
            {
                if (CurrentCore != null)
                {
                    CurrentCore.NativeSkipFramesRandom = SkipFramesState;
                }
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
        }
        public void InjectInputPlayer1(InjectedInputTypes inputType, bool forceState)
        {
            try
            {
                inputService.InjectInputPlayer1(InjectedInputMapping[inputType], forceState);
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
        }

        private Task<bool> RequestFrameActionAsync(Action action)
        {
            try
            {
                return RequestFrameActionAsync(() =>
                {
                    action.Invoke();
                    return true;
                });
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
                return null;
            }
        }

        private Task<bool> RequestFrameActionAsync(Func<bool> action)
        {
            try
            {
                if (RequestedFrameAction != null)
                {
                    return Task.FromResult(false);
                }

                RequestedFrameAction = action;
                RequestedRunFrameThreadActionTCS = new TaskCompletionSource<bool>();
                return RequestedRunFrameThreadActionTCS.Task;
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
                return null;
            }
        }

        //Synhronous since it's going to be called by a non UI thread
        private void OnRunFrameRequested(object sender, EventArgs args)
        {
            try
            {
                if (RequestedFrameAction != null)
                {
                    RequestedRunFrameThreadActionTCS.SetResult(RequestedFrameAction.Invoke());
                    RequestedFrameAction = null;
                    RequestedRunFrameThreadActionTCS = null;
                    return;
                }

                if (CurrentCore == null || gameIsLoading || CorePaused || audioService.ShouldDelayNextFrame || CurrentCore.FailedToLoadGame)
                {
                    return;
                }

                try
                {
                    CurrentCore?.RunFrame();
                }
                catch (Exception e)
                {
                    if (!StartStopOperationInProgress)
                    {
                        StartStopOperationInProgress = true;
                        StopGameAsyncInternal().Wait();
                        StartStopOperationInProgress = false;
                    }

                    var task = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, () => GameRuntimeExceptionOccurred?.Invoke(this, e));
                }
            }
            catch (Exception e)
            {
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
        }

        private void StopGame()
        {
            if (StopGameHandler != null)
            {
                StopGameHandler.Invoke(null, null);
            }
        }

        public bool vfsLogFile = false;
        public bool VFSLogFile
        {
            get
            {
                return vfsLogFile;
            }
            set
            {
                vfsLogFile = value;
            }
        }

        public EventHandler GameInfoUpdater;

        bool firstCall = true;
        StorageFolder gameTempDir;
        bool filesExtracted = false;
        List<string> archiveEntries = new List<string>();
        string folderPickerTokenName
        {
            get
            {
                return SystemName;
            }
        }

        //This variable will store the final returned result from GetGamesFolder
        //to avoid any reselect dialog or extra tasks
        StorageFolder gamesFolderFinal = null;
        private async Task<StorageFolder> GetGamesFolder()
        {
            if (gamesFolderFinal != null)
            {
                return gamesFolderFinal;
            }

            try
            {
                if (PlatformService.PreventGCAlways) GC.TryStartNoGCRegion(176400, true);
            }
            catch (Exception ea)
            {
            }
            //Get system dir in case the core doesn't require games folder (and not selected)
            //or task failed
            StorageFolder systemDir = currentCore.systemRootFolder;

            var systemCustomFolderTest = await PlatformService.GetCustomFolder(currentCore.Name, "system");

            if (systemCustomFolderTest != null)
            {
                systemDir = systemCustomFolderTest;
            }

            try
            {
                var gamesFolderReady = false;
                if (currentCore != null)
                {
                    if (!await PlatformService.IsCoreGamesFolderAlreadySelected(currentCore.SystemName))
                    {
                        gamesFolderReady = false;
                    }
                    else
                    {
                        gamesFolderReady = true;
                    }
                    var globalTest = await PlatformService.PickDirectory(folderPickerTokenName, false, true, true);
                    if (globalTest != null)
                    {
                        gamesFolderReady = true;
                    }
                }

                bool ignoreGamesFolderSelection = currentCore != null && currentCore.ignoreGamesFolderSelection && !gamesFolderReady;

                if (!await IsCoreGamesFolderAlreadySelected(folderPickerTokenName))
                {
                    if (IsCoreRequiredGamesFolder(currentCore.Name))
                    {
                        StorageFolder pickedFolder = !ignoreGamesFolderSelection ? await PickDirectory(folderPickerTokenName, false) : null;
                        if (pickedFolder != null)
                        {
                            gamesFolderFinal = pickedFolder;
                            try
                            {
                                if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                            }
                            catch (Exception ea)
                            {
                            }
                            return pickedFolder;
                        }
                        else
                        {
                            gamesFolderFinal = systemDir;
                            try
                            {
                                if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                            }
                            catch (Exception ea)
                            {
                            }
                            return systemDir;
                        }
                    }
                    else
                    {
                        try
                        {
                            var globalTest = await PlatformService.PickDirectory(folderPickerTokenName, false, true, true);
                            if (globalTest != null)
                            {
                                gamesFolderFinal = globalTest;
                                try
                                {
                                    if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                                }
                                catch (Exception ea)
                                {
                                }
                                return globalTest;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        gamesFolderFinal = systemDir;
                        try
                        {
                            if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                        }
                        catch (Exception ea)
                        {
                        }
                        return systemDir;
                    }
                }
                else
                {
                    StorageFolder restoredFolder = !ignoreGamesFolderSelection ? await PickDirectory(folderPickerTokenName, false) : null;
                    if (restoredFolder != null)
                    {
                        gamesFolderFinal = restoredFolder;
                        try
                        {
                            if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                        }
                        catch (Exception ea)
                        {
                        }
                        return restoredFolder;
                    }
                    else
                    {
                        gamesFolderFinal = systemDir;
                        try
                        {
                            if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                        }
                        catch (Exception ea)
                        {
                        }
                        return systemDir;
                    }
                }
            }
            catch (Exception ex)
            {
                //I'm not going to store any result in 'restoredFolder'
                //to give another chance in case something went wrong
                WriteLog(ex.Message);
                try
                {
                    if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                }
                catch (Exception ea)
                {
                }
                return systemDir;
            }
        }


        //Regarding to archived games..
        //This function will decide if we need to extract the game to temp folder
        //Every path will be requested will be redirect to the temp folder
        //Q: Why I dropped the stream provider from memory?
        //A: it's not very helpful when the game contains multiple files/folders and it causes high memory usage
        //   before.. each game will take double size in the memory, now the extract method will take more time and more disk space but it's more safe
        //   and it will be accurate for other functions like VFSOpenDir.
        private async Task<StorageFolder> CheckTempDir(string path)
        {
            StorageFolder tempLocation = null;
            try
            {
                if (PlatformService.PreventGCAlways) GC.TryStartNoGCRegion(176400, true);
            }
            catch (Exception ea)
            {
            }
            try
            {
                var coreLocalDir = ApplicationData.Current.LocalFolder;
                var coreInstallDir = Package.Current.InstalledLocation;
                var coreGamesDir = Task.Run(() => GetGamesFolder()).Result;

                var coreCustomSavesDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "saves")).Result;
                var coreCustomSystemDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "system")).Result;

                //For now no need to redirect any request from Local/Installation folder
                //Very rare cases you could need to extract archive from there
                //Only Games folder could contains archived games that need to be resolved if the core doesn't support archives
                if (!path.ToLower().Contains(coreLocalDir.Path.ToLower()) && !path.ToLower().Contains(coreInstallDir.Path.ToLower()) && MainFileRealPath != null && !path.ToLower().Equals(coreGamesDir.Path.ToLower()))
                {

                    //Custom saves/system added recently
                    //for easy handling I will just change the coreGamesFolder to the target custom folder if there is match
                    if (coreCustomSavesDir != null && MainFileRealPath.Contains(coreCustomSavesDir.Path))
                    {
                        //Request requires file from custom saves folder
                        coreGamesDir = coreCustomSavesDir;
                        WriteLog($"Request from custom saves folder");
                        WriteLog($"Saves folder forwared to: {coreCustomSavesDir.Path}");
                    }
                    else if (coreCustomSystemDir != null && MainFileRealPath.Contains(coreCustomSystemDir.Path))
                    {
                        //Request requires file from custom system folder
                        coreGamesDir = coreCustomSystemDir;
                        WriteLog($"Request from custom system folder");
                        WriteLog($"System folder forwared to: {coreCustomSystemDir.Path}");
                    }

                    //Verify core archive support (in case file compressed)
                    var fileExt = Path.GetExtension(MainFileRealPath).ToLower();
                    var archiveFile = ArchiveStreamProvider.SupportedExtensions.Contains(fileExt);
                    var coreArchiveSupport = currentCore.NativeArchiveSupport;
                    //Core maybe support archive but no always supports 7z, rar..etc
                    if (ArchiveStreamProvider.SupportedExtensionsNonZip.Contains(fileExt) && !currentCore.NativeArchiveNonZipSupport)
                    {
                        coreArchiveSupport = false;
                    }
                    if (archiveFile && !coreArchiveSupport)
                    {
                        bool fileRequestedFromArchive = false;
                        if (filesExtracted)
                        {
                            foreach (var archiveEntry in archiveEntries)
                            {
                                if (path.Contains(archiveEntry))
                                {
                                    fileRequestedFromArchive = true;
                                    break;
                                }
                            }
                        }
                        if (!firstCall && filesExtracted)
                        {
                            if (fileRequestedFromArchive)
                            {
                                WriteLog($"Auto redirect to temp dir {gameTempDir.Path}");
                                try
                                {
                                    if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                                }
                                catch (Exception ea)
                                {
                                }
                                return gameTempDir;
                            }
                            else
                            {
                                WriteLog($"This file is not part of archive");
                            }
                        }
                        if (firstCall || !filesExtracted)
                        {
                            firstCall = false;
                            try
                            {
                                //Get or create temp dir
                                var tempDir = (StorageFolder)Task.Run(() => coreLocalDir.TryGetItemAsync("Temporary").AsTask()).Result;
                                if (tempDir == null)
                                {
                                    tempDir = (StorageFolder)Task.Run(() => coreLocalDir.TryGetItemAsync("Temporary").AsTask()).Result;
                                }
                                if (tempDir != null)
                                {
                                    var gameID = GetGameID();
                                    if (gameID == null)
                                    {
                                        gameID = currentCore.SystemName;
                                    }
                                    gameTempDir = (StorageFolder)Task.Run(() => tempDir.TryGetItemAsync(gameID).AsTask()).Result;
                                    if (gameTempDir == null)
                                    {
                                        gameTempDir = Task.Run(() => tempDir.CreateFolderAsync(gameID).AsTask()).Result;
                                    }
                                    if (gameTempDir != null)
                                    {
                                        tempLocation = gameTempDir;
                                        WriteLog($"Temporary dir set to: {gameTempDir.Path}");

                                        //Archive short path
                                        var fileShortPath = MainFileRealPath.Replace($"{coreGamesDir.Path}{(coreGamesDir.Path.EndsWith("\\") ? "" : "\\")}", "");
                                        fileShortPath = ResolvePath(fileShortPath);
                                        WriteLog(fileShortPath);

                                        try
                                        {
                                            //Check if files already extreacted
                                            //In case of any issue user can disabled then re-enable clean temp to delete files without closing the app
                                            var testFile = (StorageFile)await gameTempDir.TryGetItemAsync($"extractDone.temp");
                                            if (testFile != null)
                                            {
                                                filesExtracted = true;
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                        if (!filesExtracted)
                                        {
                                            archiveEntries.Clear();
                                            var file = (isSingleFile ? SingleStorageFile : (StorageFile)Task.Run(() => coreGamesDir.TryGetItemAsync(fileShortPath).AsTask()).Result);
                                            var archiveProvider = new ArchiveStreamProvider(file);
                                            filesExtracted = await archiveProvider.ExtractArchive($"Temporary\\{gameTempDir.Name}", archiveEntries);
                                            await gameTempDir.CreateFileAsync("extractDone.temp", CreationCollisionOption.ReplaceExisting);
                                            archiveProvider.Dispose();
                                        }
                                        if (filesExtracted)
                                        {
                                            //Expected to request sub file/folder from the archive
                                            //No action required it will be handled by the caller function
                                            WriteLog("Archive sub file/folder requested");
                                        }
                                        else
                                        {
                                            WriteLog("Failed to extract files");
                                        }
                                    }
                                    else
                                    {
                                        WriteLog($"ERROR: Unable to create game temp folder {gameID}");
                                    }
                                }
                                else
                                {
                                    WriteLog("ERROR: Unable to create Temporary folder");
                                }
                            }
                            catch (Exception ex)
                            {
                                WriteLog(ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
            try
            {
                if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
            }
            catch (Exception ea)
            {
            }
            return tempLocation;
        }

        private string ResolveMainPath(string path)
        {
            var tempPath = path;
            if (!path.Contains("\\") && !path.Contains("/"))
            {
                //The core is requesting file without full path
                //If the file name matched with entry point 
                //Then file path should be changed to games dir
                //Otherwise I should redirect the request to core's system folder
                if (EntryPoint.Length > 0 && EntryPoint.Equals(path))
                {
                    path = $"{currentCore.SystemGamesPath}\\{path}";
                }
                else
                {
                    path = $"{currentCore.SystemRootPath}\\{path}";
                }
            }

            path = ResolveBrokenPath(path);
            if (!path.Equals(tempPath))
            {
                WriteLog($"Path changed from {tempPath} to {path}");
            }
            return path;
        }
        private Stream OnCoreOpenFileStream(string path, FileAccessMode fileAccess)
        {
            if (path == null)
            {
                return null;
            }
            try
            {
                if (PlatformService.PreventGCAlways) GC.TryStartNoGCRegion(176400, true);
            }
            catch (Exception ea)
            {
            }
            WriteLog(path);
            path = ResolveMainPath(path);
            StorageFolder tempLocation = Task.Run(() => CheckTempDir(path)).Result;
            var isDir = IsDir(path, tempLocation);

            try
            {
                var coreLocalDir = ApplicationData.Current.LocalFolder;
                var coreInstallDir = Package.Current.InstalledLocation;
                var coreGamesDir = Task.Run(() => GetGamesFolder()).Result;
                var coreCustomSavesDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "saves")).Result;
                var coreCustomSystemDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "system")).Result;

                //Custom saves/system added recently
                //for easy handling I will just change the coreGamesFolder to the target custom folder if there is match
                if (coreCustomSavesDir != null && path.Contains(coreCustomSavesDir.Path))
                {
                    //Request requires file from custom saves folder
                    coreGamesDir = coreCustomSavesDir;
                    WriteLog($"Request from custom saves folder");
                    WriteLog($"Saves folder forwared to: {coreCustomSavesDir.Path}");
                }
                else if (coreCustomSystemDir != null && path.Contains(coreCustomSystemDir.Path))
                {
                    //Request requires file from custom system folder
                    coreGamesDir = coreCustomSystemDir;
                    WriteLog($"Request from custom system folder");
                    WriteLog($"System folder forwared to: {coreCustomSystemDir.Path}");
                }

                if (path.ToLower().Contains(coreLocalDir.Path.ToLower()))
                {
                    var fileShortPath = path.Replace($"{coreLocalDir.Path}\\", "");
                    fileShortPath = ResolvePath(fileShortPath);
                    WriteLog(fileShortPath);
                    try
                    {
                        //With TryGetItemAsync I can detect the item type, I will use another check here 
                        var testType = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (testType != null)
                        {
                            isDir = testType.IsOfType(StorageItemTypes.Folder);
                            WriteLog($"Path type double check: isDir->{isDir}");
                        }
                        else
                        {
                            WriteLog($"Path type double check: isDir->File/Folder unable to check (not exists), current state isDir->{isDir}");
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"Error: {ex.Message}");
                    }

                    //Dir will be ignored, cannot open stream for dir
                    if (!isDir)
                    {
                        var targetFile = (StorageFile)Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (targetFile != null)
                        {
                            var stream = Task.Run(() => targetFile.OpenAsync(fileAccess).AsTask()).Result;
                            try
                            {
                                if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                            }
                            catch (Exception ea)
                            {
                            }
                            return stream.AsStream();
                        }
                    }
                    else
                    {
                        WriteLog($"Skipped: Cannot open stream for folder");
                    }
                }
                else if (path.ToLower().Contains(coreInstallDir.Path.ToLower()))
                {
                    //No write allowed for installation dir, this could lead to issue
                    //I will redirect the request to local-system dir instead
                    var systemDirName = Path.GetFileName(currentCore.SystemRootPath);
                    var fileShortPath = path.Replace($"{coreInstallDir.Path}\\", $"{systemDirName}\\");
                    fileShortPath = ResolvePath(fileShortPath);
                    WriteLog("Path resolved to system dir instead");
                    WriteLog(fileShortPath);
                    try
                    {
                        //With TryGetItemAsync I can detect the item type, I will use another chech here 
                        var testType = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (testType != null)
                        {
                            isDir = testType.IsOfType(StorageItemTypes.Folder);
                            WriteLog($"Path type double check: isDir->{isDir}");
                        }
                        else
                        {
                            WriteLog($"Path type double check: isDir->File/Folder unable to check (not exists), current state isDir->{isDir}");
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    //Dir will be ignored, cannot open stream for dir
                    if (!isDir)
                    {
                        var targetFile = (StorageFile)Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (targetFile != null)
                        {
                            var stream = Task.Run(() => targetFile.OpenAsync(fileAccess).AsTask()).Result;
                            try
                            {
                                if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                            }
                            catch (Exception ea)
                            {
                            }
                            return stream.AsStream();
                        }
                    }
                    else
                    {
                        WriteLog($"Skipped: Cannot open stream for folder");
                    }
                }
                else if (path.ToLower().Contains(coreGamesDir.Path.ToLower()))
                {
                    var fileShortPath = path.Replace($"{coreGamesDir.Path}{(coreGamesDir.Path.EndsWith("\\") ? "" : "\\")}", "");
                    fileShortPath = ResolvePath(fileShortPath);
                    WriteLog(fileShortPath);
                    try
                    {
                        //With TryGetItemAsync I can detect the item type, I will use another chech here 
                        var testType = Task.Run(() => (tempLocation != null ? tempLocation : coreGamesDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (testType != null)
                        {
                            isDir = testType.IsOfType(StorageItemTypes.Folder);
                            WriteLog($"Path type double check: isDir->{isDir}");
                        }
                        else
                        {
                            WriteLog($"Path type double check: isDir->File/Folder unable to check (not exists), current state isDir->{isDir}");
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    //Dir will be ignored, cannot open stream for dir
                    if (!isDir)
                    {
                        var targetFile = (StorageFile)Task.Run(() => (tempLocation != null ? tempLocation : coreGamesDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (targetFile != null)
                        {
                            var stream = Task.Run(() => targetFile.OpenAsync(fileAccess).AsTask()).Result;
                            try
                            {
                                if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                            }
                            catch (Exception ea)
                            {
                            }
                            return stream.AsStream();
                        }
                    }
                    else
                    {
                        WriteLog($"Skipped: Cannot open stream for folder");
                    }
                }
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
            WriteLog($"ERROR: not able to get {path}");
            try
            {
                if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
            }
            catch (Exception ea)
            {
            }
            return null;
        }

        private void OnCoreCloseFileStream(Stream stream)
        {
            try
            {
                WriteLog("Closing stream");
                try
                {
                    stream?.Dispose();
                }
                catch (Exception e)
                {

                }
                if (stream != null)
                {
                    if (PlatformService.PreventGCAlways) GC.SuppressFinalize(stream);
                }
                stream = null;
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
        }

        private int VFSDeleteHandler(string path)
        {
            if (path == null)
            {
                return -1;
            }
            WriteLog(path);
            path = ResolveMainPath(path);
            StorageFolder tempLocation = Task.Run(() => CheckTempDir(path)).Result;
            try
            {
                bool isDir = IsDir(path, tempLocation);

                var coreLocalDir = ApplicationData.Current.LocalFolder;
                var coreInstallDir = Package.Current.InstalledLocation;
                var coreGamesDir = Task.Run(() => GetGamesFolder()).Result;
                var coreCustomSavesDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "saves")).Result;
                var coreCustomSystemDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "system")).Result;

                //Custom saves/system added recently
                //for easy handling I will just change the coreGamesFolder to the target custom folder if there is match
                if (coreCustomSavesDir != null && path.Contains(coreCustomSavesDir.Path))
                {
                    //Request requires file from custom saves folder
                    coreGamesDir = coreCustomSavesDir;
                    WriteLog($"Request from custom saves folder");
                    WriteLog($"Saves folder forwared to: {coreCustomSavesDir.Path}");
                }
                else if (coreCustomSystemDir != null && path.Contains(coreCustomSystemDir.Path))
                {
                    //Request requires file from custom system folder
                    coreGamesDir = coreCustomSystemDir;
                    WriteLog($"Request from custom system folder");
                    WriteLog($"System folder forwared to: {coreCustomSystemDir.Path}");
                }

                if (path.ToLower().Contains(coreLocalDir.Path.ToLower()))
                {
                    var fileShortPath = path.Replace($"{coreLocalDir.Path}\\", "");
                    fileShortPath = ResolvePath(fileShortPath);
                    WriteLog(fileShortPath);
                    try
                    {
                        //With TryGetItemAsync I can detect the item type, I will use another chech here 
                        var testType = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (testType != null)
                        {
                            isDir = testType.IsOfType(StorageItemTypes.Folder);
                            WriteLog($"Path type double check: isDir->{isDir}");
                        }
                        else
                        {
                            WriteLog($"Path type double check: isDir->File/Folder unable to check (not exists), current state isDir->{isDir}");
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    if (!isDir)
                    {
                        var targetFile = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (targetFile != null)
                        {
                            Task.Run(() => targetFile.DeleteAsync()).Wait();
                            return 0;
                        }
                    }
                    else
                    {
                        var targetFolder = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (targetFolder != null)
                        {
                            Task.Run(() => targetFolder.DeleteAsync()).Wait();
                            return 0;
                        }
                    }
                }
                else if (path.ToLower().Contains(coreInstallDir.Path.ToLower()))
                {
                    //No write allowed for installation dir, this could lead to issue
                    //I will redirect the request to local-system dir instead
                    var systemDirName = Path.GetFileName(currentCore.SystemRootPath);
                    var fileShortPath = path.Replace($"{coreInstallDir.Path}\\", $"{systemDirName}\\");
                    fileShortPath = ResolvePath(fileShortPath);
                    WriteLog("Path resolved to system dir instead");
                    WriteLog(fileShortPath);

                    try
                    {
                        //With TryGetItemAsync I can detect the item type, I will use another chech here 
                        var testType = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (testType != null)
                        {
                            isDir = testType.IsOfType(StorageItemTypes.Folder);
                            WriteLog($"Path type double check: isDir->{isDir}");
                        }
                        else
                        {
                            WriteLog($"Path type double check: isDir->File/Folder unable to check (not exists), current state isDir->{isDir}");
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    if (!isDir)
                    {
                        var targetFile = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (targetFile != null)
                        {
                            Task.Run(() => targetFile.DeleteAsync()).Wait();
                            return 0;
                        }
                    }
                    else
                    {
                        var targetFolder = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (targetFolder != null)
                        {
                            Task.Run(() => targetFolder.DeleteAsync()).Wait();
                            return 0;
                        }
                    }
                }
                else if (path.ToLower().Contains(coreGamesDir.Path.ToLower()))
                {
                    var fileShortPath = path.Replace($"{coreGamesDir.Path}{(coreGamesDir.Path.EndsWith("\\") ? "" : "\\")}", "");
                    fileShortPath = ResolvePath(fileShortPath);
                    WriteLog(fileShortPath);

                    try
                    {
                        //With TryGetItemAsync I can detect the item type, I will use another chech here 
                        var testType = Task.Run(() => (tempLocation != null ? tempLocation : coreGamesDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (testType != null)
                        {
                            isDir = testType.IsOfType(StorageItemTypes.Folder);
                            WriteLog($"Path type double check: isDir->{isDir}");
                        }
                        else
                        {
                            WriteLog($"Path type double check: isDir->File/Folder unable to check (not exists), current state isDir->{isDir}");
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    if (!isDir)
                    {
                        var targetFile = Task.Run(() => (tempLocation != null ? tempLocation : coreGamesDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (targetFile != null)
                        {
                            Task.Run(() => targetFile.DeleteAsync()).Wait();
                            return 0;
                        }
                    }
                    else
                    {
                        var targetFolder = Task.Run(() => (tempLocation != null ? tempLocation : coreGamesDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (targetFolder != null)
                        {
                            Task.Run(() => targetFolder.DeleteAsync()).Wait();
                            return 0;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
            WriteLog("ERROR: Unable to delete file");
            return -1;
        }

        private int VFSRenameHandler(string path, string newPath)
        {
            if (path == null)
            {
                return -1;
            }
            WriteLog(path);
            path = ResolveMainPath(path);
            StorageFolder tempLocation = Task.Run(() => CheckTempDir(path)).Result;
            bool isDir = IsDir(path, tempLocation);

            try
            {
                var coreLocalDir = ApplicationData.Current.LocalFolder;
                var coreInstallDir = Package.Current.InstalledLocation;
                var coreGamesDir = Task.Run(() => GetGamesFolder()).Result;
                var coreCustomSavesDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "saves")).Result;
                var coreCustomSystemDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "system")).Result;

                //Custom saves/system added recently
                //for easy handling I will just change the coreGamesFolder to the target custom folder if there is match
                if (coreCustomSavesDir != null && path.Contains(coreCustomSavesDir.Path))
                {
                    //Request requires file from custom saves folder
                    coreGamesDir = coreCustomSavesDir;
                    WriteLog($"Request from custom saves folder");
                    WriteLog($"Saves folder forwared to: {coreCustomSavesDir.Path}");
                }
                else if (coreCustomSystemDir != null && path.Contains(coreCustomSystemDir.Path))
                {
                    //Request requires file from custom system folder
                    coreGamesDir = coreCustomSystemDir;
                    WriteLog($"Request from custom system folder");
                    WriteLog($"System folder forwared to: {coreCustomSystemDir.Path}");
                }

                if (path.ToLower().Contains(coreLocalDir.Path.ToLower()))
                {
                    var fileShortPath = path.Replace($"{coreLocalDir.Path}\\", "");
                    fileShortPath = ResolvePath(fileShortPath);
                    WriteLog(fileShortPath);

                    try
                    {
                        //With TryGetItemAsync I can detect the item type, I will use another chech here 
                        var testType = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (testType != null)
                        {
                            isDir = testType.IsOfType(StorageItemTypes.Folder);
                            WriteLog($"Path type double check: isDir->{isDir}");
                        }
                        else
                        {
                            WriteLog($"Path type double check: isDir->File/Folder unable to check (not exists), current state isDir->{isDir}");
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    if (!isDir)
                    {
                        var targetFile = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        var newName = Path.GetFileName(newPath);
                        if (targetFile != null)
                        {
                            Task.Run(() => targetFile.RenameAsync(newName)).Wait();
                            return 0;
                        }
                    }
                    else
                    {
                        var targetFolder = Task.Run(() => GetFolder((tempLocation != null ? tempLocation : coreLocalDir), fileShortPath)).Result;
                        var newName = Path.GetFileName(newPath);
                        if (targetFolder != null)
                        {
                            Task.Run(() => targetFolder.RenameAsync(newName)).Wait();
                            return 0;
                        }
                    }
                }
                else if (path.ToLower().Contains(coreInstallDir.Path.ToLower()))
                {
                    //No write allowed for installation dir, this could lead to issue
                    //I will redirect the request to local-system dir instead
                    var systemDirName = Path.GetFileName(currentCore.SystemRootPath);
                    var fileShortPath = path.Replace($"{coreInstallDir.Path}\\", $"{systemDirName}\\");
                    fileShortPath = ResolvePath(fileShortPath);
                    WriteLog("Path resolved to system dir instead");
                    WriteLog(fileShortPath);

                    try
                    {
                        //With TryGetItemAsync I can detect the item type, I will use another chech here 
                        var testType = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (testType != null)
                        {
                            isDir = testType.IsOfType(StorageItemTypes.Folder);
                            WriteLog($"Path type double check: isDir->{isDir}");
                        }
                        else
                        {
                            WriteLog($"Path type double check: isDir->File/Folder unable to check (not exists), current state isDir->{isDir}");
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    if (!isDir)
                    {
                        var targetFile = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        var newName = Path.GetFileName(newPath);
                        if (targetFile != null)
                        {
                            Task.Run(() => targetFile.RenameAsync(newName)).Wait();
                            return 0;
                        }
                    }
                    else
                    {
                        var targetFolder = Task.Run(() => GetFolder((tempLocation != null ? tempLocation : coreLocalDir), fileShortPath)).Result;
                        var newName = Path.GetFileName(newPath);
                        if (targetFolder != null)
                        {
                            Task.Run(() => targetFolder.RenameAsync(newName)).Wait();
                            return 0;
                        }
                    }
                }
                else if (path.ToLower().Contains(coreGamesDir.Path.ToLower()))
                {
                    var fileShortPath = path.Replace($"{coreGamesDir.Path}{(coreGamesDir.Path.EndsWith("\\") ? "" : "\\")}", "");
                    fileShortPath = ResolvePath(fileShortPath);
                    WriteLog(fileShortPath);

                    try
                    {
                        //With TryGetItemAsync I can detect the item type, I will use another chech here 
                        var testType = Task.Run(() => (tempLocation != null ? tempLocation : coreGamesDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (testType != null)
                        {
                            isDir = testType.IsOfType(StorageItemTypes.Folder);
                            WriteLog($"Path type double check: isDir->{isDir}");
                        }
                        else
                        {
                            WriteLog($"Path type double check: isDir->File/Folder unable to check (not exists), current state isDir->{isDir}");
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    if (!isDir)
                    {
                        var targetFile = Task.Run(() => (tempLocation != null ? tempLocation : coreGamesDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        var newName = Path.GetFileName(newPath);
                        if (targetFile != null)
                        {
                            Task.Run(() => targetFile.RenameAsync(newName)).Wait();
                            return 0;
                        }
                    }
                    else
                    {
                        var targetFolder = Task.Run(() => GetFolder((tempLocation != null ? tempLocation : coreGamesDir), fileShortPath)).Result;
                        var newName = Path.GetFileName(newPath);
                        if (targetFolder != null)
                        {
                            Task.Run(() => targetFolder.RenameAsync(newName)).Wait();
                            return 0;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
            WriteLog("ERROR: Unable to rename file");
            return -1;
        }

        private long VFSTruncateHandler(Stream stream, long length)
        {
            if (stream == null)
            {
                return -1;
            }
            WriteLog("Stream truncate request");
            try
            {
                long position = stream.Position;
                stream.Seek(length, SeekOrigin.Begin);
                if (position < length)
                {
                    stream.Seek(position, SeekOrigin.Begin);
                }
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
            return 0;
        }

        bool loggerInProgress = false;
        private async void WriteLog(string message, [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                loggerInProgress = true;
                if (VFSLogFile)
                {
                    LibretroCore.AddVFSLog(message, memberName, sourceLineNumber);
                }
                try
                {
                    if (PlatformService.VFSIndicatorHandler != null)
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await Task.Delay(1);
                                VFSIndicatorHandler.Invoke(message, null);

                            }
                            catch (Exception ex) { }
                        });
                    }
                    if (GameInfoUpdater != null)
                    {
                        GameInfoUpdater.Invoke(message, null);
                    }
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception e)
            {

            }
            loggerInProgress = false;
        }
        private bool IsDir(string path, StorageFolder tempLocation, bool log = true)
        {
            bool isDir = false;
            isDir = path.EndsWith("\\") || path.EndsWith("/");
            if (!isDir)
            {
                //We need to double check, not all paths ends with / or \
                var coreLocalDir = ApplicationData.Current.LocalFolder;
                var coreInstallDir = Package.Current.InstalledLocation;
                var coreGamesDir = Task.Run(() => GetGamesFolder()).Result;
                var coreCustomSavesDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "saves")).Result;
                var coreCustomSystemDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "system")).Result;

                //Custom saves/system added recently
                //for easy handling I will just change the coreGamesFolder to the target custom folder if there is match
                if (coreCustomSavesDir != null && path.Contains(coreCustomSavesDir.Path))
                {
                    //Request requires file from custom saves folder
                    coreGamesDir = coreCustomSavesDir;
                    WriteLog($"Request from custom saves folder");
                    WriteLog($"Saves folder forwared to: {coreCustomSavesDir.Path}");
                }
                else if (coreCustomSystemDir != null && path.Contains(coreCustomSystemDir.Path))
                {
                    //Request requires file from custom system folder
                    coreGamesDir = coreCustomSystemDir;
                    WriteLog($"Request from custom system folder");
                    WriteLog($"System folder forwared to: {coreCustomSystemDir.Path}");
                }

                if (!path.ToLower().Equals(coreLocalDir.Path.ToLower()) && !path.ToLower().Equals(coreInstallDir.Path.ToLower()) && !path.ToLower().Equals(coreGamesDir.Path.ToLower()))
                {
                    if (!Regex.IsMatch(path, @"\.(\w+)$"))
                    {
                        isDir = true;
                    }
                }
                else
                {
                    //Absolute main folder path is dir
                    isDir = true;
                }
            }
            if (isDir && log)
            {
                WriteLog("Path expected to be dir");
            }
            return isDir;
        }
        private string ResolvePath(string fileShortPath)
        {
            string fileShortPathResolved = fileShortPath;
            try
            {
                if (fileShortPathResolved.EndsWith("\\") || fileShortPathResolved.EndsWith("/"))
                {
                    //There is no need for extra '\' or '/' at the end
                    fileShortPathResolved = fileShortPathResolved.Remove(fileShortPathResolved.Length - 1);
                }

                fileShortPathResolved = ResolveBrokenPath(fileShortPathResolved);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }

            return fileShortPathResolved;
        }
        private string ResolveBrokenPath(string path)
        {
            string filePathResolved = path;

            try
            {
                //Some paths were like C:\folder\sub\/save\game.sav
                //                                  ^^
                filePathResolved = filePathResolved.Replace("\\/", "\\");

                //Any path with '/' could cause issue I have to revert it back
                filePathResolved = filePathResolved.Replace("/", "\\");

                //Some paths were like C:\folder\sub\saves.\game.sav
                //                                        ^
                filePathResolved = filePathResolved.Replace(".\\", "\\");

                //In some cases with DOSBox the request was like C:\localfolder\system\D: after link resolver
                //                                                                     ^^   
                //It will cause error and throw exception
                string[] chars = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
                foreach (var cItem in chars)
                {
                    filePathResolved = filePathResolved.Replace($"\\{cItem}:", "");
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
            return filePathResolved;
        }
        private async Task<StorageFolder> GetFolder(StorageFolder parent, string path, bool recursive = false)
        {
            try
            {
                if (path != null && parent != null)
                {
                    if (!recursive)
                    {
                        return (StorageFolder)await parent.TryGetItemAsync(path);
                    }
                    else
                    {
                        var pathParts = path.Split('\\');
                        if (pathParts != null && pathParts.Length > 0)
                        {
                            StorageFolder subParent = parent;
                            foreach (var pathPart in pathParts)
                            {
                                WriteLog($"{pathPart}");
                                subParent = (StorageFolder)await subParent.TryGetItemAsync(pathPart);
                                if (subParent == null)
                                {
                                    break;
                                }
                            }

                            return subParent;
                        }
                        else
                        {
                            return (StorageFolder)await parent.TryGetItemAsync(path);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
            return null;
        }
        private int VFSStatHandler(string path, IntPtr size)
        {
            if (path == null)
            {
                return 0;
            }

            WriteLog(path);
            path = ResolveMainPath(path);
            StorageFolder tempLocation = Task.Run(() => CheckTempDir(path)).Result;
            bool isDir = IsDir(path, tempLocation);

            try
            {
                var coreLocalDir = ApplicationData.Current.LocalFolder;
                var coreInstallDir = Package.Current.InstalledLocation;
                var coreGamesDir = Task.Run(() => GetGamesFolder()).Result;
                var coreCustomSavesDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "saves")).Result;
                var coreCustomSystemDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "system")).Result;

                //Custom saves/system added recently
                //for easy handling I will just change the coreGamesFolder to the target custom folder if there is match
                if (coreCustomSavesDir != null && path.Contains(coreCustomSavesDir.Path))
                {
                    //Request requires file from custom saves folder
                    coreGamesDir = coreCustomSavesDir;
                    WriteLog($"Request from custom saves folder");
                    WriteLog($"Saves folder forwared to: {coreCustomSavesDir.Path}");
                }
                else if (coreCustomSystemDir != null && path.Contains(coreCustomSystemDir.Path))
                {
                    //Request requires file from custom system folder
                    coreGamesDir = coreCustomSystemDir;
                    WriteLog($"Request from custom system folder");
                    WriteLog($"System folder forwared to: {coreCustomSystemDir.Path}");
                }

                if (path.ToLower().Contains(coreLocalDir.Path.ToLower()))
                {
                    var fileShortPath = path.Replace($"{coreLocalDir.Path}\\", "");
                    fileShortPath = ResolvePath(fileShortPath);
                    WriteLog(fileShortPath);
                    if (path.ToLower().Equals(coreLocalDir.Path.ToLower()) || path.ToLower().Equals($"{coreLocalDir.Path.ToLower()}\\"))
                    {
                        WriteLog("Local root requested");
                        WriteLog("Local is valid dir");
                        return Constants.RETRO_VFS_STAT_IS_VALID | Constants.RETRO_VFS_STAT_IS_DIRECTORY;
                    }

                    try
                    {
                        //With TryGetItemAsync I can detect the item type, I will use another chech here 
                        var testType = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (testType != null)
                        {
                            isDir = testType.IsOfType(StorageItemTypes.Folder);
                            WriteLog($"Path type double check: isDir->{isDir}");
                        }
                        else
                        {
                            WriteLog($"Path type double check: isDir->File/Folder unable to check (not exists), current state isDir->{isDir}");
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    if (!isDir)
                    {
                        try
                        {
                            var targetFile = (StorageFile)Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                            //In RetroArch they used CreateFile2 which will cause file creation (if not exists)
                            //Not sure if this step is required or not?
                            /*if(targetFile == null)
                            {
                                targetFile = Task.Run(() => coreLocalDir.CreateFileAsync(fileShortPath)).Result;
                            }*/

                            if (targetFile != null)
                            {
                                if (size != IntPtr.Zero)
                                {
                                    try
                                    {
                                        var fileSize = Task.Run(() => targetFile.GetBasicPropertiesAsync().AsTask()).Result;
                                        WriteLog($"Size: {fileSize.Size}");
                                        Marshal.WriteInt64(size, (long)fileSize.Size);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                WriteLog("Valid file");
                                return Constants.RETRO_VFS_STAT_IS_VALID;
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog(ex.Message);
                        }
                    }
                    {
                        //Try as folder
                        var targetFolder = Task.Run(() => GetFolder((tempLocation != null ? tempLocation : coreLocalDir), fileShortPath)).Result;
                        //In RetroArch they used CreateFile2 which will cause file creation (if not exists)
                        //Not sure if this step is required or not?
                        /*if(targetFolder == null)
                        {
                            targetFolder = Task.Run(() => coreLocalDir.CreateDirectoryAsync(fileShortPath)).Result;
                        }*/
                        if (targetFolder != null)
                        {
                            WriteLog("Valid dir");
                            return Constants.RETRO_VFS_STAT_IS_VALID | Constants.RETRO_VFS_STAT_IS_DIRECTORY;
                        }
                        WriteLog("Cannot validate path");
                    }
                }
                else if (path.ToLower().Contains(coreInstallDir.Path.ToLower()))
                {
                    //No write allowed for installation dir, this could lead to issue
                    //I will redirect the request to local-system dir instead
                    var systemDirName = Path.GetFileName(currentCore.SystemRootPath);
                    var fileShortPath = path.Replace($"{coreInstallDir.Path}\\", $"{systemDirName}\\");
                    fileShortPath = ResolvePath(fileShortPath);
                    WriteLog("Path resolved to system dir instead");
                    WriteLog(fileShortPath);
                    if (path.ToLower().Equals(coreInstallDir.Path.ToLower()) || path.ToLower().Equals($"{coreLocalDir.Path.ToLower()}\\"))
                    {
                        WriteLog("Local (install) root requested");
                        WriteLog("Local (install) is valid dir");
                        return Constants.RETRO_VFS_STAT_IS_VALID | Constants.RETRO_VFS_STAT_IS_DIRECTORY;
                    }
                    try
                    {
                        //With TryGetItemAsync I can detect the item type, I will use another chech here 
                        var testType = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (testType != null)
                        {
                            isDir = testType.IsOfType(StorageItemTypes.Folder);
                            WriteLog($"Path type double check: isDir->{isDir}");
                        }
                        else
                        {
                            WriteLog($"Path type double check: isDir->File/Folder unable to check (not exists), current state isDir->{isDir}");
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    if (!isDir)
                    {
                        try
                        {
                            var targetFile = (StorageFile)Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                            //In RetroArch they used CreateFile2 which will cause file creation (if not exists)
                            //Not sure if this step is required or not?
                            /*if (targetFolder == null)
                            {
                                targetFolder = Task.Run(() => coreLocalDir.CreateDirectoryAsync(fileShortPath)).Result;
                            }*/
                            if (targetFile != null)
                            {
                                if (size != IntPtr.Zero)
                                {
                                    try
                                    {
                                        var fileSize = Task.Run(() => targetFile.GetBasicPropertiesAsync().AsTask()).Result;
                                        WriteLog($"Size: {fileSize.Size}");
                                        Marshal.WriteInt64(size, (long)fileSize.Size);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                WriteLog("Valid file");
                                return Constants.RETRO_VFS_STAT_IS_VALID;
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog(ex.Message);
                        }
                    }
                    {
                        //Try as folder
                        var targetFolder = Task.Run(() => GetFolder((tempLocation != null ? tempLocation : coreLocalDir), fileShortPath)).Result;
                        //In RetroArch they used CreateFile2 which will cause file creation (if not exists)
                        //Not sure if this step is required or not?
                        /*if (targetFolder == null)
                        {
                            targetFolder = Task.Run(() => coreLocalDir.CreateDirectoryAsync(fileShortPath)).Result;
                        }*/
                        if (targetFolder != null)
                        {
                            WriteLog("Valid dir");
                            return Constants.RETRO_VFS_STAT_IS_VALID | Constants.RETRO_VFS_STAT_IS_DIRECTORY;
                        }
                        WriteLog("Cannot validate path");
                    }
                }
                else if (path.ToLower().Contains(coreGamesDir.Path.ToLower()))
                {
                    var fileShortPath = path.Replace($"{coreGamesDir.Path}{(coreGamesDir.Path.EndsWith("\\") ? "" : "\\")}", "");
                    fileShortPath = ResolvePath(fileShortPath);
                    WriteLog(fileShortPath);
                    if (path.ToLower().Equals(coreGamesDir.Path.ToLower()) || path.ToLower().Equals($"{coreGamesDir.Path.ToLower()}\\"))
                    {
                        WriteLog("Games root requested");
                        WriteLog("Games is valid dir");
                        return Constants.RETRO_VFS_STAT_IS_VALID | Constants.RETRO_VFS_STAT_IS_DIRECTORY;
                    }
                    //In RetroArch they used CreateFile2 which will cause file creation (if not exists)
                    //Not sure if this step is required or not?
                    /*if (targetFolder == null)
                    {
                        targetFolder = Task.Run(() => coreGamesDir.CreateDirectoryAsync(fileShortPath)).Result;
                    }*/

                    try
                    {
                        //With TryGetItemAsync I can detect the item type, I will use another chech here 
                        var testType = Task.Run(() => (tempLocation != null ? tempLocation : coreGamesDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                        if (testType != null)
                        {
                            isDir = testType.IsOfType(StorageItemTypes.Folder);
                            WriteLog($"Path type double check: isDir->{isDir}");
                        }
                        else
                        {
                            WriteLog($"Path type double check: isDir->File/Folder unable to check (not exists), current state isDir->{isDir}");
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    if (!isDir)
                    {
                        try
                        {
                            var targetFile = (StorageFile)Task.Run(() => (tempLocation != null ? tempLocation : coreGamesDir).TryGetItemAsync(fileShortPath).AsTask()).Result;
                            if (targetFile != null)
                            {
                                if (size != IntPtr.Zero)
                                {
                                    try
                                    {
                                        var fileSize = Task.Run(() => targetFile.GetBasicPropertiesAsync().AsTask()).Result;
                                        WriteLog($"Size: {fileSize.Size}");
                                        Marshal.WriteInt64(size, (long)fileSize.Size);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                WriteLog("Valid file");
                                return Constants.RETRO_VFS_STAT_IS_VALID;
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog(ex.Message);
                        }
                    }
                    {
                        //Try as folder
                        var targetFolder = Task.Run(() => GetFolder((tempLocation != null ? tempLocation : coreGamesDir), fileShortPath)).Result;
                        //In RetroArch they used CreateFile2 which will cause file creation (if not exists)
                        //Not sure if this step is required or not?
                        /*if (targetFolder == null)
                        {
                            targetFolder = Task.Run(() => coreGamesDir.CreateDirectoryAsync(fileShortPath)).Result;
                        }*/
                        if (targetFolder != null)
                        {
                            WriteLog("Valid dir");
                            return Constants.RETRO_VFS_STAT_IS_VALID | Constants.RETRO_VFS_STAT_IS_DIRECTORY;
                        }
                        WriteLog("Cannot validate path");
                    }
                }
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
            return 0;
        }

        private int VFSMkdirHandler(string dir)
        {
            if (dir == null)
            {
                return -1;
            }
            WriteLog(dir);
            dir = ResolveMainPath(dir);
            StorageFolder tempLocation = Task.Run(() => CheckTempDir(dir)).Result;
            try
            {
                var coreLocalDir = ApplicationData.Current.LocalFolder;
                var coreInstallDir = Package.Current.InstalledLocation;
                var coreGamesDir = Task.Run(() => GetGamesFolder()).Result;
                var coreCustomSavesDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "saves")).Result;
                var coreCustomSystemDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "system")).Result;

                //Custom saves/system added recently
                //for easy handling I will just change the coreGamesFolder to the target custom folder if there is match
                if (coreCustomSavesDir != null && dir.Contains(coreCustomSavesDir.Path))
                {
                    //Request requires file from custom saves folder
                    coreGamesDir = coreCustomSavesDir;
                    WriteLog($"Request from custom saves folder");
                    WriteLog($"Saves folder forwared to: {coreCustomSavesDir.Path}");
                }
                else if (coreCustomSystemDir != null && dir.Contains(coreCustomSystemDir.Path))
                {
                    //Request requires file from custom system folder
                    coreGamesDir = coreCustomSystemDir;
                    WriteLog($"Request from custom system folder");
                    WriteLog($"System folder forwared to: {coreCustomSystemDir.Path}");
                }

                if (dir.ToLower().Contains(coreLocalDir.Path.ToLower()))
                {
                    var folderShortPath = dir.Replace($"{coreLocalDir.Path}\\", "");
                    folderShortPath = ResolvePath(folderShortPath);
                    WriteLog(folderShortPath);
                    var folderTest = Task.Run(() => GetFolder((tempLocation != null ? tempLocation : coreLocalDir), folderShortPath)).Result;
                    if (folderTest != null)
                    {
                        WriteLog("Folder already exists");
                        return -2;
                    }
                    var folder = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).CreateFolderAsync(folderShortPath)).Result;
                    if (folder != null)
                    {
                        WriteLog("Folder created");
                        return 0;
                    }
                }
                else if (dir.ToLower().Contains(coreInstallDir.Path.ToLower()))
                {
                    //No write allowed for installation dir, this could lead to issue
                    //I will redirect the request to local-system dir instead
                    var systemDirName = Path.GetFileName(currentCore.SystemRootPath);
                    var folderShortPath = dir.Replace($"{coreInstallDir.Path}\\", $"{systemDirName}\\");
                    folderShortPath = ResolvePath(folderShortPath);
                    WriteLog("Path resolved to system dir instead");
                    WriteLog(folderShortPath);
                    var folderTest = Task.Run(() => GetFolder((tempLocation != null ? tempLocation : coreLocalDir), folderShortPath)).Result;
                    if (folderTest != null)
                    {
                        WriteLog("Folder already exists");
                        return -2;
                    }
                    var folder = Task.Run(() => (tempLocation != null ? tempLocation : coreLocalDir).CreateFolderAsync(folderShortPath)).Result;
                    if (folder != null)
                    {
                        WriteLog("Folder created");
                        return 0;
                    }
                }
                else if (dir.ToLower().Contains(coreGamesDir.Path.ToLower()))
                {
                    var folderShortPath = dir.Replace($"{coreGamesDir.Path}{(coreGamesDir.Path.EndsWith("\\") ? "" : "\\")}", "");
                    folderShortPath = ResolvePath(folderShortPath);
                    WriteLog(folderShortPath);
                    var folderTest = Task.Run(() => GetFolder((tempLocation != null ? tempLocation : coreGamesDir), folderShortPath)).Result;
                    if (folderTest != null)
                    {
                        WriteLog("Folder already exists");
                        return -2;
                    }
                    var folder = Task.Run(() => (tempLocation != null ? tempLocation : coreGamesDir).CreateFolderAsync(folderShortPath)).Result;
                    if (folder != null)
                    {
                        WriteLog("Folder created");
                        return 0;
                    }
                }
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
            WriteLog("ERROR: Unable to validate or create the folder");
            return -1;
        }

        private VFSDir VFSOpendirHandler(string path, bool include_hidden)
        {
            if (path == null)
            {
                return null;
            }
            try
            {
                if (PlatformService.PreventGCAlways) GC.TryStartNoGCRegion(176400, true);
            }
            catch (Exception ea)
            {
            }
            WriteLog(path);
            path = ResolveMainPath(path);
            StorageFolder tempLocation = Task.Run(() => CheckTempDir(path)).Result;
            try
            {
                StorageFolder targetDir = null;

                var coreLocalDir = ApplicationData.Current.LocalFolder;
                var coreInstallDir = Package.Current.InstalledLocation;
                var coreGamesDir = Task.Run(() => GetGamesFolder()).Result;
                var coreCustomSavesDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "saves")).Result;
                var coreCustomSystemDir = Task.Run(() => PlatformService.GetCustomFolder(currentCore.Name, "system")).Result;

                //Custom saves/system added recently
                //for easy handling I will just change the coreGamesFolder to the target custom folder if there is match
                if (coreCustomSavesDir != null && path.Contains(coreCustomSavesDir.Path))
                {
                    //Request requires file from custom saves folder
                    coreGamesDir = coreCustomSavesDir;
                    WriteLog($"Request from custom saves folder");
                    WriteLog($"Saves folder forwared to: {coreCustomSavesDir.Path}");
                }
                else if (coreCustomSystemDir != null && path.Contains(coreCustomSystemDir.Path))
                {
                    //Request requires file from custom system folder
                    coreGamesDir = coreCustomSystemDir;
                    WriteLog($"Request from custom system folder");
                    WriteLog($"System folder forwared to: {coreCustomSystemDir.Path}");
                }

                if (path.ToLower().Contains(coreLocalDir.Path.ToLower()))
                {
                    if (path.ToLower().Equals(coreLocalDir.Path.ToLower()) || path.ToLower().Equals($"{coreLocalDir.Path.ToLower()}\\"))
                    {
                        WriteLog("Opening local root");
                        targetDir = (tempLocation != null ? tempLocation : coreLocalDir);
                    }
                    else
                    {
                        var folderShortPath = path.Replace($"{coreLocalDir.Path}\\", "");
                        folderShortPath = ResolvePath(folderShortPath);
                        WriteLog(folderShortPath);
                        targetDir = Task.Run(() => GetFolder((tempLocation != null ? tempLocation : coreLocalDir), folderShortPath)).Result;
                    }
                }
                else if (path.ToLower().Contains(coreInstallDir.Path.ToLower()))
                {
                    //No write allowed for installation dir, this could lead to issue
                    //I will redirect the request to local-system dir instead
                    if (path.ToLower().Equals(coreInstallDir.Path.ToLower()) || path.ToLower().Equals($"{coreInstallDir.Path.ToLower()}\\"))
                    {
                        WriteLog("Opening local (install) root");
                        targetDir = (tempLocation != null ? tempLocation : coreLocalDir);
                    }
                    else
                    {
                        var systemDirName = Path.GetFileName(currentCore.SystemRootPath);
                        var folderShortPath = path.Replace($"{coreInstallDir.Path}\\", $"{systemDirName}\\");
                        folderShortPath = ResolvePath(folderShortPath);
                        WriteLog("Path resolved to system dir instead");
                        WriteLog(folderShortPath);
                        targetDir = Task.Run(() => GetFolder((tempLocation != null ? tempLocation : coreLocalDir), folderShortPath)).Result;
                    }
                }
                else if (path.ToLower().Contains(coreGamesDir.Path.ToLower()))
                {
                    if (path.ToLower().Equals(coreGamesDir.Path.ToLower()) || path.ToLower().Equals($"{coreGamesDir.Path.ToLower()}\\"))
                    {
                        WriteLog("Opening games root");
                        targetDir = (tempLocation != null ? tempLocation : coreGamesDir);
                    }
                    else
                    {
                        var folderShortPath = path.Replace($"{coreGamesDir.Path}{(coreGamesDir.Path.EndsWith("\\") ? "" : "\\")}", "");
                        folderShortPath = ResolvePath(folderShortPath);
                        WriteLog(folderShortPath);
                        targetDir = Task.Run(() => GetFolder((tempLocation != null ? tempLocation : coreGamesDir), folderShortPath)).Result;
                    }
                }

                if (targetDir != null)
                {
                    QueryOptions queryOptions = new QueryOptions();
                    queryOptions.FolderDepth = FolderDepth.Shallow;
                    if (PlatformService.UseWindowsIndexer)
                    {
                        queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
                    }
                    var folders = targetDir.CreateFolderQueryWithOptions(queryOptions);
                    var files = targetDir.CreateFileQueryWithOptions(queryOptions);
                    var dirFolders = Task.Run(() => folders.GetFoldersAsync().AsTask()).Result;
                    var dirFiles = Task.Run(() => files.GetFilesAsync().AsTask()).Result;

                    //var dirFiles = Task.Run(() => targetDir.GetFilesAsync().AsTask()).Result;
                    //var dirFolders = Task.Run(() => targetDir.GetFoldersAsync().AsTask()).Result;

                    List<VFSEntry> entriesList = new List<VFSEntry>();
                    foreach (var file in dirFiles)
                    {
                        var entry = new VFSEntry(file.Path, false);
                        entriesList.Add(entry);
                    }
                    foreach (var folder in dirFolders)
                    {
                        var entry = new VFSEntry(folder.Path, true);
                        entriesList.Add(entry);
                    }
                    VFSDir vfsDir = new VFSDir(path, entriesList);
                    vfsDir.CurrentEntry = -1;
                    try
                    {
                        if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                    }
                    catch (Exception ea)
                    {
                    }
                    return vfsDir;
                }
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
            WriteLog($"ERROR: cannot open dir {path}");
            try
            {
                if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
            }
            catch (Exception ea)
            {
            }
            return null;
        }

        private bool VFSReaddirHandler(VFSDir rdir)
        {
            if (rdir == null)
            {
                return false;
            }
            WriteLog(rdir.DirPath);
            try
            {
                if (rdir.CurrentEntry + 1 < rdir.Entries.Count)
                {
                    rdir.CurrentEntry++;
                    WriteLog("Entry Index: " + rdir.CurrentEntry);
                    return true;
                }
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
            return false;
        }

        private string VFSDirentGetNameHandler(VFSDir rdir)
        {
            string name = null;
            if (rdir == null)
            {
                return null;
            }
            WriteLog(rdir.DirPath);
            try
            {
                var entry = rdir.Entries[rdir.CurrentEntry];
                name = Path.GetFileName(entry.Path);
                WriteLog("Entry Name: " + name);
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
            return name;
        }

        private bool VFSDirentIsDirHandler(VFSDir rdir)
        {
            bool state = false;
            if (rdir == null)
            {
                return false;
            }
            WriteLog(rdir.DirPath);
            try
            {
                var entry = rdir.Entries[rdir.CurrentEntry];
                WriteLog("Is Dir? " + (entry.IsDirectory ? "true" : "false"));
                return entry.IsDirectory;
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
            return state;
        }

        private int VFSClosedirHandler(VFSDir rdir)
        {
            if (rdir == null)
            {
                return -1;
            }
            WriteLog(rdir.DirPath);
            try
            {
                rdir.Dispose();
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
                _ = Task.Run(() => PlatformService.ShowErrorMessage(e));
            }
            if (rdir != null)
            {
                if (PlatformService.PreventGCAlways) GC.SuppressFinalize(rdir);
            }
            return -1;
        }

        public int GetSamplesBufferCount()
        {
            return audioService.GetSamplesBufferCount();
        }
    }

    public class VFSDir : IDisposable
    {
        public List<VFSEntry> Entries { get; set; }
        public int CurrentEntry { get; set; }
        public string entryName;
        public string EntryName
        {
            get
            {
                return entryName;
            }
            set => SetStringAndUnmanagedMemory(value, ref entryName, ref EntryNameUnmanaged);
        }
        public IntPtr EntryNameUnmanaged;
        public string DirPath { get; set; }

        public VFSDir(string dirPath, List<VFSEntry> entries)
        {
            DirPath = dirPath;
            Entries = entries;
        }
        private void SetStringAndUnmanagedMemory(string newValue, ref string store, ref IntPtr unmanagedPtr)
        {
            try
            {
                store = newValue;
                if (unmanagedPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(unmanagedPtr);
                    unmanagedPtr = IntPtr.Zero;
                }

                if (newValue != null)
                {
                    unmanagedPtr = Marshal.StringToHGlobalAnsi(newValue);
                }
            }
            catch (Exception e)
            {

            }
        }
        public void Dispose()
        {
            Marshal.FreeHGlobal(EntryNameUnmanaged);
            EntryNameUnmanaged = IntPtr.Zero;
            Entries = null;
        }
    }
    public class VFSEntry
    {
        public string Path { get; set; }
        public bool IsDirectory { get; set; }
        public VFSEntry(string path, bool isDirectory)
        {
            Path = path;
            IsDirectory = isDirectory;
        }
    }
}
