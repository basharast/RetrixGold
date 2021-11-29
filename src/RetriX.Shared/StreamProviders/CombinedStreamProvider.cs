using RetriX.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RetriX.Shared.StreamProviders
{
    public class CombinedStreamProvider : IStreamProvider
    {
        private readonly ISet<IStreamProvider> Providers;
        private IPlatformService PlatformService { get; }

        public CombinedStreamProvider(ISet<IStreamProvider> providers)
        {
            Providers = providers;
        }

        public void Dispose()
        {
            try
            {
                foreach (var i in Providers)
                {
                    i.Dispose();
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

        public async Task<IEnumerable<string>> ListEntriesAsync()
        {
            try
            {
                var tasks = Providers.Select(d => d.ListEntriesAsync()).ToArray();
                var results = await Task.WhenAll(tasks);
                var output = results.SelectMany(d => d.ToArray()).OrderBy(d => d).ToArray();
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

        public async Task<Stream> OpenFileStreamAsync(string path, FileAccess accessType)
        {
            try
            {
                foreach (var i in Providers)
                {
                    var stream = await i.OpenFileStreamAsync(path, accessType);
                    if (stream != null)
                    {
                        return stream;
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
            return null;
        }

        public void CloseStream(Stream stream)
        {
            try
            {
                foreach (var i in Providers)
                {
                    i.CloseStream(stream);
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
