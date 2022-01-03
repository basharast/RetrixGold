using MvvmCross.Uwp.Views;
using RetriX.Shared.ViewModels;
using RetriX.UWP.Services;

namespace RetriX.UWP.Pages
{
    public sealed partial class AboutView : MvxWindowsPage
    {
        public AboutViewModel VM => ViewModel as AboutViewModel;

        public AboutView()
        {
            PlatformService.SaveGamesListStateDirect();
            this.InitializeComponent();
        }
    }
}
