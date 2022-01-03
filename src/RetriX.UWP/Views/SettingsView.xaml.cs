using Acr.UserDialogs;
using MvvmCross.Uwp.Views;
using Plugin.FileSystem;
using Plugin.FileSystem.Abstractions;
using Plugin.Settings;
using RetriX.Shared.ViewModels;
using RetriX.UWP.Services;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RetriX.UWP.Pages
{
    public sealed partial class SettingsView : MvxWindowsPage
    {
        public SettingsViewModel VM => ViewModel as SettingsViewModel;

        int InitWidthSize { get => PlatformService.InitWidthSize; }
        int InitWidthSizeCustom { get => PlatformService.InitWidthSize - 20; }
        HorizontalAlignment horizontalAlignment { get => PlatformService.horizontalAlignment; }
        bool AnyCoreLoaded = false;

        public SettingsView()
        {
            PlatformService.SaveGamesListStateDirect();
            this.InitializeComponent();

            try
            {
                string AppLocation = CrossFileSystem.Current.LocalStorage.FullName;
                string CoreLocation = AppLocation + @"\AnyCore";
                if (Directory.Exists(CoreLocation))
                {
                    AnyCoreLoaded = true;
                }
            }
            catch (Exception ed)
            {

            }
            

            try
            {
                Window.Current.SizeChanged += (sender, args) =>
                {
                    Bindings.Update();
                };
                PlatformService.checkInitWidth(false);
            }
            catch (Exception es)
            {

            }

            try
            {
                if (PlatformService.OpenBackupFile != null)
                {
                    _ = ImportSettingsSlotsAction(PlatformService.OpenBackupFile);
                }
            }
            catch (Exception ee)
            {
                PlatformService.ShowErrorMessageDirect(ee);
            }
        }


        private async void BackupButton_ClickAsync(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await ExportSettings();
        }

        private async void RestoreButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await ImportSettingsSlotsAction();
        }


        private void SystemCoresList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var TargetSystem = ((ComboBox)sender).SelectedItem.ToString();
                if (TargetSystem.Contains("All Consoles"))
                {
                    VM.ReloadFileDependency();
                }
                else
                {
                    VM.GetFileDependencyByCoreName(TargetSystem);
                }

            }
            catch (Exception ex)
            {
                PlatformService.ShowErrorMessageDirect(ex);
            }
        }

        private async Task ExportSettings()
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");

                PlatformService.PlayNotificationSoundDirect("alert.wav");
                ConfirmConfig confirmBackup = new ConfirmConfig();
                confirmBackup.SetTitle("Start Backup");
                confirmBackup.SetMessage("Do you want to backup all settings Saves/Actions/BIOS/Recents?");
                confirmBackup.UseYesNo();

                var StartBackup = await UserDialogs.Instance.ConfirmAsync(confirmBackup);
                if (StartBackup)
                {
                    var localFolder = CrossFileSystem.Current.LocalStorage;

                    if (localFolder != null)
                    {
                        SettingsIsLoadingState(true);
                        await Task.Delay(400);
                        IDirectoryInfo zipsDirectory = null;
                        zipsDirectory = CrossFileSystem.Current.RoamingStorage;
                        string targetFileName = "backup.rbp";
                        string zipFileName = zipsDirectory.FullName + "\\" + targetFileName;
                        var targetFielTest = await zipsDirectory.GetFileAsync(targetFileName);
                        if (targetFielTest != null)
                        {
                            await targetFielTest.DeleteAsync();
                        }
                        ZipFile.CreateFromDirectory(localFolder.FullName, zipFileName);
                        await DownloadExportedSlotsAsync(targetFileName);
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("faild.wav");
                        await UserDialogs.Instance.AlertAsync("Failed to export backup copy", "Backup failed");
                        SettingsIsLoadingState(false);
                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessageDirect(e);
                SettingsIsLoadingState(false);
            }
        }
        private async Task DownloadExportedSlotsAsync(string fileName)
        {
            try
            {
                var saveFile = await CrossFileSystem.Current.PickSaveFileAsync(".rbp");

                if (saveFile != null)
                {
                    await Task.Delay(3000);
                    var zipsDirectory = CrossFileSystem.Current.RoamingStorage;
                    var file = await zipsDirectory.GetFileAsync(fileName);
                    using (var inStream = await file.OpenAsync(FileAccess.Read))
                    {
                        using (var outStream = await saveFile.OpenAsync(FileAccess.ReadWrite))
                        {
                            await inStream.CopyToAsync(outStream);
                            await outStream.FlushAsync();
                        }
                    }
                    PlatformService.PlayNotificationSoundDirect("success.wav");
                    await UserDialogs.Instance.AlertAsync("Backup successfully created", "Backup done");
                }
            }
            catch (Exception e)
            {

                PlatformService.ShowErrorMessageDirect(e);

            }
            SettingsIsLoadingState(false);
        }

        private async Task ImportSettingsSlotsAction(IFileInfo PreBackupFile = null)
        {
            try
            {
                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                var extensions = new string[] { ".rbp" };
                IFileInfo file = null;
                if (PreBackupFile == null)
                {
                    file = await CrossFileSystem.Current.PickFileAsync(extensions);
                }
                else
                {
                    file = PreBackupFile;
                }
                if (file != null)
                {
                    PlatformService.PlayNotificationSoundDirect("alert.wav");
                    ConfirmConfig confirmImportSaves = new ConfirmConfig();
                    confirmImportSaves.SetTitle("Restore Backup");
                    confirmImportSaves.SetMessage("All previous Saves/BIOS/Actions/Recents will be lost, are you sure?");
                    confirmImportSaves.UseYesNo();

                    var StartImport = await UserDialogs.Instance.ConfirmAsync(confirmImportSaves);

                    if (StartImport)
                    {
                        SettingsIsLoadingState(true);
                        IDirectoryInfo zipsDirectory = null;
                        zipsDirectory = CrossFileSystem.Current.RoamingStorage;

                        string targetFileName = "backup.rbp";
                        string zipFileName = zipsDirectory.FullName + "\\" + targetFileName;
                        var targetFielTest = await zipsDirectory.GetFileAsync(targetFileName);
                        if (targetFielTest != null)
                        {
                            await targetFielTest.DeleteAsync();
                        }
                        var targetFile = await zipsDirectory.CreateFileAsync(targetFileName);
                        using (var inStream = await file.OpenAsync(FileAccess.Read))
                        {
                            using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                            {
                                await inStream.CopyToAsync(outStream);
                                await outStream.FlushAsync();
                            }
                        }
                        var localFolder = await CrossFileSystem.Current.LocalStorage.EnumerateDirectoriesAsync();
                        foreach (var DirectoryItem in localFolder)
                        {
                            try { 
                            await DirectoryItem.DeleteAsync();
                            }
                            catch
                            {

                            }
                        }

                        ZipFile.ExtractToDirectory(zipFileName, CrossFileSystem.Current.LocalStorage.FullName);
                        //ZipFile.ExtractToDirectory(zipFileName, CrossFileSystem.Current.LocalStorage.FullName, false);
                        PlatformService.PlayNotificationSoundDirect("success.wav");
                        ConfirmConfig confirmRestart = new ConfirmConfig();
                        confirmRestart.SetTitle("Restore Done");
                        confirmRestart.SetMessage("Backup Successfully Restored, Restart Retrix is needed");
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
                            await PlatformService.RetriveRecentsDirect();
                        }
                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("faild.wav");
                        if (PreBackupFile == null)
                        {
                            await UserDialogs.Instance.AlertAsync("Backup Restore Failed", "Restore Failed");
                        }
                        else
                        {
                            await UserDialogs.Instance.AlertAsync("Backup Restore Canceled", "Restore Canceled");
                        }
                    }
                }
            }
            catch (Exception e)
            {

                PlatformService.ShowErrorMessageDirect(e);

            }
            PlatformService.OpenBackupFile = null;
            SettingsIsLoadingState(false);
        }
        private void SettingsIsLoadingState(bool LoadingState)
        {
            VM.SystemSettingsIsLoadingState(LoadingState);
        }

        private async Task ImportAnyCoreAction()
        {
            try
            {

                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                var extensions = new string[] { ".dll" };
                var file = await CrossFileSystem.Current.PickFileAsync(extensions);
                if (file != null)
                {
                    PlatformService.PlayNotificationSoundDirect("alert.wav");
                    ConfirmConfig confirmImportSaves = new ConfirmConfig();
                    confirmImportSaves.SetTitle("Import Core");
                    confirmImportSaves.SetMessage("The previous AnyCore will be replaced, are you sure?");
                    confirmImportSaves.UseYesNo();

                    var StartImport = await UserDialogs.Instance.ConfirmAsync(confirmImportSaves);

                    if (StartImport)
                    {
                        SettingsIsLoadingState(true);
                        VM.ReleaseAnyCore();
                        IDirectoryInfo zipsDirectory = null;
                        zipsDirectory = CrossFileSystem.Current.LocalStorage;

                        string targetFileName = "AnyCore.dll";
                        string zipFileName = zipsDirectory.FullName + "\\" + targetFileName;
                        var targetFielTest = await zipsDirectory.GetFileAsync(targetFileName);
                        if (targetFielTest != null)
                        {
                            await targetFielTest.DeleteAsync();
                        }
                        var targetFile = await zipsDirectory.CreateFileAsync(targetFileName);
                        using (var inStream = await file.OpenAsync(FileAccess.Read))
                        {
                            using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                            {
                                await inStream.CopyToAsync(outStream);
                                await outStream.FlushAsync();
                            }
                        }

                        PlatformService.PlayNotificationSoundDirect("success.wav");
                        ConfirmConfig confirmRestart = new ConfirmConfig();
                        confirmRestart.SetTitle("Import Done");
                        confirmRestart.SetMessage("The core has been imported, Restart Retrix is required");
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
                        AnyCoreLoaded = true;
                        Bindings.Update();

                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("faild.wav");
                        await UserDialogs.Instance.AlertAsync("Core import canceled", "Import Canceled");
                    }
                }
            }
            catch (Exception e)
            {

                PlatformService.ShowErrorMessageDirect(e);

            }
            SettingsIsLoadingState(false);
        }

        private async Task ImportAnyCoreNewAction()
        {
            try
            {

                PlatformService.PlayNotificationSoundDirect("button-01.mp3");
                var extensions = new string[] { ".dll", ".rab", ".zip" };
                var file = await CrossFileSystem.Current.PickFileAsync(extensions);
                if (file != null)
                {
                    PlatformService.PlayNotificationSoundDirect("alert.wav");
                    ConfirmConfig confirmImportSaves = new ConfirmConfig();
                    confirmImportSaves.SetTitle("Import Core");
                    confirmImportSaves.SetMessage("After import the core, more settings will be available when you select the system.\n\nIf you trying to import core already loaded please remove it first.\n\nImport new core?");
                    confirmImportSaves.UseYesNo();

                    var StartImport = await UserDialogs.Instance.ConfirmAsync(confirmImportSaves);

                    if (StartImport)
                    {
                        SettingsIsLoadingState(true);
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
                        using (var inStream = await file.OpenAsync(FileAccess.Read))
                        {
                            using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                            {
                                await inStream.CopyToAsync(outStream);
                                await outStream.FlushAsync();
                            }
                        }

                        PlatformService.PlayNotificationSoundDirect("success.wav");
                        ConfirmConfig confirmRestart = new ConfirmConfig();
                        confirmRestart.SetTitle("Import Done");
                        confirmRestart.SetMessage("The core has been imported, Restart Retrix is required");
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
                        /*AnyCoreLoaded = true;
                        ImportAnyCoreButton.Icon = new SymbolIcon(Symbol.Delete);*/

                    }
                    else
                    {
                        PlatformService.PlayNotificationSoundDirect("faild.wav");
                        await UserDialogs.Instance.AlertAsync("Core import canceled", "Import Canceled");
                    }
                }

            }
            catch (Exception e)
            {

                PlatformService.ShowErrorMessageDirect(e);

            }
            SettingsIsLoadingState(false);
        }

        private async Task SetAnyCoreFolder()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".dll");
            StorageFile file = await openPicker.PickSingleFileAsync();
            string FileToken = StorageApplicationPermissions.FutureAccessList.Add(file);
            CrossSettings.Current.AddOrUpdateValue("AnyCoreToken", FileToken);

            PlatformService.PlayNotificationSoundDirect("success.wav");
            ConfirmConfig confirmRestart = new ConfirmConfig();
            confirmRestart.SetTitle("Set Done");
            confirmRestart.SetMessage("The core has been targeted, Restart Retrix is required");
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
        }
        private async void ImportAnyCoreButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await ImportAnyCoreNewAction();
            //await ImportAnyCoreAction();
            //await SetAnyCoreFolder();
        }

        private async void ResetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("button-01.mp3");
            try
            {
                PlatformService.PlayNotificationSoundDirect("alert.wav");
                ConfirmConfig confirmReset = new ConfirmConfig();
                confirmReset.SetTitle("Reset System");
                confirmReset.SetMessage("This action will reset all system's data\nIncluding BIOS, Saves, Actions..\nAre you sure?");
                confirmReset.UseYesNo();

                var StartReset = await UserDialogs.Instance.ConfirmAsync(confirmReset);

                if (StartReset)
                {
                    SettingsIsLoadingState(true);
                    var localFolder = await CrossFileSystem.Current.LocalStorage.EnumerateDirectoriesAsync();
                    foreach (var DirectoryItem in localFolder)
                    {
                        await DirectoryItem.DeleteAsync();
                    }

                    PlatformService.PlayNotificationSoundDirect("success.wav");
                    ConfirmConfig confirmRestart = new ConfirmConfig();
                    confirmRestart.SetTitle("Reset Done");
                    confirmRestart.SetMessage("The system has been reset, Restart Retrix is needed");
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
                        VM.ReloadFileDependency();
                    }
                }
            }
            catch (Exception ex)
            {

                PlatformService.ShowErrorMessageDirect(ex);

            }
            SettingsIsLoadingState(false);
        }

        private async void DeleteAnyCoreButton_Click(object sender, RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("button-01.mp3");
            PlatformService.PlayNotificationSoundDirect("alert.wav");
            ConfirmConfig confirmDeleteAnyCores = new ConfirmConfig();
            confirmDeleteAnyCores.SetTitle("Delete Cores");
            confirmDeleteAnyCores.SetMessage("Do you want to delete all AnyCore?");
            confirmDeleteAnyCores.UseYesNo();

            var StartDelete = await UserDialogs.Instance.ConfirmAsync(confirmDeleteAnyCores);
            if (StartDelete)
            {
                SettingsIsLoadingState(true);
                IDirectoryInfo zipsDirectory = null;
                zipsDirectory = CrossFileSystem.Current.LocalStorage;

                string targetFileName = "AnyCore";
                var targetFielTest = await zipsDirectory.GetDirectoryAsync(targetFileName);
                if (targetFielTest != null)
                {
                    VM.ReleaseAnyCore();
                    await targetFielTest.DeleteAsync();
                    PlatformService.PlayNotificationSoundDirect("success.wav");
                    await UserDialogs.Instance.AlertAsync("AnyCores Deleted, Restart Retrix is recommended");
                }
                else
                {
                    PlatformService.PlayNotificationSoundDirect("faild.wav");
                    await UserDialogs.Instance.AlertAsync("AnyCores not found!, unable to delete AnyCores");
                }
                AnyCoreLoaded = false;
                Bindings.Update();
            }
            SettingsIsLoadingState(false);
        }
    }
}
