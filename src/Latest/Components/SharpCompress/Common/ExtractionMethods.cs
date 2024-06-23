using SharpCompress.Readers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace SharpCompress.Common
{
    internal static class ExtractionMethods
    {
        /// <summary>
        /// Extract to specific directory, retaining filename
        /// </summary>
        public static void WriteEntryToDirectory(IEntry entry,
                                                 string destinationDirectory,
                                                 ExtractionOptions options,
                                                 Action<string, ExtractionOptions> write)
        {
            string destinationFileName;
            string fullDestinationDirectoryPath = Path.GetFullPath(destinationDirectory);

            //check for trailing slash.
            if (fullDestinationDirectoryPath[fullDestinationDirectoryPath.Length - 1] != Path.DirectorySeparatorChar)
            {
                fullDestinationDirectoryPath += Path.DirectorySeparatorChar;
            }

            if (!Directory.Exists(fullDestinationDirectoryPath))
            {
                throw new ExtractionException($"Directory does not exist to extract to: {fullDestinationDirectoryPath}");
            }

            if (options == null)
            {
                options = new ExtractionOptions()
                {
                    Overwrite = true
                };
            }
            string file = Path.GetFileName(entry.Key);
            if (options.ExtractFullPath)
            {
                string folder = Path.GetDirectoryName(entry.Key);
                string destdir = Path.GetFullPath(Path.Combine(fullDestinationDirectoryPath, folder));

                if (!Directory.Exists(destdir))
                {
                    if (!destdir.StartsWith(fullDestinationDirectoryPath, StringComparison.Ordinal))
                    {
                        throw new ExtractionException("Entry is trying to create a directory outside of the destination directory.");
                    }

                    Directory.CreateDirectory(destdir);
                }
                destinationFileName = Path.Combine(destdir, file);
            }
            else
            {
                destinationFileName = Path.Combine(fullDestinationDirectoryPath, file);

            }

            if (!entry.IsDirectory)
            {
                destinationFileName = Path.GetFullPath(destinationFileName);

                if (!destinationFileName.StartsWith(fullDestinationDirectoryPath, StringComparison.Ordinal))
                {
                    throw new ExtractionException("Entry is trying to write a file outside of the destination directory.");
                }
                write(destinationFileName, options);
            }
            else if (options.ExtractFullPath && !Directory.Exists(destinationFileName))
            {
                Directory.CreateDirectory(destinationFileName);
            }
        }
        //UWP WriteEntryToDirectory Function
        public static async Task WriteEntryToDirectory(IEntry entry, StorageFolder destinationDirectory,
                                                 ExtractionOptions options, IReader reader, CancellationTokenSource cancellationTokenSource = null)
        {
            string file = Path.GetFileName(entry.Key);
            string fullDestinationDirectoryPath = destinationDirectory.Path;

            options = options ?? new ExtractionOptions()
            {
                Overwrite = true
            };

            StorageFolder extractTarget = destinationDirectory;
            if (options.ExtractFullPath)
            {
                extractTarget = await createFolderByPath(entry.Key, destinationDirectory);
            }
            var overwrite = options.Overwrite;

            if (!entry.IsDirectory)
            {
                var fileSize = reader.Entry.Size;
                if (fileSize > 0)
                {
                    //Need to check the file if exists then ignore
                    StorageFile uncompressedFile = await extractTarget.CreateFileAsync(Path.GetFileName(reader.Entry.Key), CreationCollisionOption.ReplaceExisting);
                    using (Stream outstream = await uncompressedFile.OpenStreamForWriteAsync())
                    {
                        /*if (cancellationTokenSource != null)
                        {
                            using (Stream entryStream = reader.OpenEntryStream())
                            {
                                await entryStream.CopyToAsync(outstream, 81920, cancellationTokenSource.Token);
                                try
                                {
                                    entryStream.Dispose();
                                }
                                catch (Exception e)
                                {

                                }
                            }
                        }
                        else*/
                        {
                            reader.WriteEntryTo(outstream, cancellationTokenSource);
                        }
                    }
                }
                else
                {
                    StorageFile uncompressedFile = await extractTarget.CreateFileAsync(Path.GetFileName(reader.Entry.Key), CreationCollisionOption.ReplaceExisting);
                }
            }
            else if (options.ExtractFullPath)
            {
                var test = await extractTarget.GetFolderAsync(file);
                if (test == null || overwrite)
                {
                    await extractTarget.CreateFolderAsync(file, CreationCollisionOption.ReplaceExisting);
                }
            }
        }

        public static void WriteEntryToFile(IEntry entry, string destinationFileName,
                                            ExtractionOptions options,
                                            Action<string, FileMode> openAndWrite)
        {
            if (entry.LinkTarget != null)
            {
                if (options.WriteSymbolicLink is null)
                {
                    throw new ExtractionException("Entry is a symbolic link but ExtractionOptions.WriteSymbolicLink delegate is null");
                }
                options.WriteSymbolicLink(destinationFileName, entry.LinkTarget);
            }
            else
            {
                FileMode fm = FileMode.Create;

                if (options == null)
                    options = new ExtractionOptions()
                    {
                        Overwrite = true
                    };

                if (!options.Overwrite)
                {
                    fm = FileMode.CreateNew;
                }

                openAndWrite(destinationFileName, fm);
                entry.PreserveExtractionOptions(destinationFileName, options);
            }
        }

        #region Helpers
        private static bool IfPathContainDirectory(string entryPath)
        {
            if (string.IsNullOrEmpty(entryPath))
            {
                return false;
            }
            return entryPath.Contains("/") || entryPath.Contains("\\");
        }
        private static async Task<bool> IfFolderExistsAsync(StorageFolder storageFolder, string subFolderName)
        {
            try
            {
                var testFolder = (StorageFolder)await storageFolder.TryGetItemAsync(subFolderName);
                return testFolder != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static async Task<StorageFolder> createFolderByPath(string filePath, StorageFolder unzipFolder)
        {
            StorageFolder subFolder = unzipFolder;

            string name = Path.GetFileName(filePath);
            if (IfPathContainDirectory(filePath))
            {
                filePath = filePath.Replace($"/{name}", "");
                if (IfPathContainDirectory(filePath))
                {
                    var subs = filePath.Split('/');
                    foreach (var sub in subs)
                    {
                        // Create the sub folder. 
                        subFolder = await subFolder.CreateFolderAsync(sub, CreationCollisionOption.OpenIfExists);
                    }
                }
                else
                {
                    // Create the sub folder. 
                    subFolder = await subFolder.CreateFolderAsync(filePath, CreationCollisionOption.OpenIfExists);
                }
            }
            return subFolder;
        }
        #endregion
    }
}