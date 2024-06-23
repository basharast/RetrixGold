using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace RetriX.UWP
{
    public class PackageArchiveHelper
    {
        public delegate void ExtractEventHandler(object sender, ExtractEventArgs e);
        public event ExtractEventHandler ExtractProgress;

        public async Task<bool> ExtractFiles(StorageFolder dest, StorageFile archive, IProgress<ProgressReport> progress = null, CancellationTokenSource cancellationTokenSource = null)
        {
            double Extracted = 0;
            var TotalFiles = 0;
            using (var archiveStream = await archive.OpenReadAsync())
            {
                var stream = archiveStream.AsStream();
                var package = new ZipArchive(stream);
                var packageParts = package.Entries;
                TotalFiles = packageParts.Count;
                foreach (var packagePart in packageParts)
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        return false;
                    }
                    Extracted++;
                    if (progress != null)
                    {
                        var Progress = (Extracted / TotalFiles) * 100;
                        var report = new ProgressReport(Progress, Extracted, TotalFiles, packagePart);
                        progress.Report(report);
                    }
                    if (packagePart.FullName.EndsWith("/"))
                    {
                        continue;
                    }
                    var partStream = packagePart.Open();

                    var target = await CreateFolderFromPath(packagePart.FullName, dest);
                    using (var targetStream = await target.file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        TransferTo(partStream, targetStream.AsStream(), packagePart, cancellationTokenSource);
                    }
                }
            }
            return true;
        }
        public async Task<ArchiveTarget> CreateFolderFromPath(string uri, StorageFolder dest)
        {
            StorageFolder targetFolder = dest;
            var path = uri;
            var fileName = Path.GetFileName(path);

            if (IfPathContainDirectory(path))
            {
                // Create sub folder 
                string subFolderName = Path.GetDirectoryName(path);
                bool isSubFolderExist = await IfFolderExistsAsync(dest, subFolderName);
                StorageFolder subFolder;
                if (!isSubFolderExist)
                {
                    // Create the sub folder. 
                    subFolder = await dest.CreateFolderAsync(subFolderName, CreationCollisionOption.ReplaceExisting);
                }
                else
                {
                    // Just get the folder. 
                    subFolder = await dest.GetFolderAsync(subFolderName);
                }
                // All sub folders have been created. Just pass the file name to the Unzip function. 
                string newFilePath = Path.GetFileName(path);
                if (!string.IsNullOrEmpty(newFilePath))
                {
                    // Unzip file iteratively. 
                    return await CreateFolderFromPath(newFilePath, subFolder);
                }
            }

            var targetFile = await targetFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            return new ArchiveTarget(targetFolder, targetFile);
        }
        public long TransferTo(Stream source, Stream destination, ZipArchiveEntry entry, CancellationTokenSource cancellationTokenSource = null)
        {
            byte[] array = GetTransferByteArray();
            try
            {
                var iterations = 0;
                long total = 0;
                while (ReadTransferBlock(source, array, out int count))
                {
                    total += count;
                    destination.Write(array, 0, count);
                    iterations++;
                    if (ExtractProgress != null)
                    {
                        ExtractProgress.Invoke(null, new ExtractEventArgs(entry , total, iterations));
                    }
                    if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }
                }
                return total;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
        private byte[] GetTransferByteArray()
        {
            return ArrayPool<byte>.Shared.Rent(81920);
        }
        private bool ReadTransferBlock(Stream source, byte[] array, out int count)
        {
            return (count = source.Read(array, 0, array.Length)) != 0;
        }

        public bool IfPathContainDirectory(string entryPath)
        {
            if (string.IsNullOrEmpty(entryPath))
            {
                return false;
            }
            return entryPath.Contains("/");
        }
        /// <summary> 
        /// It checks if the specified folder exists. 
        /// </summary> 
        /// <param name="storageFolder">The container folder</param> 
        /// <param name="subFolderName">The sub folder name</param> 
        /// <returns></returns> 
        public async Task<bool> IfFolderExistsAsync(StorageFolder storageFolder, string subFolderName)
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

    }
    public class ProgressReport
    {
        public double progress;
        public double extracted;
        public int total;
        public ZipArchiveEntry entry;
        public ProgressReport(double progress, double extracted, int total, ZipArchiveEntry entry)
        {
            this.progress = progress;
            this.extracted = extracted;
            this.total = total;
            this.entry = entry;
        }
    }

    public class ArchiveTarget
    {
        public StorageFolder folder;
        public StorageFile file;
        public ArchiveTarget(StorageFolder storageFolder, StorageFile storageFile)
        {
            folder = storageFolder;
            file = storageFile;
        }
    }
    public class ExtractEventArgs
    {
        public long total;
        public long BytesTransferred;
        public int iterations;
        public double PercentageReadExact;
        public ZipArchiveEntry entry;
        public ExtractEventArgs(ZipArchiveEntry entry, long total, int iterations)
        {
            this.entry = entry;
            this.BytesTransferred = total;
            this.total = total;
            this.PercentageReadExact = iterations;
            this.iterations = iterations;
        }
    }
}
