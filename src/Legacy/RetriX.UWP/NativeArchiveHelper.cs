using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Compression;
using Windows.Storage.Search;
using Windows.Storage.Streams;

namespace RetriX.UWP
{
    /// <summary>
    /// Compresses and decompresses single files and folders.
    /// Modified by Mahdi Ghiasi
    /// </summary>
    public class ArchiverPlus
    {
        public delegate void CompressingEventHandler(object sender, CompressingEventArgs e);
        public event CompressingEventHandler CompressingProgress;

        public delegate void DecompressingEventHandler(object sender, DecompressingEventArgs e);
        public event DecompressingEventHandler DecompressingProgress;

        protected virtual void OnCompressingProgress(CompressingEventArgs e)
        {
            if (CompressingProgress != null)
                CompressingProgress(this, e);
        }

        protected virtual void OnDecompressingProgress(DecompressingEventArgs e)
        {
            if (DecompressingProgress != null)
                DecompressingProgress(this, e);
        }

        private int _processedFilesCount = 0;
        private string curRoot = "";
        public List<string> log;

        /// <summary>
        /// Compresses a folder, including all of its files and sub-folders.
        /// </summary>
        /// <param name="source">The folder containing the files to compress.</param>
        /// <param name="destination">The compressed zip file.</param>
        public async Task Compress(StorageFolder source, StorageFile destination, CompressionLevel compressionLevel, string[] customExts = null)
        {
            List<StorageFolder> l = new List<StorageFolder>();
            l.Add(source);

            await Compress(l, destination, compressionLevel, customExts);
        }

        public async Task Compress(List<StorageFolder> sources, StorageFile destination, CompressionLevel compressionLevel, string[] customExts=null)
        {
            log = new List<string>();

            _processedFilesCount = 0;
            using (Stream stream = await destination.OpenStreamForWriteAsync())
            {
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    foreach (var item in sources)
                    {
                        curRoot = item.Name;
                        await AddFolderToArchive(item, archive, item.Name + "/", compressionLevel, customExts);
                    }
                }
            }
        }

        /// <summary>
        /// Compresses a single file.
        /// </summary>
        /// <param name="source">The file to compress.</param>
        /// <param name="destination">The compressed zip file.</param>
        public async void Compress(StorageFile source, StorageFile destination, CompressionLevel compressionLevel)
        {
            using (Stream stream = await destination.OpenStreamForWriteAsync())
            {
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    ZipArchiveEntry entry = archive.CreateEntry(source.Name, compressionLevel);

                    using (Stream data = entry.Open())
                    {
                        byte[] buffer = await ConvertToBinary(source);
                        data.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        /// <summary>
        /// Decompresses the specified file to the specified folder.
        /// </summary>
        /// <param name="source">The compressed zip file.</param>
        /// <param name="destination">The folder where the file will be decompressed.</param>
        public async Task Decompress(StorageFile source, StorageFolder destination)
        {
            log = new List<string>();
            using (Stream stream = await source.OpenStreamForReadAsync())
            {
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (!string.IsNullOrEmpty(entry.FullName))
                        {
                            if (entry.FullName.EndsWith("/"))
                            {
                                //Create empty folders too.
                                string folderName = entry.FullName.Replace("/", "\\");


                                if ((await destination.TryGetItemAsync(folderName)) == null)
                                    try
                                    {
                                        await destination.CreateFolderAsync(folderName);
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Add(ex.Message);
                                    }
                            }
                            else
                            {
                                string fileName = entry.FullName.Replace("/", "\\");

                                using (Stream entryStream = entry.Open())
                                {
                                    byte[] buffer = new byte[entry.Length];
                                    entryStream.Read(buffer, 0, buffer.Length);

                                    try
                                    {
                                        StorageFile file = await destination.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                                        using (IRandomAccessStream uncompressedFileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                                        {
                                            using (Stream data = uncompressedFileStream.AsStreamForWrite())
                                            {
                                                data.Write(buffer, 0, buffer.Length);
                                                data.Flush();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Add(ex.Message);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task DecompressSpecial(StorageFile source, Dictionary<string, StorageFolder> destinations)
        {
            log = new List<string>();
            using (Stream stream = await source.OpenStreamForReadAsync())
            {
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    int counter = 0;

                    IEnumerable<ZipArchiveEntry> notSkippedEntries = from ZipArchiveEntry z in archive.Entries
                                                                     let destStr = z.FullName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[0]
                                                                     let dest = destinations.ContainsKey(destStr) ? destinations[destStr] : null
                                                                     where dest != null
                                                                     select z;

                    int total = notSkippedEntries.Count();

                    foreach (ZipArchiveEntry entry in notSkippedEntries)
                    {
                        if (!string.IsNullOrEmpty(entry.FullName))
                        {
                            StorageFolder destination;

                            string destName = entry.FullName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[0];
                            destination = destinations[destName];

                            OnDecompressingProgress(new DecompressingEventArgs(counter, total, log, destName));

                            if (entry.FullName.EndsWith("/"))
                            {
                                //Create empty folders too.
                                string folderName = entry.FullName.Replace("/", "\\");


                                if ((await destination.TryGetItemAsync(folderName)) == null)
                                    try
                                    {
                                        await destination.CreateFolderAsync(folderName);
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Add(ex.Message);
                                    }
                            }
                            else
                            {
                                string fileName = entry.FullName.Replace("/", "\\");

                                using (Stream entryStream = entry.Open())
                                {
                                    byte[] buffer = new byte[entry.Length];
                                    entryStream.Read(buffer, 0, buffer.Length);

                                    try
                                    {
                                        StorageFile file = await destination.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);

                                        using (IRandomAccessStream uncompressedFileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                                        {
                                            using (Stream data = uncompressedFileStream.AsStreamForWrite())
                                            {
                                                data.Write(buffer, 0, buffer.Length);
                                                data.Flush();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Add(ex.Message);
                                    }
                                }
                            }
                        }
                        counter++;
                    }
                }
            }
        }

        /// <summary>
        /// Adds the specified folder, along with its files and sub-folders, to the specified archive.
        /// Creadits to Jin Yanyun
        /// http://www.rapidsnail.com/Tutorial/t/2012/116/40/23786/windows-and-development-winrt-to-zip-files-unzip-and-folder-zip-compression.aspx
        /// </summary>
        /// <param name="folder">The folder to add.</param>
        /// <param name="archive">The zip archive.</param>
        /// <param name="separator">The directory separator character.</param>
        private async Task AddFolderToArchive(StorageFolder folder, ZipArchive archive, string separator, CompressionLevel compLevel, string[] customExts=null)
        {
            bool hasFiles = false;
            foreach (StorageFile file in await folder.GetFilesAsync())
            {
                try
                {
                    try
                    {
                        if (customExts != null)
                        {
                            if (!customExts.Contains(Path.GetExtension(file.Name).ToLower()))
                            {
                                continue;
                            }
                        }
                    }catch(Exception ex)
                    {

                    }
                    ZipArchiveEntry entry = archive.CreateEntry(separator + file.Name, compLevel);

                    using (Stream stream = entry.Open())
                    {
                        byte[] buffer = await ConvertToBinary(file);
                        stream.Write(buffer, 0, buffer.Length);
                    }

                    hasFiles = true;
                }
                catch (Exception ex)
                {
                    log.Add(ex.Message);
                }
                finally
                {
                    _processedFilesCount++;
                    OnCompressingProgress(new CompressingEventArgs(_processedFilesCount, curRoot, file.Name, log));
                }
            }

            if (!hasFiles)
                archive.CreateEntry(separator + "/");

            foreach (var storageFolder in await folder.GetFoldersAsync())
            {
                await AddFolderToArchive(storageFolder, archive, separator + storageFolder.Name + "/", compLevel);
            }
        }

        /// <summary>
        /// Converts the specified file to a byte array.
        /// </summary>
        /// <param name="storageFile">The file to convert.</param>
        /// <returns>A byte array representation of the file.</returns>
        private async Task<byte[]> ConvertToBinary(StorageFile storageFile)
        {
            IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync();

            using (DataReader reader = new DataReader(stream))
            {
                byte[] buffer = new byte[stream.Size];

                await reader.LoadAsync((uint)stream.Size);
                reader.ReadBytes(buffer);

                return buffer;
            }
        }
    }

    public class ArchiverError : INotifyPropertyChanged
    {
        public string Message { get; }
        public string File { get; }

        public ArchiverError(string message, string file)
        {
            Message = message;
            File = file;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class CompressingEventArgs
    {
        public int ProcessedFilesCount { get; set; }
        public string CurrentRootFolder { get; set; }
        public string CurrentFile { get; set; }
        public List<string> Log { get; set; }

        public CompressingEventArgs(int processedFilesCount, string curRootFolder, string curFile, List<string> log)
        {
            ProcessedFilesCount = processedFilesCount;
            CurrentRootFolder = curRootFolder;
            Log = log;
            CurrentFile = curFile;
        }
    }

    public class DecompressingEventArgs
    {
        public int ProcessedEntries { get; }
        public int TotalEntries { get; }
        public double Percent { get; }
        public List<string> Log { get; set; }
        public string CurrentRootFolder { get; set; }

        public DecompressingEventArgs(int processed, int total, List<string> log, string curRoot)
        {
            ProcessedEntries = processed;
            TotalEntries = total;
            Percent = Math.Min(Math.Max((100.0 * processed) / total, 0.0), 100.0);
            Log = log;
            CurrentRootFolder = curRoot;
        }
    }
}
