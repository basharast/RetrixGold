using MvvmCross.Core.ViewModels;
using RetriX.Shared.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetriX.Shared.ViewModels
{
    public class HelpViewModel : MvxViewModel
    {
        private IPlatformService PlatformService { get; }
        public HelpViewModel()
        {

        }
    }
}
