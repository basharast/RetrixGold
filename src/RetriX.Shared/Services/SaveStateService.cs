using Plugin.FileSystem.Abstractions;
using RetriX.Shared.ExtensionMethods;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public class SaveStateService : ISaveStateService
    {
        private const string SaveStatesFolderName = "SaveStates";

        private readonly IFileSystem FileSystem;

        private string GameId { get; set; }
        private IPlatformService PlatformService { get; }

        private bool OperationInProgress = false;
        private bool AllowOperations => !(OperationInProgress || SaveStatesFolder == null || GameId == null);

        private IDirectoryInfo SaveStatesFolder;

        public SaveStateService(IFileSystem fileSystem)
        {
            try
            {
                FileSystem = fileSystem;

                GetSubfolderAsync(FileSystem.LocalStorage, SaveStatesFolderName).ContinueWith(d =>
                {
                    SaveStatesFolder = d.Result;
                });

                GameId = null;
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public void SetGameId(string id)
        {
            try
            {
                GameId = null;
                if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
                {
                    return;
                }

                GameId = id.MD5();
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public async Task<Stream> GetStreamForSlotAsync(uint slotId, FileAccess access)
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
                var file = await statesFolder.GetFileAsync(fileName);
                if (file == null)
                {
                    if (access == FileAccess.Read)
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

                var stream = await file.OpenAsync(access);
                OperationInProgress = false;
                return stream;
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
                var file = await statesFolder.GetFileAsync(fileName);

                OperationInProgress = false;
                return file != null;
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
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
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        private string GenerateSaveFileName(uint slotId)
        {
            return $"{GameId}_S{slotId}.sav";
        }

        private Task<IDirectoryInfo> GetGameSaveStatesFolderAsync()
        {
            try
            {
                return GetSubfolderAsync(SaveStatesFolder, GameId);
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

        private static async Task<IDirectoryInfo> GetSubfolderAsync(IDirectoryInfo parent, string name)
        {
            try
            {
                IDirectoryInfo output = await parent.GetDirectoryAsync(name);
                if (output == null)
                {
                    output = await parent.CreateDirectoryAsync(name);
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
