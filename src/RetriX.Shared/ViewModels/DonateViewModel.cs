using MvvmCross.Core.ViewModels;
using RetriX.Shared.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetriX.Shared.ViewModels
{
    public class DonateViewModel : MvxViewModel
    {
        private IPlatformService PlatformService { get; }
        public event EventHandler eventHandler;
        public DonateViewModel()
        {
            
        }

        public override void ViewDisappearing()
        {
            eventHandler.Invoke(this, EventArgs.Empty);
        }
    }
}
