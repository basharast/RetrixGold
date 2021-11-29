using Acr.UserDialogs;
using RetriX.Shared.ViewModels;
using RetriX.UWP.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RetriX.UWP.Controls
{
    public sealed partial class ActionsPack : UserControl
    {
       
        public ActionsPack()
        {
            this.InitializeComponent();
        }

        public GamePlayerViewModel ViewModel
        {
            get { return (GamePlayerViewModel)GetValue(VMProperty); }
            set { SetValue(VMProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpButtonInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty = DependencyProperty.Register(nameof(ViewModel), typeof(GamePlayerViewModel), typeof(PlayerOverlay), new PropertyMetadata(null));


        private async void Action1Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            if (ViewModel.ActionsToSave) {
                ViewModel.SaveStateSlot1.Execute();
            }
            else
            {
                await ViewModel.ExcuteActionsAsync(1);
            }
        }

        private async void Action2Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            if(ViewModel.ActionsToSave) {
                ViewModel.SaveStateSlot2.Execute();
            }
            else
            {
                await ViewModel.ExcuteActionsAsync(2);
            }
        }

        private async void Action3Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            if (ViewModel.ActionsToSave)
            {
                ViewModel.SaveStateSlot3.Execute();
            }
            else
            {
                await ViewModel.ExcuteActionsAsync(3);
            }
        }

        private async void OptionsInfo_Click(object sender, RoutedEventArgs e)
        {
            PlatformService.PlayNotificationSoundDirect("notice.mp3");
            await UserDialogs.Instance.AlertAsync("These options only for actions\n\nSwap\nSwap between directions (Left / Right)\n\nLeft -> Right\nRight -> Left\n\nSlots\nUse action buttons as save state instead of actions");
        }

        private void Action1Button_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            if (ViewModel.ActionsToSave)
            {
                ViewModel.LoadStateSlot1.Execute();
            }
        }

        private void Action2Button_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            if (ViewModel.ActionsToSave)
            {
                ViewModel.LoadStateSlot2.Execute();
            }
        }

        private void Action3Button_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            if (ViewModel.ActionsToSave)
            {
                ViewModel.LoadStateSlot3.Execute();
            }
        }
    }
}
