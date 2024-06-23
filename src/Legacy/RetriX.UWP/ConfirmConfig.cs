using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetriX.UWP
{
    public class ConfirmConfig
    {
        public string title;
        public string message;
        public string OkText;
        public string CancelText;
        public bool yesNo = false;
        public void SetTitle(string title)
        {
            this.title = title;
        }

        public void SetOkText(string OkText)
        {
            this.OkText = OkText;
        }

        public void SetCancelText(string CancelText)
        {
            this.CancelText = CancelText;
        }

        public void SetMessage(string message)
        {
            this.message = message;
        }
        public void UseYesNo()
        {
            yesNo = true;
        }
    }
    public static class UserDialogs
    {
        public static class Instance
        {
            public static async Task<bool> ConfirmAsync(ConfirmConfig confirmConfig)
            {
                if (confirmConfig.OkText == null)
                {
                    confirmConfig.OkText = !confirmConfig.yesNo ? "Close" : "Yes";
                }
                if (confirmConfig.CancelText == null)
                {
                    confirmConfig.CancelText = "Cancel";
                }
                var buttons = new string[] { confirmConfig.OkText, confirmConfig.CancelText };
                if (!confirmConfig.yesNo)
                {
                    buttons = new string[] { confirmConfig.OkText };
                }
                var PromptDialog = Helpers.CreateDialog(confirmConfig.title, confirmConfig.message, buttons);
                var result = await PromptDialog.ShowAsync2();
                return Helpers.DialogResultCheck(PromptDialog, 2);
            }
            public static async Task AlertAsync(string message, string title, string okText)
            {
                await PlatformService.ShowMessageWithTitleDirect(message, title, okText);
            }
            public static async Task AlertAsync(string message, string title)
            {
                await PlatformService.ShowMessageWithTitleDirect(message, title);
            }
            public static async Task AlertAsync(string message)
            {
                await PlatformService.ShowMessageDirect(message);
            }
        }
    }
}
