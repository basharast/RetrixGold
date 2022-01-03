using Acr.UserDialogs;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MvvmCross.Platform;
using MvvmCross.Uwp.Views;
using Newtonsoft.Json;
using Plugin.FileSystem;
using RavinduL.LocalNotifications;
using RavinduL.LocalNotifications.Notifications;
using RetriX.Shared.Services;
using RetriX.Shared.ViewModels;
using RetriX.UWP.Controls;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WinUniversalTool.Models;

namespace RetriX.UWP.Pages
{
    public sealed partial class GamePlayerView : MvxWindowsPage
    {
        public GamePlayerViewModel VM => ViewModel as GamePlayerViewModel;
        private VideoService Renderer { get; } = Mvx.Resolve<IVideoService>() as VideoService;

        int InitWidthSize { get => PlatformService.InitWidthSize; }
        int InitWidthSizeCustom { get => PlatformService.InitWidthSize - 20; }
        HorizontalAlignment horizontalAlignment { get => PlatformService.horizontalAlignment; }
        public LocalNotificationManager LocalNotificationManager { get; private set; }
        public Thickness boxMargin = new Thickness(0, 500, 0, 0);
        public GamePlayerView()
        {
            InitializeComponent();

            try
            {
                var notificationGrid = new Grid();
                LocalNotificationManager = new LocalNotificationManager(notificationGrid);
                localNotificationGrid.Children.Add(notificationGrid);
            }
            catch (Exception ex)
            {

            }
            PlatformService.NotificationHandler += pushLocalNotification;

            Unloaded += OnUnloading;

            Renderer.RenderPanel = PlayerPanel;

            try
            {
                SetCoreOptionsHandler();
                PrepareEffectsBox();
                RightVirtualPad.GetButtonMap();
                PreviousPoint.X = 0;
                PreviousPoint.Y = 0;

                Window.Current.SizeChanged += (sender, args) =>
                {
                    CheckEffectsBoxMargin();
                    try
                    {
                        Bindings.Update();
                    }
                    catch (Exception ex)
                    {

                    }
                };
                PlatformService.checkInitWidth(false);
                CustomGamePadRetrieveAsync();
                CheckEffectsBoxMargin();
            }
            catch (Exception e)
            {

            }
        }

        private void CheckEffectsBoxMargin()
        {
            try
            {
                var currentHeight = Window.Current.CoreWindow.Bounds.Height;
                if (currentHeight > 1000)
                {
                    boxMargin = new Thickness(0, 500, 0, 0);
                }
                else if (currentHeight > 800)
                {
                    boxMargin = new Thickness(0, 400, 0, 0);
                }
                else if (currentHeight > 650)
                {
                    boxMargin = new Thickness(0, 350, 0, 0);
                }
                else if (currentHeight > 550)
                {
                    boxMargin = new Thickness(0, 250, 0, 0);
                }
                else if (currentHeight > 450)
                {
                    boxMargin = new Thickness(0, 150, 0, 0);
                }
                else
                {
                    boxMargin = new Thickness(0, 80, 0, 0);
                }
            }
            catch (Exception e)
            {

            }
        }
        private async void pushLocalNotification(string text, Color background, Color forground, char icon = '\0', int time = 3, Position position = Position.Bottom, EventHandler eventHandler = null)
        {
            try
            {
                if (background == Colors.Orange)
                {
                    background = Colors.DarkOrange;
                }
                text = text.Replace("NoTelnet", "").Replace("notelnet", "").Replace("noTelnet", "").Replace("Notelnet", "");
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                {
                    try
                    {
                        var backGroundColor = new SolidColorBrush(background == null ? Colors.DodgerBlue : background);
                        var foreGroundColor = new SolidColorBrush(forground == null ? Colors.White : forground);
                        LocalNotificationManager.Show(new SimpleNotification
                        {
                            Text = text,
                            TimeSpan = TimeSpan.FromSeconds(time),
                            VerticalAlignment = position == Position.Bottom ? VerticalAlignment.Bottom : VerticalAlignment.Top,
                            Glyph = icon.ToString(),
                            Background = backGroundColor,
                            Foreground = foreGroundColor,
                            Padding = new Thickness(20),
                            Action = () => { if (eventHandler != null) eventHandler.Invoke(null, EventArgs.Empty); },
                        });
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
            catch (Exception ex)
            {

            }
        }
        Position DefaultPosition = Position.Top;
        public void pushLocalNotification(object sender, EventArgs args)
        {
            try
            {

                var NotificationData = (LocalNotificationData)args;
                if (NotificationData != null)
                {
                    try
                    {
                        if (NotificationData.time == 0)
                        {
                            LocalNotificationManager.HideAll();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    pushLocalNotification(NotificationData.message, Colors.DodgerBlue, Colors.White, NotificationData.icon, NotificationData.time, DefaultPosition);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void SetCoreOptionsHandler()
        {
            try
            {
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        await Task.Delay(1500);
                        VM.CoreOptionsHandler += InitialCoresOptionsMenu;
                        VM.ControlsHandler += InitialControlsMenu;
                        VM.SnapshotHandler += SaveSnapshot;

                        /*VM.RightTransformXDefault = RightControlsTransform.TranslateX;
                        VM.RightTransformYDefault = RightControlsTransform.TranslateY;

                        VM.LeftTransformXDefault = LeftControlsTransform.TranslateX;
                        VM.LeftTransformYDefault = LeftControlsTransform.TranslateY;

                        VM.ActionsTransformXDefault = ActionsControlsTransform.TranslateX;
                        VM.ActionsTransformYDefault = ActionsControlsTransform.TranslateY;*/
                    }
                    catch (Exception ec)
                    {
                        PlatformService.ShowErrorMessageDirect(ec);
                        OptionsLoadingProgress.Visibility = Visibility.Collapsed;
                    }
                });

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }


        public static Point PreviousPoint;
        bool PointerPressedState = false;
        double PointerCurrentX = 0;
        double PointerCurrentY = 0;
        private void Moved(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (PointerPressedState)
                {
                    PointerPoint CurrentPosition = e.GetCurrentPoint(sender as CanvasAnimatedControl);
                    var CanvasWidth = ((CanvasAnimatedControl)sender).RenderSize.Width;
                    var CanvasHeight = ((CanvasAnimatedControl)sender).RenderSize.Height;
                    Point PointerPosition = CurrentPosition.Position;
                    var currentPointX = PointerPosition.X;
                    var currentPointY = PointerPosition.Y;
                    if (PreviousPoint != null)
                    {
                        double xDistance = PointerPosition.X - PreviousPoint.X;
                        double yDistance = PointerPosition.Y - PreviousPoint.Y;
                        InputService.MousePointerPosition[0] = xDistance * 5;
                        InputService.MousePointerPosition[1] = yDistance * 5;

                        if ((PointerCurrentX + xDistance) > CanvasWidth || (PointerCurrentX + xDistance) < 0)
                        {
                            xDistance = 0;
                        }
                        if ((PointerCurrentX + yDistance) > CanvasHeight || (PointerCurrentX + yDistance) < 0)
                        {
                            yDistance = 0;
                        }

                        PointerCurrentX += xDistance;
                        PointerCurrentY += yDistance;

                        double Distance = Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
                        // TODO use Distance
                    }
                    PreviousPoint = PointerPosition;
                    //OptionsNotice.Text = InputService.MousePointerPosition[0].ToString()+", "+ InputService.MousePointerPosition[1].ToString();
                }
            }
            catch (Exception ex)
            {
                //PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void OnUnloading(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                Renderer.RenderPanel = null;
                PlayerPanel.RemoveFromVisualTree();
                PlayerPanel.ResetElapsedTime();
                PlayerPanel.UpdateLayout();
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private new void PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerPressedState = true;
        }
        private new void PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                PointerPressedState = false;
                InputService.MousePointerPosition[0] = 0;
                InputService.MousePointerPosition[1] = 0;
            }
            catch (Exception ex)
            {

            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.CancelActionsSet();
            }
            catch (Exception ex)
            {

            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.SaveActionsSet();
            }
            catch (Exception ex)
            {

            }
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.ResetActionsSet();
            }
            catch (Exception ex)
            {

            }
        }

        private void PlayerPanel_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {

        }

        private void InitialCoresOptionsMenu(object sender, EventArgs e)
        {
            if (systemCoreReady)
            {
                return;
            }
            try
            {
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                  {
                      try
                      {
                          OptionsLoadingProgress.Visibility = Visibility.Visible;
                          await Task.Delay(500);
                          await SystemCoresOptions();
                          OptionsLoadingProgress.Visibility = Visibility.Collapsed;
                      }
                      catch (Exception ec)
                      {
                          PlatformService.ShowErrorMessageDirect(ec);
                          OptionsLoadingProgress.Visibility = Visibility.Collapsed;
                      }
                  });

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }


        private async void OptionsSave_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                if (VM.SystemName != null)
                {
                    var TargetSystem = VM.SystemName;
                    if (TargetSystem != null && TargetSystem.Length > 0)
                    {
                        PlatformService.PlayNotificationSoundDirect("alert.wav");
                        ConfirmConfig confirmSave = new ConfirmConfig();
                        confirmSave.SetTitle("Reset Options");
                        confirmSave.SetMessage($"Do you want to save {VM.SystemNamePreview}'s options as default values? ");
                        confirmSave.UseYesNo();

                        var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                        if (StartSave)
                        {
                            await VM.CoreOptionsStoreAsync(TargetSystem);
                            PlatformService.PlayNotificationSoundDirect("success.wav");
                            await UserDialogs.Instance.AlertAsync($"{VM.SystemNamePreview}'s options has been saved", "Save Done");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void OptionsCancel_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                VM.SetCoreOptionsVisible.Execute();
            }
            catch (Exception ex)
            {

            }
        }

        bool systemCoreReady = false;
        private async Task SystemCoresOptions()
        {
            try
            {
                CoreOptionsSubPage.Children.Clear();
                var TargetSystem = VM.SystemName;

                var CurrentOptions = GameSystemSelectionViewModel.SystemsOptions[TargetSystem].OptionsList.Keys;
                PlatformService.PlayNotificationSoundDirect("select.mp3");
                if (CurrentOptions.Count > 0)
                {
                    NoOptionsTextGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    foreach (var CoreItem in CurrentOptions)
                    {
                        TextBlock SystemOptionText = new TextBlock();
                        ComboBox comboBox = new ComboBox();
                        var OptionValues = GameSystemSelectionViewModel.SystemsOptions[TargetSystem].OptionsList[CoreItem];
                        SystemOptionText.Text = OptionValues.OptionsDescription;
                        SystemOptionText.Foreground = OptionsMainTitle.Foreground;
                        SystemOptionText.Margin = OptionsMainTitle.Margin;
                        SystemOptionText.TextWrapping = TextWrapping.WrapWholeWords;
                        foreach (var OptionValue in OptionValues.OptionsValues)
                        {
                            comboBox.Items.Add(OptionValue);
                        }
                        comboBox.SelectedIndex = (int)OptionValues.SelectedIndex;
                        comboBox.HorizontalAlignment = SystemCoresList.HorizontalAlignment;
                        comboBox.Foreground = SystemCoresList.Foreground;
                        comboBox.Margin = SystemCoresList.Margin;
                        comboBox.Tag = TargetSystem;
                        comboBox.Name = OptionValues.OptionsKey;
                        Grid.SetColumnSpan(comboBox, 2);
                        Grid.SetColumnSpan(SystemOptionText, 2);

                        //Add Event
                        comboBox.SelectionChanged += SelectionChangedEvent;
                        CoreOptionsSubPage.Children.Add(SystemOptionText);
                        CoreOptionsSubPage.Children.Add(comboBox);
                    }
                    systemCoreReady = true;
                }
                else
                {
                    NoOptionsTextGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    PlatformService.PlayNotificationSoundDirect("notice.mp3");
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private void SelectionChangedEvent(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var TargetSystem = ((ComboBox)sender).Tag.ToString();
                var TargetIndex = ((ComboBox)sender).SelectedIndex;
                var TargetKey = ((ComboBox)sender).Name;
                GameSystemSelectionViewModel.setCoresOptionsDictionaryDirect(TargetSystem, TargetKey, (uint)TargetIndex);
                PlatformService.PlayNotificationSoundDirect("option-changed.wav");
                VM.updateCoreOptions(TargetKey);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void PlayerPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                VM.PointerTabbedCommand.Execute();
                VM.TappedCommand2.Execute();
            }
            catch (Exception ex)
            {

            }
        }

        private void PlayerPanel_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {

        }

        private void OptionsReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.updateCoreOptions();
            }
            catch (Exception ex)
            {

            }
        }

        private async void OptionsInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("notice.mp3");
                await UserDialogs.Instance.AlertAsync("If you found 'Restart' word behind the option, that's mean you have to close the game (not the app) and reopen it to apply the changes");
            }
            catch (Exception ex)
            {

            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void ActionsHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("notice.mp3");
                await UserDialogs.Instance.AlertAsync($"Press on the buttons to record actions.\n\nActions speed can be set from\n\u2605 -> Actions -> Speed\n\nA + A (Action + Action) \u271A:\nThis options used when the movements required to press two buttons at once\nYou have to activate the 'checkbox' for each button\n\n Visit \u2754 help page for more");
            }
            catch (Exception ex)
            {

            }
        }

        private async void SaveSnapshot(object sender, EventArgs eventArgs)
        {
            try
            {
                //ClearSession();
            }
            catch
            {

            }
            try
            {
                string GameID = ((GameIDArgs)eventArgs).GameID;
                var SaveLocation = ((GameIDArgs)eventArgs).SaveLocation;
                if (SaveLocation == null)
                {
                    return;
                }
                if (VM != null && GameID.Length > 0)
                {
                    VM.SnapshotInProgress = true;
                    RenderTargetBitmap rtb = new RenderTargetBitmap();
                    await rtb.RenderAsync(PlayerPanel);
                    bool bufferOK = false;
                    byte[] pixels = null;
                    uint bufferSize;
                    try
                    {
                        var pixelBuffer = await rtb.GetPixelsAsync();
                        pixels = pixelBuffer.ToArray();
                        if (pixelBuffer.Length == 0)
                        {
                            return;
                        }
                        bufferOK = true;
                    }
                    catch (Exception es)
                    {

                    }
                    if (bufferOK)
                    {

                        var displayInformation = DisplayInformation.GetForCurrentView();

                        var TargetFile = $"{GameID}.png";
                        var TestFile = await SaveLocation.GetFileAsync(TargetFile);
                        if (TestFile != null)
                        {
                            await TestFile.DeleteAsync();
                        }
                        var saveFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync((SaveLocation.FullName.Replace($@"{CrossFileSystem.Current.LocalStorage.FullName}\", "")));
                        var file = await saveFolder.CreateFileAsync(TargetFile, CreationCollisionOption.ReplaceExisting);
                        using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        {

                            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                                 BitmapAlphaMode.Premultiplied,
                                                 (uint)rtb.PixelWidth,
                                                 (uint)rtb.PixelHeight,
                                                 displayInformation.RawDpiX,
                                                 displayInformation.RawDpiY,
                                                 pixels);
                            encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;

                            // Get the image's original width and height
                            int originalWidth = rtb.PixelWidth;
                            int originalHeight = rtb.PixelHeight;

                            // To preserve the aspect ratio
                            int maxSize = 300;
                            try
                            {
                                maxSize = new Random().Next(290, 310);
                            }
                            catch (Exception e)
                            {

                            }
                            float ratioX = (float)maxSize / (float)originalWidth;
                            float ratioY = (float)maxSize / (float)originalHeight;
                            float ratio = Math.Min(ratioX, ratioY);

                            float sourceRatio = (float)originalWidth / originalHeight;

                            // New width and height based on aspect ratio
                            int newWidth = (int)(originalWidth * ratio);
                            int newHeight = (int)(originalHeight * ratio);

                            encoder.BitmapTransform.ScaledWidth = (uint)newWidth;
                            encoder.BitmapTransform.ScaledHeight = (uint)newHeight;
                            await encoder.FlushAsync();

                        }

                        //Check if blank image
                        try
                        {
                            await Task.Delay(200);
                            var testFile = await SaveLocation.GetFileAsync(TargetFile);
                            if (testFile != null)
                            {
                                ulong testFileSize = await testFile.GetLengthAsync();
                                if (testFileSize < 1900)
                                {
                                    await testFile.DeleteAsync();
                                }
                            }
                        }
                        catch (Exception eb)
                        {

                        }
                    }
                }
                VM.SnapshotInProgress = false;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessageDirect(e);
                VM.SnapshotInProgress = false;
            }


        }

        private void SavesInfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SavesCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.HideSavesList();
            }
            catch (Exception ex)
            {

            }
        }

        private void SavesQuick_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.ShowQuickSaves();
            }
            catch (Exception ex)
            {

            }
        }

        private void SavesAuto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.ShowAutoSaves();
            }
            catch (Exception ex)
            {

            }
        }

        private void SavesSlots_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.ShowSlotsSaves();
            }
            catch (Exception ex)
            {

            }
        }

        private void SavesAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.ShowAllSaves();
            }
            catch (Exception ex)
            {

            }
        }

        private void RightControlSetup(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                if (VM.GameStopStarted || !VM.ScaleFactorVisible) return;
                var PositionX = e.Delta.Translation.X / VM.RightScaleFactorValue;
                var PositionY = e.Delta.Translation.Y / VM.RightScaleFactorValue;
                VM.RightTransformXCurrent += PositionX;
                VM.RightTransformYCurrent += PositionY;
            }
            catch (Exception ex)
            {

            }
        }

        private void LeftControlSetup(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                if (VM.GameStopStarted || !VM.ScaleFactorVisible) return;
                var PositionX = e.Delta.Translation.X;
                var PositionY = e.Delta.Translation.Y;
                VM.LeftTransformXCurrent += PositionX / VM.LeftScaleFactorValue;
                VM.LeftTransformYCurrent += PositionY / VM.LeftScaleFactorValue;
            }
            catch (Exception ex)
            {

            }
        }
        private void ActionsControlSetup(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                if (VM.GameStopStarted || !VM.ScaleFactorVisible) return;
                var PositionX = e.Delta.Translation.X;
                var PositionY = e.Delta.Translation.Y;
                VM.ActionsTransformXCurrent += PositionX / VM.LeftScaleFactorValue;
                VM.ActionsTransformYCurrent += PositionY / VM.LeftScaleFactorValue;
            }
            catch (Exception ex)
            {

            }
        }

        private void DisableEditMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.SetScaleFactorVisible.Execute();
            }
            catch (Exception ex)
            {

            }
        }

        private async void CloseLogList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.SetShowLogsList.Execute();
            }
            catch (Exception ex)
            {

            }
        }

        private async void SaveCustomization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                if (VM.SystemNamePreview != null)
                {
                    var TargetSystem = VM.SystemNamePreview;
                    if (TargetSystem != null && TargetSystem.Length > 0)
                    {
                        PlatformService.PlayNotificationSoundDirect("alert.wav");
                        ConfirmConfig confirmSave = new ConfirmConfig();
                        confirmSave.SetTitle("Save Customizations");
                        confirmSave.SetMessage($"Do you want to save these customizations for {TargetSystem}? ");
                        confirmSave.UseYesNo();

                        var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                        if (StartSave)
                        {
                            await VM.CustomTouchPadStoreAsync();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private async void DeleteCustomization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                if (VM.SystemNamePreview != null)
                {
                    var TargetSystem = VM.SystemNamePreview;
                    if (TargetSystem != null && TargetSystem.Length > 0)
                    {
                        PlatformService.PlayNotificationSoundDirect("alert.wav");
                        ConfirmConfig confirmDelete = new ConfirmConfig();
                        confirmDelete.SetTitle("Delete Customizations");
                        confirmDelete.SetMessage($"Do you want to delete current customizations for {TargetSystem}? ");
                        confirmDelete.UseYesNo();

                        var StartDelete = await UserDialogs.Instance.ConfirmAsync(confirmDelete);

                        if (StartDelete)
                        {
                            await VM.CustomTouchPadDeleteAsync();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void ResetCustomization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.ResetAdjustmentsCall();
            }
            catch (Exception ex)
            {

            }
        }

        bool systemControlsReady = false;
        private void InitialControlsMenu(object sender, EventArgs e)
        {
            if (systemControlsReady)
            {
                return;
            }
            try
            {
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        ControlsLoadingProgress.Visibility = Visibility.Visible;
                        await Task.Delay(500);
                        await SystemControls();
                        ControlsLoadingProgress.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ec)
                    {
                        PlatformService.ShowErrorMessageDirect(ec);
                        ControlsLoadingProgress.Visibility = Visibility.Collapsed;
                    }
                });

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }


        private async Task SystemControls()
        {
            try
            {
                ControlsSubPage.Children.Clear();
                var TargetSystem = VM.SystemName;

                PlatformService.PlayNotificationSoundDirect("select.mp3");
                if (InputService.CurrentButtons.Count > 0)
                {

                    foreach (var ButtonItem in InputService.CurrentButtons)
                    {
                        TextBlock SystemOptionText = new TextBlock();
                        ComboBox comboBox = new ComboBox();

                        SystemOptionText.Inlines.Add(new Windows.UI.Xaml.Documents.Run { Text = ButtonItem.Value });
                        SystemOptionText.Foreground = OptionsMainTitle.Foreground;
                        SystemOptionText.Margin = OptionsMainTitle.Margin;
                        SystemOptionText.TextWrapping = TextWrapping.WrapWholeWords;
                        foreach (var OptionValue in InputService.GamepadMap.Keys)
                        {
                            comboBox.Items.Add(OptionValue);
                        }
                        comboBox.SelectedIndex = InputService.GetGamePadSelectedIndex(ButtonItem.Key);
                        comboBox.HorizontalAlignment = SystemCoresList.HorizontalAlignment;
                        comboBox.Foreground = SystemCoresList.Foreground;
                        comboBox.Margin = SystemCoresList.Margin;
                        comboBox.Tag = ButtonItem.Key;
                        comboBox.Name = ButtonItem.Value;
                        Grid.SetColumnSpan(comboBox, 2);
                        Grid.SetColumnSpan(SystemOptionText, 2);

                        //Add Event
                        comboBox.SelectionChanged += ControlsSelectionChangedEvent;
                        ControlsSubPage.Children.Add(SystemOptionText);
                        ControlsSubPage.Children.Add(comboBox);
                    }
                    systemControlsReady = true;
                }
                else
                {
                    PlatformService.PlayNotificationSoundDirect("notice.mp3");
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void ControlsSelectionChangedEvent(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var TargetSystem = ((ComboBox)sender).Tag;
                var TargetIndex = ((ComboBox)sender).SelectedIndex;
                var TargetKey = ((ComboBox)sender).Name;
                PlatformService.PlayNotificationSoundDirect("option-changed.wav");
                InputService.ChangeGamepadButton((InjectedInputTypes)TargetSystem, TargetIndex);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private async void ControlsCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.SetControlsMapVisible.Execute();
                if (VM.EmulationService.CorePaused)
                {
                    await VM.EmulationService.ResumeGameAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void ControlsReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                if (VM.SystemNamePreview != null)
                {
                    var TargetSystem = VM.SystemNamePreview;
                    if (TargetSystem != null && TargetSystem.Length > 0)
                    {
                        PlatformService.PlayNotificationSoundDirect("alert.wav");
                        ConfirmConfig confirmDelete = new ConfirmConfig();
                        confirmDelete.SetTitle("Reset Controls");
                        confirmDelete.SetMessage($"Do you want to reset the controls for {TargetSystem}? ");
                        confirmDelete.UseYesNo();

                        var StartDelete = await UserDialogs.Instance.ConfirmAsync(confirmDelete);

                        if (StartDelete)
                        {
                            await CustomGamePadDeleteAsync();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private async void ControlsSave_Click(object sender, RoutedEventArgs e)
        {


            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                if (VM.SystemNamePreview != null)
                {
                    var TargetSystem = VM.SystemNamePreview;
                    if (TargetSystem != null && TargetSystem.Length > 0)
                    {
                        PlatformService.PlayNotificationSoundDirect("alert.wav");
                        ConfirmConfig confirmSave = new ConfirmConfig();
                        confirmSave.SetTitle("Save Gamepad");
                        confirmSave.SetMessage($"Do you want to save these controls for {TargetSystem}? ");
                        confirmSave.UseYesNo();

                        var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                        if (StartSave)
                        {
                            await CustomGamePadStoreAsync();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        string GamepadSaveLocation = "GamepadMap";
        public async Task CustomGamePadStoreAsync()
        {
            try
            {
                ControlsLoadingProgress.Visibility = Visibility.Visible;
                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(GamepadSaveLocation);
                if (localFolder == null)
                {
                    await CrossFileSystem.Current.LocalStorage.CreateDirectoryAsync(GamepadSaveLocation);
                    localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(GamepadSaveLocation);
                }

                var targetFileTest = await localFolder.GetFileAsync($"{VM.SystemName}.rgm");
                if (targetFileTest != null)
                {
                    await targetFileTest.DeleteAsync();
                }
                var targetFile = await localFolder.CreateFileAsync($"{VM.SystemName}.rgm");

                Encoding unicode = Encoding.Unicode;
                byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(InputService.GamepadMapWithInput));
                using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                {
                    await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                    await outStream.FlushAsync();
                }
                PlatformService.PlayNotificationSoundDirect("success.wav");
                await UserDialogs.Instance.AlertAsync($"Controls saved for {VM.SystemNamePreview}");
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessageDirect(e);
            }
            ControlsLoadingProgress.Visibility = Visibility.Collapsed;
        }

        public async void CustomGamePadRetrieveAsync()
        {
            try
            {
                //await RequestSessionAsync(ExtendedExecutionReason.Unspecified, SessionRevoked, $"Game Session");
            }
            catch (Exception e)
            {

            }
            try
            {
                while (VM == null || VM.SystemName == null)
                {
                    await Task.Delay(650);
                }
                InputService.ResetGamepadButtons();
                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(GamepadSaveLocation);
                if (localFolder == null)
                {
                    return;
                }

                var targetFileTest = await localFolder.GetFileAsync($"{VM.SystemName}.rgm");
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
                    var dictionaryList = JsonConvert.DeserializeObject<Dictionary<int, int>>(CoreFileContent);

                    if (dictionaryList != null)
                    {
                        InputService.GamepadMapWithInput = dictionaryList;
                        InputService.ReSyncGamepadButtons();
                    }

                }

            }
            catch (Exception e)
            {

            }
        }

        public async Task CustomGamePadDeleteAsync()
        {
            try
            {
                InputService.ResetGamepadButtons();
                //InputService.ResetButtonsPositions();
                systemControlsReady = false;
                InitialControlsMenu(null, EventArgs.Empty);
                PlatformService.PlayNotificationSoundDirect("success.wav");
                await UserDialogs.Instance.AlertAsync($"Controls for {VM.SystemNamePreview} reseted");

                var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(GamepadSaveLocation);
                if (localFolder == null)
                {
                    return;
                }

                var targetFileTest = await localFolder.GetFileAsync($"{VM.SystemName}.rgm");
                if (targetFileTest != null)
                {
                    ControlsLoadingProgress.Visibility = Visibility.Visible;
                    await targetFileTest.DeleteAsync();
                }

                /*var targetFile2Test = await localFolder.GetFileAsync($"{VM.SystemName}.rbm");
                if (targetFile2Test != null)
                {
                    await targetFile2Test.DeleteAsync();
                }*/


            }
            catch (Exception e)
            {

            }
            ControlsLoadingProgress.Visibility = Visibility.Collapsed;
        }

        private void MenusCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.HideMenuGrid();
            }
            catch (Exception ex)
            {

            }
        }



        private void RestoreButtonsPositions()
        {
            try
            {
                InputService.ResetButtonsPositions();
            }
            catch (Exception ex)
            {

            }
        }
        private async void ResetButtonsCustomization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("alert.wav");
                ConfirmConfig confirmSave = new ConfirmConfig();
                confirmSave.SetTitle("Reset Buttons");
                confirmSave.SetMessage($"Do you want to reset buttons customizations for {VM.SystemNamePreview}? ");
                confirmSave.UseYesNo();

                var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                if (StartSave)
                {
                    InputService.ResetGamepadButtons();
                    RestoreButtonsPositions();
                    PlatformService.PlayNotificationSoundDirect("success.wav");
                    await UserDialogs.Instance.AlertAsync($"Buttons for {VM.SystemNamePreview} reseted");

                    var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(GamepadSaveLocation);
                    if (localFolder == null)
                    {
                        return;
                    }

                    var targetFileTest = await localFolder.GetFileAsync($"{VM.SystemName}.rbm");
                    if (targetFileTest != null)
                    {
                        ControlsLoadingProgress.Visibility = Visibility.Visible;
                        await targetFileTest.DeleteAsync();
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
            ControlsLoadingProgress.Visibility = Visibility.Collapsed;
        }

        private async void DisableButtonsEditMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.SetButtonsCustomization.Execute();
                if (VM.EmulationService.CorePaused)
                {
                    await VM.EmulationService.ResumeGameAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void SaveButtonsCustomization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("alert.wav");
                ConfirmConfig confirmSave = new ConfirmConfig();
                confirmSave.SetTitle("Save Buttons");
                confirmSave.SetMessage($"Do you want to save buttons customizations for {VM.SystemNamePreview}? ");
                confirmSave.UseYesNo();

                var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                if (StartSave)
                {
                    VM.SetButtonsIsLoadingState(true);
                    var localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(GamepadSaveLocation);
                    if (localFolder == null)
                    {
                        await CrossFileSystem.Current.LocalStorage.CreateDirectoryAsync(GamepadSaveLocation);
                        localFolder = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync(GamepadSaveLocation);
                    }

                    var targetFileTest = await localFolder.GetFileAsync($"{VM.SystemName}.rbm");
                    if (targetFileTest != null)
                    {
                        await targetFileTest.DeleteAsync();
                    }
                    var targetFile = await localFolder.CreateFileAsync($"{VM.SystemName}.rbm");

                    Encoding unicode = Encoding.Unicode;
                    byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(InputService.ButtonsPositions));
                    using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                    {
                        await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                        await outStream.FlushAsync();
                    }
                    PlatformService.PlayNotificationSoundDirect("success.wav");
                    await UserDialogs.Instance.AlertAsync($"Buttons saved for {VM.SystemNamePreview}");
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
            try
            {
                VM.SetButtonsIsLoadingState(false);
            }
            catch (Exception ex)
            {

            }

        }

        private async void EffectsReset_Click(object sender, RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("button-01.mp3");
            if (VM != null)
            {
                PlatformService.PlayNotificationSoundDirect("alert.wav");
                ConfirmConfig confirmResetAll = new ConfirmConfig();
                confirmResetAll.SetTitle("Reset Effects");
                confirmResetAll.SetMessage($"Do you want to reset all effects?");
                confirmResetAll.UseYesNo();

                var ResetAll = await UserDialogs.Instance.ConfirmAsync(confirmResetAll);

                if (ResetAll)
                {
                    VM.ClearAllEffects.Execute();
                }
            }
        }

        private void EffectsCancel_Click(object sender, RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("button-01.mp3");
            if (VM != null)
            {
                VM.ShowAllEffects.Execute();
            }
        }

        private void RefreshLogList_Click(object sender, RoutedEventArgs e)
        {
            if (VM != null)
            {
                VM.forceReloadLogsList = true;
            }
        }

        private void EffectsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (isComboInitial)
                {
                    return;
                }
                var selectItem = (ComboBoxItem)e.AddedItems[0];
                if (selectItem != null)
                {
                    var SelectedValue = selectItem.Tag.ToString();
                    if (SelectedValue.Equals("All"))
                    {
                        PrepareEffectsBox(true);
                    }
                    else
                    {
                        PrepareEffectsBox(true, SelectedValue);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        bool isComboInitial = false;
        private async void PrepareEffectsBox(bool showOnly = false, string customTarget = "")
        {
            EffectsLoadingProgress.Visibility = Visibility.Visible;
            EffectsList.IsEnabled = false;
            await Task.Run(async () =>
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        isComboInitial = !showOnly;
                        if (isComboInitial)
                        {
                            //Why? I don't know there is some delay prevent the function from getting the elements on page load
                            while (MainPage.FindControl<Border>("HueToRgbEffect") == null)
                            {
                                await Task.Delay(1500);
                            }
                        }
                        var effectsElements = new List<Border>();
                        FindChildren(effectsElements, MainPage);
                        if (effectsElements.Count > 0)
                        {
                            foreach (var effectsElement in effectsElements.OrderBy(item => item.Name))
                            {
                                try
                                {
                                    if (effectsElement.Tag == null)
                                    {
                                        continue;
                                    }
                                    var effectsElementTag = effectsElement.Tag.ToString();
                                    if (effectsElementTag.Equals("EffectsElement"))
                                    {
                                        if (showOnly)
                                        {
                                            if (customTarget.Length > 0)
                                            {
                                                if (effectsElement.Name.Equals(customTarget))
                                                {
                                                    effectsElement.Visibility = Visibility.Visible;
                                                }
                                                else
                                                {
                                                    effectsElement.Visibility = Visibility.Collapsed;
                                                }
                                            }
                                            else
                                            {
                                                effectsElement.Visibility = Visibility.Visible;
                                            }
                                        }
                                        else
                                        {
                                            ComboBoxItem newEffect = new ComboBoxItem();

                                            var borderChilds = new List<TextBlock>();
                                            FindChildren(borderChilds, effectsElement);
                                            if (borderChilds.Count > 0)
                                            {
                                                var effectName = borderChilds.FirstOrDefault().Text;
                                                newEffect.Content = effectName;
                                            }
                                            else
                                            {
                                                newEffect.Content = effectsElement.Name;
                                            }
                                            newEffect.Tag = effectsElement.Name;
                                            EffectsList.Items.Add(newEffect);
                                            effectsElement.Visibility = Visibility.Collapsed;
                                        }
                                    }
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
                    if (isComboInitial)
                    {
                        isComboInitial = false;
                        EffectsList.SelectedIndex = 0;
                    }
                    EffectsList.IsEnabled = true;
                    EffectsLoadingProgress.Visibility = Visibility.Collapsed;
                });
            });
        }
        private void FindChildren<T>(List<T> results, DependencyObject startNode) where T : DependencyObject
        {
            try
            {
                int count = VisualTreeHelper.GetChildrenCount(startNode);
                for (int i = 0; i < count; i++)
                {
                    DependencyObject current = VisualTreeHelper.GetChild(startNode, i);
                    if ((current.GetType()).Equals(typeof(T)) || (current.GetType().GetTypeInfo().IsSubclassOf(typeof(T))))
                    {
                        T asType = (T)current;
                        results.Add(asType);
                    }
                    FindChildren<T>(results, current);
                }
            }
            catch (Exception e)
            {

            }
        }

    }
}
