using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.Writers;
using Windows.Storage;
using Windows.Storage.Search;

namespace SharpCompress.Archives
{
    public static class IWritableArchiveExtensions
    {
        public static void AddEntry(this IWritableArchive writableArchive,
                                                     string entryPath, string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("Could not AddEntry: " + filePath);
            }
            writableArchive.AddEntry(entryPath, new FileInfo(filePath).OpenRead(), true, fileInfo.Length,
                                     fileInfo.LastWriteTime);
        }

        public static void SaveTo(this IWritableArchive writableArchive, string filePath, WriterOptions options)
        {
            writableArchive.SaveTo(new FileInfo(filePath), options);
        }

        public static void SaveTo(this IWritableArchive writableArchive, FileInfo fileInfo, WriterOptions options)
        {
            using (var stream = fileInfo.Open(FileMode.Create, FileAccess.Write))
            {
                writableArchive.SaveTo(stream, options);
            }
        }

        public static void AddAllFromDirectory(
            this IWritableArchive writableArchive,
            string filePath, string searchPattern = "*.*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            using (writableArchive.PauseEntryRebuilding())
            {
                foreach (var path in Directory.EnumerateFiles(filePath, searchPattern, searchOption))
                {
                    var fileInfo = new FileInfo(path);
                    writableArchive.AddEntry(path.Substring(filePath.Length), fileInfo.OpenRead(), true, fileInfo.Length,
                                            fileInfo.LastWriteTime);
                }
            }
        }
        public static async Task AddAllFromDirectory(
            this IWritableArchive writableArchive,
            StorageFolder targetFolder, bool useWindowsIndexer, string[] searchPattern = null, string[] customFolders = null, IProgress<int> progress = null, bool IncludeRootFolder = true, CancellationTokenSource cancellationTokenSource = null)
        {
            List<string> customExts = new List<string>();
            if (searchPattern != null)
            {
                customExts = searchPattern.ToList();
            }
            using (writableArchive.PauseEntryRebuilding())
            {
                QueryOptions queryOptions = null;
                if (customExts != null && customExts.Count > 0)
                {
                    queryOptions = new QueryOptions(CommonFileQuery.OrderByName, customExts);
                }
                else
                {
                    queryOptions = new QueryOptions();
                }
                queryOptions.FolderDepth = FolderDepth.Deep;
                if (useWindowsIndexer)
                {
                    queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
                }
                var queryResult = targetFolder.CreateFileQueryWithOptions(queryOptions);

                var files = await queryResult.GetFilesAsync();
                int totalFiles = 0;
                foreach (StorageFile file in files)
                {
                    if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }
                    try
                    {
                        if (customExts != null && customExts.Count > 0)
                        {
                            if (!customExts.Contains(Path.GetExtension(file.Name).ToLower()))
                            {
                                continue;
                            }
                        }
                        if (customFolders != null && customFolders.Length > 0)
                        {
                            bool skipThisFile = true;
                            foreach (var cItem in customFolders)
                            {
                                if (file.Path.Contains(cItem))
                                {
                                    skipThisFile = false;
                                    break;
                                }
                            }
                            if (skipThisFile)
                            {
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    var path = targetFolder.Path;
                    if (IncludeRootFolder)
                    {
                        try
                        {
                            path = path.Replace(Path.GetFileName(targetFolder.Path), "");
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    var filePath = file.Path;
                    var basicProperties = await file.GetBasicPropertiesAsync();
                    Stream outstream = await file.OpenStreamForReadAsync();
                    DateTime utc = basicProperties.DateModified.Date;
                    var fileKey = filePath.Replace(path, "");
                    writableArchive.AddEntry(fileKey, outstream, true, (long)basicProperties.Size, utc);
                    try
                    {
                        if (progress != null)
                        {
                            totalFiles++;
                            progress.Report(totalFiles);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }
        public static IArchiveEntry AddEntry(this IWritableArchive writableArchive, string key, FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                throw new ArgumentException("FileInfo does not exist.");
            }
            return writableArchive.AddEntry(key, fileInfo.OpenRead(), true, fileInfo.Length, fileInfo.LastWriteTime);
        }
    }
}