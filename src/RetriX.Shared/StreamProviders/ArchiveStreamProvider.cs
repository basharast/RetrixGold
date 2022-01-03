using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Components;
using RetriX.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RetriX.Shared.StreamProviders
{
    public class ArchiveStreamProvider : IStreamProvider
    {
        public static ISet<string> SupportedExtensions { get; } = new HashSet<string> { ".zip", ".7z", ".rar", ".gz", ".tar" };

        private string HandledScheme { get; }
        private IFileInfo ArchiveFile { get; }
        private IPlatformService PlatformService { get; }

        private IDictionary<string, ArchiveData> EntriesMapping { get; } = new SortedDictionary<string, ArchiveData>();

        public ArchiveStreamProvider(string handledScheme, IFileInfo archiveFile, IPlatformService platformService)
        {
            HandledScheme = handledScheme;
            ArchiveFile = archiveFile;
            PlatformService = platformService;
        }

        public void Dispose()
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public void GCCollectForList<T>(T ListToCollect)
        {
            try
            {
                int identificador = GC.GetGeneration(ListToCollect);
                GC.Collect(identificador, GCCollectionMode.Forced);
            }
            catch (Exception e)
            {

            }
        }
        public async Task<IEnumerable<string>> ListEntriesAsync()
        {
            try
            {
                await InitializeAsync();
                return EntriesMapping.Keys;
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

        public async Task<Stream> OpenFileStreamAsync(string path, FileAccess accessType)
        {
            try
            {
                await InitializeAsync();
                if (!EntriesMapping.Keys.Contains(path))
                {
                    return default(Stream);
                }

                var archiveData = EntriesMapping[path];
                var writeable = accessType == FileAccess.ReadWrite || accessType == FileAccess.Write;

                var backingStore = new byte[archiveData.streamSize];
                var totalSize = backingStore.Length;
                IProgress<double> progress = new Progress<double>(value =>
                {
                    try
                    {
                        FramebufferConverter.currentFileEntry = archiveData.entryKey;
                        var totalCopied = (value * 1d) / (totalSize * 1d) * 100;
                        FramebufferConverter.currentFileProgress = totalCopied;
                    }
                    catch (Exception ex)
                    {

                    }
                });
                using (var tempStream = new MemoryStream(backingStore, true))
                {
                    archiveData.WriteEntryTo(tempStream, progress);
                }
                var output = new MemoryStream(backingStore, writeable);
                backingStore = null;
                GCCollectForList(backingStore);
                return output;
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

        public void CloseStream(Stream stream)
        {
            try
            {
                try
                {
                    stream.Dispose();
                    stream = null;
                    GCCollectForList(stream);
                }
                catch (Exception e)
                {

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

        private async Task InitializeAsync()
        {
            try
            {
                if (PlatformService != null)
                {
                    var stream = await ArchiveFile.OpenAsync(FileAccess.Read);
                    var EntriesMappingTemp = await PlatformService.GetFilesStreams(stream, HandledScheme);
                    if (EntriesMappingTemp != null)
                    {
                        foreach (var item in EntriesMappingTemp)
                        {
                            ArchiveData test;
                            if (!EntriesMapping.TryGetValue(item.Key, out test))
                            {
                                EntriesMapping.Add(item);
                            }
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
    }
}
