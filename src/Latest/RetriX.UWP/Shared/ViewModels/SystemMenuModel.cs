using RetriX.UWP.Pages;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using WinUniversalTool;

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

namespace RetriX.Shared.ViewModels
{
    public class SystemMenuModel : BindableBase
    {
        public string MenuName = "";
        public string menuIcon = "";
        public string MenuIcon
        {
            get
            {
                return menuIcon;
            }
            set
            {
                var transcodedState = GameSystemSelectionView.GetTranscodedImage(value, ref menuIcon);
                if (!transcodedState)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var transcodedImage = await GameSystemSelectionView.ConvertPngToBmp(value);
                            if (!transcodedImage.Equals(menuIcon))
                            {
                                menuIcon = transcodedImage;
                                RaisePropertyChanged(menuIcon);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                }
            }
        }
        public string MenuCommand = "";
        public string MenuGroup = "";
        public string StateText
        {
            get
            {
                if (!isEnabled)
                {
                    return "...";
                }
                if (MenuSwitchState)
                {
                    return "ON";
                }
                else
                {
                    return "OFF";
                }
            }
        }
        public bool MenuSwitch = false;
        public bool HideMenuAfterClick = true;
        private bool enabled = true;
        public bool isEnabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
                try
                {
                    RaisePropertyChanged(nameof(isEnabledOpacity));
                    RaisePropertyChanged(nameof(StateText));
                }
                catch (Exception ex)
                {

                }
            }
        }
        public bool reloadRequired = false;
        public double isEnabledOpacity
        {
            get
            {
                return isEnabled ? 1.0 : 0.6;
            }
        }

        bool menuSwitchState = false;

        public bool MenuSwitchState
        {
            get
            {
                return menuSwitchState;
            }
            set
            {
                menuSwitchState = value;
                RaisePropertyChanged(nameof(MenuSwitchState));
                RaisePropertyChanged(nameof(colorBrush));
                RaisePropertyChanged(nameof(StateText));
            }
        }
        public SolidColorBrush colorBrush
        {
            get
            {
                if (!isEnabled)
                {
                    return new SolidColorBrush(Colors.Gray);
                }
                if (MenuSwitchState)
                {
                    return new SolidColorBrush(Colors.Green);
                }
                else
                {
                    return new SolidColorBrush(Colors.DarkRed);
                }
            }
        }

        public int IconSize = 60;

        public SystemMenuModel(string menuName, string menuIcon, string menuCommand, bool menuSwitch = false, bool menuSwitchState = false)
        {
            MenuName = menuName;
            MenuIcon = menuIcon;
            MenuCommand = menuCommand;
            MenuSwitch = menuSwitch;
            MenuSwitchState = menuSwitchState;
            RaisePropertyChanged(nameof(MenuSwitch));
            RaisePropertyChanged(nameof(colorBrush));
            RaisePropertyChanged(nameof(StateText));
            IconSize = 50;
        }
        public void SetSwitchState(bool SwitchState)
        {
            MenuSwitchState = SwitchState;
        }
    }
}
