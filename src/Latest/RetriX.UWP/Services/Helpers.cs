using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http.Headers;
using WinUniversalTool.Models;
using WinUniversalTool.Views;

namespace RetriX.UWP.Services
{
    public static class Helpers
    {
        public static string URLPattern = @"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.(?<ext>[a-zA-Z0-9()]{1,6}\b)([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*)";
        public static string URLPatternWithoutHttp = @"[-a-zA-Z0-9@:%._\+~#=]{1,256}\.(?<ext>[a-zA-Z0-9()]{1,6}\b)([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*)$";
        public static string IPMatch = @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";
        public static string YoutubeFileName = @"(?<fileName>videoplayback).*(mime=video%2F(?<type>\w+))\&";
        public static string YoutubeFileType = @"(?<fileName>\w+).*(mime=video%2F(?<type>\w+))\&";
        public static string GeneralFileName = @"(?<fileName>[\w+\s+_\d+.\-()!@#$%^&_+=';~]+)\?.*";
        public static string BasicFileName = @"(?<fileName>[\w+\s+_\d+.\-()!@#$%^&_+=';~]+)\.\w+";
        public static string VerifyFileName = @"^[\w+\s+_\d+.\-()!@#$%^&_+=';~]+\.\w+$";
        public static string TypeSelector = @".*(?<type>\.\w+)$";
        public static string YoutubeShort = @"youtu\.be\/(?<id>.*)";
        
        public static bool isAppVisible = true;
        public static string AppTheme = "System";
        public static bool DialogInProgress = false;
        public static double DialogMinSizeTemp = 100;

        public static ContentDialogCustom CreateDialog(string title, string message, string[] buttons)
        {
            ContentDialogCustom customDialog = null;
            message = message.Trim();
            if (buttons.Length < 2)
            {
                customDialog = new ContentDialogCustom(title, message, "", "", buttons[0], false, null, null, buttons[0]);
                ((DialogsSettings)Application.Current.Resources["DialogSizeControl"]).DialogMinSize = 120;
            }
            else
            if (buttons.Length < 3)
            {
                customDialog = new ContentDialogCustom(title, message, buttons[0], "", buttons[1], false, buttons[0], null, buttons[1]);
                ((DialogsSettings)Application.Current.Resources["DialogSizeControl"]).DialogMinSize = 100;
            }
            else
            {
                customDialog = new ContentDialogCustom(title, message, buttons[0], buttons[1], buttons[2], false, buttons[0], buttons[1], buttons[2]);
                ((DialogsSettings)Application.Current.Resources["DialogSizeControl"]).DialogMinSize = DialogMinSizeTemp;
            }

            return customDialog;
        }
        public static bool DialogResultCheck(ContentDialogCustom uICommand, int TargetCommand)
        {
            if (uICommand != null)
            {
                return (int)uICommand.Result == TargetCommand;
            }
            else
            {
                return false;
            }
        }

        public static async Task ShowMessage(string title, string message)
        {
            string DialogTitle = $"{title}";
            string DialogMessage = $"{message}";
            string[] DialogButtons = new string[] { "Close" };
            var QuestionDialog = CreateDialog(DialogTitle, DialogMessage, DialogButtons);
            var QuestionResult = await QuestionDialog.ShowAsync2();
        }
        public static async Task ShowMessage(string title, string message, string okButoon)
        {
            string DialogTitle = $"{title}";
            string DialogMessage = $"{message}";
            string[] DialogButtons = new string[] { okButoon };
            var QuestionDialog = CreateDialog(DialogTitle, DialogMessage, DialogButtons);
            var QuestionResult = await QuestionDialog.ShowAsync2();
        }
        
        public static async Task ShowMessage(string message)
        {
            string DialogTitle = $"RetriXGold";
            string DialogMessage = $"{message}";
            string[] DialogButtons = new string[] { "Close" };
            var QuestionDialog = CreateDialog(DialogTitle, DialogMessage, DialogButtons);
            var QuestionResult = await QuestionDialog.ShowAsync2();
        }

        public static bool IsAllowedUri(string url, bool noHttpCheck = false)
        {
            try
            {
                if (url != null)
                {
                    Match mb = Regex.Match(url, URLPattern, RegexOptions.IgnoreCase);
                    if (noHttpCheck)
                    {
                        mb = Regex.Match(url, URLPatternWithoutHttp, RegexOptions.IgnoreCase);
                    }
                    if (mb.Success)
                    {
                        return true;
                    }
                    else if (url.StartsWith("blank:") || url.StartsWith("error:") || url.StartsWith("security:"))
                    {
                        return true;
                    }
                    else
                    {
                        mb = Regex.Match(url, IPMatch, RegexOptions.IgnoreCase);
                        if (mb.Success)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            return false;
        }
        public static string escapeSpecialChars(string original)
        {
            try
            {
                original = System.Net.WebUtility.UrlDecode(original);
                original = original.Replace("%20", " ");
            }
            catch (Exception ex)
            {

            }
            return original;
        }
        public static bool isLinkFile(string url, bool NoHttpCheck = true)
        {
            try
            {
                Match m = Regex.Match(url, NoHttpCheck ? BasicFileName : URLPatternWithoutHttp, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    return true;
                }
            }
            catch (Exception e)
            {

            }
            return false;
        }
        public static async Task<Windows.Web.Http.HttpResponseMessage> GetResponse(string url, CancellationToken cancellationToken, HttpCredentialsHeaderValue authenticationHeaderValue = null, bool showError = true, bool returnResponseAnyway = false)
        {
            var _client = new Windows.Web.Http.HttpClient();
            if (authenticationHeaderValue != null)
            {
                _client.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            }
            Windows.Web.Http.HttpResponseMessage response = null;
            try
            {
                url = url.Replace("wut::", "");
                if (IsAllowedUri(url))
                {
                    Uri uri = null;
                    try
                    {
                        uri = new Uri(url);
                    }
                    catch (Exception e)
                    {

                    }
                    if (uri != null)
                    {
                        response = await _client.GetAsync(uri, Windows.Web.Http.HttpCompletionOption.ResponseHeadersRead).AsTask(cancellationToken);

                        if (!response.IsSuccessStatusCode)
                        {
                            if (!returnResponseAnyway)
                            {
                                return null;
                            }
                            else
                            {
                                return response;
                            }
                        }

                        if (response.Content.Headers.ContentDisposition == null)
                        {
                            //IEnumerable<string> contentDisposition;
                            /*if (response.Content.Headers.TryGetValues("Content-Disposition", out contentDisposition))
                            {
                                response.Content.Headers.ContentDisposition = Windows.Web.Http.ContentDispositionHeaderValue.Parse(contentDisposition.ToArray()[0].TrimEnd(';').Replace("\"", ""));
                            }*/
                        }
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                if (showError)
                {
                    var HostName = "";
                    url = url.Replace("wut::", "");
                    if (IsAllowedUri(url))
                    {
                        Uri uri = null;
                        try
                        {
                            uri = new Uri(url);
                            HostName = uri.Host;
                        }
                        catch (Exception e)
                        {

                        }

                    }
                    var messageError = ex.Message;
                    if (HostName.Length > 0)
                    {
                        messageError = $"{messageError}\nHost: {HostName}";
                    }
                    try
                    {
                        messageError = String.Join("\n", messageError.Split('\n').Distinct(StringComparer.CurrentCultureIgnoreCase));
                        messageError = messageError.Replace("\n\r\n\r", "\n\r");
                        messageError = messageError.Replace("\n\n", "\n");
                    }
                    catch (Exception exx)
                    {

                    }
                    try
                    {
                        PlatformService.PlayNotificationSoundDirect("error");
                    }
                    catch (Exception ee)
                    {

                    }
                    LocalNotificationData localNotificationData = new LocalNotificationData();
                    localNotificationData.icon = SegoeMDL2Assets.Error;
                    localNotificationData.message = messageError;
                    localNotificationData.time = 7;
                    PlatformService.NotificationHandlerMain(null, localNotificationData);

                }
            }
            return response;
        }

        static bool ConnectionErrorAppeard = false;
        public static bool CheckInternetConnection(EventHandler onDisconnect = null)
        {
            try
            {
                bool isNetworkConnected = NetworkInterface.GetIsNetworkAvailable();
                if (isNetworkConnected)
                {
                    //Check WIFI
                    ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
                    bool isWLANConnection = (InternetConnectionProfile == null) ? false : InternetConnectionProfile.IsWlanConnectionProfile;
                    if (isWLANConnection)
                    {
                        return true;
                    }
                    else
                    {
                        ConnectionProfile InternetConnectionMobile = NetworkInformation.GetInternetConnectionProfile();
                        bool isMobileConnection = (InternetConnectionMobile == null) ? false : InternetConnectionMobile.IsWwanConnectionProfile;
                        if (isMobileConnection)
                        {
                            return true;
                        }
                        else
                        {
                            //LAN
                            ConnectionProfile LANConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
                            if (LANConnectionProfile != null)
                            {
                                return true;
                            }
                            else
                            {
                                ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
                                bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
                                if (internet)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!ConnectionErrorAppeard)
                {
                    PlatformService.ShowErrorMessageDirect(e);
                    ConnectionErrorAppeard = true;
                }
            }
            if (onDisconnect != null)
            {
                onDisconnect.Invoke(null, EventArgs.Empty);
            }
            return false;
        }
        public static object HexToColor(string hexColor)
        {
            //Remove # if present
            if (hexColor.IndexOf('#') != -1)
            {
                hexColor = hexColor.Replace("#", "");
            }

            if (hexColor.Length == 6)
            {
                hexColor = "FF" + hexColor;
            }

            //100 % — FF  //50 % — 80
            //95 % — F2  //45 % — 73
            //90 % — E6  //40 % — 66
            //85 % — D9  //30 % — 4D
            //80 % — CC     //25 % — 40
            //75 % — BF  //20 % — 33
            //70 % — B3  //15 % — 26
            //65 % — A6  //10 % — 1A
            //60 % — 99  //5 % — 0D
            //55 % — 8C  //0 % — 00

            byte alpha = 0;
            byte red = 0;
            byte green = 0;
            byte blue = 0;

            if (hexColor.Length == 8)
            {
                //#AARRGGBB
                alpha = byte.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                red = byte.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                green = byte.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);
                blue = byte.Parse(hexColor.Substring(6, 2), NumberStyles.AllowHexSpecifier);
            }
            return Windows.UI.Color.FromArgb(alpha, red, green, blue);
        }

        public static void ChangeDialogBackgroudn(ContentDialog dialog)
        {
            try
            {
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
                {
                    var uiSettings = new Windows.UI.ViewManagement.UISettings();
                    var color = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);
                    Windows.UI.Color themeColor = color;
                    switch (Helpers.AppTheme)
                    {
                        case "System":
                            if (color == Windows.UI.Colors.Black)
                            {
                                themeColor = Windows.UI.Colors.Black;
                            }
                            else
                            {
                                themeColor = Windows.UI.Colors.White;
                            }
                            break;

                        case "Dark":
                            themeColor = Windows.UI.Colors.Black;
                            break;

                        case "Light":
                            themeColor = Windows.UI.Colors.White;
                            break;
                    }
                    Windows.UI.Xaml.Media.AcrylicBrush myBrush = new Windows.UI.Xaml.Media.AcrylicBrush();
                    myBrush.BackgroundSource = Windows.UI.Xaml.Media.AcrylicBackgroundSource.HostBackdrop;
                    myBrush.TintColor = themeColor;
                    myBrush.FallbackColor = themeColor;
                    myBrush.TintOpacity = 1;
                    dialog.Background = myBrush;
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
