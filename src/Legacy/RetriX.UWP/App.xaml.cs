using RetriX.UWP.Pages;
using RetriX.UWP.Services;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

namespace RetriX.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static Frame rootFrame = Window.Current.Content as Frame;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            try
            {
                RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
            }
            catch (Exception e)
            {

            }
            
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        public static bool HandleBackPress = true;
        public static bool CleanTempOnStartup = true;
        public static bool GameStarted = false;
        public static bool backRequested = false;
        private async void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            try
            {
                //Some devices (aka Games handheld trigger GoBack with 'B' key
                //this will cause major issue while playing
                //so the user can disable this event from main page
                e.Handled = true;

                if (!HandleBackPress)
                {
                    if (GameStarted)
                    {
                        //When pressing back in-game it will terminate the game page
                        //and it will cause issue and losing the current progress
                        //we should handle the event without displying any question dialog
                        //but the only way to stop the game then is from the top bar (stop button)
                        return;
                    }
                }
                
                //Except in XBOX/Tablet it will be ignored always while game on
                if (GameStarted && PlatformService.isXBOX)
                {
                    return;
                }

                if (rootFrame.CanGoBack && rootFrame.CurrentSourcePageType.Name == "GamePlayerView")
                {
                    if (PlatformService.XBoxMenuActive)
                    {
                        PlatformService.XBoxMenuRequested(null, EventArgs.Empty);
                        PlatformService.PlayNotificationSoundDirect("option-changed");
                    }
                    else
                    if (PlatformService.CoreOptionsActive)
                    {
                        PlatformService.InvokeHideCoreOptionsHandler(rootFrame);
                        PlatformService.PlayNotificationSoundDirect("option-changed");
                    }
                    else if (PlatformService.SavesListActive)
                    {
                        PlatformService.InvokeHideSavesListHandler(rootFrame);
                        PlatformService.PlayNotificationSoundDirect("option-changed");
                    }
                    else if (PlatformService.LogsVisibile)
                    {
                        if (PlatformService.HideLogsHandler != null)
                        {
                            PlatformService.HideLogsHandler.Invoke(null, null);
                        }
                        PlatformService.PlayNotificationSoundDirect("option-changed");
                    }
                    else if (PlatformService.EffectsActive)
                    {
                        if (PlatformService.HideEffects != null)
                        {
                            PlatformService.HideEffects.Invoke(null, null);
                        }
                        PlatformService.PlayNotificationSoundDirect("option-changed");
                    }
                    else if (PlatformService.ControlsActive)
                    {
                        PlatformService.InvokeHideControlsListHandler();
                        PlatformService.PlayNotificationSoundDirect("option-changed");
                    }
                    else if (PlatformService.KeyboardVisibleState)
                    {
                        if (PlatformService.HideKeyboardHandler != null)
                        {
                            PlatformService.HideKeyboardHandler.Invoke(null, null);
                            PlatformService.PlayNotificationSoundDirect("option-changed");
                        }
                    }
                    else if (!PlatformService.StopGameInProgress)
                    {
                        PlatformService.PlayNotificationSoundDirect("root-needed");
                        ConfirmConfig confirmConfig = new ConfirmConfig();
                        confirmConfig.SetTitle("Stop Playing");
                        confirmConfig.SetMessage("Do you want to stop?");
                        confirmConfig.UseYesNo();
                        var result = await UserDialogs.Instance.ConfirmAsync(confirmConfig);
                        if (result)
                        {
                            PlatformService.InvokeStopHandler(rootFrame);
                        }
                    }
                }
                else
                {
                    if (rootFrame.CurrentSourcePageType.Name == "GameSystemSelectionView" && PlatformService.SubPageActive)
                    {
                        if ((PlatformService.XBoxMode || PlatformService.DPadActive) && !backRequested)
                        {
                            if (PlatformService.ShowNotificationMain("Press again to go back", 2))
                            {
                                backRequested = true;
                                _ = Task.Run(async () =>
                                {
                                    await Task.Delay(3000);
                                    backRequested = false;
                                });
                            }
                        }
                        else
                        {
                            PlatformService.InvokeSubPageHandler(rootFrame);
                            PlatformService.PlayNotificationSoundDirect("option-changed");
                            backRequested = false;
                        }
                    }
                    else
                    {
                        if (rootFrame.CurrentSourcePageType.Name == "GameSystemSelectionView" && PlatformService.FilterModeState)
                        {
                            PlatformService.InvokeResetSelectionHandler(rootFrame);
                            PlatformService.PlayNotificationSoundDirect("option-changed");
                            e.Handled = true;
                        }
                        else if (rootFrame.CurrentSourcePageType.Name == "AboutView" || rootFrame.CurrentSourcePageType.Name == "HelpView")
                        {
                            rootFrame.GoBack();
                        }
                        else
                        {
                            if (!PlatformService.isXBOX)
                            {
                                try
                                {
                                    PlatformService.PlayNotificationSoundDirect("alert");
                                    ConfirmConfig confirmExit = new ConfirmConfig();
                                    confirmExit.SetTitle("Exit");
                                    confirmExit.SetMessage("Do you want to exit?");
                                    confirmExit.UseYesNo();

                                    var confirmExitState = await UserDialogs.Instance.ConfirmAsync(confirmExit);
                                    if (confirmExitState)
                                    {
                                        CoreApplication.Exit();
                                    }
                                }
                                catch
                                {
                                    PlatformService.PlayNotificationSoundDirect("alert");
                                    var messageDialog = new MessageDialog("Do you want to exit?");
                                    messageDialog.Commands.Add(new UICommand("Exit", new UICommandInvokedHandler(this.CommandInvokedHandler)));
                                    messageDialog.Commands.Add(new UICommand("Dismiss"));
                                    await messageDialog.ShowAsync();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                e.Handled = true;
                PlatformService.ShowErrorMessage(ex);
            }
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            // Display message showing the label of the command that was invoked
            CoreApplication.Exit();
        }

        bool isBackPressedReady = false;
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
                if (!isBackPressedReady)
                {
                    SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
                    isBackPressedReady = true;
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
            InitializeApp(e.PreviousExecutionState, e.PrelaunchActivated, e.Arguments);
        }

        [Obsolete]
        protected async override void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            try
            {
                if (!isBackPressedReady)
                {
                    SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
                    isBackPressedReady = true;
                }
                if (args.PreviousExecutionState != ApplicationExecutionState.Running)
                {
                    InitializeApp(args.PreviousExecutionState, false, null);
                }
                ShareOperation shareOperation = args.ShareOperation;
                var data = shareOperation.Data;
                if (shareOperation != null && data != null && data.Contains(StandardDataFormats.WebLink))
                {
                    shareOperation.ReportStarted();
                    string text = (await shareOperation.Data.GetUriAsync()).ToString();
                    var uriLaunch = new Uri($"rgx:{text}");

                    // Launch the URI
                    Windows.System.Launcher.LaunchUriAsync(uriLaunch);

                    shareOperation.ReportCompleted();
                }
            }
            catch (Exception ex)
            {

            }
        }
        protected override void OnActivated(IActivatedEventArgs e)
        {
            try
            {
                if (!isBackPressedReady)
                {
                    SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
                    isBackPressedReady = true;
                }
                if (e.Kind == ActivationKind.Protocol)
                {
                    var protocolArgs = (ProtocolActivatedEventArgs)e;
                    var AbsoluteURI = protocolArgs.Uri.AbsoluteUri;
                    AbsoluteURI = Helpers.escapeSpecialChars(AbsoluteURI);
                    if (AbsoluteURI.Contains("retropass:") || AbsoluteURI.Contains("retropass::"))
                    {
                        var regexPattern = "cmd=(?<cmd>\\w+)\\s\\-L\\s(?<core>.*)\\s\\\"(?<game>.*)\\\"\\s\\&launchOnExit=(?<launchOnExit>.*)";
                        try
                        {
                            Match m = Regex.Match(AbsoluteURI, regexPattern, RegexOptions.IgnoreCase);
                            if (m.Success)
                            {
                                if (m.Groups != null && m.Groups.Count > 0)
                                {
                                    var command = m.Groups["cmd"].ToString();
                                    var core = m.Groups["core"].ToString();
                                    var game = m.Groups["game"].ToString();
                                    var callback = m.Groups["launchOnExit"].ToString();
                                    PlatformService.RetroPassLaunchCMD = command;
                                    PlatformService.RetroPassLaunchCore = core;
                                    PlatformService.RetroPassLaunchGame = game;
                                    PlatformService.RetroPassLaunchOnExit = callback;
                                    PlatformService.AppStartedByRetroPass = true;
                                    if (PlatformService.RetroPassHandler != null)
                                    {
                                        PlatformService.RetroPassHandler.Invoke(null, null);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        PlatformService.AppStartedByRetroPass = false;
                        if (AbsoluteURI.Contains("core:") || AbsoluteURI.Contains("core::"))
                        {
                            PlatformService.RequestingCoreDownload = AbsoluteURI.Replace("core::", "").Replace("core:", "");
                            if (PlatformService.CoreDownloaderHandler != null)
                            {
                                PlatformService.CoreDownloaderHandler.Invoke(null, null);
                            }
                        }else if (AbsoluteURI.Contains("rgxactivate:") || AbsoluteURI.Contains("rgxactivate::"))
                        {
                            var currentState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("RetriXActive", false);
                            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("RetriXActive", true);
                            if (!currentState && PlatformService.CoresLoaderHandler != null)
                            {
                                PlatformService.CoresLoaderHandler.Invoke(null, null);
                            }
                        }
                    }
                }
                else
                {
                    PlatformService.AppStartedByRetroPass = false;
                }
            }
            catch (Exception ex)
            {

            }
            InitializeApp(e.PreviousExecutionState, false, null);
        }
        protected override void OnFileActivated(FileActivatedEventArgs e)
        {
            try
            {
                if (GameStarted)
                {
                    //Don't accept any file open request during the game
                    return;
                }
                if (!isBackPressedReady)
                {
                    SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
                    //SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;
                    isBackPressedReady = true;
                }

                var file = e.Files.First(d => d is IStorageFile);
                var FileExtention = Path.GetExtension(file.Name);
                if (FileExtention.Equals(".rbp"))
                {
                    PlatformService.OpenBackupFile = (StorageFile)file;
                    if (GameSystemSelectionView.DirectRestore != null)
                    {
                        GameSystemSelectionView.DirectRestore.Invoke(null, null);
                    }
                }
                else
                {
                    PlatformService.OpenGameFile = (StorageFile)file;
                    if (PlatformService.pageReady)
                    {
                        PlatformService.ReloadSystemsHandler.Invoke(null, null);
                    }
                }
                InitializeApp(e.PreviousExecutionState, false, null);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        bool isInitialized;
        private void InitializeApp(ApplicationExecutionState previousExecutionState, bool prelaunchActivated, string args)
        {
            if (isInitialized)
            {
                return;
            }
            try
            {

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (rootFrame == null)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame = new Frame();

                    rootFrame.NavigationFailed += OnNavigationFailed;

                    if (previousExecutionState == ApplicationExecutionState.Terminated)
                    {
                        //TODO: Load state from previously suspended application
                    }

                    // Place the frame in the current Window

                    Grid rootGrid = Window.Current.Content as Grid;
                    //Frame rootFrame = rootGrid?.Children.Where((c) => c is Frame).Cast<Frame>().FirstOrDefault();

                    if (rootGrid == null)
                    {
                        rootGrid = new Grid();

                        //var notificationGrid = new Grid();
                        //LocalNotificationManager = new LocalNotificationManager(notificationGrid);

                        rootGrid.Children.Add(rootFrame);
                        //rootGrid.Children.Add(notificationGrid);

                        Window.Current.Content = rootGrid;
                        try
                        {
                            Window.Current.VisibilityChanged += Current_VisibilityChanged;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

                try
                {
                    if (prelaunchActivated == false)
                    {
                        if (rootFrame.Content == null)
                        {
                            // When the navigation stack isn't restored navigate to the first page,
                            // configuring the new page by passing required information as a navigation
                            // parameter
                            rootFrame.Navigate(typeof(GameSystemSelectionView), args);
                        }
                        // Ensure the current window is active
                        Window.Current.Activate();
                    }
                    isInitialized = true;
                }
                catch (Exception ex)
                {

                }
                isInitialized = true;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessageDirect(e);
            }
        }
        private void Current_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            try
            {
                Helpers.isAppVisible = e.Visible;
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            //throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
            PlatformService.ShowErrorMessageDirect(e.Exception);
        }

        /*private async void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            try
            {
                e.Handled = true;
                PlatformService.PlayNotificationSoundDirect("alert");
                var messageDialog = new MessageDialog("Do you want to exit?");
                messageDialog.Commands.Add(new UICommand("Exit", new UICommandInvokedHandler(this.CommandInvokedHandler)));
                messageDialog.Commands.Add(new UICommand("Dismiss"));
                await messageDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                CoreApplication.Exit();
            }
        }*/

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            try
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                //TODO: Save application state and stop any background activity
                deferral.Complete();
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
    }
}
