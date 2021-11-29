using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RetriX.Shared.ViewModels
{

    public class GameSystemRecentModel
    {
        public string GameName;
        public string GameNameWithoutExtension;
        public int OpenCounts;
        public string GameLocation;
        public string GameSnapshot;
        public string GameSystem;
        public string GameToken;
        public bool GameRootNeeded;
        public bool NewGame;
        public bool ShowCountsBlock;
        public bool ShowTimeBlock;
        public string GameType;
        public string GamePlayedTime;
        public string GameID;
        public bool GameFailed = false;

        public GameSystemRecentModel(string gameID, string gameName, int openCounts, long playedTime,string gameLocation, string gameSnapshot, string gameSystem, bool gameRootNeeded, string gameToken="",bool newGame=false)
        {
            try
            {
                GameID = gameID;
                GameName = gameName;
                OpenCounts = openCounts;
                GameLocation = gameLocation;
                GameSnapshot = GetRomIconBySystemAndFile(gameSystem, gameName);
                if (gameSnapshot.Length > 0)
                {
                    GameSnapshot = gameSnapshot;
                }

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
                        GamePlayedTime = PlayedTimeFormat;
                    }
                }
            }catch(Exception e)
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
            }catch(Exception e)
            {
                return "";
            }
        }
        public string GetRomIconBySystemAndFile(string SystemName, string FileName)
        {
            string SystemIcon = "";
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

                case ".iso":
                case ".ccd":
                case ".bin":
                case ".cue":
                    SystemIcon = "ms-appx:///Assets/RomsIcons/CD.png";
                    break;
            }

            switch (SystemName)
            {
                case "Game Boy Micro":
                    switch (FileExtension)
                    {
                        case ".gba":
                            SystemName = "Game Boy";
                            break;
                    }
                    break;

                case "Game Boy Advance SP":
                    switch (FileExtension)
                    {
                        case ".gb":
                            SystemName = "Game Boy";
                            break;

                        case ".gbc":
                        case ".cgb":
                            SystemName = "Game Boy Color";
                            break;

                        case ".gba":
                        case ".sgb":
                            SystemName = "Game Boy Advance";
                            break;
                    }
                    break;
            }

            if (SystemIcon.Length > 0)
            {
                return SystemIcon;
            }

            SystemIcon = GameSystemViewModel.GetSystemIconByName(SystemName);
            return SystemIcon.Replace("Icons", "RomsIcons").Replace(".gif",".png");
        }
    }
}
