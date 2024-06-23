using RetriX.UWP.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;

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
    public class SaveStateService
    {
        public string SaveStatesFolderName = "SaveStates";

        private string GameId { get; set; }

        private bool OperationInProgress = false;
        private bool AllowOperations => !(OperationInProgress || SaveStatesFolder == null || GameId == null);

        public StorageFolder SaveStatesFolder;
        private StorageFolder SaveStatesFolderOld;

        public SaveStateService()
        {
            try
            {

                GetSubfolderAsync(ApplicationData.Current.LocalFolder, SaveStatesFolderName).ContinueWith(d =>
                {
                    SaveStatesFolder = d.Result;
                    SaveStatesFolderOld = SaveStatesFolder;
                });

                GameId = null;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }

        bool customFolder = false;
        public async Task SetSaveStatesFolder(StorageFolder storageFolder)
        {
            if (storageFolder == null)
            {
                SaveStatesFolder = SaveStatesFolderOld;
                customFolder = false;
            }
            else
            {
                TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

                await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                {
                    try
                    {
                        var savesFolder = await storageFolder.CreateFolderAsync("SavedStates", CreationCollisionOption.OpenIfExists);
                        if (savesFolder != null)
                        {
                            SaveStatesFolder = savesFolder;
                        }
                        else
                        {
                            SaveStatesFolder = storageFolder;
                        }
                    }
                    catch (Exception ex)
                    {
                        SaveStatesFolder = storageFolder;
                    }
                    taskCompletionSource.SetResult(true);
                });
                await taskCompletionSource.Task;
                customFolder = true;
            }
        }

        public async Task SetGameId(string id)
        {
            try
            {
                GameId = null;
                if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
                {
                    return;
                }
                else
                {
                    GameId = id;

                    try
                    {
                        TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                        if (customFolder)
                        {
                            await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                            {
                                try
                                {
                                    //Resolve old saves folders
                                    var testFolder = (StorageFolder)await SaveStatesFolderOld.TryGetItemAsync(id);
                                    if (testFolder != null)
                                    {
                                        var newFolder = await SaveStatesFolder.CreateFolderAsync(id, CreationCollisionOption.OpenIfExists);
                                        if (newFolder != null)
                                        {
                                            PlatformService.ShowNotificationDirect("Checking old saves..", 3);
                                            var files = await testFolder.GetFilesAsync();
                                            if (files.Count > 0)
                                            {
                                                int counter = 0;
                                                PlatformService.ShowNotificationDirect("Resolving old saves..", 3);
                                                foreach (var file in files)
                                                {
                                                    PlatformService.ShowNotificationDirect($"Resolving old saves ({counter + 1} of {files.Count})..", 3);
                                                    await file.MoveAsync(newFolder, file.Name, NameCollisionOption.ReplaceExisting);
                                                    counter++;
                                                }
                                                await testFolder.DeleteAsync();
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                taskCompletionSource.SetResult(true);
                            });
                            await taskCompletionSource.Task;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }

        public async Task<StorageFile> GetStreamForSlotAsync(uint slotId, FileAccessMode access)
        {
            try
            {
                if (!AllowOperations)
                {
                    return null;
                }

                OperationInProgress = true;

                var statesFolder = await GetGameSaveStatesFolderAsync();
                var fileName = GenerateSaveFileName(slotId);
                var file = (StorageFile)await statesFolder.TryGetItemAsync(fileName);
                if (file == null)
                {
                    if (access == FileAccessMode.Read)
                    {
                        OperationInProgress = false;
                        return null;
                    }

                    file = await statesFolder.CreateFileAsync(fileName);
                    //This should never happen
                    if (file == null)
                    {
                        return null;
                    }
                }

                OperationInProgress = false;
                return file;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                return null;
            }
        }

        public async Task<bool> SlotHasDataAsync(uint slotId)
        {
            try
            {
                if (!AllowOperations)
                {
                    return false;
                }

                OperationInProgress = true;

                var statesFolder = await GetGameSaveStatesFolderAsync();
                var fileName = GenerateSaveFileName(slotId);
                var file = (StorageFile)await statesFolder.TryGetItemAsync(fileName);

                OperationInProgress = false;
                return file != null;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                return false;
            }
        }

        public async Task ClearSavesAsync()
        {
            try
            {
                if (!AllowOperations)
                {
                    return;
                }

                OperationInProgress = true;

                var statesFolder = await GetGameSaveStatesFolderAsync();
                await statesFolder.DeleteAsync();
                await GetGameSaveStatesFolderAsync();

                OperationInProgress = false;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }

        private string GenerateSaveFileName(uint slotId)
        {
            return $"{GameId}_S{slotId}.sav";
        }

        private Task<StorageFolder> GetGameSaveStatesFolderAsync()
        {
            try
            {
                return GetSubfolderAsync(SaveStatesFolder, GameId);
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                return null;
            }
        }

        private static async Task<StorageFolder> GetSubfolderAsync(StorageFolder parent, string name)
        {
            try
            {
                StorageFolder output = (StorageFolder)await parent.TryGetItemAsync(name);
                if (output == null)
                {
                    output = await parent.CreateFolderAsync(name);
                }

                return output;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
