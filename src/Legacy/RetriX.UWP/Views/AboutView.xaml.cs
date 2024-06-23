using RetriX.UWP.Services;
using Windows.UI.Xaml.Controls;

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

namespace RetriX.UWP.Pages
{
    public sealed partial class AboutView : Page
    { 
        public AboutView()
        {
            PlatformService.SaveGamesListStateDirect();
            this.InitializeComponent();
        }

        private void WebView_LoadCompleted(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            
        }

        private void WebView_ContentLoading(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewContentLoadingEventArgs args)
        {
            WeLoadingProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            WeLoadingProgressRing.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void WebView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void WebView_DOMContentLoaded(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewDOMContentLoadedEventArgs args)
        {
            WeLoadingProgress.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            WeLoadingProgressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
