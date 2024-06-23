using RetriX.Shared.Components;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

namespace RetriX.Shared.StreamProviders
{
    public class ArchiveStreamProvider
    {
        public static ISet<string> SupportedExtensions { get; set; } = new HashSet<string> { ".zip", ".7z", ".rar", ".gz", ".tar" };
        public static ISet<string> SupportedExtensionsNonZip { get; set; } = new HashSet<string> { ".7z", ".rar", ".gz", ".tar" };

        private StorageFile ArchiveFile { get; set; }
        private List<string> EntriesList { get; set; } = new List<string>();

        public ArchiveStreamProvider(StorageFile archiveFile)
        {
            ArchiveFile = archiveFile;
        }

        public void Dispose()
        {
            try
            {
                EntriesList.Clear();
                ArchiveFile = null; ;
                EntriesList = null;
                //GCCollectForList(EntriesList);
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
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
        public async Task<List<string>> ListEntriesAsync()
        {
            try
            {
                await InitializeAsync();
                return EntriesList;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                return null;
            }
        }

        public async Task<bool> ExtractArchive(string destination, List<string> archiveEntries)
        {
            bool extarctState = false;
            try
            {
                IProgress<Dictionary<string, long[]>> progress = new Progress<Dictionary<string, long[]>>(value =>
                {
                    try
                    {
                        var entryKey = value.FirstOrDefault().Key;
                        if (!archiveEntries.Contains(entryKey))
                        {
                            archiveEntries.Add(entryKey);
                            try
                            {
                                var pathParts = entryKey.Split('\\');
                                foreach (var part in pathParts)
                                {
                                    if (!archiveEntries.Contains(part) && !entryKey.EndsWith(part))
                                    {
                                        archiveEntries.Add(part);
                                    }
                                }
                            }
                            catch (Exception e)
                            {

                            }
                        }
                        var data = value.FirstOrDefault().Value;
                        var current = data[0];
                        var total = data[1];
                        try
                        {
                            if (entryKey.Contains("\\"))
                            {
                                var dataSplit = entryKey.Split('\\');
                                entryKey = dataSplit[dataSplit.Length - 1];
                            }
                        }
                        catch (Exception e)
                        {

                        }
                        FramebufferConverter.currentFileEntry = entryKey;
                        var totalCopied = (current * 1d) / (total * 1d) * 100;
                        FramebufferConverter.currentFileProgress = totalCopied;
                    }
                    catch (Exception ex)
                    {

                    }
                });
                extarctState = await PlatformService.ExtractTemporaryFiles(ArchiveFile, destination, progress);
            }
            catch (Exception xe)
            {
                PlatformService.ShowErrorMessage(xe);
            }
            return extarctState;
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
                PlatformService.ShowErrorMessage(e);
            }
        }

        private async Task InitializeAsync()
        {
            try
            {
                var stream = await ArchiveFile.OpenAsync(FileAccessMode.Read);
                var EntriesListTest = PlatformService.GetFilesEntries(stream.AsStream());
                if (EntriesListTest != null)
                {
                    EntriesList = EntriesListTest;
                }
                else
                {
                    EntriesList = new List<string>();
                }

            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }
    }
}
