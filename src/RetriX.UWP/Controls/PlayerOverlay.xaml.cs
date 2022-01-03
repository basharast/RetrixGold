using RetriX.Shared.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace RetriX.UWP.Controls
{
    public sealed partial class PlayerOverlay : UserControl
    {
        public GamePlayerViewModel ViewModel
        {
            get { return (GamePlayerViewModel)GetValue(VMProperty); }
            set { SetValue(VMProperty, value); }
        }
        public VirtualPadActions VA { get; } = new VirtualPadActions();
        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty = DependencyProperty.Register(nameof(ViewModel), typeof(GamePlayerViewModel), typeof(PlayerOverlay), new PropertyMetadata(null));

        public PlayerOverlay()
        {
            InitializeComponent();
        }

        private void OnEffectClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            FlyoutBase.ShowAttachedFlyout(element);
        }

        private void MenuFlyout_Closed(object sender, object e)
        {

        }

        private void MenuFlyout_Closing(FlyoutBase sender, FlyoutBaseClosingEventArgs args)
        {
           //args.Cancel = true;
        }
    }
}
