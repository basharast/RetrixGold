using Newtonsoft.Json;
using RetriX.UWP.Pages;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
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

    public class GameSystemRecentModel : BindableBase
    {
        public string GameName;
        public string GameParent;
        public string GameNameWithoutExtension;
        public int OpenCounts;
        public string GameLocation;
        private string gameSnapshot;
        public TextWrapping textWrapping
        {
            get
            {
                if(GamePlayedTime!=null && GamePlayedTime.Trim().Length > 0)
                {
                    return TextWrapping.NoWrap;
                }
                else
                {
                    return TextWrapping.WrapWholeWords;
                }
            }
        }
        public string GameSnapshot
        {
            get
            {
                if (gameSnapshot != null && gameSnapshot.Contains("Assets\\Icons"))
                {
                    try
                    {
                        var fileName = Path.GetFileName(gameSnapshot);
                        return $"ms-appx:///Assets/Icons/{fileName}";
                    }
                    catch(Exception ex)
                    {

                    }
                }else if (gameSnapshot != null && gameSnapshot.Contains("Assets\\RomsIcons"))
                {
                    try
                    {
                        var fileName = Path.GetFileName(gameSnapshot);
                        return $"ms-appx:///Assets/RomsIcons/{fileName}";
                    }
                    catch (Exception ex)
                    {

                    }
                }
                return gameSnapshot;
            }
            set
            {
                var transcodedState = GameSystemSelectionView.GetTranscodedImage(value, ref gameSnapshot);
                if (!transcodedState)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var transcodedImage = await GameSystemSelectionView.ConvertPngToBmp(value);
                            if (!transcodedImage.Equals(gameSnapshot))
                            {
                                gameSnapshot = transcodedImage;
                                RaisePropertyChanged(GameSnapshot);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                }
            }
        }
        public string GameSystem;
        public string GameCore;
        public string GameToken;
        public bool GameRootNeeded;
        public bool NewGame;
        public bool ShowCountsBlock;
        public bool ShowTimeBlock;
        public string GameType;
        public string GamePlayedTime;
        public string GameID;
        public bool GameFailed = false;

        [JsonIgnore]
        public bool thumnailsIconsState
        {
            get
            {
                return PlatformService.ShowThumbNailsIcons;
            }
        }

        [JsonIgnore]
        public Visibility progressState = Visibility.Collapsed;

        [JsonIgnore]
        public Visibility ProgressState
        {
            get
            {
                return progressState;
            }
            set
            {
                progressState = value;
                RaisePropertyChanged(nameof(ProgressState));
                RaisePropertyChanged(nameof(ProgressActive));
            }
        }

        [JsonIgnore]
        public bool ProgressActive
        {
            get
            {
                if (progressState == Visibility.Visible)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [JsonIgnore]
        public bool thumnailsIconsState2
        {
            get
            {
                return !PlatformService.ShowThumbNailsIcons;
            }
        }

        public void RaiseProgressState()
        {
            try
            {
                RaisePropertyChanged(nameof(ProgressState));
                RaisePropertyChanged(nameof(ProgressActive));
            }
            catch (Exception ex)
            {

            }
        }

        [JsonIgnore]
        public StorageFile attachedFile = null;

        [JsonIgnore]
        public Thickness margin
        {
            get
            {
                if (PlatformService.ShowThumbNailsIcons)
                {
                    return new Thickness(10, 0, 0, 0);
                }
                else
                {
                    return new Thickness(0, 0, 0, 0);
                }
            }
        }

        public void updateSnapShotBinding()
        {
            if (GameSnapshot.Length == 0)
            {
                GameSnapshot = GetRomIconBySystemAndFile(GameSystem, GameName);
            }
            RaisePropertyChanged(nameof(GameSnapshot));
            RaisePropertyChanged(nameof(thumnailsIconsState));
            RaisePropertyChanged(nameof(textWrapping));
        }

        [JsonIgnore]
        Color[] colors = new Color[] { Colors.DodgerBlue, Colors.Orange, Colors.Gray, Colors.Tomato, Colors.Violet, Colors.DarkGreen, Colors.CornflowerBlue, Colors.RoyalBlue, Colors.DodgerBlue, Colors.Orange, Colors.Gray, Colors.Tomato, Colors.Violet, Colors.DarkGreen, Colors.CornflowerBlue, Colors.RoyalBlue, Colors.DodgerBlue, Colors.Orange, Colors.Gray, Colors.Tomato, Colors.Violet, Colors.DarkGreen, Colors.CornflowerBlue, Colors.RoyalBlue, Colors.DodgerBlue, Colors.Orange, Colors.Gray, Colors.Tomato, Colors.Violet, Colors.DarkGreen, Colors.CornflowerBlue, Colors.RoyalBlue };

        [JsonIgnore]
        Dictionary<string, Color> extenstionsColors = new Dictionary<string, Color>()
        {
            {"7z" , Colors.Gold },
            {"zip" , Colors.Brown },
            {"rar" , Colors.Purple },
            {"exe" , Colors.DodgerBlue },
            {"img" , Colors.Gray },
        };

        [JsonIgnore]
        public SolidColorBrush GameColor
        {
            get
            {
                var color = Colors.Gray;
                try
                {
                    if (!extenstionsColors.TryGetValue(GameType.ToLower(), out color))
                    {
                        var RandomIndex = new Random().Next(0, 30);
                        color = colors[RandomIndex];
                    }
                }
                catch (Exception ex)
                {

                }
                return new SolidColorBrush(color);
            }
        }

        [JsonIgnore]
        public Stretch StretchState
        {
            get
            {
                if (GameSnapshot?.Length > 0 && !GameSnapshot.StartsWith("ms-appx:///") && !GameSnapshot.Contains("\\Assets\\"))
                {
                    return Stretch.UniformToFill;
                }
                else
                {
                    return Stretch.Uniform;
                }
            }
        }

        public GameSystemRecentModel(string gameID, string gameName, int openCounts, long playedTime, string gameLocation, string gameSnapshot, string gameSystem, bool gameRootNeeded, string gameToken = "", bool newGame = false, bool gamesList = false, string gameCore = "")
        {
            try
            {
                GameID = gameID;
                GameName = gameName;
                OpenCounts = openCounts;
                GameLocation = gameLocation;
                GameCore = gameCore;

                RaisePropertyChanged(nameof(thumnailsIconsState));
                if (!gameID.Equals("dummy"))
                {
                    try
                    {
                        GameParent = Path.GetFileName(Path.GetDirectoryName(gameLocation));
                    }
                    catch (Exception ex)
                    {

                    }
                    GameSnapshot = GetRomIconBySystemAndFile(gameSystem, gameName);
                    if (gameSnapshot?.Length > 0)
                    {
                        GameSnapshot = gameSnapshot;
                    }
                    /*else
                    {
                       StretchState = Stretch.Uniform;
                    }*/

                    GameSystem = gameSystem;
                    GameToken = gameToken;
                    GameRootNeeded = gameRootNeeded;
                    NewGame = newGame;
                    ShowCountsBlock = openCounts > 0;
                    ShowTimeBlock = playedTime > 0;
                    if (playedTime > 0)
                    {
                        var PlayedTimeFormat = FormatTotalPlayedTime(playedTime);
                        if (PlayedTimeFormat.Length > 0)
                        {
                            GamePlayedTime = gamesList ? $"{PlayedTimeFormat} ({OpenCounts})" : PlayedTimeFormat;
                        }
                    }
                    RaisePropertyChanged(nameof(textWrapping));
                }
            }
            catch (Exception e)
            {
                GameFailed = true;
            }
        }

        public static string FormatTotalPlayedTime(long playedTime)
        {
            try
            {
                TimeSpan time = TimeSpan.FromMilliseconds(playedTime);
                string GamePlayedTime = $"{(time.Hours < 10 ? "0" : "")}{time.Hours} : {(time.Minutes < 10 ? "0" : "")}{time.Minutes} : {(time.Seconds < 10 ? "0" : "")}{time.Seconds}";
                return GamePlayedTime;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public static string[] FormatTotalPlayedTimePrettyPrint(long playedTime)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(playedTime);
            var finalPrint = "";
            var subPrint = "";
            var RequestHours = false;
            var RequestMinutes = false;
            var RequestSeconds = false;
            if (timeSpan.Hours > 0)
            {
                RequestHours = true;
            }
            if (timeSpan.Minutes > 0)
            {
                RequestMinutes = true;
            }
            if (timeSpan.Seconds > 0)
            {
                RequestSeconds = true;
            }
            if (RequestHours)
            {
                finalPrint = $"{timeSpan.Hours}H";
                if (RequestMinutes)
                {
                    if (RequestSeconds)
                    {
                        subPrint = $"+{timeSpan.Minutes}M, {timeSpan.Seconds}S";
                    }
                    else
                    {
                        subPrint = $"+{timeSpan.Minutes} Minute";
                    }
                }
                else
                {
                    if (RequestSeconds)
                    {
                        subPrint = $"+{timeSpan.Seconds} Second";
                    }
                }
            }
            else if (RequestMinutes)
            {
                finalPrint = $"{timeSpan.Minutes}M";
                if (RequestSeconds)
                {
                    subPrint = $"+{timeSpan.Seconds} Second";
                }
            }
            else if (RequestSeconds)
            {
                finalPrint = $"{timeSpan.Seconds}S";
            }
            return new string[] { finalPrint, subPrint };
        }
        public string GetRomIconBySystemAndFile(string SystemName, string FileName)
        {
            string SystemIcon = "";
            if (FileName != null)
            {
                string FileExtension = Path.GetExtension(FileName).ToLower();
                GameNameWithoutExtension = GameName.Replace(FileExtension, "");
                GameType = FileExtension.Replace(".", "").ToUpper();

                switch (FileExtension)
                {
                    case ".zip":
                        SystemIcon = "ms-appx:///Assets/RomsIcons/Zip.png";
                        break;

                    case ".7z":
                        SystemIcon = "ms-appx:///Assets/RomsIcons/7Zip.png";
                        break;

                    case ".rar":
                        SystemIcon = "ms-appx:///Assets/RomsIcons/RAR.png";
                        break;

                    case ".gz":
                    case ".tar":
                        SystemIcon = "ms-appx:///Assets/RomsIcons/GZ.png";
                        break;

                    case ".iso":
                    case ".ccd":
                    case ".bin":
                    case ".cue":
                        SystemIcon = "ms-appx:///Assets/RomsIcons/CD.png";
                        break;
                }

                switch (SystemName)
                {
                    case "GameBoy Micro":
                        switch (FileExtension)
                        {
                            case ".gba":
                                SystemName = "GameBoy";
                                break;
                        }
                        break;

                    case "Game Boy Advance SP":
                    case "GameBoy+":
                        switch (FileExtension)
                        {
                            case ".gb":
                                SystemName = "GameBoy";
                                break;

                            case ".gbc":
                            case ".cgb":
                                SystemName = "GameBoy Color";
                                break;

                            case ".gba":
                            case ".sgb":
                                SystemName = "GameBoy Advance";
                                break;
                        }
                        break;
                }
            }
            if (SystemIcon.Length > 0)
            {
                return SystemIcon;
            }

            SystemIcon = GameSystemViewModel.GetSystemIconByName(SystemName, GameCore);
            return SystemIcon.Replace("Icons", "RomsIcons").Replace(".gif", ".png");
        }
    }
}
