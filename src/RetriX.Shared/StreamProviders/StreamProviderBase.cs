using RetriX.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RetriX.Shared.StreamProviders
{
    public abstract class StreamProviderBase : IStreamProvider
    {
        public abstract Task<IEnumerable<string>> ListEntriesAsync();
        protected abstract Task<Stream> OpenFileStreamAsyncInternal(string path, FileAccess accessType);
        private IPlatformService PlatformService { get; }

        public virtual void Dispose()
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
        public async Task<Stream> OpenFileStreamAsync(string path, FileAccess accessType)
        {
            try
            {
                var stream = await OpenFileStreamAsyncInternal(path, accessType);
               
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

        public void CloseStream(Stream stream)
        {
            try
            {
                stream.Dispose();
                stream = null;
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
