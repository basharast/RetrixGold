using MvvmCross.Core.ViewModels;
using Plugin.VersionTracking.Abstractions;
using RetriX.Shared.Services;

namespace RetriX.Shared.ViewModels
{
    public class AboutViewModel : MvxViewModel
    {
        public string Version { get; }
        private IPlatformService PlatformService { get; }
        public AboutViewModel(IVersionTracking versionTracker)
        {
            Version = versionTracker.CurrentVersion;
        }
    }
}