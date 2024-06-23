using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using static System.Net.WebRequestMethods;

namespace FilesManagement
{
    static class CopyFile
    {
        public static async Task<bool> CopyFileWithProgress(StorageFile src, StorageFolder folder, IProgress<double> progress)
        {
            try
            {
                StorageFile dest = null;
                var sourceSize = await getFileSize(src);

                //In some cases if the file very small like 1k it will fail with 'CopyFileWithProgress'
                //so it's better to handle the small files with 'CopyAsync'
                if (sourceSize < 1000)
                {
                    await src.CopyAsync(folder, src.Name, NameCollisionOption.ReplaceExisting);
                }
                else
                {
                    CachedFileManager.DeferUpdates(dest);
                    using (var destStream = await dest.OpenAsync(FileAccessMode.ReadWrite))
                    using (var srcStream = await src.OpenAsync(FileAccessMode.Read))
                    {
                        await TransferTo(srcStream.AsStream(), destStream.AsStream(), src, progress);
                    }
                    Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(dest);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private static async Task<long> TransferTo(Stream source, Stream destination, StorageFile file, IProgress<double> progress)
        {
            byte[] array = GetTransferByteArray();
            try
            {
                var fileSize = (long)(await file.GetBasicPropertiesAsync()).Size;
                var iterations = 0;
                long total = 0;
                while (ReadTransferBlock(source, array, out int count))
                {
                    total += count;
                    await destination.WriteAsync(array, 0, count);
                    iterations++;
                    if (progress != null)
                    {
                        var totalCopied = (total * 1d) / (fileSize * 1d) * 100;
                        progress.Report(totalCopied);
                    }
                }
                return total;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
        private static byte[] GetTransferByteArray()
        {
            return ArrayPool<byte>.Shared.Rent(81920);
        }
        private static bool ReadTransferBlock(Stream source, byte[] array, out int count)
        {
            return (count = source.Read(array, 0, array.Length)) != 0;
        }
        public static async Task<long> getFileSize(StorageFile storageFile)
        {
            long fileSize = (long)new Random().Next();
            try
            {
                BasicProperties fileAttr = null;
                try
                {
                    fileAttr = await storageFile.GetBasicPropertiesAsync();
                }
                catch (Exception ex)
                {
                    var state = await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, () =>
                    {
                        try
                        {
                            fileAttr = storageFile.GetBasicPropertiesAsync().AsTask().Result;
                        }
                        catch (Exception ecx)
                        {

                        }
                    });
                }

                if (fileAttr != null)
                {
                    fileSize = (long)fileAttr.Size;
                }
            }
            catch (Exception e)
            {
            }

            return fileSize;
        }
    }
}
