using System;
using System.Collections.Generic;
using System.Text;

namespace RetriX.Shared.ViewModels
{    
    public class SystemMenuModel
    {
        public string MenuName = "";
        public string MenuIcon = "";
        public string MenuCommand = "";
        public string MenuGroup = "";
        public bool MenuSwitch=false;
        public bool MenuSwitchState=false;
        public bool MenuSwitchStateNegative=false;
        public SystemMenuModel(string menuName, string menuIcon, string menuCommand, bool menuSwitch=false, bool menuSwitchState = false)
        {
            MenuName = menuName;
            MenuIcon = menuIcon;
            MenuCommand = menuCommand;
            MenuSwitch = menuSwitch;
            MenuSwitchState = menuSwitchState;
            MenuSwitchStateNegative = !MenuSwitchState;
        }
        public void SetSwitchState(bool SwitchState)
        {
            MenuSwitchState = SwitchState;
            MenuSwitchStateNegative = !MenuSwitchState;
        }
    }
}
