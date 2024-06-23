using LibRetriX;
using Newtonsoft.Json;
using RetriX.Shared.Models;
using RetriX.Shared.Services;
using RetriX.Shared.StreamProviders;
using RetriX.UWP;
using RetriX.UWP.Pages;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.BulkAccess;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinUniversalTool;
using static RetriX.UWP.Services.PlatformService;

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

namespace RetriX.Shared.ViewModels
{
    public class GameSystemSelectionViewModel : BindableBase
    {
        public GameSystemsProviderServiceBase GameSystemsProviderService { get => PlatformService.GameSystemsProviderService; }

        private GameFile SelectedGameFile { get; set; }

        public string folderPickerTokenName
        {
            get
            {
                if (SelectedSystem != null)
                {
                    return SelectedSystem.TempName;
                }
                return "dummy";
            }
        }
        public ICommand ShowSettings { get; set; }
        public ICommand ShowAbout { get; set; }
        public ICommand ShowHelp { get; set; }
        public ICommand ShowDonate { get; set; }
        public ICommand GameSystemRecentSelected { get; set; }
        public ICommand GameSystemRecentsHolding { get; set; }
        public ICommand GameSystemSelected { get; set; }
        public ICommand GameSystemHolding { get; set; }

        public bool SystemCoresIsLoading = true;
        public static Dictionary<string, CoresOptions> SystemsOptionsTemp = new Dictionary<string, CoresOptions>();
        public static Dictionary<string, CoresOptions> SystemsOptions = new Dictionary<string, CoresOptions>();
        public static Dictionary<string, GameSystemAnyCore> SystemsAnyCore = new Dictionary<string, GameSystemAnyCore>();
        public ObservableCollection<string> GamesList = new ObservableCollection<string>();

        public ObservableCollection<GameSystemRecentModel> GamesRecentsList = new ObservableCollection<GameSystemRecentModel>();
        public ObservableCollection<GameSystemRecentModel> GamesMainList = new ObservableCollection<GameSystemRecentModel>();

        public bool GamesListVisible = false;
        public bool NoGamesListVisible = false;
        public bool SearchBoxVisible = false;
        public bool NoRecentsListVisible = true;
        public bool SystemsLoadFailed = false;
        public string LoadingStatus = "Please wait..";
        public string StatusBar = "";
        public bool ShowAnyCoreOptions = false;
        public bool ShowUpdateOption = false;
        public bool ShowCoreInfo = true;
        public bool ShowBIOSMapOptions = false;

        //Just to avoid crash on Windows Phone in case two dialogs appears at once
        #region DIALOG
        bool isDialogInProgress
        {
            get
            {
                return Helpers.DialogInProgress;
            }
            set
            {
                Helpers.DialogInProgress = value;
            }
        }
        private async Task GeneralDialog(string Message, string title = null, string okButton = null)
        {
            if (isDialogInProgress)
            {
                return;
            }
            try
            {
                await ShowMessageWithTitleDirect(Message, title, okButton);
            }
            catch (Exception ex)
            {

            }
        }

        #endregion
        public void setShowAnyCoreOptions(bool state)
        {
            ShowAnyCoreOptions = state;
            ShowCoreInfo = !state;
            ShowUpdateOption = !state;
            RaisePropertyChanged(nameof(ShowAnyCoreOptions));
            RaisePropertyChanged(nameof(ShowUpdateOption));
            RaisePropertyChanged(nameof(ShowCoreInfo));
        }
        public Dictionary<string, CoresOptions> CoresOptionsDictionary
        {
            get => SystemsOptions;
        }

        public async Task resetCoresOptionsDictionary(GameSystemViewModel system)
        {
            var SystemName = system.TempName;
            var CoreName = system.Core.Name;
            var IsNewCore = system.Core.IsNewCore;

            var expectedName = $"{CoreName}_{SystemName}";
            CoresOptions coreOption;
            try
            {
                if (!SystemsOptions.TryGetValue(expectedName, out coreOption))
                {
                    if (!IsNewCore)
                    {
                        SystemsOptions.TryGetValue(SystemName, out coreOption);
                        expectedName = SystemName;
                    }
                }
                if (coreOption != null)
                {
                    SystemsOptions[expectedName].OptionsList = SystemsOptionsTemp[expectedName].OptionsList;
                }
                await DeleteSavedOptions(system);
                await CoreOptionsStoreAsync(system);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }
        public static void setCoresOptionsDictionaryDirect(GameSystemViewModel system, string OptionKey, uint SelectedIndex)
        {
            var SystemName = system.TempName;
            var CoreName = system.Core.Name;
            var IsNewCore = system.Core.IsNewCore;

            var expectedName = $"{CoreName}_{SystemName}";
            CoresOptions coreOption;
            try
            {
                if (!SystemsOptions.TryGetValue(expectedName, out coreOption))
                {
                    if (!IsNewCore)
                    {
                        SystemsOptions.TryGetValue(SystemName, out coreOption);
                        expectedName = SystemName;
                    }
                }
                if (coreOption != null)
                {
                    SystemsOptions[expectedName].OptionsList[OptionKey].SelectedIndex = SelectedIndex;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void setCoresOptionsDictionary(GameSystemViewModel system, string OptionKey, uint SelectedIndex)
        {
            var SystemName = system.TempName;
            var CoreName = system.Core.Name;
            var IsNewCore = system.Core.IsNewCore;

            var expectedName = $"{CoreName}_{SystemName}";
            CoresOptions coreOption;
            try
            {
                if (!SystemsOptions.TryGetValue(expectedName, out coreOption))
                {
                    if (!IsNewCore)
                    {
                        SystemsOptions.TryGetValue(SystemName, out coreOption);
                        expectedName = SystemName;
                    }
                }
                if (coreOption != null)
                {
                    if (SystemsOptions[expectedName].OptionsList[OptionKey].OptionsValues.Count <= SelectedIndex)
                    {
                        SelectedIndex = 0;
                    }
                    SystemsOptions[expectedName].OptionsList[OptionKey].SelectedIndex = SelectedIndex;
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }

        }
        public static void setCoresOptionsDictionaryDirect(string CoreName, string SystemName, bool IsNewCore, string OptionKey, uint SelectedIndex)
        {
            var expectedName = $"{CoreName}_{SystemName}";
            CoresOptions coreOption;
            try
            {
                if (!SystemsOptions.TryGetValue(expectedName, out coreOption))
                {
                    if (!IsNewCore)
                    {
                        SystemsOptions.TryGetValue(SystemName, out coreOption);
                        expectedName = SystemName;
                    }
                }
                if (coreOption != null)
                {
                    if (SystemsOptions[expectedName].OptionsList[OptionKey].OptionsValues.Count <= SelectedIndex)
                    {
                        SelectedIndex = 0;
                    }
                    SystemsOptions[expectedName].OptionsList[OptionKey].SelectedIndex = SelectedIndex;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task UpdateCoversByFolder(List<GameSystemRecentModel> games = null)
        {
            try
            {
                var coversFolder = await GetCoversFolder();
                if (coversFolder != null)
                {
                    var files = await coversFolder.GetFilesAsync();
                    if (files != null && files.Count > 0)
                    {
                        foreach (var fItem in files)
                        {
                            try
                            {
                                CancellationTokenSource cts = new CancellationTokenSource();
                                ParallelOptions po = new ParallelOptions();
                                po.CancellationToken = cts.Token;
                                po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
                                if (games == null)
                                {
                                    Parallel.ForEach(GamesMainList, po, () => 0, (game, loopState, n) =>
                                {
                                    var gameID = Path.GetFileNameWithoutExtension(fItem.Name);
                                    if (game.GameID.Equals(gameID))
                                    {
                                        try
                                        {
                                            var gameIndex = GamesMainList.IndexOf(game);
                                            GamesMainList[gameIndex].GameSnapshot = fItem.Path;
                                            GamesMainList[gameIndex].updateSnapShotBinding();

                                            gameIndex = GamesMainListTemp.IndexOf(game);
                                            GamesMainListTemp[gameIndex].GameSnapshot = fItem.Path;
                                        }
                                        catch (Exception xe)
                                        {

                                        }
                                        cts.Cancel();
                                    }
                                    return 0;
                                },
                            (n) => { });
                                }
                                else
                                {
                                    Parallel.ForEach(games, po, () => 0, (game, loopState, n) =>
                                    {
                                        var gameID = Path.GetFileNameWithoutExtension(fItem.Name);
                                        if (game.GameID.Equals(gameID))
                                        {
                                            try
                                            {
                                                var gameIndex = games.IndexOf(game);
                                                games[gameIndex].GameSnapshot = fItem.Path;
                                            }
                                            catch (Exception xe)
                                            {

                                            }
                                            cts.Cancel();
                                        }
                                        return 0;
                                    },
                            (n) => { });
                                }

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task UpdateCoversByID(string gameID, string cover = "")
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                ParallelOptions po = new ParallelOptions();
                po.CancellationToken = cts.Token;
                po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
                Parallel.ForEach(GamesMainList, po, () => 0, (game, loopState, n) =>
                {
                    if (game.GameID.Equals(gameID))
                    {
                        try
                        {
                            var gameIndex = GamesMainList.IndexOf(game);
                            GamesMainList[gameIndex].GameSnapshot = cover;
                            GamesMainList[gameIndex].updateSnapShotBinding();

                            gameIndex = GamesMainListTemp.IndexOf(game);
                            GamesMainListTemp[gameIndex].GameSnapshot = cover;
                            GamesMainListTemp[gameIndex].updateSnapShotBinding();
                        }
                        catch (Exception xe)
                        {

                        }
                        cts.Cancel();
                    }
                    return 0;
                },
        (n) => { });
            }
            catch (Exception ex)
            {

            }
        }

        int IconSize = 60;
        public GameSystemSelectionViewModel()
        {
            if (PlatformService.isXBOX)
            {
                IconSize = 80;
            }
            else
            {
                IconSize = 65;
            }
        }

        bool LoaderReady = false;
        //This function will be called from Systems Page
        public async Task AsyncLoader()
        {
            SystemCoreIsLoadingState(true);
            SetStatus("Loading Systems..");
            while (!PlatformService.IsCoresLoaded)
            {
                await Task.Delay(1000);
            }
            PlatformService.SetResetSelectionHandler(callResetSelection);

            ResetSystemsSelection();

            ShowAbout = new Command(new Action(() =>
            {
                PlatformService.PlayNotificationSound("button-01");
                App.rootFrame.Navigate(typeof(AboutView));
            }));
            ShowHelp = new Command(new Action(() =>
            {
                PlatformService.PlayNotificationSound("button-01");
                App.rootFrame.Navigate(typeof(HelpView));
            }));
            StatusHandler = StatusHandlerCall;
            LoaderReady = true;
        }
        public void SystemCoreIsLoadingState(bool LoadingState, string status = "...")
        {
            _ = CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, () =>
              {
                  SetStatus(status);
                  try
                  {
                      SystemCoresIsLoading = LoadingState;
                      RaisePropertyChanged(nameof(SystemCoresIsLoading));
                  }
                  catch (Exception e)
                  {
                      PlatformService.ShowErrorMessage(e);
                  }
              });
        }
        public void Prepare()
        {
            try
            {
                ResetSystemsSelection();
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }

        public void Prepare(StorageFile parameter)
        {
            SelectedGameFile = new GameFile(parameter);
        }

        public async void Initialize()
        {
            InitializeAsync();
        }

        public bool IsCoreRequiredGamesFolder(string coreName)
        {
            return PlatformService.IsCoreRequiredGamesFolder(coreName);
        }
        public async Task<bool> IsCoreGamesFolderAlreadySelected(string systemName)
        {
            return await PlatformService.IsCoreGamesFolderAlreadySelected(systemName);
        }
        public async void InitializeAsync()
        {
            bool StartedByFile = false;

            while (!LoaderReady)
            {
                await Task.Delay(1000);
            }

            try
            {
                ResetSystemsSelection();
                if (PlatformService.Consoles != null)
                {
                    int systemIndex = 0;
                    foreach (var SystemItem in PlatformService.Consoles)
                    {
                        try
                        {
                            await OptionsRetrieveAsync(SystemItem);
                        }
                        catch (Exception ecp)
                        {

                        }
                        try
                        {
                            //await AnyCoreRetrieveAsync();
                        }
                        catch (Exception eac)
                        {

                        }
                        systemIndex++;
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                SystemsLoadFailed = true;
                _ = RaisePropertyChanged(nameof(SystemsLoadFailed));
            }
            SystemCoreIsLoadingState(false);
        }

        bool RootFolderInProgress = false;
        bool SelectFileInProgress = false;
        public static Dictionary<string, StorageFolder> SystemRoots = new Dictionary<string, StorageFolder>();
        public bool ReSelectIsActive = false;
        public bool CheckByTokenActive = false;

        //Hmm, need a lot of clean up.. many things now not in use, just useless conditions and variables
        public async Task GameSystemHoldingHandler(GameSystemViewModel system, EventHandler eventHandler = null)
        {
            SystemSelectDone = false;
            try
            {
                if (!RootFolderInProgress && !SelectFileInProgress)
                {
                    SelectedSystem = system;

                    PlatformService.PlayNotificationSound("option-changed");
                    RootFolderInProgress = true;
                    StorageFolder systemRootFolder = null;
                    if (ReSelectIsActive)
                    {
                        systemRootFolder = await PlatformService.PickDirectory(folderPickerTokenName, ReSelectIsActive);
                        if (systemRootFolder != null)
                        {
                            GamesMainList.Clear();
                            GamesMainListTemp.Clear();
                            GamesSubFolders.Clear();
                            GamesExtens.Clear();
                            ParentFilter = "";
                            SystemCoreIsLoadingState(true, "Checking games..");
                            StorageFolder TestRoot = null;
                            if (SystemRoots.Count > 0 && SystemRoots.TryGetValue(folderPickerTokenName, out TestRoot))
                            {
                                SystemRoots[folderPickerTokenName] = systemRootFolder;
                            }
                            else
                            {
                                SystemRoots.Add(folderPickerTokenName, systemRootFolder);
                            }
                        }
                        else
                        {
                            ReSelectIsActive = false;
                            CheckByTokenActive = false;
                            RootFolderInProgress = false;
                            SystemSelectDone = true;
                            return;
                        }
                    }
                    ReSelectIsActive = false;

                    if ((!ReSelectIsActive && !CheckByTokenActive))
                    {
                        if (systemRootFolder == null)
                        {
                            //Allow core to open page
                            //Games folder can be set later
                        }
                        else
                        {
                            StorageFolder TestRoot = null;
                            if (SystemRoots.Count > 0 && SystemRoots.TryGetValue(folderPickerTokenName, out TestRoot))
                            {
                                SystemRoots[folderPickerTokenName] = systemRootFolder;
                            }
                            else
                            {
                                SystemRoots.Add(folderPickerTokenName, systemRootFolder);
                            }

                            CheckByTokenActive = false;
                            //Open Games List
                            SystemSelected = true;
                            var extensions = system.SupportedExtensions.Concat(ArchiveStreamProvider.SupportedExtensions).ToArray();
                            SelectedSystemExtensions = extensions;

                            try
                            {
                                //BIOS trick is now almost useless as the core has it's own BIOS management
                                //it could be dropped in the future?
                                if ((StorageFolder)(await systemRootFolder.TryGetItemAsync("BFiles")) != null)
                                {
                                    SystemCoreIsLoadingState(true);
                                    SetStatus("Wait, RetriXGold tricks...");
                                }
                                else
                                {
                                    SystemCoreIsLoadingState(true);
                                    SetStatus("...");
                                }
                            }
                            catch (Exception eb)
                            {

                            }

                            if (eventHandler != null)
                            {
                                eventHandler.Invoke(null, null);
                            }
                        }

                        setGamesListState(true);
                        SystemCoreIsLoadingState(false);
                        SetStatus("...");
                        RootFolderInProgress = false;
                        SystemSelectDone = true;
                    }
                    else
                    {
                        CheckByTokenActive = false;
                        RootFolderInProgress = false;
                        SystemSelectDone = true;
                    }
                }
                else
                {
                    RootFolderInProgress = false;
                    SystemSelectDone = true;
                }
            }
            catch (Exception e)
            {
                RootFolderInProgress = false;
                SystemSelectDone = true;
                PlatformService.ShowErrorMessage(e);
            }
        }
        public void setGamesListState(bool state)
        {
            try
            {
                GamesListVisible = state;

                RaisePropertyChanged(nameof(GamesListVisible));
                PlatformService.SetSubPageState(GamesListVisible);
                if (SelectedSystem.AnyCore)
                {
                    setShowAnyCoreOptions(true);
                }
                else
                {
                    setShowAnyCoreOptions(false);
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }
        bool DeleteRecentInProgress = false;
        public async Task GameSystemRecentsHoldingHandler(GameSystemRecentModel recent, EventHandler eventHandler = null)
        {
            try
            {
                if (targetFilesRequested)
                {
                    DeleteRecentInProgress = true;
                    PlatformService.PlayNotificationSound("alert");
                    while (PlatformService.DialogInProgress)
                    {
                        await Task.Delay(1000);
                    }
                    ConfirmConfig confirmRecentDelete = new ConfirmConfig();
                    confirmRecentDelete.SetTitle("Game Info");
                    confirmRecentDelete.SetMessage($"Do you want to get size and date for this file?\n{recent.GameName}");
                    confirmRecentDelete.UseYesNo();
                    confirmRecentDelete.OkText = "Get Info";
                    confirmRecentDelete.CancelText = "Cancel";
                    bool RootFolderState = PlatformService.ExtraConfirmation ? await UserDialogs.Instance.ConfirmAsync(confirmRecentDelete) : true;
                    if (RootFolderState)
                    {
                        //SystemCoreIsLoadingState(true);
                        recent.ProgressState = Visibility.Visible;

                        {
                            var info = await recent.attachedFile.GetBasicPropertiesAsync();
                            var GameSize = (long)info.Size;
                            var GameDate = info.DateModified;
                            //SystemCoreIsLoadingState(false);
                            recent.ProgressState = Visibility.Collapsed;
                            await GeneralDialog($"{recent.attachedFile.Name}\nSize: {GameSize.ToFileSize()}\nDate: {GameDate.UtcDateTime}\nPath: {Path.GetDirectoryName(recent.attachedFile.Path)}");
                        }
                    }
                    DeleteRecentInProgress = false;
                    //SystemCoreIsLoadingState(false);
                    recent.ProgressState = Visibility.Collapsed;
                }
                else
                if (recent != null && !recent.NewGame && !DeleteRecentInProgress)
                {
                    DeleteRecentInProgress = true;
                    PlatformService.PlayNotificationSound("alert");
                    while (PlatformService.DialogInProgress)
                    {
                        await Task.Delay(1000);
                    }
                    ConfirmConfig confirmRecentDelete = new ConfirmConfig();
                    confirmRecentDelete.SetTitle("Delete Recent");
                    confirmRecentDelete.SetMessage($"Do you want to delete this record?\n{recent.GameName}");
                    confirmRecentDelete.UseYesNo();
                    confirmRecentDelete.OkText = "Delete";
                    confirmRecentDelete.CancelText = "Cancel";
                    bool RootFolderState = await UserDialogs.Instance.ConfirmAsync(confirmRecentDelete);
                    if (RootFolderState)
                    {
                        recent.ProgressState = Visibility.Visible;
                        //SystemCoreIsLoadingState(true);
                        await PlatformService.AddGameToRecents(GetCoreNameCleanForSelectedSystem(), recent.GameSystem, recent.GameLocation, false, recent.GameID, 0, SelectedSystem.Core.IsNewCore, true);
                        DeleteRecentInProgress = false;
                    }
                    else
                    {
                        DeleteRecentInProgress = false;
                    }
                    if (eventHandler != null)
                    {
                        eventHandler.Invoke(RootFolderState, null);
                    }
                    recent.ProgressState = Visibility.Collapsed;
                    SystemCoreIsLoadingState(false);
                }
                else if (recent != null && recent.NewGame && !DeleteRecentInProgress)
                {
                    DeleteRecentInProgress = true;
                    PlatformService.PlayNotificationSound("alert");
                    while (PlatformService.DialogInProgress)
                    {
                        await Task.Delay(1000);
                    }
                    ConfirmConfig confirmRecentDelete = new ConfirmConfig();
                    confirmRecentDelete.SetTitle("Game Info");
                    confirmRecentDelete.SetMessage($"Do you want to get size and date for this file?\n{recent.GameName}");
                    confirmRecentDelete.UseYesNo();
                    confirmRecentDelete.OkText = "Get Info";
                    confirmRecentDelete.CancelText = "Cancel";
                    bool RootFolderState = PlatformService.ExtraConfirmation ? await UserDialogs.Instance.ConfirmAsync(confirmRecentDelete) : true;
                    if (RootFolderState)
                    {
                        var rootFolder = GetRootFolerBySystemName(SelectedSystem.Name);
                        //SystemCoreIsLoadingState(true);
                        recent.ProgressState = Visibility.Visible;
                        var fileName = recent.GameLocation.Replace($"{rootFolder.Path}\\", "");
                        var GameFile = (StorageFile)await rootFolder.TryGetItemAsync(fileName);
                        if (GameFile != null)
                        {
                            var info = await GameFile.GetBasicPropertiesAsync();
                            var GameSize = (long)info.Size;
                            var GameDate = info.DateModified;
                            //SystemCoreIsLoadingState(false);
                            recent.ProgressState = Visibility.Collapsed;
                            await GeneralDialog($"{GameFile.Name}\nSize: {GameSize.ToFileSize()}\nDate: {GameDate.UtcDateTime}\nPath: {Path.GetDirectoryName(GameFile.Path)}");
                        }
                        else
                        {
                            PlatformService.PlayNotificationSound("faild");
                            await GeneralDialog($"{recent.GameName} not found!\nOriginal Location: {recent.GameLocation}");
                        }

                        DeleteRecentInProgress = false;
                    }
                    else
                    {
                        DeleteRecentInProgress = false;
                    }
                    //SystemCoreIsLoadingState(false);
                    recent.ProgressState = Visibility.Collapsed;
                    if (eventHandler != null)
                    {
                        eventHandler.Invoke(RootFolderState, null);
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.DialogInProgress = false;
                PlatformService.ShowErrorMessage(e);
                DeleteRecentInProgress = false;
                SystemCoreIsLoadingState(false);
                recent.ProgressState = Visibility.Collapsed;
            }
        }

        public void GameSystemRecentSelectedHandler(GameSystemRecentModel recent)
        {
            try
            {
                PlayGameByName(recent.GameName);
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }

        public bool SystemSelected = false;
        public bool SystemSelectDone = false;
        public GameSystemViewModel SelectedSystem = null;
        string[] SelectedSystemExtensions = null;
        public void ClearSelectedSystem()
        {
            try
            {
                if (SelectedSystem != null)
                {
                    //FilesListCache[SelectedSystem.TempName].Clear();
                    GCCollectForList(FilesListCache);
                }
            }
            catch (Exception ex)
            {

            }
            SystemSelected = false;
            SelectedSystem = null;
            SelectedSystemExtensions = null;
        }
        bool requestedByList = false;
        public async Task GameSystemSelectedHandler(GameSystemViewModel system)
        {
            SelectedSystem = null;
            SelectedSystemExtensions = null;
            SystemSelected = false;
            SystemSelectDone = false;
            if (system != null && system.Core.RestartRequired)
            {
                PlatformService.PlayNotificationSound("alert");
                if (!PlatformService.ShowNotificationMain($"Restart required to use this core!\nor try to reset crash log", 4))
                {
                    await GeneralDialog($"Restart required to use this core!\nor try to reset crash log");
                }
                SetCoresQuickAccessContainerState(true);
                return;
            }
            try
            {
                SelectedSystem = system;

                if (system != null && (system.Core.FailedToLoad || system.Core.SkippedCore))
                {
                    if (system.AnyCore)
                    {
                        ConfirmConfig confirmDeleteCore = new ConfirmConfig();
                        confirmDeleteCore.SetTitle("Core Failed");
                        confirmDeleteCore.SetMessage($"Core failed to load!\nDo you want to delete this core?\nSelect 'No' to open core page");
                        confirmDeleteCore.SetCancelText("No");
                        confirmDeleteCore.UseYesNo();
                        bool confirmDeleteCoreState = await UserDialogs.Instance.ConfirmAsync(confirmDeleteCore);
                        if (confirmDeleteCoreState)
                        {
                            SystemCoreIsLoadingState(true, "Deleting core..");
                            system.Core.FreeLibretroCore();
                            StorageFolder zipsDirectory = null;
                            zipsDirectory = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("AnyCore");

                            string targetFileName = System.IO.Path.GetFileName((system.Core.DLLName + (system.Core.DLLName.ToLower().EndsWith(".dll") ? "" : ".dll")));
                            var targetFielTest = (StorageFile)await zipsDirectory.TryGetItemAsync(targetFileName);
                            if (targetFielTest != null)
                            {
                                await targetFielTest.DeleteAsync();
                                await DeleteAnyCore(system.TempName, true);

                                try
                                {
                                    string BIOSFileName = Path.GetFileNameWithoutExtension(((system.Core.DLLName.ToLower().EndsWith(".dll") ? system.Core.DLLName.Replace(".dll", "") : system.Core.DLLName) + ".rab"));
                                    var targetBIOSTest = (StorageFile)await zipsDirectory.TryGetItemAsync(BIOSFileName);
                                    if (targetBIOSTest != null)
                                    {
                                        await targetBIOSTest.DeleteAsync();
                                    }
                                }
                                catch (Exception ex)
                                {

                                }

                                if (system.DLLName != null)
                                {
                                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(system.DLLName, false);
                                }
                                if (system.ConsoleName != null && system.ConsoleName.Length > 0)
                                {
                                    Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(system.ConsoleName, false);
                                }
                                await Task.Delay(2000);
                                system.Core.RestartRequired = true;
                                PlatformService.PlayNotificationSound("success");
                                if (!PlatformService.ShowNotificationMain($"Core deleted, Please restart RetriXGold\nor try to reset crash log", 4))
                                {
                                    await GeneralDialog($"Core deleted, Please restart RetriXGold");
                                }
                                SystemCoreIsLoadingState(false);
                                SetCoresQuickAccessContainerState(true);
                                return;
                            }
                            else
                            {
                                PlatformService.PlayNotificationSound("faild");
                                if (!PlatformService.ShowNotificationMain($"Core not found!, or already deleted.", 3))
                                {
                                    await GeneralDialog($"Core not found!, or already deleted.");
                                }
                                SystemCoreIsLoadingState(false);
                                SetCoresQuickAccessContainerState(true);
                                return;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            StorageFolder zipsDirectory = null;
                            zipsDirectory = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("PureCore");
                            if (zipsDirectory != null)
                            {
                                string targetFileName = System.IO.Path.GetFileName((system.Core.DLLName));
                                var targetFielTest = (StorageFile)await zipsDirectory.TryGetItemAsync(targetFileName);
                                if (targetFielTest != null)
                                {
                                    ConfirmConfig confirmDeleteCore = new ConfirmConfig();
                                    confirmDeleteCore.SetTitle("Core Failed");
                                    confirmDeleteCore.SetMessage($"Core failed to load!\nThis could happen becuase of recent update\nDo you want to delete the last update?\nSelect 'No' to open core page");
                                    confirmDeleteCore.UseYesNo();
                                    bool confirmDeleteCoreState = await UserDialogs.Instance.ConfirmAsync(confirmDeleteCore);

                                    if (confirmDeleteCoreState)
                                    {
                                        SystemCoreIsLoadingState(true, "Removing updates..");
                                        system.Core.FreeLibretroCore();
                                        await targetFielTest.DeleteAsync();

                                        if (system.DLLName != null)
                                        {
                                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(system.DLLName, false);
                                        }
                                        if (system.ConsoleName != null && system.ConsoleName.Length > 0)
                                        {
                                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(system.ConsoleName, false);
                                        }
                                        await Task.Delay(2000);
                                        system.Core.RestartRequired = true;
                                        PlatformService.PlayNotificationSound("success");
                                        if (!PlatformService.ShowNotificationMain($"Updates deleted, Please restart RetriXGold\nor try to reset crash log", 4))
                                        {
                                            await GeneralDialog($"Updates deleted, Please restart RetriXGold");
                                        }
                                        SystemCoreIsLoadingState(false);
                                        SetCoresQuickAccessContainerState(true);
                                        return;
                                    }
                                }
                                else
                                {
                                    //Allow core page to open
                                    //Online/Offline updates could solve the issue
                                }
                            }
                            else
                            {
                                //Allow core page to open
                                //Online/Offline updates could solve the issue
                            }
                        }
                        catch (Exception ex)
                        {
                            PlatformService.DialogInProgress = false;
                            PlatformService.ShowErrorMessage(ex);
                        }
                    }
                    PlatformService.DialogInProgress = false;
                }
            }
            catch (Exception ex)
            {
                PlatformService.DialogInProgress = false;
                PlatformService.ShowErrorMessage(ex);
            }
            try
            {
                if (!RootFolderInProgress && !SelectFileInProgress)
                {
                    StorageFolder TestRootAsk = null;
                    if ((SystemRoots.Count == 0 || !SystemRoots.TryGetValue(folderPickerTokenName, out TestRootAsk) && !PlatformService.IsAppStartedByFile))
                    {
                        if ((!ReSelectIsActive))
                        {
                            PlatformService.PlayNotificationSound("select");
                            SelectedSystem = system;
                            SystemSelected = true;
                            var extensions = system.SupportedExtensions.Concat(ArchiveStreamProvider.SupportedExtensions).ToArray();
                            SelectedSystemExtensions = extensions;
                            GameSystemHoldingHandler(system);
                            return;
                        }
                    }
                    PlatformService.PlayNotificationSound("select");
                    SelectFileInProgress = true;
                    if (SelectedGameFile == null && !SystemSelected)
                    {
                        SystemSelected = true;
                        var extensions = system.SupportedExtensions.Concat(ArchiveStreamProvider.SupportedExtensions).ToArray();
                        SelectedSystemExtensions = extensions;
                        StorageFolder TestRoot = null;
                        if (SystemRoots.Count > 0 && SystemRoots.TryGetValue(folderPickerTokenName, out TestRoot))
                        {
                            setGamesListState(true);
                        }
                        else
                        {
                            SelectedGameFile = new GameFile(await PlatformService.PickSingleFile(extensions));
                            await LoadGameByFile(system, SelectedGameFile);
                        }
                    }
                    else
                    {
                        await LoadGameByFile(system, SelectedGameFile);
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.DialogInProgress = false;
                PlatformService.ShowErrorMessage(e);
                SystemSelected = false;
            }
            SelectFileInProgress = false;
        }
        public async Task StartSingleGame(StorageFile selectedFile, bool customStart = false)
        {
            try
            {
                if (selectedFile != null)
                {
                    SelectedGameFile = new GameFile(selectedFile);
                    await LoadGameByFile(SelectedSystem, SelectedGameFile, false, false, true, selectedFile, customStart);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }
        public async void BrowseSingleGame()
        {
            try
            {
                var selectedFile = await PlatformService.PickSingleFile(SelectedSystemExtensions);
                if (selectedFile != null)
                {
                    SelectedGameFile = new GameFile(selectedFile);
                    await LoadGameByFile(SelectedSystem, SelectedGameFile, false, false, true, selectedFile);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
        }

        private async Task LoadGameByFile(GameSystemViewModel system, GameFile SelectedGameFile, bool arcadeSmartCheck = false, bool psxWithAnalog = false, bool singleFile = false, StorageFile sFile = null, bool customStart = false)
        {
            if (SelectedGameFile == null)
            {
                SystemSelected = false;
            }
            else
            {
                SystemCoreIsLoadingState(true);
                await StartGameAsync(system, SelectedGameFile, arcadeSmartCheck, psxWithAnalog, singleFile, sFile, customStart);
                SystemSelected = false;
            }
        }

        public bool searchCleardByGetter = false;
        public bool CoresQuickAccessContainerState = false;

        public bool ReloadGamesVisible = false;
        public void ReloadGamesVisibleState(bool VisibleState)
        {
            ReloadGamesVisible = VisibleState;
            RaisePropertyChanged(nameof(ReloadGamesVisible));
        }
        public void SetCoresQuickAccessContainerState(bool EnableeState)
        {
            CoresQuickAccessContainerState = EnableeState;
            RaisePropertyChanged(nameof(CoresQuickAccessContainerState));
        }

        public Dictionary<string, Dictionary<string, string>> FilesListCache = new Dictionary<string, Dictionary<string, string>>();
        EventHandler eventHandlerTemp = null;
        public string GetCoreNameCleanForSelectedSystem()
        {
            if (SelectedSystem != null)
            {
                return SelectedSystem.Core.Name.Replace("-", "_").Replace("&", "").Replace("'", "").Replace("  ", " ").Replace(" ", "_");
            }
            else
            {
                return "none";
            }
        }

        public bool gamesLoadingInProgress = false;
        public List<string> GamesSubFolders = new List<string>();
        public List<string> GamesExtens = new List<string>();
        public async void GetAllGames(EventHandler eventHandler = null, bool ReloadGamesList = false, bool ignorePicker = false, EventHandler NoGamesEventHandler = null)
        {
            try
            {
                if (gamesLoadingInProgress)
                {
                    return;
                }
                gamesLoadingInProgress = true;
                SearchBoxVisible = false;
                RaisePropertyChanged(nameof(SearchBoxVisible));
                if (SelectedSystem != null)
                {
                    var testRoot = GetRootFolerBySystemName(SelectedSystem.TempName);
                    if (testRoot == null)
                    {
                        var gamesFolderSelectionState = await PlatformService.IsCoreGamesFolderAlreadySelected(folderPickerTokenName);
                        var systemRootFolder = await PlatformService.PickDirectory(folderPickerTokenName, ReSelectIsActive, ignorePicker);
                        if (systemRootFolder != null)
                        {
                            StorageFolder TestRoot = null;
                            if (SystemRoots.Count > 0 && SystemRoots.TryGetValue(folderPickerTokenName, out TestRoot))
                            {
                                SystemRoots[folderPickerTokenName] = systemRootFolder;
                            }
                            else
                            {
                                SystemRoots.Add(folderPickerTokenName, systemRootFolder);
                            }
                            if (!CheckByTokenActive && !gamesFolderSelectionState)
                            {
                                /*PlatformService.PlayNotificationSound("success");
                                if (!PlatformService.ShowNotificationMain("Games folder set for " + SelectedSystem.Name, 2))
                                {
                                    await GeneralDialog("Games folder set for " + SelectedSystem.Name, "Games Folder");
                                }*/
                            }
                            CheckByTokenActive = false;

                            SystemCoreIsLoadingState(true);
                            SetStatus("Please Wait...");
                            if (PlatformService.ExtraDelay)
                            {
                                await Task.Delay(PlatformService.isMobile ? 500 : 500);
                            }
                            Dictionary<string, string> FilesListTest = new Dictionary<string, string>();
                            if (FilesListCache.Count > 0 && FilesListCache.TryGetValue(SelectedSystem.TempName, out FilesListTest))
                            {
                                FilesListCache[SelectedSystem.TempName].Clear();
                                
                            }
                            else
                            {
                                try
                                {
                                    FilesListCache.Add(SelectedSystem.TempName, new Dictionary<string, string>());
                                }
                                catch (Exception ed)
                                {
                                    PlatformService.ShowErrorMessage(ed);
                                }
                            }
                        }
                        else
                        {
                            if (!ignorePicker)
                            {
                                PlatformService.PlayNotificationSound("error");
                                if (!PlatformService.ShowNotificationMain("Games folder selection cancelled", 2))
                                {
                                    await GeneralDialog("Games folder selection cancelled", "Games Folder");
                                }
                            }
                            if (GamesMainList.Count == 0)
                            {
                                NoGamesListVisible = true;
                                RaisePropertyChanged(nameof(NoGamesListVisible));
                                if (NoGamesEventHandler != null)
                                {
                                    NoGamesEventHandler.Invoke(null, null);
                                }
                            }
                            gamesLoadingInProgress = false;
                            return;
                        }
                    }

                    eventHandlerTemp = eventHandler;
                    ReloadGamesVisibleState(true);
                    try
                    {
                        GamesMainList.Clear();
                        GamesMainListTemp.Clear();
                        GamesSubFolders.Clear();
                        GamesExtens.Clear();
                        ParentFilter = "";
                        GCCollectForList(GamesMainList);
                        GCCollectForList(GamesMainListTemp);
                        SearchText = "";
                        searchCleardByGetter = true;
                        RaisePropertyChanged(nameof(SearchText));
                        StatusBar = "";
                        RaisePropertyChanged(nameof(StatusBar));
                    }
                    catch (Exception ea)
                    {
                        PlatformService.ShowErrorMessage(ea);
                    }
                    await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
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
                            SystemCoreIsLoadingState(true, $"Getting games..");

                            {
                                string[] FilesList = await GetAllFile(ReloadGamesList);

                                //GamesList.Clear();
                                if (FilesList.Length > 0)
                                {
                                    var snapshotsFolder = await PlatformService.GetRecentsLocationAsync();
                                    var gamesNumber = 0;
                                    var totalFiles = FilesList.Count();
                                    List<GameSystemRecentModel> games = new List<GameSystemRecentModel>();
                                    //var currentEachIndexerLocal = 0;
                                    bool gamesCacheFound = false;

                                    List<GameSystemRecentModel> cacheTest = ReloadGamesList ? null : await GetFormatedGamesListResults();
                                    if (cacheTest != null)
                                    {
                                        gamesCacheFound = true;
                                        games = cacheTest;
                                    }
                                    if (!gamesCacheFound)
                                    {
                                        var taskEachComplete1 = new TaskCompletionSource<bool>();
                                        await Task.Run(async () =>
                                        {
                                            try
                                            {
                                                ParallelOptions po = new ParallelOptions();
                                                po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
                                                Parallel.ForEach(FilesList, po, () => 0, (GameLocation, loopState, n) =>
                                        {
                                            //lock (games)
                                            {
                                                //foreach (var GameLocation in FilesList)
                                                {
                                                    string snapshotLocation = "";
                                                    string GameID = "";
                                                    //gamesNumber++;
                                                    SystemCoreIsLoadingState(true, $"Preparing games ({gamesNumber})..");
                                                    try
                                                    {
                                                        GameID = PlatformService.GetGameIDByLocation(SelectedSystem.Core.Name, SelectedSystem.TempName, GameLocation, SelectedSystem.Core.IsNewCore, true);

                                                        if (GameID.Length > 0)
                                                        {
                                                            bool coverFound = false;
                                                            try
                                                            {
                                                                var SnapshotFile = GetGameCover(GameID).Result;
                                                                if (SnapshotFile != null)
                                                                {
                                                                    snapshotLocation = SnapshotFile.Path;
                                                                    coverFound = true;
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {

                                                            }
                                                            if (!coverFound)
                                                            {
                                                                //After 3.0.5 snapshots will be stored in core's saves folder
                                                                var currentFolder = SelectedSystem.GetSaveDirectoryAsync().Result;
                                                                var tag = "saves";
                                                                var testCustomFolder = PlatformService.GetCustomFolder(SelectedSystem.Core.Name, tag).Result;
                                                                if (testCustomFolder != null)
                                                                {
                                                                    currentFolder = testCustomFolder;
                                                                }
                                                                var SnapshotFile = (StorageFile)currentFolder.TryGetItemAsync($"SavedStates\\{GameID}\\{GetCoreNameCleanForSelectedSystem()}_{GameID}.png").AsTask().Result;
                                                                if (SnapshotFile == null)
                                                                {
                                                                    //Fallback to the old way
                                                                    SnapshotFile = (StorageFile)currentFolder.TryGetItemAsync($"SavedStates\\{GameID}\\{GameID}.png").AsTask().Result;
                                                                }

                                                                if (SnapshotFile == null)
                                                                {
                                                                    //(core name added in 3.0 to avoid conflict between multiple cores for the same system)
                                                                    SnapshotFile = (StorageFile)snapshotsFolder.TryGetItemAsync($"{GetCoreNameCleanForSelectedSystem()}_{GameID}.png").AsTask().Result;
                                                                    if (SnapshotFile == null)
                                                                    {
                                                                        //Fallback to the old way
                                                                        SnapshotFile = (StorageFile)snapshotsFolder.TryGetItemAsync($"{GameID}.png").AsTask().Result;
                                                                    }
                                                                }

                                                                //BMP lookup
                                                                SnapshotFile = (StorageFile)currentFolder.TryGetItemAsync($"SavedStates\\{GameID}\\{GetCoreNameCleanForSelectedSystem()}_{GameID}.bmp").AsTask().Result;
                                                                if (SnapshotFile == null)
                                                                {
                                                                    //Fallback to the old way
                                                                    SnapshotFile = (StorageFile)currentFolder.TryGetItemAsync($"SavedStates\\{GameID}\\{GameID}.bmp").AsTask().Result;
                                                                }

                                                                if (SnapshotFile == null)
                                                                {
                                                                    //(core name added in 3.0 to avoid conflict between multiple cores for the same system)
                                                                    SnapshotFile = (StorageFile)snapshotsFolder.TryGetItemAsync($"{GetCoreNameCleanForSelectedSystem()}_{GameID}.bmp").AsTask().Result;
                                                                    if (SnapshotFile == null)
                                                                    {
                                                                        //Fallback to the old way
                                                                        SnapshotFile = (StorageFile)snapshotsFolder.TryGetItemAsync($"{GameID}.bmp").AsTask().Result;
                                                                    }
                                                                }
                                                                if (SnapshotFile != null)
                                                                {
                                                                    snapshotLocation = SnapshotFile.Path;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    catch (Exception ef)
                                                    {

                                                    }
                                                    string gameName = FilesListCache[SelectedSystem.TempName][GameLocation];
                                                    int OpenCounts = PlatformService.GetGamePlaysCount(GetCoreNameCleanForSelectedSystem(), SelectedSystem.TempName, GameLocation, SelectedSystem.Core.IsNewCore, true);
                                                    long PlayedTime = PlatformService.GetGamePlayedTime(GetCoreNameCleanForSelectedSystem(), SelectedSystem.TempName, GameLocation, SelectedSystem.Core.IsNewCore, true);
                                                    string gameLocation = GameLocation;
                                                    string gameSnapshot = snapshotLocation;
                                                    string gameSystem = SelectedSystem.TempName;
                                                    string gameToken = "";
                                                    bool gameRootNeeded = false;
                                                    bool gameNew = true;
                                                    GameSystemRecentModel gameRow = new GameSystemRecentModel(GameID, gameName, OpenCounts, PlayedTime, gameLocation, gameSnapshot, gameSystem, gameRootNeeded, gameToken, gameNew, true, SelectedSystem.Core.Name);
                                                    games.Add(gameRow);
                                                }
                                            }
                                            Interlocked.Increment(ref gamesNumber);
                                            return gamesNumber;
                                        },
                                       (n) =>
                                       {
                                           if (totalFiles == n || n == 0) try
                                               {
                                                   taskEachComplete1.SetResult(true);
                                               }
                                               catch (Exception e)
                                               {
                                               }
                                       });
                                            }
                                            catch (Exception ex)
                                            {
                                                taskEachComplete1.SetResult(true);
                                            }
                                        });

                                        await taskEachComplete1.Task;
                                    }
                                    if (!gamesCacheFound)
                                    {
                                        await CacheFormatedGamesListResults(games);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            //Below code is disabled, because we already have recent items in main page, there is no need to push played games to the top again
                                            //Update some entries by recents
                                            if (GamesRecentsListTemp != null && GamesRecentsListTemp.Count > 0)
                                            {
                                                foreach (var recentItem in GamesRecentsListTemp)
                                                {
                                                    try
                                                    {
                                                        CancellationTokenSource cts = new CancellationTokenSource();
                                                        ParallelOptions po = new ParallelOptions();
                                                        po.CancellationToken = cts.Token;
                                                        po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
                                                        Parallel.ForEach(games, po, () => 0, (game, loopState, n) =>
                                                        {
                                                            if (game.GameID.Equals(recentItem.GameID))
                                                            {
                                                                try
                                                                {
                                                                    var gameIndex = games.IndexOf(game);
                                                                    games[gameIndex] = recentItem;
                                                                    games[gameIndex].NewGame = true;
                                                                }
                                                                catch (Exception xe)
                                                                {

                                                                }
                                                                cts.Cancel();
                                                            }
                                                            return 0;
                                                        },
                                                    (n) => { });
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                        try
                                        {
                                            //Update covers if exists
                                            await UpdateCoversByFolder(games);
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }

                                    //games = games.OrderByDescending(d => d.GamePlayedTime).ThenByDescending(c => c.GamePlayedTime).ThenBy(x => x.GameName).ToList();
                                    games = games.OrderBy(x => x.GameName).ToList();

                                    foreach (var gameRow in games)
                                    {
                                        GamesMainList.Add(gameRow);
                                        GamesMainListTemp.Add(gameRow);
                                        if (!GamesSubFolders.Contains(gameRow.GameParent))
                                        {
                                            GamesSubFolders.Add(gameRow.GameParent);
                                        }
                                        try
                                        {
                                            var fileExt = Path.GetExtension(gameRow.GameName);
                                            if (!GamesExtens.Contains(fileExt))
                                            {
                                                GamesExtens.Add(fileExt.ToLower());
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                        }
                                    }
                                    NoGamesListVisible = false;
                                    RaisePropertyChanged(nameof(NoGamesListVisible));
                                    SearchBoxVisible = true;
                                    RaisePropertyChanged(nameof(SearchBoxVisible));
                                    StatusBar = $"{GamesMainList.Count}";
                                    RaisePropertyChanged(nameof(StatusBar));
                                    try
                                    {
                                        if (PlatformService.PreventGCAlways)
                                        {
                                            GC.SuppressFinalize(FilesList);
                                            FilesList = null;
                                            GC.SuppressFinalize(games);
                                            games = null;
                                        }
                                    }
                                    catch (Exception e)
                                    {

                                    }
                                }
                                else
                                {
                                    NoGamesListVisible = true;
                                    RaisePropertyChanged(nameof(NoGamesListVisible));
                                    SearchBoxVisible = false;
                                    RaisePropertyChanged(nameof(SearchBoxVisible));
                                    PlatformService.PlayNotificationSound("notice");
                                    if (NoGamesEventHandler != null)
                                    {
                                        NoGamesEventHandler.Invoke(null, null);
                                    }
                                }
                            }
                            if (eventHandler != null)
                            {
                                eventHandler.Invoke(StatusBar, EventArgs.Empty);
                            }
                            SystemCoreIsLoadingState(false);
                            gamesLoadingInProgress = false;
                        }
                        catch (Exception e)
                        {
                            PlatformService.ShowErrorMessage(new Exception($"{e}\nNote: Try to reload the cache to fix this issue"));
                            NoGamesListVisible = true;
                            RaisePropertyChanged(nameof(NoGamesListVisible));
                            SearchBoxVisible = false;
                            RaisePropertyChanged(nameof(SearchBoxVisible));
                            PlatformService.PlayNotificationSound("notice");
                            SystemCoreIsLoadingState(false);
                            gamesLoadingInProgress = false;
                            if (NoGamesEventHandler != null)
                            {
                                NoGamesEventHandler.Invoke(null, null);
                            }
                        }
                        try
                        {
                            if (PlatformService.PreventGCAlways) GC.EndNoGCRegion();
                        }
                        catch (Exception ea)
                        {
                        }
                    });
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                gamesLoadingInProgress = false;
            }
        }
        public async Task CacheGamesListResults(Dictionary<string, string> FilesList)
        {
            try
            {
                SystemCoreIsLoadingState(true, $"Saving results cache..");
                var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("GamesCache", CreationCollisionOption.OpenIfExists);

                var SystemName = SelectedSystem.TempName;
                var CoreName = SelectedSystem.Core.Name;
                var expectedFile = $"{CoreName}_{SystemName}.rtg";

                var targetFile = await localFolder.CreateFileAsync(expectedFile, CreationCollisionOption.ReplaceExisting);

                Encoding unicode = Encoding.Unicode;
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(FilesList));
                using (var stream = await targetFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var outStream = stream.AsStream();
                    await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                    await outStream.FlushAsync();
                }

            }
            catch (Exception e)
            {

            }
        }
        public async Task CacheFormatedGamesListResults(List<GameSystemRecentModel> FilesList, bool showLoader = true)
        {
            try
            {
                if(showLoader)SystemCoreIsLoadingState(true, $"Saving results cache..");
                var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("GamesCache", CreationCollisionOption.OpenIfExists);

                var SystemName = SelectedSystem.TempName;
                var CoreName = SelectedSystem.Core.Name;
                var expectedFile = $"{CoreName}_{SystemName}.rfg";

                var targetFile = await localFolder.CreateFileAsync(expectedFile, CreationCollisionOption.ReplaceExisting);

                Encoding unicode = Encoding.Unicode;
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(FilesList));
                using (var stream = await targetFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var outStream = stream.AsStream();
                    await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                    await outStream.FlushAsync();
                }

            }
            catch (Exception e)
            {

            }
        }
        public async Task<List<GameSystemRecentModel>> GetFormatedGamesListResults()
        {
            try
            {
                if (PlatformService.CacheGamesListResults)
                {
                    var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("GamesCache", CreationCollisionOption.OpenIfExists);

                    var SystemName = SelectedSystem.TempName;
                    var CoreName = SelectedSystem.Core.Name;
                    var expectedFile = $"{CoreName}_{SystemName}.rfg";

                    var targetFile = (StorageFile)await localFolder.TryGetItemAsync(expectedFile);
                    if (targetFile != null)
                    {
                        SystemCoreIsLoadingState(true, $"Getting games cache..");

                        Encoding unicode = Encoding.Unicode;
                        byte[] result;
                        using (var stream = await targetFile.OpenAsync(FileAccessMode.Read))
                        {
                            var outStream = stream.AsStream();
                            using (var memoryStream = new MemoryStream())
                            {
                                outStream.CopyTo(memoryStream);
                                result = memoryStream.ToArray();
                            }
                            await outStream.FlushAsync();
                        }
                        string GamesFileContent = unicode.GetString(result);
                        var dictionaryList = JsonConvert.DeserializeObject<List<GameSystemRecentModel>>(GamesFileContent);

                        return dictionaryList;
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public async Task<Dictionary<string, string>> GetGamesListResults()
        {
            try
            {
                if (PlatformService.CacheGamesListResults)
                {
                    var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("GamesCache", CreationCollisionOption.OpenIfExists);

                    var SystemName = SelectedSystem.TempName;
                    var CoreName = SelectedSystem.Core.Name;
                    var expectedFile = $"{CoreName}_{SystemName}.rtg";

                    var targetFile = (StorageFile)await localFolder.TryGetItemAsync(expectedFile);
                    if (targetFile != null)
                    {
                        SystemCoreIsLoadingState(true, $"Getting results cache..");

                        Encoding unicode = Encoding.Unicode;
                        byte[] result;
                        using (var stream = await targetFile.OpenAsync(FileAccessMode.Read))
                        {
                            var outStream = stream.AsStream();
                            using (var memoryStream = new MemoryStream())
                            {
                                outStream.CopyTo(memoryStream);
                                result = memoryStream.ToArray();
                            }
                            await outStream.FlushAsync();
                        }
                        string GamesFileContent = unicode.GetString(result);
                        var dictionaryList = JsonConvert.DeserializeObject<Dictionary<string, string>>(GamesFileContent);

                        return dictionaryList;
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void GamesListNotVisible()
        {
            try
            {
                NoGamesListVisible = true;
                RaisePropertyChanged(nameof(NoGamesListVisible));
                SearchBoxVisible = false;
                RaisePropertyChanged(nameof(SearchBoxVisible));
                PlatformService.PlayNotificationSound("notice");
            }
            catch (Exception ex)
            {

            }
        }
        int MaxSubDirectory = 3;
        private async Task<string[]> GetAllFile(bool ReloadGames = false)
        {
            SystemCoreIsLoadingState(true);
            var testGetter = new Dictionary<string, string>();

            if (!ReloadGames && FilesListCache.Count > 0 && FilesListCache.TryGetValue(SelectedSystem.TempName, out testGetter) && testGetter.Count > 0)
            {
                //Don't Get Files
            }
            else
            {
                Dictionary<string, string> testList = new Dictionary<string, string>();
                if (FilesListCache.TryGetValue(SelectedSystem.TempName, out testList))
                {
                    FilesListCache[SelectedSystem.TempName].Clear();
                }
                else
                {
                    FilesListCache.Add(SelectedSystem.TempName, new Dictionary<string, string>());
                }
                GCCollectForList(FilesListCache);
                await GetFilesByExtensions(GetRootFolerBySystemName(SelectedSystem.TempName), ReloadGames);
            }
            return FilesListCache[SelectedSystem.TempName].Keys.ToArray();
        }
        private async Task GetFilesByExtensions(StorageFolder directoryInfo, bool ReloadGames, int CurrentSubDirectory = 0)
        {
            if (!ReloadGames)
            {
                var cachedResults = await GetGamesListResults();
                if (cachedResults != null)
                {
                    FilesListCache[SelectedSystem.TempName] = cachedResults;
                    return;
                }
            }
            SystemCoreIsLoadingState(true, $"Getting games..\nThis will take a while, please wait..\nDon't worry results will be cached");
            //var FoldersList = await directoryInfo.GetFoldersAsync();
            //var FilesList = await directoryInfo.GetFilesAsync();
            QueryOptions queryOptions = new QueryOptions();
            if (PlatformService.UseWindowsIndexer)
            {
                try
                {
                    var customExts = new List<string>();
                    if (SelectedSystem.Name.ToLower().Equals("scummvm"))
                    {
                        customExts.Add(".scummvm");
                    }
                    else
                    {
                        customExts.AddRange(SelectedSystem.SupportedExtensions);
                    }
                    foreach (var zItem in ArchiveStreamProvider.SupportedExtensions)
                    {
                        if (!customExts.Contains(zItem))
                        {
                            customExts.Add(zItem);
                        }
                    }
                    queryOptions = new QueryOptions(CommonFileQuery.OrderByName, customExts);
                }
                catch (Exception ex)
                {

                }
            }
            queryOptions.FolderDepth = PlatformService.UseWindowsIndexerSubFolders ? FolderDepth.Deep : FolderDepth.Shallow;
            if (PlatformService.UseWindowsIndexer)
            {
                queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
            }
            //var folders = directoryInfo.CreateFolderQueryWithOptions(queryOptions);
            var files = directoryInfo.CreateFileQueryWithOptions(queryOptions);
            //var FoldersList = await folders.GetFoldersAsync();
            var FilesList = await files.GetFilesAsync();
            if (FilesList == null || FilesList.Count == 0)
            {
                try
                {
                    queryOptions.IndexerOption = IndexerOption.DoNotUseIndexer;
                    files = directoryInfo.CreateFileQueryWithOptions(queryOptions);
                    FilesList = await files.GetFilesAsync();
                }
                catch (Exception ex)
                {

                }
            }
            var gamesNumber = 0;
            foreach (var FileItem in FilesList)
            {
                try
                {
                    if (SelectedSystem.Name.ToLower().Equals("scummvm"))
                    {
                        //ScummVM supports a lot of extensions
                        //If I will get all the matched files it will be chaos
                        //let's get only files with .scummvm and matched archives
                        string[] ScummVMExtensions = new string[] { ".scummvm" };

                        if (ArchiveStreamProvider.SupportedExtensions.Contains(Path.GetExtension(FileItem.Name).ToLower()) && SelectedSystem.Core.NativeArchiveSupport == false && SelectedSystem.MultiFileExtensions == null)
                        {
                            var archiveProvider = new ArchiveStreamProvider(FileItem);
                            var entries = await archiveProvider.ListEntriesAsync();
                            var virtualMainFilePath = entries.FirstOrDefault(d => SelectedSystem.SupportedExtensions.Contains(Path.GetExtension(d.Replace("#", "")).ToLower()));
                            if (!string.IsNullOrEmpty(virtualMainFilePath))
                            {
                                gamesNumber++;
                                SystemCoreIsLoadingState(true, $"Loading games ({gamesNumber})..");
                                FilesListCache[SelectedSystem.TempName].Add(FileItem.Path, FileItem.Name);
                                archiveProvider = null;
                            }
                        }
                        else
                        {
                            if (ScummVMExtensions.Contains(Path.GetExtension(FileItem.Name).ToLower()) || ArchiveStreamProvider.SupportedExtensions.Contains(Path.GetExtension(FileItem.Name).ToLower()))
                            {
                                gamesNumber++;
                                SystemCoreIsLoadingState(true, $"Loading games ({gamesNumber})..");
                                FilesListCache[SelectedSystem.TempName].Add(FileItem.Path, FileItem.Name);
                            }
                        }
                    }
                    else
                    {
                        if (ArchiveStreamProvider.SupportedExtensions.Contains(Path.GetExtension(FileItem.Name).ToLower()) || SelectedSystemExtensions.Contains(Path.GetExtension(FileItem.Name).ToLower()))
                        {
                            if (ArchiveStreamProvider.SupportedExtensions.Contains(Path.GetExtension(FileItem.Name).ToLower()) && SelectedSystem.Core.NativeArchiveSupport == false && SelectedSystem.MultiFileExtensions == null)
                            {
                                //var fileSize = await FileItem.GetLengthAsync();
                                //fileSize = ((fileSize / 1024) / 1024);
                                //if(fileSize < 50)
                                var archiveProvider = new ArchiveStreamProvider(FileItem);
                                var entries = await archiveProvider.ListEntriesAsync();
                                var virtualMainFilePath = entries.FirstOrDefault(d => SelectedSystem.SupportedExtensions.Contains(Path.GetExtension(d.Replace("#", "")).ToLower()));
                                if (!string.IsNullOrEmpty(virtualMainFilePath))
                                {
                                    gamesNumber++;
                                    SystemCoreIsLoadingState(true, $"Loading games ({gamesNumber})..");
                                    FilesListCache[SelectedSystem.TempName].Add(FileItem.Path, FileItem.Name);
                                    archiveProvider = null;
                                }
                            }
                            else
                            {
                                gamesNumber++;
                                SystemCoreIsLoadingState(true, $"Loading games ({gamesNumber})..");
                                FilesListCache[SelectedSystem.TempName].Add(FileItem.Path, FileItem.Name);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            if (FilesListCache[SelectedSystem.TempName] != null && FilesListCache[SelectedSystem.TempName].Count > 0)
            {
                if (PlatformService.CacheGamesListResults)
                {
                    await CacheGamesListResults(FilesListCache[SelectedSystem.TempName]);
                }
            }
            /*CurrentSubDirectory++;
             foreach (var FolderItem in FoldersList)
             {
                 if (CurrentSubDirectory > MaxSubDirectory)
                 {
                     continue;
                 }
                 await GetFilesByExtensions(FolderItem, CurrentSubDirectory);
             }*/
        }
        private string GetFileLocationByName(string FileName)
        {
            if (FilesListCache[SelectedSystem.TempName] != null)
            {
                foreach (var FileItem in FilesListCache[SelectedSystem.TempName].Keys)
                {
                    if (FilesListCache[SelectedSystem.TempName][FileItem].Contains(FileName))
                    {
                        return FileItem;
                    }
                }
            }
            return null;
        }

        public ObservableCollection<GameSystemRecentModel> GamesRecentsListTemp = new ObservableCollection<GameSystemRecentModel>();
        public ObservableCollection<GameSystemRecentModel> GamesMainListTemp = new ObservableCollection<GameSystemRecentModel>();
        public bool filterInProgress = false;
        public bool NoResultsListVisible = false;
        public string SearchText = "";
        public string SearchTextTemp = "";
        public string ParentFilter = "";
        public string CoreTargetFolder = "";
        List<GameSystemRecentModel> FilteredListTemp = new List<GameSystemRecentModel>();
        public bool targetFilesRequested = false;

        public async Task FilterCurrentGamesList(string filterText, EventHandler eventHandler = null)
        {
            try
            {
                if (searchCleardByGetter)
                {
                    searchCleardByGetter = false;
                    if (ParentFilter.Length == 0)
                    {
                        return;
                    }
                }
                SearchTextTemp = filterText;
                filterInProgress = true;
                RaisePropertyChanged(nameof(filterInProgress));
                await Task.Delay(1000);
                SearchText = filterText;
                if (SearchText != SearchTextTemp)
                {
                    return;
                }
                IEnumerable<GameSystemRecentModel> FilteredList = new List<GameSystemRecentModel>();


                if (filterText.Length > 0 || ParentFilter.Length > 0)
                {
                    //var FilteredList = GamesMainListTemp.Where(item => item.GameName.ToLower().Contains(filterText.ToLower()));

                    if (filterText.Length == 0 && ParentFilter.Length > 0)
                    {
                        var parentData = ParentFilter.Split(':');
                        //Usually 'ParentFilter' will contains something like: parent:ABC or ext:.zip
                        switch (parentData[0].ToLower())
                        {
                            case "parent":
                                FilteredListTemp = new List<GameSystemRecentModel>();
                                targetFilesRequested = false;
                                CoreTargetFolder = "";
                                FilteredList = GamesMainListTemp.Where(item => item.GameParent.ToLower().Contains(parentData[1].ToLower()));
                                break;

                            case "ext":
                                FilteredListTemp = new List<GameSystemRecentModel>();
                                targetFilesRequested = false;
                                CoreTargetFolder = "";
                                FilteredList = GamesMainListTemp.Where(item => $".{item.GameType}".ToLower().Contains(parentData[1].ToLower()));
                                break;

                            case "core":
                                FilteredListTemp = new List<GameSystemRecentModel>();
                                StorageFolder targetDir = null;
                                targetFilesRequested = true;
                                switch (parentData[1].ToLower())
                                {
                                    case "system":
                                        targetDir = await SelectedSystem.GetSystemDirectoryAsync();
                                        break;

                                    case "saves":
                                        targetDir = await SelectedSystem.GetSaveDirectoryAsync();
                                        break;
                                }
                                CoreTargetFolder = parentData[1].ToLower();
                                if (targetDir != null)
                                {
                                    QueryOptions queryOptions = new QueryOptions();
                                    queryOptions.FolderDepth = PlatformService.UseWindowsIndexerSubFolders ? FolderDepth.Deep : FolderDepth.Shallow;
                                    if (PlatformService.UseWindowsIndexer)
                                    {
                                        queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
                                    }
                                    StorageFileQueryResult queryResult = targetDir.CreateFileQueryWithOptions(queryOptions);
                                    var files = await queryResult.GetFilesAsync();
                                    if (files != null && files.Count > 0)
                                    {
                                        foreach (var file in files)
                                        {
                                            var snapshot = "";
                                            if (file.Name.EndsWith(".bmp") || file.Name.EndsWith(".png") || file.Name.EndsWith(".jpg") || file.Name.EndsWith(".gif"))
                                            {
                                                snapshot = file.Path;
                                            }
                                            GameSystemRecentModel tempFile = new GameSystemRecentModel(file.Name, file.Name, 0, 0, file.Path, snapshot, "", false);
                                            tempFile.attachedFile = file;
                                            FilteredListTemp.Add(tempFile);
                                        }
                                        FilteredList = FilteredListTemp;
                                    }
                                }
                                break;

                            default:
                                FilteredListTemp = new List<GameSystemRecentModel>();
                                targetFilesRequested = false;
                                break;
                        }
                    }
                    else
                    {
                        try
                        {
                            if (ParentFilter.Length == 0 || !ParentFilter.StartsWith("core:"))
                            {
                                FilteredListTemp = new List<GameSystemRecentModel>();
                                targetFilesRequested = false;
                                CoreTargetFolder = "";
                            }
                            else
                            if (ParentFilter.Length > 0)
                            {
                                var parentData = ParentFilter.Split(':');
                                //Usually 'ParentFilter' will contains something like: parent:ABC or ext:.zip
                                switch (parentData[0].ToLower())
                                {
                                    case "core":
                                        if (!CoreTargetFolder.Equals(parentData[1].ToLower()))
                                        {
                                            FilteredListTemp = new List<GameSystemRecentModel>();
                                            StorageFolder targetDir = null;
                                            targetFilesRequested = true;
                                            switch (parentData[1].ToLower())
                                            {
                                                case "system":
                                                    targetDir = await SelectedSystem.GetSystemDirectoryAsync();
                                                    break;

                                                case "saves":
                                                    targetDir = await SelectedSystem.GetSaveDirectoryAsync();
                                                    break;
                                            }
                                            CoreTargetFolder = parentData[1].ToLower();
                                            if (targetDir != null)
                                            {
                                                QueryOptions queryOptions = new QueryOptions();
                                                queryOptions.FolderDepth = PlatformService.UseWindowsIndexerSubFolders ? FolderDepth.Deep : FolderDepth.Shallow;
                                                if (PlatformService.UseWindowsIndexer)
                                                {
                                                    queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
                                                }
                                                StorageFileQueryResult queryResult = targetDir.CreateFileQueryWithOptions(queryOptions);
                                                var files = await queryResult.GetFilesAsync();
                                                if (files != null && files.Count > 0)
                                                {
                                                    foreach (var file in files)
                                                    {
                                                        var snapshot = "";
                                                        if (file.Name.EndsWith(".bmp") || file.Name.EndsWith(".png") || file.Name.EndsWith(".jpg") || file.Name.EndsWith(".gif"))
                                                        {
                                                            snapshot = file.Path;
                                                        }
                                                        GameSystemRecentModel tempFile = new GameSystemRecentModel(file.Name, file.Name, 0, 0, file.Path, snapshot, "", false);
                                                        tempFile.attachedFile = file;
                                                        FilteredListTemp.Add(tempFile);
                                                    }
                                                    FilteredList = FilteredListTemp;
                                                }
                                            }
                                        }
                                        break;
                                }
                            }

                            //Regex will provide advanced search
                            string filterTextResolved = filterText;
                            filterTextResolved = filterTextResolved.Replace(@"\", @"\\");
                            filterTextResolved = filterTextResolved.Replace(".", @"\.");
                            filterTextResolved = filterTextResolved.Replace("-", @"\-");
                            filterTextResolved = filterTextResolved.Replace("*", @"\*");
                            filterTextResolved = filterTextResolved.Replace("?", @"\?");
                            filterTextResolved = filterTextResolved.Replace("&", @"\&");
                            filterTextResolved = filterTextResolved.Replace("(", @"\(");
                            filterTextResolved = filterTextResolved.Replace(")", @"\)");
                            filterTextResolved = filterTextResolved.Replace("[", @"\[");
                            filterTextResolved = filterTextResolved.Replace("]", @"\]");
                            var regexTest = $"{filterTextResolved.ToLower().Trim().Replace(" ", @"+[\s+\w+.*]+")}";
                            try
                            {
                                if (targetFilesRequested)
                                {
                                    FilteredList = (FilteredListTemp).Where(item => Regex.Match(item.GameName.ToLower(), regexTest, RegexOptions.IgnoreCase).Success);
                                }
                                else
                                {
                                    FilteredList = (GamesMainListTemp).Where(item => Regex.Match(item.GameName.ToLower(), regexTest, RegexOptions.IgnoreCase).Success);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (targetFilesRequested)
                                {
                                    FilteredList = FilteredListTemp.Where(item => Regex.Match(item.GameName.ToLower(), regexTest, RegexOptions.IgnoreCase).Success);
                                }
                                else
                                {
                                    FilteredList = GamesMainListTemp.Where(item => Regex.Match(item.GameName.ToLower(), regexTest, RegexOptions.IgnoreCase).Success);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            //Fallback to normal match method
                            FilteredList = GamesMainListTemp.Where(item => item.GameName.ToLower().Contains(filterText.ToLower()));
                        }
                        if (FilteredList.Count() > 0 && ParentFilter.Length > 0)
                        {
                            var parentData = ParentFilter.Split(':');
                            //Usually 'ParentFilter' will contains something like: parent:ABC or ext:.zip
                            switch (parentData[0].ToLower())
                            {
                                case "parent":
                                    FilteredList = FilteredList.Where(item => item.GameParent.ToLower().Contains(parentData[1].ToLower()));
                                    break;

                                case "ext":
                                    FilteredList = FilteredList.Where(item => $".{item.GameType}".ToLower().Contains(parentData[1].ToLower()));
                                    break;

                                default:
                                    FilteredListTemp = new List<GameSystemRecentModel>();
                                    targetFilesRequested = false;
                                    break;
                            }
                        }
                        else
                        {
                            FilteredListTemp = new List<GameSystemRecentModel>();
                            targetFilesRequested = false;
                        }
                    }
                    GamesMainList.Clear();
                    GCCollectForList(GamesMainList);

                    foreach (var MatchedITem in FilteredList)
                    {
                        GamesMainList.Add(MatchedITem);
                    }
                    NoResultsListVisible = GamesMainList.Count == 0;
                    RaisePropertyChanged(nameof(NoResultsListVisible));
                    StatusBar = $"{GamesMainList.Count}";
                    RaisePropertyChanged(nameof(StatusBar));
                    if (eventHandler != null)
                    {
                        eventHandler.Invoke(StatusBar, null);
                    }
                }
                else
                {
                    FilteredListTemp = new List<GameSystemRecentModel>();
                    targetFilesRequested = false;
                    NoResultsListVisible = false;
                    RaisePropertyChanged(nameof(NoResultsListVisible));
                    GamesMainList.Clear();
                    GCCollectForList(GamesMainList);

                    foreach (var MatchedITem in GamesMainListTemp)
                    {
                        GamesMainList.Add(MatchedITem);
                    }
                    StatusBar = $"{GamesMainList.Count}";
                    RaisePropertyChanged(nameof(StatusBar));
                    if (eventHandler != null)
                    {
                        eventHandler.Invoke(StatusBar, null);
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            filterInProgress = false;
            RaisePropertyChanged(nameof(filterInProgress));
        }

        public bool recentGetterInProgress = false;
        public long totalPlayedTime = 0;
        public long totalDummyOpens = 0;
        public long totalDummyTime = 0;
        public async void GetRecentGames(EventHandler eventHandler = null)
        {
            try
            {
                if (recentGetterInProgress)
                {
                    return;
                }
                totalPlayedTime = 0;
                totalDummyOpens = 0;
                totalDummyTime = 0;
                recentGetterInProgress = true;
                ReloadGamesVisibleState(false);
                try
                {
                    searchCleardByGetter = true;
                    GamesRecentsList.Clear();
                    GamesRecentsListTemp.Clear();
                    //GCCollectForList(GamesRecentsList);
                    //GCCollectForList(GamesRecentsListTemp);
                }
                catch (Exception ea)
                {
                    PlatformService.ShowErrorMessage(ea);
                }
                await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                {
                    try
                    {
                        if (SelectedSystem != null)
                        {
                            var RecentGamesList = PlatformService.GetGamesRecents();
                            //GamesList.Clear();
                            if (RecentGamesList != null && RecentGamesList.Count > 0)
                            {
                                List<GameSystemRecentModel> games = new List<GameSystemRecentModel>();
                                List<string[]> TestGet = new List<string[]>();
                                if (RecentGamesList.TryGetValue(SelectedSystem.TempName, out TestGet) && TestGet.Count > 0)
                                {
                                    var snapshotsFolder = await PlatformService.GetRecentsLocationAsync();

                                    //Get entries for the exact core
                                    foreach (var GameName in TestGet.Where(a => a[7].Equals(GetCoreNameCleanForSelectedSystem())))
                                    {
                                        string snapshotLocation = "";
                                        string GameID = "";
                                        try
                                        {
                                            GameID = GameName[5];
                                            bool coverFound = false;
                                            try
                                            {
                                                var SnapshotFile = await GetGameCover(GameID);
                                                if (SnapshotFile != null)
                                                {
                                                    snapshotLocation = SnapshotFile.Path;
                                                    coverFound = true;
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                            if (!coverFound)
                                            {
                                                var gameSnapshotName = $"{GameID}.png";
                                                //(core name added in 3.0 to avoid conflict between multiple cores for the same system)
                                                var fileName = $"{GetCoreNameCleanForSelectedSystem()}_{gameSnapshotName}";

                                                //After 3.0.5 snapshots will be stored in core's saves folder
                                                var currentFolder = await SelectedSystem.GetSaveDirectoryAsync();
                                                var tag = "saves";
                                                var testCustomFolder = await PlatformService.GetCustomFolder(SelectedSystem.Core.Name, tag);
                                                if (testCustomFolder != null)
                                                {
                                                    currentFolder = testCustomFolder;
                                                }
                                                var SnapshotFile = (StorageFile)await currentFolder.TryGetItemAsync($"SavedStates\\{GameID}\\{GetCoreNameCleanForSelectedSystem()}_{GameID}.png");
                                                if (SnapshotFile == null)
                                                {
                                                    //Fallback to the old way
                                                    SnapshotFile = (StorageFile)await currentFolder.TryGetItemAsync($"SavedStates\\{GameID}\\{GameID}.png");
                                                }

                                                if (SnapshotFile == null)
                                                {
                                                    SnapshotFile = (StorageFile)await snapshotsFolder.TryGetItemAsync(fileName);
                                                    if (SnapshotFile == null)
                                                    {
                                                        //Fallback to the old way
                                                        SnapshotFile = (StorageFile)await snapshotsFolder.TryGetItemAsync(gameSnapshotName);
                                                    }
                                                }

                                                //BMP lookup
                                                SnapshotFile = (StorageFile)await currentFolder.TryGetItemAsync($"SavedStates\\{GameID}\\{GetCoreNameCleanForSelectedSystem()}_{GameID}.bmp");
                                                if (SnapshotFile == null)
                                                {
                                                    //Fallback to the old way
                                                    SnapshotFile = (StorageFile)await currentFolder.TryGetItemAsync($"SavedStates\\{GameID}\\{GameID}.bmp");
                                                }

                                                if (SnapshotFile == null)
                                                {
                                                    SnapshotFile = (StorageFile)await snapshotsFolder.TryGetItemAsync(fileName.Replace(".png", ".bmp"));
                                                    if (SnapshotFile == null)
                                                    {
                                                        //Fallback to the old way
                                                        SnapshotFile = (StorageFile)await snapshotsFolder.TryGetItemAsync(gameSnapshotName.Replace(".png", ".bmp"));
                                                    }
                                                }
                                                if (SnapshotFile != null)
                                                {
                                                    snapshotLocation = SnapshotFile.Path;
                                                }
                                            }
                                        }
                                        catch (Exception ef)
                                        {

                                        }

                                        long PlayedTime = 0;
                                        try
                                        {
                                            PlayedTime = Int32.Parse(GameName[6]);
                                        }
                                        catch (Exception e)
                                        {

                                        }
                                        int gameCount = Int32.Parse(GameName[4]);
                                        string gameLocation = GameName[1];
                                        bool gameRootNeeded = GameName[3] == "1";
                                        string gameToken = GameName[2];
                                        string gameName = GameName[0];
                                        bool GameNew = false;
                                        if (SelectedSystem == null)
                                        {
                                            if (eventHandler != null)
                                            {
                                                eventHandler.Invoke(this, EventArgs.Empty);
                                            }
                                            NoRecentsListVisible = true;
                                            RaisePropertyChanged(nameof(NoRecentsListVisible));
                                            recentGetterInProgress = false;
                                            return;
                                        }
                                        GameSystemRecentModel gameRow = new GameSystemRecentModel(GameID, gameName, gameCount, PlayedTime, gameLocation, snapshotLocation, SelectedSystem.TempName, gameRootNeeded, gameToken, GameNew, false, SelectedSystem.Core.Name);
                                        if (!gameRow.GameFailed)
                                        {
                                            games.Add(gameRow);
                                            totalPlayedTime += PlayedTime;
                                            if (GameID.Equals("dummy"))
                                            {
                                                try
                                                {
                                                    totalDummyOpens += Int32.Parse(GameName[4]);
                                                    totalDummyTime += PlayedTime;
                                                }
                                                catch (Exception e)
                                                {

                                                }
                                            }
                                        }
                                    }

                                    games = games.OrderByDescending(d => d.GamePlayedTime).ThenByDescending(c => c.GamePlayedTime).ThenBy(x => x.GameName).ToList();

                                    foreach (GameSystemRecentModel item in games)
                                    {
                                        //GamesList.Add(item.Key);
                                        if (item.GameID.Equals("dummy"))
                                        {
                                            //No need to add dummy entry
                                            //dummy used only to keep tracking core/system usage if there is no content
                                            continue;
                                        }
                                        GamesRecentsList.Add(item);
                                        GamesRecentsListTemp.Add(item);
                                    }

                                    NoRecentsListVisible = false;
                                    string ExtraInfo = "";
                                    if (totalPlayedTime > 0)
                                    {
                                        ExtraInfo = $", [{GameSystemRecentModel.FormatTotalPlayedTime(totalPlayedTime)}] Total Play";
                                    }
                                    /*if (eventHandler != null)
                                    {
                                        eventHandler.Invoke(this, EventArgs.Empty);
                                    }*/
                                }
                                else
                                {
                                    NoRecentsListVisible = true;
                                    //PlatformService.PlayNotificationSound("notice");
                                }
                                RaisePropertyChanged(nameof(NoRecentsListVisible));
                            }
                            else
                            {
                                NoRecentsListVisible = true;
                                RaisePropertyChanged(nameof(NoRecentsListVisible));
                                //PlatformService.PlayNotificationSound("notice");
                            }
                            if (eventHandler != null)
                            {
                                eventHandler.Invoke(this, EventArgs.Empty);
                            }
                            recentGetterInProgress = false;

                        }
                    }
                    catch (Exception e)
                    {
                        if (eventHandler != null)
                        {
                            eventHandler.Invoke(this, EventArgs.Empty);
                        }
                        PlatformService.ShowErrorMessage(e);
                        NoRecentsListVisible = true;
                        RaisePropertyChanged(nameof(NoRecentsListVisible));
                        //PlatformService.PlayNotificationSound("notice");
                        recentGetterInProgress = false;
                    }
                });
            }
            catch (Exception e)
            {
                recentGetterInProgress = false;

                PlatformService.ShowErrorMessage(e);
                if (eventHandler != null)
                {
                    eventHandler.Invoke(this, EventArgs.Empty);
                }
            }

        }

        public async void ResolveGames()
        {
            try
            {
                var resolvedGames = 0;
                foreach (var rItem in GamesRecentsList)
                {

                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }

        public async Task<bool> PlayGameByNamePathTest(string GameName)
        {
            try
            {
                if (SelectedSystem != null)
                {
                    string GameLocation = GetFileLocationByName(GameName);

                    if (GameLocation == null)
                    {
                        GameLocation = PlatformService.GetGameLocation(GetCoreNameCleanForSelectedSystem(), SelectedSystem.TempName, GameName, SelectedSystem.Core.IsNewCore);
                    }
                    if (GameLocation != null)
                    {
                        StorageFolder directoryInfo = GetRootFolerBySystemName(SelectedSystem.TempName);
                        if (GameLocation.Contains(directoryInfo.Path))
                        {
                            //Game Found
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.DialogInProgress = false;
                PlatformService.ShowErrorMessage(e);
            }
            return true;
        }

        public async void PlayGameByName(string GameName, bool arcadeSmartCheck = false, bool psxWithAnalog = false, bool vfsOff = false)
        {
            try
            {
                if (SelectedSystem != null)
                {
                    //First check and verify game folder
                    //usually core page will open directly without any extra checkup

                    StorageFolder TestRoot = null;
                    if (SystemRoots.Count == 0 || !SystemRoots.TryGetValue(folderPickerTokenName, out TestRoot))
                    {
                        var systemRootFolder = await PlatformService.PickDirectory(folderPickerTokenName, false);
                        SystemRoots.Add(folderPickerTokenName, systemRootFolder);
                        Dictionary<string, string> FilesListTest = new Dictionary<string, string>();
                        if (FilesListCache.Count > 0 && FilesListCache.TryGetValue(folderPickerTokenName, out FilesListTest))
                        {
                            FilesListCache[folderPickerTokenName].Clear();
                            
                        }
                        else
                        {
                            try
                            {
                                FilesListCache.Add(folderPickerTokenName, new Dictionary<string, string>());
                            }
                            catch (Exception ed)
                            {
                                PlatformService.ShowErrorMessage(ed);
                            }
                        }
                    }


                    bool isArchive = false;
                    var ext = Path.GetExtension(GameName);
                    switch (ext)
                    {
                        case ".zip":
                        case ".7z":
                        case ".rar":
                        case ".gz":
                        case ".tar":
                            isArchive = true;
                            break;
                    }


                    bool isCompressed = false;
                    bool isCompressedNonZip = false;
                    try
                    {
                        if (!SelectedSystem.Core.NativeArchiveSupport)
                        {
                            ext = Path.GetExtension(GameName);
                            switch (ext)
                            {
                                case ".zip":
                                case ".7z":
                                case ".rar":
                                case ".gz":
                                case ".tar":
                                    isCompressed = true;
                                    break;
                            }
                        }
                        if (!isCompressed && !SelectedSystem.Core.NativeArchiveNonZipSupport)
                        {
                            ext = Path.GetExtension(GameName);
                            switch (ext)
                            {
                                case ".7z":
                                case ".rar":
                                case ".gz":
                                case ".tar":
                                    isCompressedNonZip = true;
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    var pattern = @"\s\(\d+\)$";
                    GameName = Regex.Replace(GameName, pattern, "");
                    while (PlatformService.DialogInProgress)
                    {
                        await Task.Delay(1000);
                    }
                    ConfirmConfig confirmPlayGame = new ConfirmConfig();
                    confirmPlayGame.SetTitle("Start Play?");
                    string advice = "";
                    bool IgnoreWarning = false;
                    switch (SelectedSystem.Core.Name)
                    {
                        case "DOSBox-pure":
                            if (!isArchive)
                            {
                                ext = Path.GetExtension(GameName);
                                switch (ext)
                                {
                                    case ".img":
                                    case ".iso":
                                    case ".vhd":
                                    case ".dsk":
                                        break;

                                    default:
                                        advice = "\n\nNote: If this rom contains multiple files it's better to load it compressed due VFS issue";
                                        break;
                                }
                            }
                            break;

                        case "FCEUmm":
                        case "Nestopia":
                        case "QuickNES":
                        case "Gambatte":
                        case "Gearboy":
                        case "Genesis Plus GX":
                        case "Genesis Plus GX Wide":
                        case "Mednafen NeoPop":
                        case "Beetle NeoPop":
                        case "RACE":
                        case "Mednafen WonderSwan":
                        case "Beetle WonderSwan":
                        case "melonDS":
                        case "DeSmuME":
                        case "ParaLLEl N64":
                        case "PicoDrive":
                        case "Snes9x":
                        case "Snes9x 2005":
                        case "VBA-M":
                        case "VBA Next":
                        case "Meteor GBA":
                        case "TGB Dual":
                        case "Beetle VB":
                        case "Mednafen VB":
                        case "Beetle Lynx":
                        case "Mednafen Lynx":
                        case "Handy":
                        case "Atari Lynx":
                        case "ZX81":
                        case "Sinclair ZX81":
                        case "EightyOne":
                        case "Game & Watch":
                        case "GW":
                        case "Intellivision":
                        case "FreeIntv":
                        case "Amstrad CPC":
                        case "Caprice32":
                        case "cap32":
                        case "VecX":
                        case "ProSystem":
                        case "Virtual Jaguar":
                        case "Beetle Saturn":
                        case "Yabause":
                        case "Atari 5200":
                        case "Atari800":
                        case "MSX Computer":
                        case "BlueMSX":
                        case "fMSX":
                        case "FreeChaF":
                        case "O2EM":
                        case "o2em":
                        case "Magnavox Odyssey 2":
                        case "Game MUsic":
                        case "GME":
                        case "Game Music Emu":
                        case "PokeMini":
                        case "Stella":
                        case "Stella 2014":
                        case "Potator":
                        case "JNB":
                        case "Jump n Bump":
                        case "TIC-80":
                        case "LowResNX":
                        case "LowRes NX":
                        case "Lowres NX":
                        case "PC 8000-8800":
                        case "Quasi88":
                        case "Gearcoleco":
                        case "GearColeco":
                            IgnoreWarning = true;
                            break;
                    }

                    if (vfsOff)
                    {
                        advice = "\n\nRemember: VFS will be disabled for this request.";

                    }
                    if (GamePlayerView.isUpscaleActive)
                    {
                        advice = "\n\nImportant: AI upscale (BETA) will use extra processing and memory, slow performance and issues with low hardware";
                    }
                    if ((SelectedSystem.Core.NativeArchiveSupport && !isCompressedNonZip))
                    {
                        PlatformService.PlayNotificationSound("alert");
                        confirmPlayGame.SetMessage("Do you want to start:\n" + Path.GetFileName(GameName) + $"{advice}");
                    }
                    else
                    {
                        if (!IgnoreWarning)
                        {
                            PlatformService.PlayNotificationSound("notice");
                        }
                        else
                        {
                            PlatformService.PlayNotificationSound("alert");
                        }
                        if (isCompressedNonZip)
                        {
                            if (!IgnoreWarning)
                            {
                                confirmPlayGame.SetMessage("Do you want to start:\n" + Path.GetFileName(GameName) + $"\n\nNote: Archives (7z, rar, tar) are not supported by this core, RetriXGold will extract the content to temp folder and resolve the links{advice}");
                            }
                            else
                            {
                                confirmPlayGame.SetMessage("Do you want to start:\n" + Path.GetFileName(GameName) + $"{advice}");
                            }
                        }
                        else if (isCompressed)
                        {
                            if (!IgnoreWarning)
                            {
                                confirmPlayGame.SetMessage("Do you want to start:\n" + Path.GetFileName(GameName) + $"\n\nNote: Native archive is not supported by this core, RetriXGold will extract the content to temp folder and resolve the links{advice}");
                            }
                            else
                            {
                                confirmPlayGame.SetMessage("Do you want to start:\n" + Path.GetFileName(GameName) + $"{advice}");
                            }
                        }
                        else if (!SelectedSystem.Core.VFSSupport)
                        {
                            if (!IgnoreWarning)
                            {
                                confirmPlayGame.SetMessage("Do you want to start:\n" + Path.GetFileName(GameName) + $"\n\nNote: VFS is not supported by this core, RetriXGold will copy the content to temp folder and resolve the links\n\nImportant: If the game contains multiple files load it compressed otherwise it will fail{advice}");
                            }
                            else
                            {
                                confirmPlayGame.SetMessage("Do you want to start:\n" + Path.GetFileName(GameName) + $"{advice}");
                            }
                        }
                        else if (SelectedSystem.Core.VFSSupport)
                        {
                            confirmPlayGame.SetMessage("Do you want to start:\n" + Path.GetFileName(GameName) + $"{advice}");
                        }
                        else
                        {
                            confirmPlayGame.SetMessage("Do you want to start:\n" + Path.GetFileName(GameName) + $"\n\nNote: File will copied to temp folder due missing VFS or Archive support, RetriXGold will resolve the links.{advice}");
                        }
                    }
                    confirmPlayGame.UseYesNo();
                    bool PlayGame = PlatformService.ExtraConfirmation ? await UserDialogs.Instance.ConfirmAsync(confirmPlayGame) : true;
                    if (PlayGame)
                    {
                        string GameLocation = GetFileLocationByName(GameName);
                        if (GameLocation == null)
                        {
                            GameLocation = PlatformService.GetGameLocation(GetCoreNameCleanForSelectedSystem(), SelectedSystem.TempName, GameName, SelectedSystem.Core.IsNewCore);
                        }
                        if (GameLocation != null)
                        {
                            var GameLocationTemp = GameLocation;

                            StorageFolder directoryInfo = GetRootFolerBySystemName(SelectedSystem.TempName);

                            if (GameLocation.Contains(directoryInfo.Path))
                            {
                                if (directoryInfo.Path.EndsWith("\\"))
                                {
                                    //This case happend when selecting root drive, there is no need to add '\' at the end
                                    GameLocation = GameLocation.Replace(directoryInfo.Path, "");
                                }
                                else
                                {
                                    GameLocation = GameLocation.Replace(directoryInfo.Path + @"\", "");
                                }
                                //Send old location with the file to be used as ID
                                StorageFile testFile = null;
                                try
                                {
                                    testFile = (StorageFile)await directoryInfo.TryGetItemAsync(GameLocation);
                                }
                                catch (Exception ex)
                                {

                                }
                                if (testFile != null)
                                {
                                    SelectedGameFile = new GameFile(testFile, GameLocationTemp);

                                    if (SelectedGameFile.file != null)
                                    {
                                        await LoadGameByFile(SelectedSystem, SelectedGameFile, arcadeSmartCheck, psxWithAnalog);
                                    }
                                    else
                                    {
                                        PlatformService.PlayNotificationSound("faild");
                                        await GeneralDialog("Game not found, or games folder is not correct, Original Path:\n" + GameLocation + "\n\nCurrent folder:\n" + directoryInfo.Path, "Start Failed");
                                    }
                                }
                                else
                                {
                                    PlatformService.PlayNotificationSound("faild");
                                    await GeneralDialog("Game not found, or games folder is not correct, Original Path:\n" + GameLocation + "\n\nCurrent folder:\n" + directoryInfo.Path, "Start Failed");
                                }
                            }
                            else
                            {
                                var GameFileName = Path.GetFileName(GameLocation);
                                //Send old location with the file to be used as ID
                                SelectedGameFile = new GameFile((StorageFile)await directoryInfo.TryGetItemAsync(GameFileName), GameLocationTemp);
                                if (SelectedGameFile.file != null)
                                {
                                    await LoadGameByFile(SelectedSystem, SelectedGameFile, arcadeSmartCheck, psxWithAnalog);
                                }
                                else
                                {
                                    bool gameFound = false;
                                    //Try to check if the game in sub folder (as last chance)
                                    var pathParts = GameLocation.Split(Path.DirectorySeparatorChar);
                                    if (pathParts.Length > 0)
                                    {
                                        var gameName = pathParts[pathParts.Length - 1];
                                        var folderParent = pathParts[pathParts.Length - 2];
                                        var expectedGamePath = $@"{folderParent}\{gameName}";
                                        //Send old location with the file to be used as ID
                                        SelectedGameFile = new GameFile((StorageFile)await directoryInfo.TryGetItemAsync(expectedGamePath), GameLocationTemp);
                                        if (SelectedGameFile.file != null)
                                        {
                                            gameFound = true;
                                            await LoadGameByFile(SelectedSystem, SelectedGameFile, arcadeSmartCheck, psxWithAnalog);
                                        }
                                    }

                                    if (!gameFound)
                                    {
                                        PlatformService.PlayNotificationSound("faild");
                                        await GeneralDialog("Game not found, or games folder is not correct, Original Path:\n" + GameLocation + "\n\nCurrent folder:\n" + directoryInfo.Path + "\n\nRetrix can only resolve the path if the game available in the current folder directly, or in sub folder.", "Start Failed");
                                    }
                                }
                            }
                        }
                        else
                        {
                            PlatformService.PlayNotificationSound("faild");
                            await GeneralDialog("Game not found, or games folder is not correct", "Start Failed");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.DialogInProgress = false;
                PlatformService.ShowErrorMessage(e);
            }
        }

        public async Task DeleteGameByName(GameSystemRecentModel game, bool showLoader = false)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("alert");
                ConfirmConfig confirmDelete = new ConfirmConfig();
                if (targetFilesRequested)
                {
                    confirmDelete.SetTitle("Delete File");
                    confirmDelete.SetMessage($"Do you want to delete this file? \n\nImportant: Be careful this action will remove the file from the disk!");
                }
                else
                {
                    confirmDelete.SetTitle("Delete Game");
                    confirmDelete.SetMessage($"Do you want to delete this game? \n\nImportant: Be careful this action will remove the game from the disk!");
                }
                confirmDelete.UseYesNo();

                var GameName = game.GameName;

                var StartDelete = await UserDialogs.Instance.ConfirmAsync(confirmDelete);

                if (StartDelete)
                {
                    if (targetFilesRequested)
                    {
                        await game.attachedFile.DeleteAsync();
                        GamesMainList.Remove(game);
                    }
                    else
                    {
                        GameFile SelectedGameFile = null;
                        if(showLoader)SystemCoreIsLoadingState(true, $"Removing game..\n{GameName}");
                        string GameLocation = GetFileLocationByName(GameName);
                        if (GameLocation == null)
                        {
                            GameLocation = PlatformService.GetGameLocation(GetCoreNameCleanForSelectedSystem(), SelectedSystem.TempName, GameName, SelectedSystem.Core.IsNewCore);
                        }
                        if (GameLocation != null)
                        {
                            var GameLocationTemp = GameLocation;

                            StorageFolder directoryInfo = GetRootFolerBySystemName(SelectedSystem.TempName);

                            if (GameLocation.Contains(directoryInfo.Path))
                            {
                                if (directoryInfo.Path.EndsWith("\\"))
                                {
                                    //This case happend when selecting root drive, there is no need to add '\' at the end
                                    GameLocation = GameLocation.Replace(directoryInfo.Path, "");
                                }
                                else
                                {
                                    GameLocation = GameLocation.Replace(directoryInfo.Path + @"\", "");
                                }
                                //Send old location with the file to be used as ID
                                StorageFile testFile = null;
                                try
                                {
                                    testFile = (StorageFile)await directoryInfo.TryGetItemAsync(GameLocation);
                                }
                                catch (Exception ex)
                                {

                                }
                                if (testFile != null)
                                {
                                    SelectedGameFile = new GameFile(testFile, GameLocationTemp);

                                    if (SelectedGameFile.file != null)
                                    {
                                        await SelectedGameFile.file.DeleteAsync();
                                        PlatformService.ShowNotificationMain($"Game successfully deleted", 3);
                                        GamesMainList.Remove(game);
                                        GamesMainListTemp.Remove(game);

                                        if (showLoader) SystemCoreIsLoadingState(true, $"Updating cache..");
                                        await CacheFormatedGamesListResults(GamesMainListTemp.ToList(), showLoader);
                                    }
                                    else
                                    {
                                        PlatformService.PlayNotificationSound("faild");
                                        await GeneralDialog("Game not found, or games folder is not correct, Original Path:\n" + GameLocation + "\n\nCurrent folder:\n" + directoryInfo.Path, "Start Failed");
                                    }
                                }
                                else
                                {
                                    PlatformService.PlayNotificationSound("faild");
                                    await GeneralDialog("Game not found, or games folder is not correct, Original Path:\n" + GameLocation + "\n\nCurrent folder:\n" + directoryInfo.Path, "Start Failed");
                                }
                            }
                            else
                            {
                                var GameFileName = Path.GetFileName(GameLocation);
                                //Send old location with the file to be used as ID
                                SelectedGameFile = new GameFile((StorageFile)await directoryInfo.TryGetItemAsync(GameFileName), GameLocationTemp);
                                if (SelectedGameFile.file != null)
                                {
                                    await SelectedGameFile.file.DeleteAsync();
                                    PlatformService.ShowNotificationMain($"Game successfully deleted", 3);
                                    GamesMainList.Remove(game);
                                    GamesMainListTemp.Remove(game);

                                    if (showLoader) SystemCoreIsLoadingState(true, $"Updating cache..");
                                    await CacheFormatedGamesListResults(GamesMainListTemp.ToList(), showLoader);
                                }
                                else
                                {
                                    bool gameFound = false;
                                    //Try to check if the game in sub folder (as last chance)
                                    var pathParts = GameLocation.Split(Path.DirectorySeparatorChar);
                                    if (pathParts.Length > 0)
                                    {
                                        var gameName = pathParts[pathParts.Length - 1];
                                        var folderParent = pathParts[pathParts.Length - 2];
                                        var expectedGamePath = $@"{folderParent}\{gameName}";
                                        //Send old location with the file to be used as ID
                                        SelectedGameFile = new GameFile((StorageFile)await directoryInfo.TryGetItemAsync(expectedGamePath), GameLocationTemp);
                                        if (SelectedGameFile.file != null)
                                        {
                                            gameFound = true;
                                            await SelectedGameFile.file.DeleteAsync();
                                            PlatformService.ShowNotificationMain($"Game successfully deleted", 3);
                                            GamesMainList.Remove(game);
                                            GamesMainListTemp.Remove(game);

                                            if (showLoader) SystemCoreIsLoadingState(true, $"Updating cache..");
                                            await CacheFormatedGamesListResults(GamesMainListTemp.ToList(), showLoader);
                                        }
                                    }

                                    if (!gameFound)
                                    {
                                        PlatformService.PlayNotificationSound("faild");
                                        await GeneralDialog("Game not found, or games folder is not correct, Original Path:\n" + GameLocation + "\n\nCurrent folder:\n" + directoryInfo.Path + "\n\nRetrix can only resolve the path if the game available in the current folder directly, or in sub folder.", "Start Failed");
                                    }
                                }
                            }
                        }
                        else
                        {
                            PlatformService.PlayNotificationSound("faild");
                            await GeneralDialog("Game not found, or games folder is not correct", "Start Failed");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
            }
            SystemCoreIsLoadingState(false);
        }
        private StorageFolder GetRootFolerBySystemName(string SystemName)
        {
            StorageFolder SystemRootsTest = null;
            if (SystemRoots.TryGetValue(SystemName, out SystemRootsTest))
            {
                return SystemRootsTest;
            }
            return null;
        }

        void SetStatus(string statusMessage)
        {
            if (statusMessage.Equals("..."))
            {
                statusMessage = "Pease Wait..";
            }
            try
            {
                LoadingStatus = statusMessage;
                RaisePropertyChanged(nameof(LoadingStatus));
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }


        async Task<GameSystemViewModel> ResloveReopenSystemIssue(GameSystemViewModel system, bool psxWithAnalog = false)
        {
            try
            {
                SetStatus($"Loading {system.Name}..");
                //Reload the core for some consoles because of Deinit issues


                var SystemNameTemp = system.Name;
                switch (SystemNameTemp)
                {
                    case "PlayStation":
                    case "PlayStation*":
                        if (psxWithAnalog)
                        {
                            PlatformService.PlayNotificationSound("notice");
                            while (PlatformService.DialogInProgress)
                            {
                                await Task.Delay(1000);
                            }
                            ConfirmConfig confirmPlayGame = new ConfirmConfig();
                            confirmPlayGame.SetTitle("Start Play?");
                            confirmPlayGame.SetMessage("Do you want to use Analog as preferred controller?\nSome games will not detect the gamepad if you attach the Analog controller.");
                            confirmPlayGame.UseYesNo();
                            bool CustomInput = await UserDialogs.Instance.ConfirmAsync(confirmPlayGame);
                            system.Core.ReInitialCore(CustomInput);
                        }
                        else
                        {
                            system.Core.ReInitialCore(false);
                        }
                        break;

                    default:
                        system.Core.ReInitialCore(true);
                        break;
                }
            }
            catch (Exception e)
            {
                PlatformService.DialogInProgress = false;
                PlatformService.ShowErrorMessage(e);
            }
            await Task.Delay(500);
            return system;
        }

        public async Task<StorageFile> GetGameCover(string gameID)
        {
            StorageFile cover = null;
            try
            {
                var systemFolder = await SelectedSystem.GetSystemDirectoryAsync();
                var coversFolder = await GetCoversFolder();
                if (coversFolder == null)
                {
                    coversFolder = await systemFolder.CreateFolderAsync("Covers", CreationCollisionOption.OpenIfExists);
                }
                cover = (StorageFile)await coversFolder.TryGetItemAsync($"{gameID}.png");
                if (cover == null)
                {
                    cover = (StorageFile)await coversFolder.TryGetItemAsync($"{gameID}.jpg");
                }
                if (cover == null)
                {
                    cover = (StorageFile)await coversFolder.TryGetItemAsync($"{gameID}.jpeg");
                }
                if (cover == null)
                {
                    cover = (StorageFile)await coversFolder.TryGetItemAsync($"{gameID}.gif");
                }
                if (cover == null)
                {
                    cover = (StorageFile)await coversFolder.TryGetItemAsync($"{gameID}.bmp");
                }
            }
            catch (Exception ex)
            {

            }
            return cover;
        }
        public async Task<StorageFolder> GetCoversFolder()
        {
            StorageFolder covers = null;
            try
            {
                var systemFolder = await SelectedSystem.GetSystemDirectoryAsync();
                if (systemFolder != null)
                {
                    covers = await systemFolder.CreateFolderAsync("Covers", CreationCollisionOption.OpenIfExists);
                }
            }
            catch (Exception ex)
            {

            }
            return covers;
        }

        private event EventHandler StatusHandler;
        public void StatusHandlerCall(object sender, EventArgs o)
        {
            try
            {
                string Message = ((StatusMessageArgs)o).StatusMessage;
                if (Message != null && Message.Length > 0)
                {
                    SetStatus(Message);
                }
            }
            catch (Exception e)
            {

            }
        }

        public GameSystemViewModel systemTemp;
        bool startCoreInProgress = false;
        public async Task<bool> StartCore()
        {
            if (startCoreInProgress)
            {
                return false;
            }
            startCoreInProgress = true;
            var system = SelectedSystem;
            try
            {
                if (systemTemp != null)
                {
                    systemTemp.Core.FreeLibretroCore();
                }
            }
            catch (Exception e)
            {

            }
            try
            {
                var dependenciesMet = await system.CheckDependenciesMetAsync();
                if (!dependenciesMet)
                {
                    if (!SelectedSystem.Core.FailedToLoad && !SelectedSystem.Core.SkippedCore)
                    {
                        var notificationBody = string.Format(GetLocalString("SystemUnmetDependenciesAlertMessage"), "\u26EF") + "\n\nClick [Ignore] to try without BIOS \n(If the app crashed then BIOS files needed)";
                        PlatformService.PlayNotificationSound("alert");
                        while (PlatformService.DialogInProgress)
                        {
                            await Task.Delay(1000);
                        }
                        ConfirmConfig confirmIgnoreBIOS = new ConfirmConfig();
                        confirmIgnoreBIOS.SetTitle(GetLocalString("SystemUnmetDependenciesAlertTitle"));
                        confirmIgnoreBIOS.SetMessage(notificationBody);
                        confirmIgnoreBIOS.UseYesNo();
                        confirmIgnoreBIOS.OkText = "Ok";
                        confirmIgnoreBIOS.CancelText = "Ignore";
                        bool IgnoreBIOS = await UserDialogs.Instance.ConfirmAsync(confirmIgnoreBIOS);
                        if (IgnoreBIOS)
                        {
                            try
                            {
                                system.Core.FreeLibretroCore();
                            }
                            catch (Exception ex)
                            {

                            }
                            ResetSystemsSelection();
                            startCoreInProgress = false;
                            return false;
                        }
                    }
                    else
                    {
                        PlatformService.ShowNotificationMain($"Core failed to load at the moment!\nTry to restart RetriXGold", 4);
                        ResetSystemsSelection();
                        startCoreInProgress = false;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowNotificationMain($"Error: {ex.Message}", 3);
                ResetSystemsSelection();
                startCoreInProgress = false;
                return false;
            }

            try
            {
                system = await ResloveReopenSystemIssue(system);
                system.Core.RequestNoGame = true;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            try
            {
                SystemCoreIsLoadingState(true);
                SetStatus("Checking core..");
                var result = await GameSystemsProviderService.GenerateGameLaunchEnvironmentAsync(system, null, false, StatusHandler);

                result.Item1.RootNeeded = false;
                SetStatus("Starting core..");


                PlatformService.SetGameStopInProgress(false);
                PlatformService.gameLaunchEnvironment = result.Item1;
                App.rootFrame.Navigate(typeof(GamePlayerView));
                ResetSystemsSelection();
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                startCoreInProgress = false;
                return false;
            }
            startCoreInProgress = false;
            return true;
        }
        bool startGameInProgress = false;
        private async Task StartGameAsync(GameSystemViewModel system, GameFile gfile, bool arcadeSmartCheck = false, bool psxWithAnalog = false, bool singleFile = false, StorageFile sFile = null, bool customStart = false)
        {
            if (startGameInProgress)
            {
                return;
            }
            startGameInProgress = true;
            var file = gfile.file;
            try
            {
                //For some reason allocated memory space didn't get free when use zip rom
                //So I will free the last selected core, just to be sure that it's not the problem
                if (systemTemp != null)
                {
                    systemTemp.Core.FreeLibretroCore();
                }
            }
            catch (Exception e)
            {

            }
            systemTemp = system;
            //Verify the file before open?
            //This step disabled for now, because some core required empty file to run specific games!
            /*var fileLength = file == null ? 0 : (await file.GetBasicPropertiesAsync()).Size;
            if (fileLength == 0)
            {
                ResetSystemsSelection();
                PlatformService.PlayNotificationSound("faild");
                await GeneralDialog("The file is not valid or not accessible, try another one.", "Invalid File");
                return;
            }*/

            try
            {
                system = await ResloveReopenSystemIssue(system, psxWithAnalog);
                system.Core.RequestNoGame = false;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }

            try
            {
                var dependenciesMet = await system.CheckDependenciesMetAsync();
                if (!dependenciesMet)
                {
                    if (!systemTemp.Core.FailedToLoad && !systemTemp.Core.SkippedCore)
                    {
                        var notificationBody = string.Format(GetLocalString("SystemUnmetDependenciesAlertMessage"), "\u26EF") + "\n\nClick [Ignore] to try without BIOS \n(If the app crashed then BIOS files needed)";
                        PlatformService.PlayNotificationSound("alert");
                        while (PlatformService.DialogInProgress)
                        {
                            await Task.Delay(1000);
                        }
                        ConfirmConfig confirmIgnoreBIOS = new ConfirmConfig();
                        confirmIgnoreBIOS.SetTitle(GetLocalString("SystemUnmetDependenciesAlertTitle"));
                        confirmIgnoreBIOS.SetMessage(notificationBody);
                        confirmIgnoreBIOS.UseYesNo();
                        confirmIgnoreBIOS.OkText = "Ok";
                        confirmIgnoreBIOS.CancelText = "Ignore";
                        bool IgnoreBIOS = await UserDialogs.Instance.ConfirmAsync(confirmIgnoreBIOS);
                        if (IgnoreBIOS)
                        {
                            try
                            {
                                system.Core.FreeLibretroCore();
                            }
                            catch (Exception ex)
                            {

                            }
                            ResetSystemsSelection();
                            startGameInProgress = false;
                            return;
                        }
                    }
                    else
                    {
                        PlatformService.ShowNotificationMain($"Core failed to load at the moment!\nTry to restart RetriXGold", 4);
                        ResetSystemsSelection();
                        startGameInProgress = false;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessage(ex);
                startGameInProgress = false;
                return;
            }
            try
            {
                SystemCoreIsLoadingState(true);
                bool RootNeeded = false;
                SetStatus("Checking game..");
                var result = await GameSystemsProviderService.GenerateGameLaunchEnvironmentAsync(system, gfile, null, false, StatusHandler, arcadeSmartCheck, singleFile, sFile, customStart);

                if (result.Item2 == GameLaunchEnvironment.GenerateResult.RootFolderRequired)
                {
                    StorageFolder folder = null;
                    var AutoLoad = false;
                    StorageFolder SystemRootsTest = GetRootFolerBySystemName(system.TempName);
                    if (SystemRootsTest != null)
                    {
                        if (file.Path.Contains(ApplicationData.Current.LocalFolder.Path))
                        {
                            SystemRootsTest = ApplicationData.Current.LocalFolder;
                            string SubRoot = file.Path.Replace(SystemRootsTest.Path, "").Replace(file.Name, "");
                            if (SubRoot.Replace("\\", "").Length > 0)
                            {
                                string[] directories = SubRoot.TrimStart('\\').Split('\\');
                                foreach (string directory in directories)
                                {
                                    if (directory.Length > 0)
                                    {
                                        SystemRootsTest = (StorageFolder)await SystemRootsTest.TryGetItemAsync(directory);
                                    }
                                }
                            }
                        }
                        else
                        {
                            string SubRoot = file.Path.Replace(SystemRootsTest.Path, "").Replace(file.Name, "");
                            if (SubRoot.Replace("\\", "").Length > 0)
                            {
                                string[] directories = SubRoot.TrimStart('\\').Split('\\');
                                foreach (string directory in directories)
                                {
                                    if (directory.Length > 0)
                                    {
                                        SystemRootsTest = (StorageFolder)await SystemRootsTest.TryGetItemAsync(directory);
                                    }
                                }
                            }
                        }
                        folder = SystemRootsTest;
                        AutoLoad = true;
                    }

                    if (!customStart && !AutoLoad && PlatformService.GetGameRootNeeded(GetCoreNameCleanForSelectedSystem(), system.TempName, file.Path, SelectedSystem.Core.IsNewCore))
                    {
                        PlatformService.PlayNotificationSound("root-needed");
                        while (PlatformService.DialogInProgress)
                        {
                            await Task.Delay(1000);
                        }
                        ConfirmConfig confirmLoadAuto = new ConfirmConfig();
                        confirmLoadAuto.SetTitle(GetLocalString("SelectFolderRequestAlertTitle"));
                        confirmLoadAuto.SetMessage(GetLocalString("SelectFolderRequestAlertMessage"));
                        confirmLoadAuto.UseYesNo();
                        AutoLoad = await UserDialogs.Instance.ConfirmAsync(confirmLoadAuto);
                    }
                    else
                    {
                        if (customStart)
                        {
                            folder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("TestContent");
                            if (folder != null)
                            {
                                AutoLoad = true;
                            }
                        }
                    }
                    if (AutoLoad)
                    {
                        if (folder == null)
                        {
                            folder = await PlatformService.PickSingleFolder();
                        }


                        if (folder == null)
                        {
                            ResetSystemsSelection();
                            startGameInProgress = false;
                            return;
                        }

                        if (!Path.GetDirectoryName(file.Path).StartsWith(folder.Path))
                        {
                            ResetSystemsSelection();
                            PlatformService.PlayNotificationSound("faild");
                            await GeneralDialog(GetLocalString("SelectFolderInvalidAlertMessage"), GetLocalString("SelectFolderInvalidAlertTitle"));
                            startGameInProgress = false;
                            return;
                        }
                        SetStatus("Loading game..");
                        result = await GameSystemsProviderService.GenerateGameLaunchEnvironmentAsync(system, gfile, folder, false, StatusHandler, arcadeSmartCheck, singleFile, sFile);
                        RootNeeded = true;
                    }
                    else
                    {
                        if (!file.Path.Contains(".cue"))
                        {
                            SetStatus("Loading game..");
                            await Task.Delay(700);
                            result = await GameSystemsProviderService.GenerateGameLaunchEnvironmentAsync(system, gfile, folder, true, StatusHandler, arcadeSmartCheck, singleFile, sFile);
                        }
                        else
                        {
                            ResetSystemsSelection();
                            startGameInProgress = false;
                            return;
                        }
                    }
                }

                switch (result.Item2)
                {
                    case GameLaunchEnvironment.GenerateResult.DependenciesUnmet:
                        {
                            //Not In use anymore
                            break;
                        }
                    case GameLaunchEnvironment.GenerateResult.NoMainFileFound:
                        {
                            ResetSystemsSelection();
                            PlatformService.PlayNotificationSound("faild");
                            await GeneralDialog(GetLocalString("NoCompatibleFileInArchiveAlertMessage"), GetLocalString("NoCompatibleFileInArchiveAlertTitle"));
                            startGameInProgress = false;
                            return;
                        }
                    case GameLaunchEnvironment.GenerateResult.Success:
                        {

                            result.Item1.RootNeeded = RootNeeded;
                            SetStatus("Starting game..");

                            PlatformService.SetGameStopInProgress(false);
                            PlatformService.gameLaunchEnvironment = result.Item1;
                            App.rootFrame.Navigate(typeof(GamePlayerView));
                            ResetSystemsSelection();
                            startGameInProgress = false;
                            return;
                        }
                    default:
                        throw new Exception("Error detected!, be sure to select the right game for the right system. or maybe the game need root folder");
                }
            }
            catch (Exception e)
            {
                PlatformService.DialogInProgress = false;
                PlatformService.ShowErrorMessage(e);
                ResetSystemsSelection();
            }
            startGameInProgress = false;
        }

        bool SystemsInFilterMode = false;

        private void callResetSelection(object sender, EventArgs e)
        {
            ResetSystemsSelection();
        }
        private void ResetSystemsSelection()
        {
            SetStatus("...");

            try
            {
                if (SystemsInFilterMode)
                {
                    SystemsInFilterMode = false;
                    PlatformService.SetFilterModeState(SystemsInFilterMode);
                    PlatformService.IsAppStartedByFile = false;
                }

                SelectedGameFile = null;
                SystemSelected = false;
                SystemCoreIsLoadingState(false);
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                SystemSelected = false;
                SelectedGameFile = null;
                SystemCoreIsLoadingState(false);
            }
        }

        public void RaiseUpdate(string name)
        {
            RaisePropertyChanged(name);
        }

        static string OptionsSaveLocation = "SaveRecents";
        public static async Task<bool> CoreOptionsStoreAsyncDirect(string core, string system, bool IsNewCore, string GameName = "", bool checkIfSaved = false)
        {
            try
            {
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.CreateFolderAsync("CoresOptions", CreationCollisionOption.OpenIfExists);
                if (localFolder == null)
                {
                    PlatformService.ShowErrorMessage(new Exception("Unable to create cores options folder!"));
                    return false;
                }
                var SystemName = system;
                var CoreName = core;
                var expectedNameTemp = $"{CoreName}_{SystemName}";
                var expectedName = $"{CoreName}_{SystemName}";
                if (GameName.Length > 0)
                {
                    expectedName = $"{CoreName}_{SystemName}_{GameName}";
                }
                if (checkIfSaved)
                {
                    try
                    {
                        var testFile = (StorageFile)await localFolder.TryGetItemAsync($"{expectedName}.rto");
                        if (testFile != null)
                        {
                            //This step (checkIfSaved) made to save cores options if not saved yet
                            //because some core will not generate options until game loaded
                            //and that will make them not visible in core page
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                if (expectedName.EndsWith(".dll"))
                {
                    return false;
                }
                var targetFile = await localFolder.CreateFileAsync($"{expectedName}.rto", CreationCollisionOption.ReplaceExisting);

                Encoding unicode = Encoding.Unicode;
                var options = SystemsOptions[expectedNameTemp];
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(options));
                using (var stream = await targetFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var outStream = stream.AsStream();
                    await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                    await outStream.FlushAsync();
                }
                return true;
            }
            catch (Exception e)
            {

            }
            return false;
        }

        public async Task CoreOptionsStoreAsync(GameSystemViewModel system)
        {
            SystemCoreIsLoadingState(true);
            await CoreOptionsStoreAsyncDirect(system.Core.Name, system.TempName, system.Core.IsNewCore);
            SystemCoreIsLoadingState(false);
        }

        public async Task<bool> OptionsRetrieveAsync(GameSystemViewModel system, string GameName = "")
        {
            try
            {
                var SystemName = system.TempName;
                var CoreName = system.Core.Name;
                var IsNewCore = system.Core.IsNewCore;
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.CreateFolderAsync("CoresOptions", CreationCollisionOption.OpenIfExists);
                if (localFolder == null)
                {
                    return false;
                }
                StorageFile optionsFile = null;
                var expectedName = $"{CoreName}_{SystemName}";
                if (GameName.Length > 0)
                {
                    expectedName = $"{CoreName}_{SystemName}_{GameName}";
                }
                optionsFile = (StorageFile)await localFolder.TryGetItemAsync($"{expectedName}.rto");

                if (optionsFile != null)
                {
                    Encoding unicode = Encoding.Unicode;
                    byte[] result;
                    using (var stream = await optionsFile.OpenAsync(FileAccessMode.Read))
                    {
                        var outStream = stream.AsStream();
                        using (var memoryStream = new MemoryStream())
                        {
                            outStream.CopyTo(memoryStream);
                            result = memoryStream.ToArray();
                        }
                        await outStream.FlushAsync();
                    }
                    string OptionsFileContent = unicode.GetString(result);
                    var dictionaryList = JsonConvert.DeserializeObject<CoresOptions>(OptionsFileContent);
                    SystemsOptions[expectedName] = dictionaryList;
                    return true;
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            return false;
        }
        public async static Task<CoresOptions> OptionsRetrieveAsync(string CoreName, string SystemName, bool IsNewCore, string GameName = "")
        {
            try
            {
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.CreateFolderAsync("CoresOptions", CreationCollisionOption.OpenIfExists);
                if (localFolder == null)
                {
                    return null;
                }
                StorageFile optionsFile = null;
                var expectedNameTemp = $"{CoreName}_{SystemName}";
                var expectedName = $"{CoreName}_{SystemName}";
                if (GameName.Length > 0)
                {
                    expectedName = $"{CoreName}_{SystemName}_{GameName}";
                }
                optionsFile = (StorageFile)await localFolder.TryGetItemAsync($"{expectedName}.rto");

                if (optionsFile != null)
                {
                    Encoding unicode = Encoding.Unicode;
                    byte[] result;
                    using (var stream = await optionsFile.OpenAsync(FileAccessMode.Read))
                    {
                        var outStream = stream.AsStream();
                        using (var memoryStream = new MemoryStream())
                        {
                            outStream.CopyTo(memoryStream);
                            result = memoryStream.ToArray();
                        }
                        await outStream.FlushAsync();
                    }
                    string OptionsFileContent = unicode.GetString(result);
                    var dictionaryList = JsonConvert.DeserializeObject<CoresOptions>(OptionsFileContent);
                    SystemsOptions[expectedNameTemp] = dictionaryList;
                    return dictionaryList;
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
            return null;
        }

        public static async Task<bool> DeleteSavedOptions(GameSystemViewModel system, string GameName = "")
        {
            var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.CreateFolderAsync("CoresOptions", CreationCollisionOption.OpenIfExists);
            if (localFolder == null)
            {
                return false;
            }
            var SystemName = system.TempName;
            var CoreName = system.Core.Name;
            var IsNewCore = system.Core.IsNewCore;
            StorageFile optionsFile = null;
            var expectedName = $"{CoreName}_{SystemName}.rto";
            if (GameName.Length > 0)
            {
                expectedName = $"{CoreName}_{SystemName}_{GameName}.rto";
            }
            optionsFile = (StorageFile)await localFolder.TryGetItemAsync(expectedName);

            if (optionsFile != null)
            {
                await optionsFile.DeleteAsync();
                return true;
            }
            return false;
        }

        public static async Task<bool> DeleteSavedOptions(string CoreName, string SystemName, bool IsNewCore, string GameName = "")
        {
            var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.CreateFolderAsync("CoresOptions", CreationCollisionOption.OpenIfExists);
            if (localFolder == null)
            {
                return false;
            }
            StorageFile optionsFile = null;
            var expectedName = $"{CoreName}_{SystemName}.rto";
            if (GameName.Length > 0)
            {
                expectedName = $"{CoreName}_{SystemName}_{GameName}.rto";
            }
            optionsFile = (StorageFile)await localFolder.TryGetItemAsync(expectedName);

            if (optionsFile != null)
            {
                await optionsFile.DeleteAsync();
                return true;
            }
            return false;
        }

        public async void GetConsoleInfo(EventHandler eventHandler = null, bool dialog = true)
        {
            try
            {
                StorageFile targetFileTest = null;
                if (SelectedSystem != null)
                {
                    string SystemName = SelectedSystem.TempName.Replace("ROCore", "");

                    var localFolder = (StorageFolder)await Package.Current.InstalledLocation.TryGetItemAsync("Infos");
                    if (localFolder == null)
                    {
                        //Try to get info from system folder
                        localFolder = (StorageFolder)await SelectedSystem.GetSystemDirectoryAsync();

                        if (localFolder != null)
                        {
                            targetFileTest = (StorageFile)await localFolder.TryGetItemAsync($"{SystemName}.rti");
                            if (targetFileTest == null)
                            {
                                if (eventHandler != null)
                                {
                                    eventHandler.Invoke(null, null);
                                }
                                return;
                            }
                        }
                        else
                        {
                            if (eventHandler != null)
                            {
                                eventHandler.Invoke(null, null);
                            }
                            return;
                        }
                    }
                    if (targetFileTest == null)
                    {
                        targetFileTest = (StorageFile)await localFolder.TryGetItemAsync($"{SystemName}.rti");
                    }
                    if (targetFileTest == null)
                    {
                        //Try to get info from system folder
                        localFolder = (StorageFolder)await SelectedSystem.GetSystemDirectoryAsync();

                        if (localFolder != null)
                        {
                            targetFileTest = (StorageFile)await localFolder.TryGetItemAsync($"{SystemName}.rti");
                            if (targetFileTest == null)
                            {
                                if (eventHandler != null)
                                {
                                    eventHandler.Invoke(null, null);
                                }
                                return;
                            }
                        }
                        else
                        {
                            if (eventHandler != null)
                            {
                                eventHandler.Invoke(null, null);
                            }
                            return;
                        }
                    }
                    if (targetFileTest != null)
                    {
                        Encoding unicode = Encoding.Unicode;
                        byte[] result;
                        using (var stream = await targetFileTest.OpenAsync(FileAccessMode.Read))
                        {
                            var outStream = stream.AsStream();
                            using (var memoryStream = new MemoryStream())
                            {
                                outStream.CopyTo(memoryStream);
                                result = memoryStream.ToArray();
                            }
                            await outStream.FlushAsync();
                        }
                        string OptionsFileContent = "";
                        try
                        {
                            OptionsFileContent = unicode.GetString(result);
                        }
                        catch
                        {
                            try
                            {
                                unicode = Encoding.UTF8;
                                OptionsFileContent = unicode.GetString(result);
                            }
                            catch
                            {
                                unicode = Encoding.ASCII;
                                OptionsFileContent = unicode.GetString(result);
                            }
                        }
                        Dictionary<string, string> dictionaryList = null;
                        try
                        {
                            dictionaryList = JsonConvert.DeserializeObject<Dictionary<string, string>>(OptionsFileContent);
                        }
                        catch
                        {
                            unicode = Encoding.UTF8;
                            OptionsFileContent = unicode.GetString(result);
                            try
                            {
                                dictionaryList = JsonConvert.DeserializeObject<Dictionary<string, string>>(OptionsFileContent);
                            }
                            catch
                            {
                                unicode = Encoding.ASCII;
                                OptionsFileContent = unicode.GetString(result);
                                dictionaryList = JsonConvert.DeserializeObject<Dictionary<string, string>>(OptionsFileContent);
                            }
                        }
                        string SystemDescriptions = dictionaryList["desc"];
                        string SystemCompany = dictionaryList["company"];
                        string SystemYear = dictionaryList["year"];
                        if (dialog)
                        {
                            string SystemMessage = $"Console: {SystemName}\nCompany: {SystemCompany}\nYear: {SystemYear}\n\n{SystemDescriptions}";
                            PlatformService.PlayNotificationSound("success");
                            await GeneralDialog(SystemMessage);
                        }
                        else
                        {
                            if (eventHandler != null)
                            {
                                eventHandler.Invoke(dictionaryList, null);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (eventHandler != null)
                {
                    eventHandler.Invoke(null, null);
                }
                PlatformService.ShowErrorMessage(e);
            }
        }



        static string AnyCoreSaveLocation = "AnyCore";
        public static async Task AnyCoreStoreAsyncDirect()
        {
            try
            {
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync(AnyCoreSaveLocation);
                if (localFolder == null)
                {
                    localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(AnyCoreSaveLocation);
                }

                var targetFile = await localFolder.CreateFileAsync("cores.rac", CreationCollisionOption.ReplaceExisting);

                Encoding unicode = Encoding.Unicode;
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(SystemsAnyCore));
                using (var stream = await targetFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var outStream = stream.AsStream();
                    await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                    await outStream.FlushAsync();
                }

            }
            catch (Exception e)
            {

            }
        }

        public async Task AnyCoreStoreAsync(bool deleting = false)
        {
            if (!deleting) SystemCoreIsLoadingState(true);
            await AnyCoreStoreAsyncDirect();
            if (!deleting) SystemCoreIsLoadingState(false);
        }

        public static async Task AnyCoreRetrieveAsync()
        {
            try
            {
                var localFolder = (StorageFolder)await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(AnyCoreSaveLocation);
                if (localFolder == null)
                {
                    return;
                }

                var targetFileTest = (StorageFile)await localFolder.TryGetItemAsync("cores.rac");
                if (targetFileTest != null)
                {
                    Encoding unicode = Encoding.Unicode;
                    byte[] result;
                    using (var stream = await targetFileTest.OpenAsync(FileAccessMode.Read))
                    {
                        var outStream = stream.AsStream();
                        using (var memoryStream = new MemoryStream())
                        {
                            outStream.CopyTo(memoryStream);
                            result = memoryStream.ToArray();
                        }
                        await outStream.FlushAsync();
                    }
                    string CoreFileContent = unicode.GetString(result);
                    var dictionaryList = JsonConvert.DeserializeObject<Dictionary<string, GameSystemAnyCore>>(CoreFileContent);

                    if (dictionaryList != null)
                    {
                        SystemsAnyCore = dictionaryList;
                    }

                }

            }
            catch (Exception e)
            {

            }
        }
        public async Task SetAnyCoreInfo(string systemName, string Name, string System = "", string Icon = "")
        {
            try
            {
                if (Icon?.Length > 0 && !Icon.ToLower().Equals(SystemsAnyCore[systemName].CoreIcon.ToLower()) && !SystemsAnyCore[systemName].CoreIcon.Contains("ms-appx"))
                {
                    var localStorage = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("AnyCore");
                    if (localStorage != null)
                    {
                        var fileName = Path.GetFileName(SystemsAnyCore[systemName].CoreIcon);
                        var testFile = (StorageFile)await localStorage.TryGetItemAsync(fileName);
                        if (testFile != null)
                        {
                            await testFile.DeleteAsync();
                        }
                    }
                }

                if (Name?.Length > 0) SystemsAnyCore[systemName].CoreName = Name;
                if (System?.Length > 0) SystemsAnyCore[systemName].CoreSystem = System;
                if (Icon?.Length > 0) SystemsAnyCore[systemName].CoreIcon = Icon;
                if (Name?.Length > 0 || System?.Length > 0 || Icon?.Length > 0)
                {
                    await AnyCoreStoreAsync();
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }
        public async Task DeleteAnyCore(string SystemName, bool deleting)
        {
            try
            {
                GameSystemAnyCore testCore = null;
                if (SystemsAnyCore.TryGetValue(SystemName, out testCore))
                {
                    var SystemIcon = SystemsAnyCore[SystemName].CoreIcon;
                    if (SystemIcon.Length > 0 && !SystemIcon.Contains("ms-appx"))
                    {
                        var localStorage = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("AnyCore");
                        if (localStorage != null)
                        {
                            var fileName = Path.GetFileName(SystemIcon);
                            var testFile = (StorageFile)await localStorage.TryGetItemAsync(fileName);
                            if (testFile != null)
                            {
                                await testFile.DeleteAsync();
                            }
                        }
                    }
                    SystemsAnyCore.Remove(SystemName);
                    await AnyCoreStoreAsync(deleting);
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }

        public static bool isSupportCD(string filePath)
        {
            try
            {
                var DLLName = Path.GetFileNameWithoutExtension(filePath);
                if (SystemsAnyCore.Count > 0)
                {
                    foreach (var SystemItem in SystemsAnyCore.Values)
                    {
                        if (SystemItem.CDSupport)
                        {
                            return SystemItem.CDSupport;
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            return false;
        }

        public static async Task<FileDependency[]> GetAnyCoreBIOSFiles(string filePath)
        {
            try
            {
                Dictionary<string, string[]> BIOSTempList = new Dictionary<string, string[]>();
                var DLLName = Path.GetFileNameWithoutExtension(filePath);
                if (SystemsAnyCore.Count > 0)
                {
                    foreach (var SystemItem in SystemsAnyCore.Values)
                    {
                        if (SystemItem.DLLName != null && SystemItem.DLLName.Replace(".dll", "").Equals(DLLName))
                        {
                            if (SystemItem.BiosFiles == null || SystemItem.BiosFiles.Count == 0)
                            {
                                await SystemItem.RetriveBIOSMap();
                            }
                            BIOSTempList = SystemItem.BiosFiles;
                            break;
                        }
                    }
                    if (BIOSTempList.Count > 0)
                    {
                        List<FileDependency> tempFileDependencies = new List<FileDependency>();
                        foreach (var BIOSItem in BIOSTempList.Keys)
                        {
                            bool isOptional = false;
                            bool isFolder = false;
                            if (BIOSTempList[BIOSItem].Length >= 3)
                            {
                                isOptional = BIOSTempList[BIOSItem][2].Equals("1");
                            }
                            if (BIOSTempList[BIOSItem].Length >= 4)
                            {
                                isFolder = BIOSTempList[BIOSItem][3].Equals("1");
                            }
                            tempFileDependencies.Add(new FileDependency(BIOSItem, BIOSTempList[BIOSItem][0], BIOSTempList[BIOSItem][1], isOptional, isFolder));
                        }
                        return tempFileDependencies.ToArray();
                    }
                }
            }
            catch (Exception e)
            {

            }

            return null;
        }


        public static void GCCollect()
        {
            try
            {
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
            }
            catch (Exception e)
            {

            }
        }
        public static void GCCollectForList<T>(T ListToCollect)
        {
            try
            {
                //int identificador = GC.GetGeneration(ListToCollect);
                //GC.Collect(identificador, GCCollectionMode.Forced);
            }
            catch (Exception e)
            {

            }
        }

        #region BIOS Section
        /*private ObservableCollection<FileImporterViewModel> fileDependencyImporters;
        public ObservableCollection<FileImporterViewModel> FileDependencyImporters
        {
            get => fileDependencyImporters;
            set
            {
                fileDependencyImporters = value;
            }
        }*/

        public ObservableCollection<GroupBIOSListItems> biosGroupped = new ObservableCollection<GroupBIOSListItems>();
        private async Task<List<FileImporterViewModel>> GetFileDependencyImportersAsync()
        {
            try
            {
                var importers = new List<FileImporterViewModel>();
                {
                    foreach (var aItem in SystemsAnyCore)
                    {
                        var dllName = Path.GetFileNameWithoutExtension(aItem.Value.DLLName);
                        if (dllName.Equals(SelectedSystem.DLLName.Replace(".dll", "")) && (SelectedSystem.Core.FileDependencies == null || SelectedSystem.Core.FileDependencies.Count == 0))
                        {
                            await aItem.Value.RetriveBIOSMap();
                            var depsTest = aItem.Value.BiosFiles;
                            if (depsTest != null && depsTest.Count > 0)
                            {
                                var BIOSFiles = await GameSystemSelectionViewModel.GetAnyCoreBIOSFiles(SelectedSystem.DLLName);
                                SelectedSystem.Core.FileDependencies = BIOSFiles.ToList();
                            }
                            break;
                        }
                    }
                    var core = SelectedSystem.Core;
                    var systemFolder = await SelectedSystem.GetSystemDirectoryAsync();
                    foreach (var d in core.FileDependencies)
                    {
                        importers.Add(await FileImporterViewModel.CreateFileImporterAsync(core, systemFolder, d.Name, d.Description, d.MD5, d.Optional, d.IsFolder));
                    }
                }

                return importers;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                return null;
            }
        }
        public bool NoBIOSTextGridVisible = false;
        public bool BIOSGuidTextGridVisible
        {
            get
            {
                return !NoBIOSTextGridVisible;
            }
        }

        public string BIOSHeaderTitle = "BIOS";
        public async void GetFileDependencyForSelectedCore(bool clear = false)
        {
            try
            {
                biosGroupped.Clear();
                ShowBIOSMapOptions = false;
                Dictionary<string, GroupBIOSListItems> groups = new Dictionary<string, GroupBIOSListItems>();
                if (!clear)
                {
                    var FileDependencyImportersList = await GetFileDependencyImportersAsync();
                    if (FileDependencyImportersList == null || FileDependencyImportersList.Count == 0)
                    {
                        NoBIOSTextGridVisible = true;
                        BIOSHeaderTitle = "BIOS";
                    }
                    else
                    {
                        var count = 0;
                        foreach (var item in FileDependencyImportersList)
                        {
                            var groupName = item.isOptional ? "OPTIONAL" : "REQUIRED";
                            GroupBIOSListItems testList;
                            if (!groups.TryGetValue(groupName, out testList))
                            {
                                testList = new GroupBIOSListItems(groupName);
                                groups.Add(groupName, testList);
                            }
                            testList.Add(item);

                            count++;
                        }
                        foreach (var gItem in groups)
                        {
                            biosGroupped.Add(gItem.Value);
                        }
                        NoBIOSTextGridVisible = false;
                        BIOSHeaderTitle = $"BIOS ({count})";
                        if (SelectedSystem.AnyCore)
                        {
                            ShowBIOSMapOptions = true;
                        }
                    }
                }
                else
                {
                    biosGroupped.Clear();
                    NoBIOSTextGridVisible = true;
                    BIOSHeaderTitle = "BIOS";
                }

                RaisePropertyChanged(nameof(biosGroupped));
                RaisePropertyChanged(nameof(NoBIOSTextGridVisible));
                RaisePropertyChanged(nameof(BIOSGuidTextGridVisible));
                RaisePropertyChanged(nameof(BIOSHeaderTitle));
                RaisePropertyChanged(nameof(ShowBIOSMapOptions));
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }
        #endregion

    }
}