using Acr.UserDialogs;
using MvvmCross.Uwp.Views;
using Plugin.FileSystem;
using Plugin.FileSystem.Abstractions;
using RavinduL.LocalNotifications;
using RavinduL.LocalNotifications.Notifications;
using RetriX.Shared.ViewModels;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using WinUniversalTool.Models;

namespace RetriX.UWP.Pages
{
    public sealed partial class GameSystemSelectionView : MvxWindowsPage
    {
        public GameSystemSelectionViewModel VM => ViewModel as GameSystemSelectionViewModel;

        int InitWidthSize { get => PlatformService.InitWidthSize; }
        int InitWidthSizeCustom { get => PlatformService.InitWidthSize - 20; }
        bool ShowErrorsIcon { get { return GameSystemsProviderService.ShowErrorNotification; } }
        bool ShowErrorsList = false;
        ObservableCollection<string> SkippedList { get { return GameSystemsProviderService.SkippedList; } }
        HorizontalAlignment horizontalAlignment { get => PlatformService.horizontalAlignment; }
        public LocalNotificationManager LocalNotificationManager { get; private set; }
        public GameSystemSelectionView()
        {
            this.InitializeComponent();

            try
            {
                var notificationGrid = new Grid();
                LocalNotificationManager = new LocalNotificationManager(notificationGrid);
                localNotificationGrid.Children.Add(notificationGrid);
            }
            catch (Exception ex)
            {

            }
            PlatformService.NotificationHandlerMain += pushLocalNotification;

            if (PlatformService.pageReady)
            {
                ProgressContainer.Visibility = Visibility.Collapsed;
                //return;
            }
            try
            {
                PlatformService.MuteSFX = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("MuteSFX", false);
                MusteSFX.IsChecked = PlatformService.MuteSFX;
            }
            catch (Exception e)
            {

            }
            ProgressContainer.Visibility = Visibility.Visible;
            CoresOptionsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            try
            {
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                  {
                      try
                      {
                          await Task.Delay(1500);
                          while (VM == null)
                          {
                              await Task.Delay(500);
                          }
                          InitialCoresOptionsMenu();
                          PlatformService.SetHideSubPageHandler(HideSubPages);

                          if (PlatformService.OpenBackupFile != null)
                          {
                              VM.ShowSettings.Execute();
                          }
                      }
                      catch (Exception er)
                      {
                          PlatformService.ShowErrorMessageDirect(er);
                      }
                  });
                PlatformService.SaveListStateGlobal += GameSystemSelectionView_eventHandler;
                PlatformService.RestoreGamesListStateGlobal += GameSystemSelectionView_xeventHandler;

                Window.Current.SizeChanged += (sender, args) =>
                {
                    PlatformService.checkInitWidth(false);
                    /*ApplicationView currentView = ApplicationView.GetForCurrentView();

                    if (currentView.Orientation == ApplicationViewOrientation.Landscape)
                    {
                        System.Diagnostics.Debug.WriteLine("currentView's size changed - width: " + Window.Current.CoreWindow.Bounds.Width + " height: " + Window.Current.CoreWindow.Bounds.Height);
                    }
                    else if (currentView.Orientation == ApplicationViewOrientation.Portrait)
                    {
                        System.Diagnostics.Debug.WriteLine("currentView's size changed - width: " + Window.Current.CoreWindow.Bounds.Width + " height: " + Window.Current.CoreWindow.Bounds.Height);
                    }*/
                    Bindings.Update();
                };
                PlatformService.checkInitWidth();
                UpdateBindingDelay();
            }
            catch (Exception e)
            {
                ProgressContainer.Visibility = Visibility.Collapsed;
                PlatformService.ShowErrorMessageDirect(e);
            }
            if (PlatformService.pageReady)
            {
                restoreListPosition();
            }


            try
            {
                var currentView = SystemNavigationManager.GetForCurrentView();
                currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            catch (Exception e)
            {
            }

            PlatformService.pageReady = true;
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
        private async void restoreListPosition()
        {
            /*try
            {
                if (PlatformService.isGamesList)
                {
                    AllGetter_Click(null, null);
                }
                else
                {
                    RecentsGetter_Click(null, null);
                }
            }catch(Exception ex)
            {

            }*/
            try
            {
                while (ProgressContainer.Visibility == Visibility.Visible || ProgressContainerX.Visibility == Visibility.Visible)
                {
                    await Task.Delay(1000);
                }
                try
                {
                    RecentsListPage.UpdateLayout();
                    SystemRecentsGrid.UpdateLayout();
                    SystemRecentsList.UpdateLayout();
                    RecentsListPage.UpdateLayout();
                }
                catch (Exception ex)
                {

                }

                try
                {
                    RecentsListPage.ChangeView(0, PlatformService.vScroll, 1);
                    //RecentsListPage.ScrollToVerticalOffset(currentIndex);
                }
                catch (Exception ex)
                {
                    PlatformService.ShowErrorMessageDirect(ex);
                }

            }
            catch (Exception ex)
            {

            }
        }
        private void GameSystemSelectionView_eventHandler(object sender, EventArgs e)
        {
            try
            {
                RecentsListPage.UpdateLayout();
                SystemRecentsGrid.UpdateLayout();
                SystemRecentsList.UpdateLayout();
                RecentsListPage.UpdateLayout();
            }
            catch (Exception ex)
            {

            }
            try
            {
                PlatformService.vScroll = RecentsListPage.VerticalOffset;
                PlatformService.gameSystemViewModel = (GameSystemRecentModel)sender;
            }
            catch (Exception ex)
            {
                //PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        double currentIndex = -1;
        private async void GameSystemSelectionView_xeventHandler(object sender, EventArgs e)
        {
            return;
            try
            {
                currentIndex = (double)sender;
            }
            catch (Exception ex)
            {

            }
            try
            {
                while (ProgressContainer.Visibility == Visibility.Visible || ProgressContainerX.Visibility == Visibility.Visible)
                {
                    await Task.Delay(1000);
                }
                try
                {
                    RecentsListPage.UpdateLayout();
                    SystemRecentsGrid.UpdateLayout();
                    SystemRecentsList.UpdateLayout();
                    RecentsListPage.UpdateLayout();
                }
                catch (Exception ex)
                {

                }

                try
                {
                    RecentsListPage.ChangeView(0, currentIndex, 1);
                    //RecentsListPage.ScrollToVerticalOffset(currentIndex);
                }
                catch (Exception ex)
                {
                    PlatformService.ShowErrorMessageDirect(ex);
                }
                /*if (PlatformService.gameSystemViewModel != null)
                {
                    foreach(var lItem in SystemRecentsGrid.Items)
                    {
                        var currentItem = (GameSystemRecentModel)lItem;
                        if (currentItem.GameLocation.Equals(PlatformService.gameSystemViewModel.GameLocation))
                        {
                            SystemRecentsGrid.ScrollIntoView(lItem, ScrollIntoViewAlignment.Leading);
                            break;
                        }
                    }
                    
                    //SystemRecentsList.ScrollIntoView(PlatformService.gameSystemViewModel, ScrollIntoViewAlignment.Leading);
                    //SystemRecentsList.UpdateLayout();
                }*/
            }
            catch (Exception ex)
            {
                //PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        public async void UpdateBindingDelay(bool delayRequest = true)
        {
            try
            {
                if (delayRequest)
                {
                    await Task.Delay(2000);
                }
                Bindings.Update();

            }
            catch (Exception e)
            {

            }
        }
        public static bool CoresOptionsVisible = false;
        private Dictionary<string, string> SystemsMap = new Dictionary<string, string>();
        private string GetSystemNameByPreviewName(string previewName)
        {
            foreach (var SystemsMapItem in SystemsMap.Keys)
            {
                if (previewName.Equals(SystemsMap[SystemsMapItem]))
                {
                    return SystemsMapItem;
                }
            }
            return previewName;
        }
        private void InitialCoresOptionsMenu()
        {
            try
            {
                ProgressContainer.Visibility = Visibility.Collapsed;
            }
            catch (Exception e)
            {

            }
            try
            {
                SystemCoresList.Items.Clear();
                SystemsMap.Clear();
                foreach (var CoreItem in VM.CoresOptionsDictionary.Keys)
                {
                    CoresOptions testObject = null;
                    if (VM.CoresOptionsDictionary.TryGetValue(CoreItem, out testObject))
                    {
                        SystemsMap.Add(CoreItem, testObject.OriginalSystemName);
                        SystemCoresList.Items.Add(testObject.OriginalSystemName);
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessageDirect(e);
            }
        }


        private void AppBarButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                CoresOptionsVisible = !CoresOptionsVisible;
                if (!CoresOptionsVisible)
                {
                    ResetOptionsSelection();
                }
                CoresOptionsPanel.Visibility = CoresOptionsVisible ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
                HideSystemGames();
                PlatformService.SubPageActive = CoresOptionsVisible;
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                PlatformService.checkInitWidth(false);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void OptionsSave_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                /** SAVE **/
                PlatformService.ShowMessageDirect("Options saved successfuly", "success.wav");
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void OptionsCancel_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            HideCoreOptions();
        }
        public void HideCoreOptions()
        {
            try
            {
                CoresOptionsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                CoresOptionsVisible = false;
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                ResetOptionsSelection();
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private void ResetOptionsSelection()
        {
            senderTemp = null;
            CoreOptionsSubPage.Children.Clear();
            SystemCoresList.SelectedIndex = -1;
            NoOptionsTextGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            PlatformService.SubPageActive = false;
        }

        object senderTemp = null;
        private void SystemCoresList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        senderTemp = sender;
                        if (((ComboBox)sender).SelectedIndex == -1)
                        {
                            return;
                        }
                        VM.SystemCoreIsLoadingState(true);
                        await Task.Delay(500);
                        await ShowSelectedSystemOptions(sender, e);
                        VM.SystemCoreIsLoadingState(false);
                    }
                    catch (Exception ec)
                    {
                        PlatformService.ShowErrorMessageDirect(ec);
                        VM.SystemCoreIsLoadingState(false);
                    }
                    PlatformService.checkInitWidth(false);
                });
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private async Task ShowSelectedSystemOptions(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                CoreOptionsSubPage.Children.Clear();
                var TargetSystem = ((ComboBox)sender).SelectedItem.ToString();
                var CurrentOptions = VM.CoresOptionsDictionary[GetSystemNameByPreviewName(TargetSystem)].OptionsList.Keys;
                PlatformService.PlayNotificationSoundDirect("select.mp3");
                if (CurrentOptions.Count > 0)
                {
                    NoOptionsTextGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    foreach (var CoreItem in CurrentOptions)
                    {
                        TextBlock SystemOptionText = new TextBlock();
                        ComboBox comboBox = new ComboBox();
                        var OptionValues = VM.CoresOptionsDictionary[GetSystemNameByPreviewName(TargetSystem)].OptionsList[CoreItem];
                        SystemOptionText.Text = OptionValues.OptionsDescription.Replace("(Needs Restart)", "").Replace("(restart)", "").Replace("(Restart)", "").Replace("(Need Restart)", "").Replace("Need", "").Replace("Restart", "");
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
                        comboBox.Tag = GetSystemNameByPreviewName(TargetSystem);
                        comboBox.Name = OptionValues.OptionsKey;
                        Grid.SetColumnSpan(comboBox, 2);
                        Grid.SetColumnSpan(SystemOptionText, 2);

                        //Add Event
                        comboBox.SelectionChanged += SelectionChangedEvent;
                        CoreOptionsSubPage.Children.Add(SystemOptionText);
                        CoreOptionsSubPage.Children.Add(comboBox);
                    }
                }
                else
                {
                    NoOptionsTextGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    PlatformService.PlayNotificationSoundDirect("notice.mp3");
                }
                PlatformService.checkInitWidth(false);
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
                VM.setCoresOptionsDictionary(TargetSystem, TargetKey, (uint)TargetIndex);
                PlatformService.PlayNotificationSoundDirect("option-changed.wav");
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        int TempRecentsListComboIndex = 0;
        private void SystemRecentsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (VM != null)
                {
                    var TargetSystem = ((ComboBox)sender).SelectedIndex;
                    TempRecentsListComboIndex = TargetSystem;
                    switch (TargetSystem)
                    {
                        case 0:
                            VM.GetRecentGames();
                            break;
                        case 1:
                            break;
                        case 2:
                            VM.GetAllGames();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void GamesPick_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("select.mp3");
            VM.BrowseSingleGame();
        }
        private void GamesCancel_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            HideSystemGames();
        }
        public void HideSystemGames()
        {
            try
            {
                RecentsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                VM.GamesListVisible = false;
                VM.ClearSelectedSystem();
                //RecentsListCombo.SelectedIndex = 0;
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                PlatformService.SubPageActive = false;
                PlatformService.isGamesList = false;
                PlatformService.vScroll = 0;
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }

        }


        public event EventHandler eventHandler;
        private void RecentsGetter_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PlatformService.vScroll = 0;
            PlatformService.PlayNotificationSoundDirect("button-01.mp3");

            VM.GetRecentGames(eventHandler);
            GamesListFilter.PlaceholderText = "Search in recent..";
            PlatformService.checkInitWidth(false);
            PlatformService.isGamesList = false;
        }

        private void AllGetter_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PlatformService.vScroll = 0;
            PlatformService.PlayNotificationSoundDirect("button-01.mp3");

            VM.GetAllGames(eventHandler);
            GamesListFilter.PlaceholderText = "Search in games..";
            PlatformService.checkInitWidth(false);
            PlatformService.isGamesList = true;
        }

        private void AllGetterReload_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PlatformService.vScroll = 0;
            PlatformService.PlayNotificationSoundDirect("button-01.mp3");

            VM.GetAllGames(eventHandler, true);
            GamesListFilter.PlaceholderText = "Search in games..";
            PlatformService.checkInitWidth(false);
            PlatformService.isGamesList = true;
        }

        private void RecentsList_DataContextChanged(Windows.UI.Xaml.FrameworkElement sender, Windows.UI.Xaml.DataContextChangedEventArgs args)
        {
            try
            {
                if (eventHandler != null)
                {
                    eventHandler.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private bool pushNormalNotification(string message, char icon = SegoeMDL2Assets.GameConsole, int time = 3, EventHandler eventHandler = null)
        {
            try
            {
                pushLocalNotification(message, Colors.DodgerBlue, Colors.White, icon, time, DefaultPosition, eventHandler);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        private async void OptionsReset_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                if (senderTemp != null)
                {
                    var TargetSystem = ((ComboBox)senderTemp)?.SelectedItem?.ToString();
                    if (TargetSystem != null && TargetSystem?.Length > 0)
                    {
                        PlatformService.PlayNotificationSoundDirect("alert.wav");
                        ConfirmConfig confirmReset = new ConfirmConfig();
                        confirmReset.SetTitle("Reset Options");
                        confirmReset.SetMessage($"Do you want to reset {TargetSystem}'s options to the default values? ");
                        confirmReset.UseYesNo();

                        var StartReset = await UserDialogs.Instance.ConfirmAsync(confirmReset);

                        if (StartReset)
                        {
                            VM.resetCoresOptionsDictionary(GetSystemNameByPreviewName(TargetSystem));
                            PlatformService.PlayNotificationSoundDirect("success.wav");
                            if (!pushNormalNotification($"{TargetSystem}'s options has been reset"))
                            {
                                await UserDialogs.Instance.AlertAsync($"{TargetSystem}'s options has been reset", "Reset Done");
                            }
                            if (senderTemp != null)
                            {
                                SystemCoresList_SelectionChanged(senderTemp, null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private async void OptionsSave_Click_1(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                if (senderTemp != null)
                {
                    var TargetSystem = ((ComboBox)senderTemp)?.SelectedItem?.ToString();
                    if (TargetSystem != null && TargetSystem?.Length > 0)
                    {
                        PlatformService.PlayNotificationSoundDirect("alert.wav");
                        ConfirmConfig confirmSave = new ConfirmConfig();
                        confirmSave.SetTitle("Save Options");
                        confirmSave.SetMessage($"Do you want to save {TargetSystem}'s options as default values? ");
                        confirmSave.UseYesNo();

                        var StartSave = await UserDialogs.Instance.ConfirmAsync(confirmSave);

                        if (StartSave)
                        {
                            await VM.CoreOptionsStoreAsync(GetSystemNameByPreviewName(TargetSystem));
                            PlatformService.PlayNotificationSoundDirect("success.wav");
                            if (!pushNormalNotification($"{TargetSystem}'s options has been saved"))
                            {
                                await UserDialogs.Instance.AlertAsync($"{TargetSystem}'s options has been saved", "Save Done");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void ConsoleInfo_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("button-01.mp3");
            VM.GetConsoleInfo(eventHandler);
        }
        void HideSubPages(object sender, EventArgs e)
        {
            HideCoreOptions();
            HideSystemGames();
        }


        private async void GamesListFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var CurrentText = ((TextBox)sender).Text.Trim();
                await VM.FilterCurrentGamesList(CurrentText);
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        StorageFile IconFile = null;
        private async void ConsoleSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IconFile = null;
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");

                int left = 0;
                int top = 3;
                int right = 0;
                int bottom = 5;


                TextBox NameTextBox = new TextBox();
                NameTextBox.AcceptsReturn = false;
                NameTextBox.Height = 32;
                NameTextBox.Text = VM.SelectedSystem.Name;
                NameTextBox.VerticalAlignment = VerticalAlignment.Center;
                NameTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                NameTextBox.PlaceholderText = "Core Name";
                NameTextBox.Margin = new Thickness(left, top, right, bottom);

                TextBox ManufacturerTextBox = new TextBox();
                ManufacturerTextBox.AcceptsReturn = false;
                ManufacturerTextBox.Height = 32;
                ManufacturerTextBox.Text = VM.SelectedSystem.ManufacturerTemp;
                ManufacturerTextBox.VerticalAlignment = VerticalAlignment.Center;
                ManufacturerTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                ManufacturerTextBox.PlaceholderText = "Manufacturer";
                ManufacturerTextBox.Margin = new Thickness(left, top, right, bottom);

                CheckBox checkBoxPinned = new CheckBox();
                checkBoxPinned.IsChecked = VM.SelectedSystem.Pinned;
                checkBoxPinned.Content = "Pinned";
                checkBoxPinned.VerticalAlignment = VerticalAlignment.Center;
                checkBoxPinned.HorizontalAlignment = HorizontalAlignment.Left;
                checkBoxPinned.Margin = new Thickness(left, top, right, bottom);

                CheckBox checkBoxCD = new CheckBox();
                checkBoxCD.IsChecked = VM.SelectedSystem.Pinned;
                checkBoxCD.Content = "CD Support";
                checkBoxCD.VerticalAlignment = VerticalAlignment.Center;
                checkBoxCD.HorizontalAlignment = HorizontalAlignment.Left;
                checkBoxCD.Margin = new Thickness(left, top, right, bottom);

                Image SystemImage = new Image();
                SystemImage.Height = 75;
                SystemImage.Width = 75;
                BitmapImage ImageSource = new BitmapImage(new Uri(VM.SelectedSystem.Symbol));
                SystemImage.Source = ImageSource;
                SystemImage.VerticalAlignment = VerticalAlignment.Center;
                SystemImage.Stretch = Windows.UI.Xaml.Media.Stretch.UniformToFill;
                SystemImage.Margin = new Thickness(left, top, right, bottom);
                SystemImage.Tapped += SelectImage;

                StackPanel dialogContent = new StackPanel();
                dialogContent.Children.Add(NameTextBox);
                dialogContent.Children.Add(ManufacturerTextBox);
                dialogContent.Children.Add(checkBoxPinned);
                dialogContent.Children.Add(checkBoxCD);
                dialogContent.Children.Add(SystemImage);

                ContentDialog dialog = new ContentDialog();
                dialog.Content = dialogContent;
                dialog.Title = $"{VM.SelectedSystem.Name} Settings";
                dialog.IsSecondaryButtonEnabled = true;
                dialog.PrimaryButtonText = "Save";
                dialog.SecondaryButtonText = "Cancel";
                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    updateAnyCore(NameTextBox.Text, ManufacturerTextBox.Text, await SystemIconImport(IconFile), checkBoxPinned.IsChecked.Value);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }


        }
        private async void SelectImage(object sender, object e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                var extensions = new string[] { ".png", ".jpg" };
                FileOpenPicker picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");


                IconFile = await picker.PickSingleFileAsync();
                if (IconFile != null)
                {
                    using (IRandomAccessStream fileStream = await IconFile.OpenAsync(FileAccessMode.Read))
                    {
                        // Set the image source to the selected bitmap
                        BitmapImage bitmapImage = new BitmapImage();
                        // Decode pixel sizes are optional
                        // It's generally a good optimisation to decode to match the size you'll display
                        bitmapImage.DecodePixelHeight = 75;
                        bitmapImage.DecodePixelWidth = 75;

                        await bitmapImage.SetSourceAsync(fileStream);
                        ((Image)sender).Source = bitmapImage;
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private async void updateAnyCore(string AnyCoreName, string AnyCoreManufactur, string AnyCoreSaveIconValue, bool Pinned)
        {
            try
            {
                var CurrentSystem = VM.SelectedSystem;
                if (CurrentSystem != null)
                {
                    PlatformService.PlayNotificationSoundDirect("notice.mp3");
                    ConfirmConfig confirCoreSettings = new ConfirmConfig();
                    confirCoreSettings.SetTitle("Save settings?");
                    confirCoreSettings.SetMessage("Do you want to save the current settings?");
                    confirCoreSettings.UseYesNo();
                    bool confirCoreSettingsState = await UserDialogs.Instance.ConfirmAsync(confirCoreSettings);
                    if (confirCoreSettingsState)
                    {
                        await VM.SetAnyCoreInfo(CurrentSystem.TempName, AnyCoreName, AnyCoreManufactur, AnyCoreSaveIconValue, Pinned);
                        PlatformService.PlayNotificationSoundDirect("success.wav");
                        ConfirmConfig confirmRestart = new ConfirmConfig();
                        confirmRestart.SetTitle("Update Done");
                        confirmRestart.SetMessage("The core has been updated, Restart Retrix is required");
                        confirmRestart.UseYesNo();
                        confirmRestart.SetOkText(GameSystemsProviderService.isX64() ? "Restart Retrix" : "Exit Retrix");
                        confirmRestart.SetCancelText("Later");
                        var RestartState = await UserDialogs.Instance.ConfirmAsync(confirmRestart);
                        if (RestartState)
                        {
                            CoreApplication.Exit();
                            if (!GameSystemsProviderService.isX64())
                            {
                                //CoreApplication.Exit();
                            }
                            else
                            {
                                /*AppRestartFailureReason result =
                                 await CoreApplication.RequestRestartAsync("-fastInit -level 1 -foo");

                                if (result == AppRestartFailureReason.NotInForeground
                                    || result == AppRestartFailureReason.Other)
                                {
                                    var msgBox = new MessageDialog("Restart Failed", "Please manually restart.");
                                    await msgBox.ShowAsync();
                                }*/
                            }

                        }
                        else
                        {
                            //await PlatformService.RetriveRecentsDirect();
                        }
                    }

                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessageDirect(e);
            }
        }

        private async Task<string> SystemIconImport(StorageFile file)
        {
            try
            {
                if (file != null)
                {
                    IDirectoryInfo zipsDirectory = null;
                    var localFolder = CrossFileSystem.Current.LocalStorage;
                    zipsDirectory = await localFolder.GetDirectoryAsync("AnyCore");
                    if (zipsDirectory == null)
                    {
                        await localFolder.CreateDirectoryAsync("AnyCore");
                        zipsDirectory = await localFolder.GetDirectoryAsync("AnyCore");
                    }
                    string targetFileName = file.Name;
                    string zipFileName = zipsDirectory.FullName + "\\" + targetFileName;
                    var targetFielTest = await zipsDirectory.GetFileAsync(targetFileName);
                    if (targetFielTest != null)
                    {
                        await targetFielTest.DeleteAsync();
                    }
                    var targetFile = await zipsDirectory.CreateFileAsync(targetFileName);
                    using (Stream inStream = await IconFile.OpenStreamForReadAsync())
                    {
                        using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                        {
                            inStream.CopyTo(outStream);
                            await outStream.FlushAsync();
                        }
                    }
                    targetFile = await zipsDirectory.GetFileAsync(targetFileName);
                    if (targetFile != null)
                    {
                        return targetFile.FullName;
                    }
                }
            }
            catch (Exception ex)
            {

                PlatformService.ShowErrorMessageDirect(ex);

            }
            return "";
        }

        private async void SystemBiosImport(object sender, object eh)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                PlatformService.PlayNotificationSoundDirect("notice.mp3");
                ConfirmConfig confirCoreSettings = new ConfirmConfig();
                confirCoreSettings.SetTitle("Advanced Import?");
                confirCoreSettings.SetMessage("1-You can update the core\n2-You can import BIOS map\n\nDo you want to start?");
                confirCoreSettings.UseYesNo();
                bool confirCoreSettingsState = await UserDialogs.Instance.ConfirmAsync(confirCoreSettings);
                if (!confirCoreSettingsState)
                {
                    return;
                }

                FileOpenPicker picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".rab");
                picker.FileTypeFilter.Add(".dll");

                StorageFile file = await picker.PickSingleFileAsync();

                if (file != null)
                {
                    IDirectoryInfo zipsDirectory = null;
                    var localFolder = CrossFileSystem.Current.LocalStorage;
                    zipsDirectory = await localFolder.GetDirectoryAsync("AnyCore");
                    if (zipsDirectory == null)
                    {
                        await localFolder.CreateDirectoryAsync("AnyCore");
                        zipsDirectory = await localFolder.GetDirectoryAsync("AnyCore");
                    }
                    var isDLL = System.IO.Path.GetExtension(file.Name.ToLower()).Equals(".dll");
                    string targetFileName = VM.SelectedSystem.DLLName + (isDLL ? ".dll" : ".rab");
                    string zipFileName = zipsDirectory.FullName + "\\" + targetFileName;
                    if (isDLL)
                    {
                        VM.SelectedSystem.Core.FreeLibretroCore();
                    }
                    var targetFielTest = await zipsDirectory.GetFileAsync(targetFileName);
                    if (targetFielTest != null)
                    {
                        await targetFielTest.DeleteAsync();
                    }
                    var targetFile = await zipsDirectory.CreateFileAsync(targetFileName);
                    using (Stream inStream = await file.OpenStreamForReadAsync())
                    {
                        using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                        {
                            inStream.CopyTo(outStream);
                            await outStream.FlushAsync();
                        }
                    }
                    targetFile = await zipsDirectory.GetFileAsync(targetFileName);
                    if (targetFile != null)
                    {
                        PlatformService.PlayNotificationSoundDirect("success.wav");
                        ConfirmConfig confirmRestart = new ConfirmConfig();
                        confirmRestart.SetTitle("Import Done");
                        confirmRestart.SetMessage($"{(isDLL ? "Core" : "BIOS map")} has been updated, Restart Retrix is required");
                        confirmRestart.UseYesNo();
                        confirmRestart.SetOkText(GameSystemsProviderService.isX64() ? "Restart Retrix" : "Exit Retrix");
                        confirmRestart.SetCancelText("Later");
                        var RestartState = await UserDialogs.Instance.ConfirmAsync(confirmRestart);
                        if (RestartState)
                        {
                            CoreApplication.Exit();

                            if (!GameSystemsProviderService.isX64())
                            {
                                //CoreApplication.Exit();
                            }
                            else
                            {
                                /*AppRestartFailureReason result =
                                 await CoreApplication.RequestRestartAsync("-fastInit -level 1 -foo");

                                if (result == AppRestartFailureReason.NotInForeground
                                    || result == AppRestartFailureReason.Other)
                                {
                                    var msgBox = new MessageDialog("Restart Failed", "Please manually restart.");
                                    await msgBox.ShowAsync();
                                }*/
                            }

                        }
                        else
                        {
                            //await PlatformService.RetriveRecentsDirect();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                PlatformService.ShowErrorMessageDirect(ex);

            }
        }

        private async void AnyCoreDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                PlatformService.PlayNotificationSoundDirect("alert.wav");
                ConfirmConfig confirmDelete = new ConfirmConfig();
                confirmDelete.SetTitle("Delete Core");
                confirmDelete.SetMessage($"Do you want to delete {VM.SelectedSystem.Name}?");
                confirmDelete.UseYesNo();

                var confirmDeleteState = await UserDialogs.Instance.ConfirmAsync(confirmDelete);
                if (confirmDeleteState)
                {
                    VM.SystemCoreIsLoadingState(true);
                    IDirectoryInfo zipsDirectory = null;
                    zipsDirectory = await CrossFileSystem.Current.LocalStorage.GetDirectoryAsync("AnyCore");

                    string targetFileName = System.IO.Path.GetFileName((VM.SelectedSystem.Core.DLLName + ".dll"));
                    var targetFielTest = await zipsDirectory.GetFileAsync(targetFileName);
                    if (targetFielTest != null)
                    {
                        VM.SelectedSystem.Core.FreeLibretroCore();
                        await targetFielTest.DeleteAsync();
                        await VM.DeleteAnyCore(VM.SelectedSystem.TempName);

                        string BIOSFileName = System.IO.Path.GetFileName((VM.SelectedSystem.Core.DLLName + ".rab"));
                        var targetBIOSTest = await zipsDirectory.GetFileAsync(BIOSFileName);
                        if (targetBIOSTest != null)
                        {
                            await targetBIOSTest.DeleteAsync();
                        }
                        PlatformService.PlayNotificationSoundDirect("success.wav");
                        if (!pushNormalNotification($"{VM.SelectedSystem.Name} Deleted, Restart Retrix is recommended"))
                        {
                            await UserDialogs.Instance.AlertAsync($"{VM.SelectedSystem.Name} Deleted, Restart Retrix is recommended");
                        }
                        VM.setGamesListState(false);
                        VM.SelectedSystem.Core.FailedToLoad = true;
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("faild.wav");
                        if (!pushNormalNotification($"{VM.SelectedSystem.Name} not found!, unable to delete {VM.SelectedSystem.Name}"))
                        {
                            await UserDialogs.Instance.AlertAsync($"{VM.SelectedSystem.Name} not found!, unable to delete {VM.SelectedSystem.Name}");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
            VM.SystemCoreIsLoadingState(false);
        }

        private async void AnyCoreBiosSample_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                PlatformService.PlayNotificationSoundDirect("alert.wav");
                ConfirmConfig confirmDelete = new ConfirmConfig();
                confirmDelete.SetTitle("Save Sample");
                confirmDelete.SetMessage($"This option will help you to save\nBIOS Map sample file\nAfter saving the file open it using any text editor\n\nDo you want to start?");
                confirmDelete.UseYesNo();

                var confirmDeleteState = await UserDialogs.Instance.ConfirmAsync(confirmDelete);
                if (confirmDeleteState)
                {

                    await VM.DownloadBIOSMapSample();

                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void CloseLogList_Click(object sender, RoutedEventArgs e)
        {
            ShowErrorsList = false;
            Bindings.Update();
        }

        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            ShowErrorsList = !ShowErrorsList;
            Bindings.Update();
        }

        private async void ResetLogList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                PlatformService.PlayNotificationSoundDirect("alert.wav");
                ConfirmConfig confirmDelete = new ConfirmConfig();
                confirmDelete.SetTitle("Reset Detector");
                confirmDelete.SetMessage($"Do you want to reset all skipped cores?");
                confirmDelete.UseYesNo();

                var confirmDeleteState = await UserDialogs.Instance.ConfirmAsync(confirmDelete);
                if (confirmDeleteState)
                {

                    foreach (var SkippedCoreItem in GameSystemsProviderService.SkippedList)
                    {
                        var skippedCoreName = SkippedCoreItem.Replace(" Skipped due compatibility issues", "");
                        Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(skippedCoreName, false);
                    }
                    PlatformService.PlayNotificationSoundDirect("success.wav");
                    if (!pushNormalNotification("Cores has been reseted, restart to reload the cores again"))
                    {
                        await UserDialogs.Instance.AlertAsync("Cores has been reseted, restart to reload the cores again");
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }



        private async void PureCoreUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                PlatformService.PlayNotificationSoundDirect("notice.mp3");
                ConfirmConfig confirCoreSettings = new ConfirmConfig();
                confirCoreSettings.SetTitle("Update Core?");
                confirCoreSettings.SetMessage("This option used to update the core to newer version\n\nDo you want to start?");
                confirCoreSettings.UseYesNo();
                bool confirCoreSettingsState = await UserDialogs.Instance.ConfirmAsync(confirCoreSettings);
                if (!confirCoreSettingsState)
                {
                    return;
                }

                FileOpenPicker picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                //picker.FileTypeFilter.Add(".rxe");
                picker.FileTypeFilter.Add(".dll");

                StorageFile file = await picker.PickSingleFileAsync();

                if (file != null)
                {
                    IDirectoryInfo zipsDirectory = null;
                    var localFolder = CrossFileSystem.Current.LocalStorage;
                    zipsDirectory = await localFolder.GetDirectoryAsync("PureCore");
                    if (zipsDirectory == null)
                    {
                        await localFolder.CreateDirectoryAsync("PureCore");
                        zipsDirectory = await localFolder.GetDirectoryAsync("PureCore");
                    }
                    var isDLL = System.IO.Path.GetExtension(file.Name.ToLower()).Equals(".dll");
                    string targetFileName = VM.SelectedSystem.DLLName;
                    string zipFileName = zipsDirectory.FullName + "\\" + targetFileName;
                    if (isDLL)
                    {
                        VM.SelectedSystem.Core.FreeLibretroCore();
                    }
                    var targetFielTest = await zipsDirectory.GetFileAsync(targetFileName);
                    if (targetFielTest != null)
                    {
                        await targetFielTest.DeleteAsync();
                    }
                    var targetFile = await zipsDirectory.CreateFileAsync(targetFileName);
                    using (Stream inStream = await file.OpenStreamForReadAsync())
                    {
                        using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                        {
                            inStream.CopyTo(outStream);
                            await outStream.FlushAsync();
                        }
                    }
                    targetFile = await zipsDirectory.GetFileAsync(targetFileName);
                    if (targetFile != null)
                    {
                        PlatformService.PlayNotificationSoundDirect("success.wav");
                        ConfirmConfig confirmRestart = new ConfirmConfig();
                        confirmRestart.SetTitle("Update Done");
                        confirmRestart.SetMessage($"{(isDLL ? "Core" : "Extras")} has been updated, Restart Retrix is required");
                        confirmRestart.UseYesNo();
                        confirmRestart.SetOkText(GameSystemsProviderService.isX64() ? "Restart Retrix" : "Exit Retrix");
                        confirmRestart.SetCancelText("Later");
                        var RestartState = await UserDialogs.Instance.ConfirmAsync(confirmRestart);
                        if (RestartState)
                        {
                            CoreApplication.Exit();
                            if (!GameSystemsProviderService.isX64())
                            {
                                //CoreApplication.Exit();
                            }
                            else
                            {
                                /*AppRestartFailureReason result =
                                 await CoreApplication.RequestRestartAsync("-fastInit -level 1 -foo");

                                if (result == AppRestartFailureReason.NotInForeground
                                    || result == AppRestartFailureReason.Other)
                                {
                                    var msgBox = new MessageDialog("Restart Failed", "Please manually restart.");
                                    await msgBox.ShowAsync();
                                }*/
                            }

                        }
                        else
                        {
                            //await PlatformService.RetriveRecentsDirect();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                PlatformService.ShowErrorMessageDirect(ex);

            }
        }

        private async void RemoveCoreUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                PlatformService.PlayNotificationSoundDirect("notice.mp3");
                ConfirmConfig confirCoreSettings = new ConfirmConfig();
                confirCoreSettings.SetTitle("Remove Updates?");
                confirCoreSettings.SetMessage("This option used to remove all the updates\n\nDo you want to start?");
                confirCoreSettings.UseYesNo();
                bool confirCoreSettingsState = await UserDialogs.Instance.ConfirmAsync(confirCoreSettings);
                if (!confirCoreSettingsState)
                {
                    return;
                }

                IDirectoryInfo zipsDirectory = null;
                var localFolder = CrossFileSystem.Current.LocalStorage;
                zipsDirectory = await localFolder.GetDirectoryAsync("PureCore");
                if (zipsDirectory == null)
                {
                    PlatformService.PlayNotificationSoundDirect("faild.wav");
                    if (!pushNormalNotification($"{VM.SelectedSystem.Name}'s updates not found!, unable to delete"))
                    {
                        await UserDialogs.Instance.AlertAsync($"{VM.SelectedSystem.Name}'s updates not found!, unable to delete");
                    }
                    return;
                }
                var isDLL = true;
                string targetFileName = VM.SelectedSystem.DLLName;
                string zipFileName = zipsDirectory.FullName + "\\" + targetFileName;
                if (isDLL)
                {
                    VM.SelectedSystem.Core.FreeLibretroCore();
                }
                var targetFielTest = await zipsDirectory.GetFileAsync(targetFileName);
                if (targetFielTest != null)
                {
                    await targetFielTest.DeleteAsync();

                    PlatformService.PlayNotificationSoundDirect("success.wav");
                    ConfirmConfig confirmRestart = new ConfirmConfig();
                    confirmRestart.SetTitle("Updates Deleted");
                    confirmRestart.SetMessage($"{(isDLL ? "Core" : "Scripts")} updates has been deleted, Restart Retrix is required");
                    confirmRestart.UseYesNo();
                    confirmRestart.SetOkText(GameSystemsProviderService.isX64() ? "Restart Retrix" : "Exit Retrix");
                    confirmRestart.SetCancelText("Later");
                    var RestartState = await UserDialogs.Instance.ConfirmAsync(confirmRestart);
                    if (RestartState)
                    {
                        CoreApplication.Exit();
                        if (!GameSystemsProviderService.isX64())
                        {
                            //CoreApplication.Exit();
                        }
                        else
                        {
                            /*AppRestartFailureReason result =
                             await CoreApplication.RequestRestartAsync("-fastInit -level 1 -foo");

                            // Restart request denied, send a toast to tell the user to restart manually.
                            if (result == AppRestartFailureReason.NotInForeground
                                || result == AppRestartFailureReason.Other)
                            {
                                var msgBox = new MessageDialog("Restart Failed", "Please manually restart.");
                                await msgBox.ShowAsync();
                            }*/
                        }

                    }
                    else
                    {
                        //await PlatformService.RetriveRecentsDirect();
                    }

                }
                else
                {
                    PlatformService.PlayNotificationSoundDirect("faild.wav");
                    if (!pushNormalNotification($"{VM.SelectedSystem.Name}'s updates not found!, unable to delete {VM.SelectedSystem.Name}"))
                    {
                        await UserDialogs.Instance.AlertAsync($"{VM.SelectedSystem.Name}'s updates not found!, unable to delete {VM.SelectedSystem.Name}");
                    }
                }


            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void SetGamesFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (VM != null && VM.SelectedSystem != null)
                {
                    VM.ReSelectIsActive = true;
                    VM.GameSystemHoldingHandler(VM.SelectedSystem);
                }
            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private void AppBarToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (UseListView.IsChecked.Value)
            {
                SystemRecentsGrid.Visibility = Visibility.Collapsed;
                SystemRecentsList.Visibility = Visibility.Visible;
                GameSystemsGrid.Visibility = Visibility.Collapsed;
                GameSystemsList.Visibility = Visibility.Visible;
            }
            else
            {
                SystemRecentsGrid.Visibility = Visibility.Visible;
                SystemRecentsList.Visibility = Visibility.Collapsed;
                GameSystemsGrid.Visibility = Visibility.Visible;
                GameSystemsList.Visibility = Visibility.Collapsed;
            }
        }

        private void MusteSFX_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlatformService.MuteSFX = ((AppBarToggleButton)sender).IsChecked.Value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("MuteSFX", PlatformService.MuteSFX);
            }
            catch (Exception ex)
            {

            }
        }
    }
}


