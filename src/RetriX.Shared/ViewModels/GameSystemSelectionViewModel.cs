using Acr.UserDialogs;
using LibRetriX;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Platform.Core;
using Newtonsoft.Json;
using Plugin.FileSystem;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Models;
using RetriX.Shared.Services;
using RetriX.Shared.StreamProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RetriX.Shared.ViewModels
{
    public class GameSystemSelectionViewModel : MvxViewModel<IFileInfo>
    {
        private IMvxNavigationService NavigationService { get; }
        private IFileSystem FileSystem { get; }
        private IUserDialogs DialogsService { get; }
        private IPlatformService PlatformService { get; }
        private IGameSystemsProviderService GameSystemsProviderService { get; set; }

        private IFileInfo SelectedGameFile { get; set; }

        private IList<GameSystemViewModel> gameSystems;
        public IList<GameSystemViewModel> GameSystems
        {
            get => gameSystems;
            private set => SetProperty(ref gameSystems, value);
        }

        public IMvxCommand ShowSettings { get; set; }
        public IMvxCommand ShowAbout { get; set; }
        public IMvxCommand ShowHelp { get; set; }
        public IMvxCommand ShowDonate { get; set; }
        public IMvxCommand<GameSystemRecentModel> GameSystemRecentSelected { get; set; }
        public IMvxCommand<GameSystemRecentModel> GameSystemRecentsHolding { get; set; }
        public IMvxCommand<GameSystemViewModel> GameSystemSelected { get; set; }
        public IMvxCommand<GameSystemViewModel> GameSystemHolding { get; set; }

        public bool SystemCoresIsLoading = true;
        public static Dictionary<string, CoresOptions> SystemsOptionsTemp = new Dictionary<string, CoresOptions>();
        public static Dictionary<string, CoresOptions> SystemsOptions = new Dictionary<string, CoresOptions>();
        public static Dictionary<string, GameSystemAnyCore> SystemsAnyCore = new Dictionary<string, GameSystemAnyCore>();
        public ObservableCollection<string> GamesList = new ObservableCollection<string>();

        public ObservableCollection<GameSystemRecentModel> GamesRecentsList = new ObservableCollection<GameSystemRecentModel>();

        public bool GamesListVisible = false;
        public bool NoGamesListVisible = false;
        public bool NoRecentsListVisible = true;
        public bool SystemsLoadFailed = false;
        public string LoadingStatus = "Please wait..";
        public string StatusBar = "";
        public bool ShowAnyCoreOptions = false;
        public bool ShowUpdateOption = false;
        public bool ShowCoreInfo = true;

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
        public void setCoresOptionsDictionary(string SystemName, string OptionKey, uint SelectedIndex)
        {
            SystemsOptions[SystemName].OptionsList[OptionKey].SelectedIndex = SelectedIndex;
        }
        public void resetCoresOptionsDictionary(string SystemName)
        {
            SystemsOptions[SystemName].OptionsList = SystemsOptionsTemp[SystemName].OptionsList;
            DeleteSavedOptions(SystemName);
        }
        public static void setCoresOptionsDictionaryDirect(string SystemName, string OptionKey, uint SelectedIndex)
        {
            SystemsOptions[SystemName].OptionsList[OptionKey].SelectedIndex = SelectedIndex;
        }

        public GameSystemSelectionViewModel(IMvxNavigationService navigationService, IFileSystem fileSystem, IUserDialogs dialogsService, IPlatformService platformService, IGameSystemsProviderService gameSystemsProviderService)
        {
            try
            {
                NavigationService = navigationService;
                FileSystem = fileSystem;
                DialogsService = dialogsService;
                PlatformService = platformService;

                AsyncLoader(navigationService, fileSystem, dialogsService, platformService, gameSystemsProviderService);

            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                SystemCoreIsLoadingState(false);
            }
        }

        bool LoaderReady = false;
        public async void AsyncLoader(IMvxNavigationService navigationService, IFileSystem fileSystem, IUserDialogs dialogsService, IPlatformService platformService, IGameSystemsProviderService gameSystemsProviderService)
        {
            SystemCoreIsLoadingState(true);
            SetStatus("Loading Systems..");
            if (PlatformService != null)
            {
                while (!PlatformService.IsCoresLoaded)
                {
                    await Task.Delay(1000);
                }
            }
            GameSystemsProviderService = gameSystemsProviderService;
            PlatformService.SetResetSelectionHandler(callResetSelection);

            GameSystems = GameSystemsProviderService.Systems;

            ResetSystemsSelection();

            ShowSettings = new MvxCommand(new Action(() =>
            {
                NavigationService.Navigate<SettingsViewModel>();
                if (PlatformService != null)
                {
                    PlatformService.PlayNotificationSound("button-01.mp3");
                }
            }));
            ShowAbout = new MvxCommand(new Action(() =>
            {
                NavigationService.Navigate<AboutViewModel>();
                if (PlatformService != null)
                {
                    PlatformService.PlayNotificationSound("button-01.mp3");
                }
            }));
            ShowHelp = new MvxCommand(new Action(() =>
            {
                NavigationService.Navigate<HelpViewModel>();
                if (PlatformService != null)
                {
                    PlatformService.PlayNotificationSound("button-01.mp3");
                }
            }));
            ShowDonate = new MvxCommand(new Action(() =>
            {
                NavigationService.Navigate<DonateViewModel>();
                if (PlatformService != null)
                {
                    PlatformService.PlayNotificationSound("button-01.mp3");
                }
            }));
            GameSystemRecentSelected = new MvxCommand<GameSystemRecentModel>(GameSystemRecentSelectedHandler);
            GameSystemRecentsHolding = new MvxCommand<GameSystemRecentModel>(GameSystemRecentsHoldingHandler);
            GameSystemSelected = new MvxCommand<GameSystemViewModel>(GameSystemSelectedHandler);
            GameSystemHolding = new MvxCommand<GameSystemViewModel>(GameSystemHoldingHandler);


            Task HelpNoticeMessage = new Task(new Action(async () =>
            {

                bool ShowNoticeState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("NeverShowHelp", true);
                if (ShowNoticeState)
                {
                    await Task.Delay(2000);
                    PlatformService.PlayNotificationSound("notice.mp3");
                    ConfirmConfig confirmLoadNotice = new ConfirmConfig();
                    confirmLoadNotice.SetTitle("New User?");
                    confirmLoadNotice.SetMessage("Visit help page \u2754 to read about the important features and guidelines.");
                    confirmLoadNotice.UseYesNo();
                    confirmLoadNotice.SetOkText("Never Show");
                    confirmLoadNotice.SetCancelText("Dismiss");

                    var NeverShow = await UserDialogs.Instance.ConfirmAsync(confirmLoadNotice);
                    if (NeverShow)
                    {
                        PlatformService.PlayNotificationSound("button-01.mp3");
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("NeverShowHelp", false);
                        confirmLoadNotice.DisposeIfDisposable();
                    }
                    else
                    {
                        PlatformService.PlayNotificationSound("button-01.mp3");
                    }
                }
            }));

            Task AppLaunched = new Task(new Action(async () =>
            {
                if (!PlatformService.IsAppStartedByFile)
                {
                    await Task.Delay(1000);
                    PlatformService.PlayNotificationSound("launched.wav");
                }
                await Task.Delay(400);
                //await PlatformService.RetriveRecents();
                IDirectoryInfo zipsDirectory = null;
                zipsDirectory = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync("tempExports");
                if (zipsDirectory != null)
                {
                    await zipsDirectory.DeleteAsync();
                }
                //Clean backup cache
                zipsDirectory = CrossFileSystem.Current.RoamingStorage;
                string targetFileName = "backup.rbp";
                string zipFileName = zipsDirectory.FullName + "\\" + targetFileName;
                var targetFielTest = await zipsDirectory.GetFileAsync(targetFileName);
                if (targetFielTest != null)
                {
                    await targetFielTest.DeleteAsync();
                }
                if (!PlatformService.IsAppStartedByFile)
                {
                    HelpNoticeMessage.RunSynchronously();
                }
                StatusHandler += StatusHandlerCall;
            }));
            AppLaunched.RunSynchronously();
            LoaderReady = true;
        }
        public void SystemCoreIsLoadingState(bool LoadingState)
        {
            try
            {
                SystemCoresIsLoading = LoadingState;
                RaisePropertyChanged(nameof(SystemCoresIsLoading));
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public override void Prepare()
        {
            try
            {
                ResetSystemsSelection();
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public override void Prepare(IFileInfo parameter)
        {
            SelectedGameFile = parameter;
        }

        public override async Task Initialize()
        {
            InitializeAsync();
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
                //Find compatible systems for file extension
                var compatibleSystems = await GameSystemsProviderService.GetCompatibleSystems(SelectedGameFile);
                switch (compatibleSystems.Count)
                {
                    case 0:
                        {
                            ResetSystemsSelection();
                            break;
                        }
                    case 1:
                        {
                            StartedByFile = true;
                            SystemCoreIsLoadingState(true);
                            await StartGameAsync(compatibleSystems.Single(), SelectedGameFile);
                            break;
                        }
                    default:
                        {
                            SystemsInFilterMode = true;
                            PlatformService?.SetFilterModeState(SystemsInFilterMode);
                            GameSystems = compatibleSystems.ToArray();
                            break;
                        }
                }
                if (GameSystemsProviderService.Systems != null)
                {
                    int systemIndex = 0;
                    foreach (var SystemItem in GameSystemsProviderService.Systems)
                    {
                        try
                        {
                            await OptionsRetrieveAsync(SystemItem.TempName);
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
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                SystemsLoadFailed = true;
                RaisePropertyChanged(nameof(SystemsLoadFailed));
            }
            if (GameSystems.Count <= 1 && !StartedByFile)
            {
                SystemsLoadFailed = true;
                RaisePropertyChanged(nameof(SystemsLoadFailed));
            }
            SystemCoreIsLoadingState(false);
        }

        bool RootFolderInProgress = false;
        bool SelectFileInProgress = false;
        Dictionary<string, IDirectoryInfo> SystemRoots = new Dictionary<string, IDirectoryInfo>();
        public bool ReSelectIsActive = false;
        public bool CheckByTokenActive = false;
        public async void GameSystemHoldingHandler(GameSystemViewModel system)
        {
            try
            {
                if (system != null && system.Core.FailedToLoad)
                {
                    return;
                }

                if (!RootFolderInProgress && !SelectFileInProgress)
                {
                    PlatformService.PlayNotificationSound("option-changed.wav");
                    RootFolderInProgress = true;
                    IDirectoryInfo systemRootFolder = await PlatformService.PickDirectory(system.TempName, ReSelectIsActive);
                    ReSelectIsActive = false;


                    if (systemRootFolder != null)
                    {
                        IDirectoryInfo TestRoot = null;
                        if (SystemRoots.Count > 0 && SystemRoots.TryGetValue(system.TempName, out TestRoot))
                        {
                            SystemRoots[system.TempName] = systemRootFolder;
                        }
                        else
                        {
                            SystemRoots.Add(system.TempName, systemRootFolder);
                        }

                        if (!CheckByTokenActive)
                        {
                            PlatformService.PlayNotificationSound("success.wav");
                            if (!PlatformService.ShowNotificationMain("Games folder set for " + system.Name, 2))
                            {
                                await UserDialogs.Instance.AlertAsync("Games folder set for " + system.Name, "Games Folder");
                            }
                        }
                        CheckByTokenActive = false;
                        //Open Games List
                        SelectedSystem = system;
                        SystemSelected = true;
                        var extensions = system.SupportedExtensions.Concat(ArchiveStreamProvider.SupportedExtensions).ToArray();
                        SelectedSystemExtensions = extensions;

                        try
                        {
                            if (await systemRootFolder.GetDirectoryAsync("BFiles") != null)
                            {
                                SystemCoreIsLoadingState(true);
                                SetStatus("Wait, Retrix tricks...");
                                await SelectedSystem.SyncAllBIOSFiles(systemRootFolder);
                            }
                            else
                            {
                                SystemCoreIsLoadingState(true);
                                SetStatus("Please Wait...");
                            }
                        }
                        catch (Exception eb)
                        {

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
                                PlatformService?.ShowErrorMessage(ed);
                            }
                        }

                        setGamesListState(true);
                        GetRecentGames();
                        SystemCoreIsLoadingState(false);
                        SetStatus("...");
                        RootFolderInProgress = false;
                    }
                    else
                    {
                        CheckByTokenActive = false;
                        RootFolderInProgress = false;
                    }
                }
                else
                {
                    RootFolderInProgress = false;
                }
            }
            catch (Exception e)
            {
                RootFolderInProgress = false;
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
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
                PlatformService?.ShowErrorMessage(e);
            }
        }
        bool DeleteRecentInProgress = false;
        private async void GameSystemRecentsHoldingHandler(GameSystemRecentModel recent)
        {
            try
            {
                if (recent != null && !recent.NewGame && !DeleteRecentInProgress)
                {
                    DeleteRecentInProgress = true;
                    PlatformService.PlayNotificationSound("alert.wav");
                    ConfirmConfig confirmRecentDelete = new ConfirmConfig();
                    confirmRecentDelete.SetTitle("Recent Action");
                    confirmRecentDelete.SetMessage($"Choose action for:\n{recent.GameName}\n\nDelete:\nDelete game from recent only");
                    confirmRecentDelete.UseYesNo();
                    confirmRecentDelete.OkText = "Delete";
                    confirmRecentDelete.CancelText = "Cancel";
                    bool RootFolderState = await UserDialogs.Instance.ConfirmAsync(confirmRecentDelete);
                    if (RootFolderState)
                    {
                        await PlatformService.AddGameToRecents(recent.GameSystem, recent.GameLocation, false, recent.GameID, 0, true);
                        GetRecentGames();
                        DeleteRecentInProgress = false;
                    }
                    else
                    {
                        DeleteRecentInProgress = false;
                    }
                }
                else if (recent != null && recent.NewGame && !DeleteRecentInProgress)
                {
                    DeleteRecentInProgress = true;
                    PlatformService.PlayNotificationSound("alert.wav");
                    ConfirmConfig confirmRecentDelete = new ConfirmConfig();
                    confirmRecentDelete.SetTitle("Game Action");
                    confirmRecentDelete.SetMessage($"Choose action for:\n{recent.GameName}\n\nGet Size:\nGet game total size");
                    confirmRecentDelete.UseYesNo();
                    confirmRecentDelete.OkText = "Get Size";
                    confirmRecentDelete.CancelText = "Cancel";
                    bool RootFolderState = await UserDialogs.Instance.ConfirmAsync(confirmRecentDelete);
                    if (RootFolderState)
                    {
                        SystemCoreIsLoadingState(true);
                        var GameFile = await FileSystem.GetFileFromPathAsync(recent.GameLocation);
                        if (GameFile != null)
                        {
                            var GameSize = await GameFile.GetLengthAsync();
                            PlatformService.PlayNotificationSound("success.wav");
                            await UserDialogs.Instance.AlertAsync($"{recent.GameName}'s size is:\n{GameSize.ToFileSize()}");
                        }
                        else
                        {
                            PlatformService.PlayNotificationSound("faild.wav");
                            await UserDialogs.Instance.AlertAsync($"{recent.GameName} not found!\nOriginal Location: {recent.GameLocation}");
                        }

                        DeleteRecentInProgress = false;
                    }
                    else
                    {
                        DeleteRecentInProgress = false;
                    }
                    SystemCoreIsLoadingState(false);
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
                DeleteRecentInProgress = false;
                SystemCoreIsLoadingState(false);
            }
        }

        private async void GameSystemRecentSelectedHandler(GameSystemRecentModel recent)
        {
            try
            {
                PlayGameByName(recent.GameName);
                if (eventHandlerTemp != null)
                {
                    eventHandlerTemp.Invoke(recent, EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
        }

        bool SystemSelected = false;
        public GameSystemViewModel SelectedSystem = null;
        string[] SelectedSystemExtensions = null;
        public void ClearSelectedSystem()
        {
            SystemSelected = false;
            SelectedSystem = null;
            SelectedSystemExtensions = null;
        }
        private async void GameSystemSelectedHandler(GameSystemViewModel system)
        {
            SelectedSystem = null;
            SelectedSystemExtensions = null;

            if (system != null && system.Core.FailedToLoad)
            {
                if (system.AnyCore)
                {
                    PlatformService.PlayNotificationSound("faild.wav");
                    await UserDialogs.Instance.AlertAsync("Core failed to load, you can't use the core at the moment", "Core Failed");
                    PlatformService.PlayNotificationSound("alert.wav");
                    ConfirmConfig confirmDeleteCore = new ConfirmConfig();
                    confirmDeleteCore.SetTitle("Delete Core");
                    confirmDeleteCore.SetMessage($"Do you want to delete this core?");
                    confirmDeleteCore.UseYesNo();
                    bool confirmDeleteCoreState = await UserDialogs.Instance.ConfirmAsync(confirmDeleteCore);
                    if (confirmDeleteCoreState)
                    {
                        SystemCoreIsLoadingState(true);
                        IDirectoryInfo zipsDirectory = null;
                        zipsDirectory = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync("AnyCore");

                        string targetFileName = System.IO.Path.GetFileName((system.Core.DLLName + ".dll"));
                        var targetFielTest = await zipsDirectory.GetFileAsync(targetFileName);
                        if (targetFielTest != null)
                        {
                            system.Core.FreeLibretroCore();
                            await targetFielTest.DeleteAsync();
                            await DeleteAnyCore(system.TempName);

                            string BIOSFileName = System.IO.Path.GetFileName((system.Core.DLLName + ".rab"));
                            var targetBIOSTest = await zipsDirectory.GetFileAsync(BIOSFileName);
                            if (targetBIOSTest != null)
                            {
                                await targetBIOSTest.DeleteAsync();
                            }
                            PlatformService.PlayNotificationSound("success.wav");
                            if(!PlatformService.ShowNotificationMain($"{system.Name} Deleted, Restart Retrix is recommended", 3)) {
                               await UserDialogs.Instance.AlertAsync($"{system.Name} Deleted, Restart Retrix is recommended");
                            }
                            system.Core.FailedToLoad = true;
                        }
                        else
                        {
                            PlatformService.PlayNotificationSound("faild.wav");
                            await UserDialogs.Instance.AlertAsync($"{system.Name} not found!, unable to delete {system.Name}");
                        }
                    }
                }
                else
                {
                    PlatformService.PlayNotificationSound("faild.wav");
                    await UserDialogs.Instance.AlertAsync("Core failed to load, you can't use the core at the moment", "Core Failed");
                    PlatformService.PlayNotificationSound("alert.wav");
                    ConfirmConfig confirmDeleteCore = new ConfirmConfig();
                    confirmDeleteCore.SetTitle("Delete Update");
                    confirmDeleteCore.SetMessage($"This could happen becuase of an update\n\nDo you want to delete the last update?");
                    confirmDeleteCore.UseYesNo();
                    bool confirmDeleteCoreState = await UserDialogs.Instance.ConfirmAsync(confirmDeleteCore);
                    if (confirmDeleteCoreState)
                    {
                        SystemCoreIsLoadingState(true);
                        IDirectoryInfo zipsDirectory = null;
                        zipsDirectory = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync("PureCore");

                        string targetFileName = System.IO.Path.GetFileName((system.Core.DLLName));
                        var targetFielTest = await zipsDirectory.GetFileAsync(targetFileName);
                        if (targetFielTest != null)
                        {
                            system.Core.FreeLibretroCore();
                            await targetFielTest.DeleteAsync();

                            PlatformService.PlayNotificationSound("success.wav");
                            if(!PlatformService.ShowNotificationMain($"{system.Name}'s update Deleted, Restart Retrix is recommended", 3)) { 
                            await UserDialogs.Instance.AlertAsync($"{system.Name}'s update Deleted, Restart Retrix is recommended");
                            }
                            system.Core.FailedToLoad = true;
                        }
                        else
                        {
                            PlatformService.PlayNotificationSound("faild.wav");
                            await UserDialogs.Instance.AlertAsync($"{system.Name}'s update not found!, unable to delete");
                        }
                    }
                }
                SystemCoreIsLoadingState(false);
                return;
            }

            try
            {

                if (!RootFolderInProgress && !SelectFileInProgress)
                {
                    IDirectoryInfo TestRootAsk = null;
                    if (SystemRoots.Count == 0 || !SystemRoots.TryGetValue(system.TempName, out TestRootAsk) && !PlatformService.IsAppStartedByFile)
                    {
                        if (PlatformService.CheckDirectoryToken(system.TempName))
                        {
                            CheckByTokenActive = true;
                            GameSystemHoldingHandler(system);
                            return;
                        }
                        else
                        {
                            PlatformService.PlayNotificationSound("alert.wav");
                            ConfirmConfig confirmRootFolder = new ConfirmConfig();
                            confirmRootFolder.SetTitle("Open Action");
                            confirmRootFolder.SetMessage($"Choose action for:\n{system.Name}\n\nGames Folder (Recommended):\nSelect games folder\n\nSingle Game:\nSelect one game");
                            confirmRootFolder.UseYesNo();
                            confirmRootFolder.OkText = "Games Folder";
                            confirmRootFolder.CancelText = "Single Game";
                            bool RootFolderState = await UserDialogs.Instance.ConfirmAsync(confirmRootFolder);
                            if (RootFolderState)
                            {
                                GameSystemHoldingHandler(system);
                                return;
                            }
                        }
                    }
                    PlatformService.PlayNotificationSound("select.mp3");
                    SelectFileInProgress = true;
                    if (SelectedGameFile == null && !SystemSelected)
                    {
                        SelectedSystem = system;
                        SystemSelected = true;
                        var extensions = system.SupportedExtensions.Concat(ArchiveStreamProvider.SupportedExtensions).ToArray();
                        SelectedSystemExtensions = extensions;
                        IDirectoryInfo TestRoot = null;
                        if (SystemRoots.Count > 0 && SystemRoots.TryGetValue(system.TempName, out TestRoot))
                        {
                            setGamesListState(true);
                            GetRecentGames();
                        }
                        else
                        {
                            SelectedGameFile = await FileSystem.PickFileAsync(extensions);
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
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                SystemSelected = false;
            }
            SelectFileInProgress = false;
        }
        public async void BrowseSingleGame()
        {
            SelectedGameFile = await FileSystem.PickFileAsync(SelectedSystemExtensions);
            await LoadGameByFile(SelectedSystem, SelectedGameFile);
        }

        private async Task LoadGameByFile(GameSystemViewModel system, IFileInfo SelectedGameFile)
        {
            if (SelectedGameFile == null)
            {
                SystemSelected = false;
            }
            else
            {
                SystemCoreIsLoadingState(true);
                await StartGameAsync(system, SelectedGameFile);
                SystemSelected = false;
            }
        }

        bool searchCleardByGetter = false;
        public bool ReloadGamesVisible = false;
        public void ReloadGamesVisibleState(bool VisibleState)
        {
            ReloadGamesVisible = VisibleState;
            RaisePropertyChanged(nameof(ReloadGamesVisible));
        }

        Dictionary<string, Dictionary<string, string>> FilesListCache = new Dictionary<string, Dictionary<string, string>>();
        EventHandler eventHandlerTemp = null;
        public void GetAllGames(EventHandler eventHandler = null, bool ReloadGamesList = false)
        {
            try
            {
                if (SelectedSystem != null)
                {
                    eventHandlerTemp = eventHandler;
                    ReloadGamesVisibleState(true);
                    try
                    {
                        GamesRecentsList.Clear();
                        GamesRecentsListTemp.Clear();
                        SearchText = "";
                        searchCleardByGetter = true;
                        RaisePropertyChanged(nameof(SearchText));
                        StatusBar = "";
                        RaisePropertyChanged(nameof(StatusBar));
                        NoRecentsListVisible = false;
                        RaisePropertyChanged(nameof(NoRecentsListVisible));
                    }
                    catch (Exception ea)
                    {
                        PlatformService?.ShowErrorMessage(ea);
                    }
                    Dispatcher.RequestMainThreadAction((Action)(async () =>
                    {
                        try
                        {
                            string[] FilesList = await GetAllFile(ReloadGamesList);

                            //GamesList.Clear();
                            if (FilesList.Length > 0)
                            {
                                var snapshotsFolder = await PlatformService.GetRecentsLocationAsync();
                                foreach (var GameLocation in FilesList)
                                {
                                    string snapshotLocation = "";
                                    string GameID = "";
                                    try
                                    {
                                        GameID = PlatformService.GetGameID(SelectedSystem.TempName, GameLocation);
                                        if (GameID.Length > 0)
                                        {
                                            var SpnapshotFile = await snapshotsFolder.GetFileAsync($"{GameID}.png");
                                            if (SpnapshotFile != null)
                                            {
                                                snapshotLocation = SpnapshotFile.FullName;
                                            }
                                        }
                                    }
                                    catch (Exception ef)
                                    {

                                    }
                                    string gameName = FilesListCache[SelectedSystem.TempName][GameLocation];
                                    int OpenCounts = PlatformService.GetGamePlaysCount(SelectedSystem.TempName, GameLocation);
                                    long PlayedTime = PlatformService.GetGamePlayedTime(SelectedSystem.TempName, GameLocation);
                                    string gameLocation = GameLocation;
                                    string gameSnapshot = snapshotLocation;
                                    string gameSystem = SelectedSystem.TempName;
                                    string gameToken = "";
                                    bool gameRootNeeded = false;
                                    bool gameNew = true;
                                    GameSystemRecentModel gameRow = new GameSystemRecentModel(GameID, gameName, OpenCounts, PlayedTime, gameLocation, gameSnapshot, gameSystem, gameRootNeeded, gameToken, gameNew);
                                    GamesRecentsList.Add(gameRow);
                                    GamesRecentsListTemp.Add(gameRow);
                                }
                                NoGamesListVisible = false;
                                RaisePropertyChanged(nameof(NoGamesListVisible));
                                StatusBar = $"{GamesRecentsList.Count} Game{(GamesRecentsList.Count < 10 ? "s" : "")}";
                                RaisePropertyChanged(nameof(StatusBar));
                                if (eventHandler != null)
                                {
                                    eventHandler.Invoke(this, EventArgs.Empty);
                                }
                            }
                            else
                            {
                                NoGamesListVisible = true;
                                RaisePropertyChanged(nameof(NoGamesListVisible));
                                PlatformService.PlayNotificationSound("notice.mp3");
                            }
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            SystemCoreIsLoadingState(false);
                        }
                        catch (Exception e)
                        {
                            if (PlatformService != null)
                            {
                                PlatformService.ShowErrorMessage(e);
                            }
                            NoGamesListVisible = true;
                            RaisePropertyChanged(nameof(NoGamesListVisible));
                            PlatformService.PlayNotificationSound("notice.mp3");
                            SystemCoreIsLoadingState(false);
                        }
                    }));
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
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
                FilesListCache[SelectedSystem.TempName].Clear();
                await GetFilesByExtensions(GetRootFolerBySystemName(SelectedSystem.TempName));
            }
            return FilesListCache[SelectedSystem.TempName].Keys.ToArray();
        }
        private async Task GetFilesByExtensions(IDirectoryInfo directoryInfo, int CurrentSubDirectory = 0)
        {
            var FoldersList = await directoryInfo.EnumerateDirectoriesAsync();
            var FilesList = await directoryInfo.EnumerateFilesAsync();
            var vfsRomPath = "ROM";
            foreach (var FileItem in FilesList)
            {
                if (ArchiveStreamProvider.SupportedExtensions.Contains(Path.GetExtension(FileItem.Name).ToLower()) || SelectedSystemExtensions.Contains(Path.GetExtension(FileItem.Name).ToLower()))
                {
                    if (ArchiveStreamProvider.SupportedExtensions.Contains(Path.GetExtension(FileItem.Name).ToLower()) && SelectedSystem.Core.NativeArchiveSupport == false && SelectedSystem.MultiFileExtensions == null)
                    {
                        //var fileSize = await FileItem.GetLengthAsync();
                        //fileSize = ((fileSize / 1024) / 1024);
                        //if(fileSize < 50)
                        var archiveProvider = new ArchiveStreamProvider(vfsRomPath, FileItem);
                        var entries = await archiveProvider.ListEntriesAsync();
                        var virtualMainFilePath = entries.FirstOrDefault(d => SelectedSystem.SupportedExtensions.Contains(Path.GetExtension(d.Replace("#", "")).ToLower()));
                        if (!string.IsNullOrEmpty(virtualMainFilePath))
                        {
                            FilesListCache[SelectedSystem.TempName].Add(FileItem.FullName, FileItem.Name);
                            archiveProvider.DisposeIfDisposable();
                            archiveProvider = null;
                        }
                    }
                    else
                    {
                        FilesListCache[SelectedSystem.TempName].Add(FileItem.FullName, FileItem.Name);
                    }
                }
            }
            CurrentSubDirectory++;
            foreach (var FolderItem in FoldersList)
            {
                if (CurrentSubDirectory > MaxSubDirectory)
                {
                    continue;
                }
                await GetFilesByExtensions(FolderItem, CurrentSubDirectory);
            }
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

        public override void ViewAppeared()
        {
            RaisePropertyChanged(nameof(SearchText));
            base.ViewAppeared();
        }

        public ObservableCollection<GameSystemRecentModel> GamesRecentsListTemp = new ObservableCollection<GameSystemRecentModel>();
        public bool filterInProgress = false;
        public string SearchText = "";
        public string SearchTextTemp = "";
        public async Task FilterCurrentGamesList(string filterText)
        {
            try
            {
                if (searchCleardByGetter)
                {
                    searchCleardByGetter = false;
                    return;
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
                if (filterText.Length > 0)
                {
                    var FilteredList = GamesRecentsListTemp.Where(item => item.GameName.ToLower().Contains(filterText.ToLower()));
                    GamesRecentsList.Clear();

                    foreach (var MatchedITem in FilteredList)
                    {
                        GamesRecentsList.Add(MatchedITem);
                    }
                    StatusBar = $"{GamesRecentsList.Count} Game{(GamesRecentsList.Count < 10 ? "s" : "")}";
                    RaisePropertyChanged(nameof(StatusBar));
                }
                else
                {
                    GamesRecentsList.Clear();
                    foreach (var MatchedITem in GamesRecentsListTemp)
                    {
                        GamesRecentsList.Add(MatchedITem);
                    }
                    StatusBar = $"{GamesRecentsList.Count} Game{(GamesRecentsList.Count < 10 ? "s" : "")}";
                    RaisePropertyChanged(nameof(StatusBar));
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
            filterInProgress = false;
            RaisePropertyChanged(nameof(filterInProgress));
        }
        public void GetRecentGames(EventHandler eventHandler = null)
        {
            try
            {
                ReloadGamesVisibleState(false);
                try
                {
                    searchCleardByGetter = true;
                    StatusBar = "";
                    RaisePropertyChanged(nameof(StatusBar));
                    NoGamesListVisible = false;
                    RaisePropertyChanged(nameof(NoGamesListVisible));
                    SystemCoreIsLoadingState(true);
                    GamesRecentsList.Clear();
                    GamesRecentsListTemp.Clear();
                    SearchText = "";
                    RaisePropertyChanged(nameof(SearchText));
                }
                catch (Exception ea)
                {
                    PlatformService?.ShowErrorMessage(ea);
                }
                Dispatcher.RequestMainThreadAction((Action)(async () =>
                {
                    try
                    {
                        if (SelectedSystem != null)
                        {
                            var RecentGamesList = PlatformService.GetGamesRecents();
                            //GamesList.Clear();
                            if (RecentGamesList != null && RecentGamesList.Count > 0)
                            {
                                Dictionary<GameSystemRecentModel, int> GamesListTemp = new Dictionary<GameSystemRecentModel, int>();
                                List<string[]> TestGet = new List<string[]>();
                                if (RecentGamesList.TryGetValue(SelectedSystem.TempName, out TestGet) && TestGet.Count > 0)
                                {
                                    var snapshotsFolder = await PlatformService.GetRecentsLocationAsync();
                                    long totalPlayedTime = 0;
                                    foreach (var GameName in TestGet)
                                    {
                                        string snapshotLocation = "";
                                        string GameID = "";
                                        try
                                        {
                                            GameID = GameName[5];
                                            var gameSnapshotName = $"{GameID}.png";
                                            var SpnapshotFile = await snapshotsFolder.GetFileAsync(gameSnapshotName);
                                            if (SpnapshotFile != null)
                                            {
                                                snapshotLocation = SpnapshotFile.FullName;
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
                                        GameSystemRecentModel gameRow = new GameSystemRecentModel(GameID, gameName, gameCount, PlayedTime, gameLocation, snapshotLocation, SelectedSystem.TempName, gameRootNeeded, gameToken, GameNew);
                                        if (!gameRow.GameFailed)
                                        {
                                            GamesListTemp.Add(gameRow, gameCount);
                                            totalPlayedTime += PlayedTime;
                                        }
                                    }
                                    foreach (KeyValuePair<GameSystemRecentModel, int> item in GamesListTemp.OrderByDescending(key => key.Value))
                                    {
                                        //GamesList.Add(item.Key);
                                        GamesRecentsList.Add(item.Key);
                                        GamesRecentsListTemp.Add(item.Key);
                                    }
                                    NoRecentsListVisible = false;
                                    string ExtraInfo = "";
                                    if (totalPlayedTime > 0)
                                    {
                                        ExtraInfo = $", [{GameSystemRecentModel.FormatTotalPlayedTime(totalPlayedTime)}] Total Play";
                                    }
                                    StatusBar = $"{GamesRecentsList.Count} Game{(GamesRecentsList.Count < 10 ? "s" : "")}{ExtraInfo}";
                                    RaisePropertyChanged(nameof(StatusBar));
                                    if (eventHandler != null)
                                    {
                                        eventHandler.Invoke(this, EventArgs.Empty);
                                    }
                                }
                                else
                                {
                                    NoRecentsListVisible = true;
                                    PlatformService.PlayNotificationSound("notice.mp3");
                                }
                                RaisePropertyChanged(nameof(NoRecentsListVisible));
                            }
                            else
                            {
                                NoRecentsListVisible = true;
                                RaisePropertyChanged(nameof(NoRecentsListVisible));
                                PlatformService.PlayNotificationSound("notice.mp3");
                            }
                            SystemCoreIsLoadingState(false);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                    }
                    catch (Exception e)
                    {
                        if (PlatformService != null)
                        {
                            PlatformService.ShowErrorMessage(e);
                        }
                        NoRecentsListVisible = true;
                        RaisePropertyChanged(nameof(NoRecentsListVisible));
                        PlatformService.PlayNotificationSound("notice.mp3");
                        SystemCoreIsLoadingState(false);
                    }
                }));
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public async void PlayGameByName(string GameName)
        {
            try
            {
                if (SelectedSystem != null)
                {
                    var pattern = @"\s\(\d+\)$";
                    GameName = Regex.Replace(GameName, pattern, "");
                    PlatformService.PlayNotificationSound("root-needed.wav");
                    ConfirmConfig confirmPlayGame = new ConfirmConfig();
                    confirmPlayGame.SetTitle("Start Play?");
                    confirmPlayGame.SetMessage("Do you want to start\n" + Path.GetFileName(GameName));
                    confirmPlayGame.UseYesNo();
                    bool PlayGame = await UserDialogs.Instance.ConfirmAsync(confirmPlayGame);
                    if (PlayGame)
                    {
                        string GameLocation = GetFileLocationByName(GameName);

                        if (GameLocation == null)
                        {
                            GameLocation = PlatformService.GetGameLocation(SelectedSystem.TempName, GameName);
                        }
                        if (GameLocation != null)
                        {
                            IDirectoryInfo directoryInfo = GetRootFolerBySystemName(SelectedSystem.TempName);
                            if (GameLocation.Contains(directoryInfo.FullName))
                            {
                                GameLocation = GameLocation.Replace(directoryInfo.FullName + @"\", "");
                                SelectedGameFile = await directoryInfo.GetFileAsync(GameLocation);
                                await LoadGameByFile(SelectedSystem, SelectedGameFile);
                            }
                            else
                            {
                                PlatformService.PlayNotificationSound("faild.wav");
                                await UserDialogs.Instance.AlertAsync("Game not found, or games folder is not correct, Original Path:\n" + GameLocation, "Start Failed");
                            }
                        }
                        else
                        {
                            PlatformService.PlayNotificationSound("faild.wav");
                            await UserDialogs.Instance.AlertAsync("Game not found, or games folder is not correct", "Start Failed");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        private IDirectoryInfo GetRootFolerBySystemName(string SystemName)
        {
            IDirectoryInfo SystemRootsTest = null;
            if (SystemRoots.TryGetValue(SystemName, out SystemRootsTest))
            {
                return SystemRootsTest;
            }
            return null;
        }

        void SetStatus(string statusMessage)
        {
            try
            {
                LoadingStatus = statusMessage;
                RaisePropertyChanged(nameof(LoadingStatus));
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
        }


        async Task<GameSystemViewModel> ResloveReopenSystemIssue(GameSystemViewModel system)
        {
            try
            {
                SetStatus($"Loading {system.Name}..");
                //Reload the core for some consoles because of Deinit issues


                var SystemNameTemp = system.Name;
                var SystemFileTemp = system.FileSystem;
                switch (SystemNameTemp)
                {
                    case "PlayStation":
                    case "PlayStation*":
                        PlatformService.PlayNotificationSound("alert.wav");
                        ConfirmConfig confirmPlayGame = new ConfirmConfig();
                        confirmPlayGame.SetTitle("Start Play?");
                        confirmPlayGame.SetMessage("Do you want to include Analog control?\nSome games will not detect the gamepad if you attach the Analog control.");
                        confirmPlayGame.UseYesNo();
                        bool CustomInput = await UserDialogs.Instance.ConfirmAsync(confirmPlayGame);
                        system.Core.ReInitialCore(CustomInput);
                        break;

                    default:
                        system.Core.ReInitialCore(true);
                        break;
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
            await Task.Delay(500);
            return system;
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
        private async Task StartGameAsync(GameSystemViewModel system, IFileInfo file)
        {
            //Verify the file before open
            var fileLength = await file.GetLengthAsync();
            if (fileLength == 0)
            {
                ResetSystemsSelection();
                PlatformService.PlayNotificationSound("faild.wav");
                await DialogsService.AlertAsync("The file is not valid or not accessible, try another one.", "Invalid File");
                return;
            }

            try
            {
                system = await ResloveReopenSystemIssue(system);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }

            var dependenciesMet = await system.CheckDependenciesMetAsync();
            if (!dependenciesMet)
            {
                var notificationBody = string.Format(Resources.Strings.SystemUnmetDependenciesAlertMessage, "\u26EF") + "\n\nClick [Ignore] to try without BIOS \n(If the app crashed then BIOS files needed)";
                PlatformService.PlayNotificationSound("alert.wav");
                ConfirmConfig confirmIgnoreBIOS = new ConfirmConfig();
                confirmIgnoreBIOS.SetTitle(Resources.Strings.SystemUnmetDependenciesAlertTitle);
                confirmIgnoreBIOS.SetMessage(notificationBody);
                confirmIgnoreBIOS.UseYesNo();
                confirmIgnoreBIOS.OkText = "Ok";
                confirmIgnoreBIOS.CancelText = "Ignore";
                bool IgnoreBIOS = await UserDialogs.Instance.ConfirmAsync(confirmIgnoreBIOS);
                if (IgnoreBIOS)
                {
                    ResetSystemsSelection();
                    return;
                }
            }

            try
            {
                SystemCoreIsLoadingState(true);
                bool RootNeeded = false;
                SetStatus("Checking game..");
                var result = await GameSystemsProviderService.GenerateGameLaunchEnvironmentAsync(system, file, null, false, PlatformService, StatusHandler);

                if (result.Item2 == GameLaunchEnvironment.GenerateResult.RootFolderRequired)
                {
                    IDirectoryInfo folder = null;
                    var AutoLoad = false;
                    IDirectoryInfo SystemRootsTest = GetRootFolerBySystemName(system.TempName);
                    if (SystemRootsTest != null)
                    {
                        string SubRoot = file.FullName.Replace(SystemRootsTest.FullName, "").Replace(file.Name, "");
                        if (SubRoot.Replace("\\", "").Length > 0)
                        {
                            string[] directories = SubRoot.TrimStart('\\').Split('\\');
                            foreach (string directory in directories)
                            {
                                if (directory.Length > 0)
                                {
                                    SystemRootsTest = await SystemRootsTest.GetDirectoryAsync(directory);
                                }
                            }
                        }
                        folder = SystemRootsTest;
                        AutoLoad = true;
                    }

                    if (!AutoLoad && PlatformService.GetGameRootNeeded(system.TempName, file.FullName))
                    {
                        PlatformService.PlayNotificationSound("root-needed.wav");
                        ConfirmConfig confirmLoadAuto = new ConfirmConfig();
                        confirmLoadAuto.SetTitle(Resources.Strings.SelectFolderRequestAlertTitle);
                        confirmLoadAuto.SetMessage(Resources.Strings.SelectFolderRequestAlertMessage);
                        confirmLoadAuto.UseYesNo();
                        AutoLoad = await UserDialogs.Instance.ConfirmAsync(confirmLoadAuto);
                    }
                    else
                    {

                    }
                    if (AutoLoad)
                    {


                        if (folder == null)
                        {
                            folder = await FileSystem.PickDirectoryAsync();
                        }


                        if (folder == null)
                        {
                            ResetSystemsSelection();
                            return;
                        }

                        if (!Path.GetDirectoryName(file.FullName).StartsWith(folder.FullName))
                        {
                            ResetSystemsSelection();
                            PlatformService.PlayNotificationSound("faild.wav");
                            await DialogsService.AlertAsync(Resources.Strings.SelectFolderInvalidAlertMessage, Resources.Strings.SelectFolderInvalidAlertTitle);
                            return;
                        }
                        SetStatus("Loading game..");
                        result = await GameSystemsProviderService.GenerateGameLaunchEnvironmentAsync(system, file, folder, false, PlatformService, StatusHandler);
                        RootNeeded = true;
                    }
                    else
                    {
                        if (!file.FullName.Contains(".cue"))
                        {
                            SetStatus("Loading game..");
                            await Task.Delay(700);
                            result = await GameSystemsProviderService.GenerateGameLaunchEnvironmentAsync(system, file, folder, true, PlatformService, StatusHandler);
                        }
                        else
                        {
                            ResetSystemsSelection();
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
                            PlatformService.PlayNotificationSound("faild.wav");
                            await DialogsService.AlertAsync(Resources.Strings.NoCompatibleFileInArchiveAlertMessage, Resources.Strings.NoCompatibleFileInArchiveAlertTitle);
                            return;
                        }
                    case GameLaunchEnvironment.GenerateResult.Success:
                        {

                            result.Item1.RootNeeded = RootNeeded;
                            SetStatus("Starting game..");


                            //PlatformService.InitialServices();

                            //Mvx.ConstructAndRegisterSingleton<ISaveStateService, SaveStateService>();
                            //Mvx.LazyConstructAndRegisterSingleton<IEmulationService, EmulationService>();

                            PlatformService.SetGameStopInProgress(false);
                            await NavigationService.Navigate<GamePlayerViewModel, GameLaunchEnvironment>(result.Item1);
                            ResetSystemsSelection();
                            return;
                        }
                    default:
                        throw new Exception("Error detected!, be sure to select the right game for the right system. or maybe the game need games folder");
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                ResetSystemsSelection();
            }
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
                    GameSystems = GameSystemsProviderService.Systems;
                    SystemsInFilterMode = false;
                    PlatformService?.SetFilterModeState(SystemsInFilterMode);
                    PlatformService.IsAppStartedByFile = false;
                }

                SelectedGameFile = null;
                SystemSelected = false;
                SystemCoreIsLoadingState(false);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                SystemSelected = false;
                SelectedGameFile = null;
                SystemCoreIsLoadingState(false);
            }
        }


        static string OptionsSaveLocation = "SaveRecents";
        public static async Task CoreOptionsStoreAsyncDirect(string SystemName)
        {
            try
            {
                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(OptionsSaveLocation);
                if (localFolder == null)
                {
                    await CrossFileSystem.Current.LocalStorage.CreateDirectoryAsync(OptionsSaveLocation);
                    localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(OptionsSaveLocation);
                }
                var StatesDirectory = await localFolder.GetDirectoryAsync(SystemName);
                if (StatesDirectory == null)
                {
                    StatesDirectory = await localFolder.CreateDirectoryAsync(SystemName);
                }
                var targetFileTest = await StatesDirectory.GetFileAsync("options.rto");
                if (targetFileTest != null)
                {
                    await targetFileTest.DeleteAsync();
                }
                var targetFile = await StatesDirectory.CreateFileAsync("options.rto");



                Encoding unicode = Encoding.Unicode;
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(SystemsOptions[SystemName]));
                using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                {
                    await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                    await outStream.FlushAsync();
                }

            }
            catch (Exception e)
            {

            }
        }

        public async Task CoreOptionsStoreAsync(string SystemName)
        {
            SystemCoreIsLoadingState(true);
            await CoreOptionsStoreAsyncDirect(SystemName);
            SystemCoreIsLoadingState(false);
        }

        public async Task OptionsRetrieveAsync(string SystemName)
        {
            try
            {
                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(OptionsSaveLocation);
                if (localFolder == null)
                {
                    return;
                }
                var StatesDirectory = await localFolder.GetDirectoryAsync(SystemName);
                if (StatesDirectory != null)
                {
                    var targetFileTest = await StatesDirectory.GetFileAsync("options.rto");
                    if (targetFileTest != null)
                    {
                        Encoding unicode = Encoding.Unicode;
                        byte[] result;
                        using (var outStream = await targetFileTest.OpenAsync(FileAccess.Read))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                outStream.CopyTo(memoryStream);
                                result = memoryStream.ToArray();
                            }
                            await outStream.FlushAsync();
                        }
                        string OptionsFileContent = unicode.GetString(result);
                        var dictionaryList = JsonConvert.DeserializeObject<CoresOptions>(OptionsFileContent);
                        SystemsOptions[SystemName] = dictionaryList;
                    }
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public async void DeleteSavedOptions(string SystemName)
        {
            var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(OptionsSaveLocation);
            if (localFolder == null)
            {
                return;
            }
            var StatesDirectory = await localFolder.GetDirectoryAsync(SystemName);
            if (StatesDirectory == null)
            {
                return;
            }
            var targetFileTest = await StatesDirectory.GetFileAsync("options.rto");
            if (targetFileTest != null)
            {
                await targetFileTest.DeleteAsync();
            }
        }

        public async void GetConsoleInfo(EventHandler eventHandler = null)
        {
            try
            {
                if (SelectedSystem != null)
                {
                    var localFolder = await CrossFileSystem.Current.InstallLocation.GetDirectoryAsync("Infos");
                    if (localFolder == null)
                    {
                        return;
                    }
                    string SystemName = SelectedSystem.TempName;
                    var targetFileTest = await localFolder.GetFileAsync($"{SystemName}.rti");
                    if (targetFileTest != null)
                    {
                        Encoding unicode = Encoding.Unicode;
                        byte[] result;
                        using (var outStream = await targetFileTest.OpenAsync(FileAccess.Read))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                outStream.CopyTo(memoryStream);
                                result = memoryStream.ToArray();
                            }
                            await outStream.FlushAsync();
                        }
                        string OptionsFileContent = unicode.GetString(result);
                        var dictionaryList = JsonConvert.DeserializeObject<Dictionary<string, string>>(OptionsFileContent);
                        string SystemDescriptions = dictionaryList["desc"];
                        string SystemCompany = dictionaryList["company"];
                        string SystemYear = dictionaryList["year"];
                        string SystemMessage = $"Console: {SystemName}\nCompany: {SystemCompany}\nYear: {SystemYear}\n\n{SystemDescriptions}";
                        PlatformService.PlayNotificationSound("success.wav");
                        await UserDialogs.Instance.AlertAsync(SystemMessage);
                    }
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }



        static string AnyCoreSaveLocation = "AnyCore";
        public static async Task AnyCoreStoreAsyncDirect()
        {
            try
            {
                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(AnyCoreSaveLocation);
                if (localFolder == null)
                {
                    await CrossFileSystem.Current.LocalStorage.CreateDirectoryAsync(AnyCoreSaveLocation);
                    localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(AnyCoreSaveLocation);
                }

                var targetFileTest = await localFolder.GetFileAsync("cores.rac");
                if (targetFileTest != null)
                {
                    await targetFileTest.DeleteAsync();
                }
                var targetFile = await localFolder.CreateFileAsync("cores.rac");

                Encoding unicode = Encoding.Unicode;
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(SystemsAnyCore));
                using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                {
                    await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                    await outStream.FlushAsync();
                }

            }
            catch (Exception e)
            {

            }
        }

        public async Task AnyCoreStoreAsync()
        {
            SystemCoreIsLoadingState(true);
            await AnyCoreStoreAsyncDirect();
            SystemCoreIsLoadingState(false);
        }

        public static async void AnyCoreRetrieveAsync()
        {
            try
            {
                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(AnyCoreSaveLocation);
                if (localFolder == null)
                {
                    return;
                }

                var targetFileTest = await localFolder.GetFileAsync("cores.rac");
                if (targetFileTest != null)
                {
                    Encoding unicode = Encoding.Unicode;
                    byte[] result;
                    using (var outStream = await targetFileTest.OpenAsync(FileAccess.Read))
                    {
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
        public async Task SetAnyCoreInfo(string systemName, string Name, string System = "", string Icon = "", bool Pinned = false)
        {
            try
            {
                if (Icon?.Length > 0 && !Icon.ToLower().Equals(SystemsAnyCore[systemName].CoreIcon.ToLower()) && !SystemsAnyCore[systemName].CoreIcon.Contains("ms-appx"))
                {
                    var localStorage = await FileSystem.LocalStorage.GetDirectoryAsync("AnyCore");
                    if (localStorage != null)
                    {
                        var fileName = Path.GetFileName(SystemsAnyCore[systemName].CoreIcon);
                        var testFile = await localStorage.GetFileAsync(fileName);
                        if (testFile != null)
                        {
                            await testFile.DeleteAsync();
                        }
                    }
                }

                if (Name?.Length > 0) SystemsAnyCore[systemName].CoreName = Name;
                if (System?.Length > 0) SystemsAnyCore[systemName].CoreSystem = System;
                if (Icon?.Length > 0) SystemsAnyCore[systemName].CoreIcon = Icon;
                SystemsAnyCore[systemName].Pinned = Pinned;
                if (Name?.Length > 0 || System?.Length > 0 || Icon?.Length > 0)
                {
                    await AnyCoreStoreAsync();
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
        }
        public async Task DeleteAnyCore(string SystemName)
        {
            try
            {
                GameSystemAnyCore testCore = null;
                if (SystemsAnyCore.TryGetValue(SystemName, out testCore))
                {
                    var SystemIcon = SystemsAnyCore[SystemName].CoreIcon;
                    if (SystemIcon.Length > 0 && !SystemIcon.Contains("ms-appx"))
                    {
                        var localStorage = await FileSystem.LocalStorage.GetDirectoryAsync("AnyCore");
                        if (localStorage != null)
                        {
                            var fileName = Path.GetFileName(SystemIcon);
                            var testFile = await localStorage.GetFileAsync(fileName);
                            if (testFile != null)
                            {
                                await testFile.DeleteAsync();
                            }
                        }
                    }
                    SystemsAnyCore.Remove(SystemName);
                    await AnyCoreStoreAsync();
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
        }
        public static bool isPinnedCore(string filePath)
        {
            try
            {
                var DLLName = Path.GetFileNameWithoutExtension(filePath);
                if (SystemsAnyCore.Count > 0)
                {
                    foreach (var SystemItem in SystemsAnyCore.Values)
                    {
                        if (SystemItem.DLLName != null && SystemItem.DLLName.Equals(DLLName))
                        {
                            return SystemItem.Pinned;
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            return false;
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

        public async Task DownloadBIOSMapSample()
        {
            try
            {
                SystemCoreIsLoadingState(true);
                if (SystemsAnyCore.Count > 0)
                {
                    Encoding unicodeText = Encoding.Unicode;
                    byte[] dictionaryListBytes = unicodeText.GetBytes(JsonConvert.SerializeObject(SystemsAnyCore.First().Value.BiosFilesSample));
                    var saveFile = await CrossFileSystem.Current.PickSaveFileAsync(".rab");
                    await Task.Delay(700);
                    if (saveFile != null)
                    {
                        using (var outStream = await saveFile.OpenAsync(FileAccess.ReadWrite))
                        {
                            await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                            await outStream.FlushAsync();
                        }

                        PlatformService.PlayNotificationSound("success.wav");
                        await UserDialogs.Instance.AlertAsync("BIOS map sample file saved\nUse text editor to change it", "Save Done");
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }
            SystemCoreIsLoadingState(false);
        }

        public static FileDependency[] GetAnyCoreBIOSFiles(string filePath)
        {

            try
            {
                Dictionary<string, string[]> BIOSTempList = new Dictionary<string, string[]>();
                var DLLName = Path.GetFileNameWithoutExtension(filePath);
                if (SystemsAnyCore.Count > 0)
                {
                    foreach (var SystemItem in SystemsAnyCore.Values)
                    {
                        if (SystemItem.DLLName != null && SystemItem.DLLName.Equals(DLLName))
                        {
                            BIOSTempList = SystemItem.BiosFiles;
                            break;
                        }
                    }
                    if (BIOSTempList.Count > 0)
                    {
                        List<FileDependency> tempFileDependencies = new List<FileDependency>();
                        foreach (var BIOSItem in BIOSTempList.Keys)
                        {
                            tempFileDependencies.Add(new FileDependency(BIOSItem, BIOSTempList[BIOSItem][0], BIOSTempList[BIOSItem][1]));
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


        public static string SystemCacheLocation = "SystemsCache";
        public static async Task StoreSystemsCache(GameSystemViewModel gameSystemViewModels)
        {
            try
            {
                var localFolder = await CrossFileSystem.Current.RoamingStorage.GetDirectoryAsync(SystemCacheLocation);
                if (localFolder == null)
                {
                    await CrossFileSystem.Current.RoamingStorage.CreateDirectoryAsync(SystemCacheLocation);
                    localFolder = await CrossFileSystem.Current.RoamingStorage.GetDirectoryAsync(SystemCacheLocation);
                }

                var targetFileTest = await localFolder.GetFileAsync($"{gameSystemViewModels.TempName}Core.rca");
                if (targetFileTest != null)
                {
                    await targetFileTest.DeleteAsync();
                }
                var targetFile = await localFolder.CreateFileAsync($"{gameSystemViewModels.TempName}Core.rca");

                Encoding unicode = Encoding.Unicode;
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(gameSystemViewModels));
                using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                {
                    await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                    await outStream.FlushAsync();
                }

            }
            catch (Exception e)
            {

            }
        }

        public static async Task<IList<GameSystemViewModel>> GetSystemsCache()
        {
            try
            {
                var localFolder = await CrossFileSystem.Current.RoamingStorage.GetDirectoryAsync(SystemCacheLocation);
                if (localFolder == null)
                {
                    return null;
                }

                var targetFileTest = await localFolder.GetFileAsync("cores.rca");
                if (targetFileTest != null)
                {
                    Encoding unicode = Encoding.Unicode;
                    byte[] result;
                    using (var outStream = await targetFileTest.OpenAsync(FileAccess.Read))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            outStream.CopyTo(memoryStream);
                            result = memoryStream.ToArray();
                        }
                        await outStream.FlushAsync();
                    }
                    string CoreFileContent = unicode.GetString(result);
                    var dictionaryList = JsonConvert.DeserializeObject<IList<GameSystemViewModel>>(CoreFileContent);

                    if (dictionaryList != null)
                    {
                        return dictionaryList;
                    }

                }

            }
            catch (Exception e)
            {

            }
            return null;
        }
    }
    public static class ExtensionMethods
    {
        public static string ToFileSize(this ulong l)
        {
            try
            {
                return String.Format(new FileSizeFormatProvider(), "{0:fs}", l);
            }
            catch (Exception e)
            {
                return "0 KB";
            }
        }
    }

}