using Acr.UserDialogs;
using LibRetriX;
using MvvmCross.Core.ViewModels;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RetriX.Shared.ViewModels
{
    public class SettingsViewModel : MvxViewModel
    {
        private IGameSystemsProviderService GameSystemsProvider { get; }
        private IFileSystem FileSystem { get; }
        private IUserDialogs DialogsService { get; }
        private IPlatformService PlatformService { get; }
        private ICryptographyService CryptographyService { get; }

        private IReadOnlyList<FileImporterViewModel> fileDependencyImporters;
        public IReadOnlyList<FileImporterViewModel> FileDependencyImporters
        {
            get => fileDependencyImporters;
            private set => SetProperty(ref fileDependencyImporters, value);
        }

        public ObservableCollection<string> CoresList = new ObservableCollection<string>();
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
        public SettingsViewModel(IGameSystemsProviderService gameSystemsProvider, IFileSystem fileSystem, IUserDialogs dialogsService, IPlatformService platformService, ICryptographyService cryptographyService)
        {
            try
            {
                GameSystemsProvider = gameSystemsProvider;
                FileSystem = fileSystem;
                DialogsService = dialogsService;
                PlatformService = platformService;
                CryptographyService = cryptographyService;
                CoresList.Add("All Consoles");
                Task.Run(GetFileDependencyImportersAsync).ContinueWith(d => FileDependencyImporters = d.Result, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public bool SystemSettingsIsLoading = false;
        public bool BiosFilterIsLoading = false;
        public event EventHandler eventHandler;
        public void GetFileDependencyByCoreName(string coreName="")
        {
            try
            {
                coreName = GetSystemNameByPreviewName(coreName);
                Task.Run(() => GetFileDependencyImportersAsync(coreName)).ContinueWith(d => FileDependencyImporters = d.Result, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public void ReloadFileDependency()
        {
            try { 
            Task.Run(GetFileDependencyImportersAsync).ContinueWith(d => FileDependencyImporters = d.Result, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        private async Task<List<FileImporterViewModel>> GetFileDependencyImportersAsync(string coreName)
        {
            try
            {
                BiosFilterIsLoadingState(true);
                var importers = new List<FileImporterViewModel>();
                var distinctCores = new HashSet<ICore>();
                foreach (var i in GameSystemsProvider.Systems)
                {
                    var core = i.Core;
                    if (core.FailedToLoad || !core.SystemName.Equals(coreName) || distinctCores.Contains(core))
                    {
                        continue;
                    }

                    distinctCores.Add(core);
                    var systemFolder = await i.GetSystemDirectoryAsync();

                    var tasks = core.FileDependencies.Select(d => FileImporterViewModel.CreateFileImporterAsync(core, FileSystem, DialogsService, PlatformService, CryptographyService, systemFolder, d.Name, d.Description, d.MD5)).ToArray();
                    try
                    {
                        var newImporters = await Task.WhenAll(tasks);
                        importers.AddRange(newImporters);
                    }
                    catch (Exception ex)
                    {
                        if (PlatformService != null)
                        {
                            PlatformService.ShowErrorMessage(ex);
                        }
                    }

                }
                BiosFilterIsLoadingState(false);
                return importers;
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                BiosFilterIsLoadingState(false);
                return null;
            }
            }

            int ImportersTempCount = 0;
            private async Task<List<FileImporterViewModel>> GetFileDependencyImportersAsync()
            {
            try
            {
                BiosFilterIsLoadingState(true);
                var importers = new List<FileImporterViewModel>();
                var distinctCores = new HashSet<ICore>();
                await Task.Delay(3000);
                foreach (var i in GameSystemsProvider.Systems)
                {
                    var core = i.Core;
                    if (core.FailedToLoad || distinctCores.Contains(core))
                    {
                        continue;
                    }
                    
                    
                    distinctCores.Add(core);
                    var systemFolder = await i.GetSystemDirectoryAsync();

                    var tasks = core.FileDependencies.Select(d => FileImporterViewModel.CreateFileImporterAsync(core, FileSystem, DialogsService, PlatformService, CryptographyService, systemFolder, d.Name, d.Description, d.MD5)).ToArray();
                    try { 
                    var newImporters = await Task.WhenAll(tasks);
                        importers.AddRange(newImporters);
                    }
                    catch(Exception ex)
                    {
                        if (PlatformService != null)
                        {
                            PlatformService.ShowErrorMessage(ex);
                        }
                    }
                    if (importers.Count - ImportersTempCount > 0)
                    {
                        try
                        {
                            Dispatcher.RequestMainThreadAction(() => addNewConsoleToFiltersList(core.SystemName, core.OriginalSystemName));
                        }
                        catch (Exception ex)
                        {
                            if (PlatformService != null)
                            {
                                PlatformService.ShowErrorMessage(ex);
                            }
                        }
                        ImportersTempCount = importers.Count;
                    }
                }
                BiosFilterIsLoadingState(false);
                if (eventHandler != null) { 
                eventHandler.Invoke(this, EventArgs.Empty);
                }
                return importers;
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                BiosFilterIsLoadingState(false);
                return null;
            }
        }

        private void addNewConsoleToFiltersList(string SystemName, string OriginalSystemName)
        {
            try
            {
                lock (CoresList)
                {
                    if (CoresList.IndexOf(SystemName) == -1)
                    {
                        CoresList.Add(OriginalSystemName);
                        SystemsMap.Add(SystemName, OriginalSystemName);
                    }
                }
            }
            catch (Exception ex)
            {
                if (PlatformService != null)
                {
                    //PlatformService.ShowErrorMessage(ex);
                }
            }
        }
        public void SystemSettingsIsLoadingState(bool LoadingState)
        {
            try
            {

                SystemSettingsIsLoading = LoadingState;
                RaisePropertyChanged(nameof(SystemSettingsIsLoading));

            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public void BiosFilterIsLoadingState(bool LoadingState)
        {
            try
            {
                BiosFilterIsLoading = LoadingState;
                RaisePropertyChanged(nameof(BiosFilterIsLoading));
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public void ReleaseAnyCore(string CoreName="Any Core")
        {
            foreach(var SystemItem in GameSystemsProvider.Systems)
            {
                if (SystemItem.TempName == CoreName || (CoreName.Length==0 && SystemItem.AnyCore))
                {
                    SystemItem.Core.FreeLibretroCore();
                    SystemItem.Core.FailedToLoad = true;
                    return;
                }
            }
        }
    }
}
