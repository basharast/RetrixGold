using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.ViewModels;
using RetriX.UWP.Services;
using System;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace RetriX.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private Frame RootFrame => Window.Current.Content as Frame;
        private PlatformService platformService;
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            try { 
            RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
            }catch(Exception e)
            {

            }
            try
            {
                var y = MemoryManager.AppMemoryUsageLimit;
                try
                {
                    var u = MemoryManager.AppMemoryUsage;
                    if (y <= u || y < 100000000)
                    {
                        bool result = MemoryManager.TrySetAppMemoryUsageLimit(y + 100000000);
                    }
                }
                catch (Exception e)
                {
                }
            }
            catch
            {

            }

            //System.Threading.Thread.Sleep(500);
            this.InitializeComponent();


            platformService = new PlatformService();

            try
            {
                _ = new PlatformService().RetriveRecents();
                PlatformService.GetAnyCores();
                GameSystemSelectionViewModel.AnyCoreRetrieveAsync();
            }
            catch
            {

            }
            //System.Threading.Thread.Sleep(700);
            this.Suspending += OnSuspending;

            
        }


        private async void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            try
            {

                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame.CanGoBack && rootFrame.CurrentSourcePageType.Name == "GamePlayerView")
                {
                    e.Handled = true;
                    if (PlatformService.CoreOptionsActive)
                    {
                        platformService.InvokeHideCoreOptionsHandler(rootFrame);
                        PlatformService.PlayNotificationSoundDirect("option-changed.wav");
                    }
                    else if (PlatformService.SavesListActive)
                    {
                        platformService.InvokeHideSavesListHandler(rootFrame);
                        PlatformService.PlayNotificationSoundDirect("option-changed.wav");
                    }
                    else if (!PlatformService.StopGameInProgress)
                    {
                        PlatformService.PlayNotificationSoundDirect("root-needed.wav");
                        ConfirmConfig confirmConfig = new ConfirmConfig();
                        confirmConfig.SetTitle("Stop Playing");
                        confirmConfig.SetMessage("Do you want to stop?");
                        confirmConfig.UseYesNo();
                        var result = await UserDialogs.Instance.ConfirmAsync(confirmConfig);
                        if (result)
                        {
                            //rootFrame.GoBack(new DrillInNavigationTransitionInfo());
                            platformService.InvokeStopHandler(rootFrame);
                        }
                    }
                }
                else
                {
                    if (rootFrame.CurrentSourcePageType.Name == "GameSystemSelectionView" && PlatformService.SubPageActive)
                    {
                        platformService.InvokeSubPageHandler(rootFrame);
                        PlatformService.PlayNotificationSoundDirect("option-changed.wav");
                    }
                    else
                    {
                        if (rootFrame.CurrentSourcePageType.Name == "GameSystemSelectionView" && PlatformService.FilterModeState)
                        {
                            platformService.InvokeResetSelectionHandler(rootFrame);
                            PlatformService.PlayNotificationSoundDirect("option-changed.wav");
                        }
                        else
                        {
                            rootFrame.GoBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //var messageDialog = new MessageDialog(ex.Message.ToString());
                PlatformService.PlayNotificationSoundDirect("alert.wav");
                var messageDialog = new MessageDialog("Do you want to exit?");
                messageDialog.Commands.Add(new UICommand("Exit", new UICommandInvokedHandler(this.CommandInvokedHandler)));
                messageDialog.Commands.Add(new UICommand("Dismiss"));
                await messageDialog.ShowAsync();
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
                    SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;
                    isBackPressedReady = true;
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
            InitializeApp(e.PreviousExecutionState, e.PrelaunchActivated, null);
        }

        protected override void OnFileActivated(FileActivatedEventArgs e)
        {
            try
            {
                if (!isBackPressedReady)
                {
                    SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
                    SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;
                    isBackPressedReady = true;
                }

                var file = e.Files.First(d => d is IStorageFile);
                var wrappedFile = new Plugin.FileSystem.FileInfo((StorageFile)file);
                var FileExtention = Path.GetExtension(file.Name);
                if (FileExtention.Equals(".rbp"))
                {
                    PlatformService.OpenBackupFile = wrappedFile;
                    InitializeApp(e.PreviousExecutionState, false, null);
                }
                else
                {
                    PlatformService.AppStartedByFile = true;
                    InitializeApp(e.PreviousExecutionState, false, wrappedFile);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void InitializeApp(ApplicationExecutionState previousExecutionState, bool prelaunchActivated, IFileInfo file)
        {
            try
            {
                var rootFrame = RootFrame;

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
                    Window.Current.Content = rootFrame;
                }

                if (PlatformService.isCoresLoaded && file == null)
                {
                    if (prelaunchActivated == false)
                    {
                        // Ensure the current window is active
                        Window.Current.Activate();
                    }
                }else
                if (prelaunchActivated == false)
                {
                    if (rootFrame.Content == null)
                    {
                        // When the navigation stack isn't restored navigate to the first page,
                        // configuring the new page by passing required information as a navigation
                        // parameter

                        var setup = new Setup(rootFrame);
                        setup.Initialize();
                    }

                    var start = Mvx.Resolve<IMvxAppStart>();
                    start.Start(file);

                    // Ensure the current window is active
                    Window.Current.Activate();
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessageDirect(e);
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

        private async void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            try
            {
                e.Handled = true;
                PlatformService.PlayNotificationSoundDirect("alert.wav");
                var messageDialog = new MessageDialog("Do you want to exit?");
                messageDialog.Commands.Add(new UICommand("Exit", new UICommandInvokedHandler(this.CommandInvokedHandler)));
                messageDialog.Commands.Add(new UICommand("Dismiss"));
                await messageDialog.ShowAsync();
            }catch(Exception ex)
            {
                CoreApplication.Exit();
            }
        }

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
