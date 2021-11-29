using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RetriX.Shared.StreamProviders
{
    public class SingleFileStreamProvider : StreamProviderBase
    {
        private readonly string Path;
        private readonly IFileInfo File;
        private IPlatformService PlatformService { get; }

        public SingleFileStreamProvider(string path, IFileInfo file)
        {
            Path = path;
            File = file;
        }
        
        public override Task<IEnumerable<string>> ListEntriesAsync()
        {
            try
            {
                var output = new string[] { Path };
                return Task.FromResult(output as IEnumerable<string>);
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

        protected override Task<Stream> OpenFileStreamAsyncInternal(string path, FileAccess accessType)
        {
            try
            {
                if (Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    return File.OpenAsync(accessType);
                }

                return Task.FromResult(default(Stream));
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
    }
}
