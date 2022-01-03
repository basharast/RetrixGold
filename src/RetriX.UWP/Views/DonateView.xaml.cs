
using MvvmCross.Uwp.Views;
using RetriX.Shared.Services;
using RetriX.Shared.ViewModels;
using RetriX.UWP.Services;
using System;
using System.Threading.Tasks;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RetriX.UWP.Pages
{
    public sealed partial class DonateView : MvxWindowsPage
    {
        public DonateViewModel VM => ViewModel as DonateViewModel;
        private PlatformService platformService = new PlatformService();
        public DonateView()
        {
            PlatformService.SaveGamesListStateDirect();
            this.InitializeComponent();

            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => StartDonatePage());
            
        }
        public async Task StartDonatePage()
        {
            try
            {
                //platformService.StopNotificationSound("donate.mp3");
                //platformService.PlayNotificationSound("donate.mp3");
                await Task.Delay(700);
                VM.eventHandler += ViewDisappear;
            }
            catch (Exception e)
            {
                platformService.ShowErrorMessage(e);
            }
        }
        public void ViewDisappear(object sender, EventArgs e)
        {
            try
            {
                //platformService.StopNotificationSound("donate.mp3");
            }
            catch (Exception ex)
            {
                platformService.ShowErrorMessage(ex);
            }
        }
    }
}
