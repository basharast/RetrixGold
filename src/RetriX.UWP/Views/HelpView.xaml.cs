﻿using MvvmCross.Uwp.Views;
using RetriX.Shared.ViewModels;
using RetriX.UWP.Services;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RetriX.UWP.Pages
{
    public sealed partial class HelpView : MvxWindowsPage
    {
        public HelpViewModel VM => ViewModel as HelpViewModel;
        public HelpView()
        {
            PlatformService.SaveGamesListStateDirect();
            this.InitializeComponent();
        }
    }
}
