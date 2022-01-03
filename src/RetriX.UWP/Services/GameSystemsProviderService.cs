using LibRetriX;
using Plugin.FileSystem;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Services;
using RetriX.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace RetriX.UWP.Services
{
    public class GameSystemsProviderService : GameSystemsProviderServiceBase
    {
        PlatformService platformService = new PlatformService();
        public GameSystemsProviderService(IFileSystem fileSystem) : base(fileSystem)
        {

        }
        private void AddConsoles()
        {
            AddNewConsole("Arcade");
            if (!isX64())
            {
                AddNewConsole("TIC-80");
                AddNewConsole("Pokemon Mini");
            }
            AddNewConsole("NES");
            AddNewConsole("Super Nintendo");
            AddNewConsole("Virtual Boy");
            AddNewConsole("Game Boy");
            AddNewConsole("Game Boy Color");
            AddNewConsole("Game Boy Advance");
            //AddNewConsole("GameBoy (TGB Dual)");
            AddNewConsole("Game Boy Micro");
            AddNewConsole("Nintendo DS");
            AddNewConsole("Nintendo 64");
            //AddNewConsole("PocketCDG");
            //AddNewConsole("DOSBox");
            AddNewConsole("SG-1000");
            AddNewConsole("Master System");
            AddNewConsole("Game Gear");
            AddNewConsole("Mega Drive");
            AddNewConsole("Mega CD");
            AddNewConsole("Saturn");
            //AddNewConsole("Saturn (Yabause)");
            //AddNewConsole("SEGA Wide");
            AddNewConsole("PlayStation");
            AddNewConsole("PlayStation Old");
            //AddNewConsole("PlayStation Portable"); //Later Publish
            AddNewConsole("PC Engine");
            AddNewConsole("PC Engine CD");
            AddNewConsole("PC-FX");
            //AddNewConsole("3DO");
            AddNewConsole("WonderSwan Color");
            AddNewConsole("Lynx");
            //AddNewConsole("Lynx (Handy)");
            AddNewConsole("Jaguar");
            //AddNewConsole("Atari 2600");
            //AddNewConsole("Atari 5200");
            //AddNewConsole("Atari7800");
            //AddNewConsole("LowresNX");
            AddNewConsole("Neo Geo Pocket Color");
            //AddNewConsole("Neo Geo Pocket (RACE)");
            AddNewConsole("Neo Geo");
            AddNewConsole("Vectrex");
            //AddNewConsole("MSX");
            if (!isX64())
            {
                AddNewConsole("Watara Supervision");
                //AddNewConsole("ColecoVision");
                AddNewConsole("Fairchild ChannelF");
            }
            //AddNewConsole("Magnavox Odyssey 2");
            //AddNewConsole("PC 8000-8800");
            //AddNewConsole("Game Music");
            AddNewConsole("PolyGame Master");
            AddNewConsole("32X");
            AddNewConsole("NES (Nestopia)");
            if (!isX64())
            {
                AddNewConsole("NES (QuickNES)");
            }
            //AddNewConsole("Cannonball");
            //AddNewConsole("Doom");
            //AddNewConsole("REminiscence Flashback");
            //AddNewConsole("Quake");
            if (isARM())
            {
                //AddNewConsole("Rick Dangerous");
            }
        }
        Dictionary<string, int> ConsolesSequence = new Dictionary<string, int>();
        public static bool ShowErrorNotification { get { return SkippedList.Count() > 0; } }
        public static ObservableCollection<string> SkippedList = new ObservableCollection<string>();
        public override async Task<List<GameSystemViewModel>> GenerateSystemsList(IFileSystem fileSystem)
        {
            await Task.Run(() =>
            {
                try
                {
                    AddConsoles();
                }
                catch (Exception ex)
                {

                }
            });

            List<GameSystemViewModel> gameSystemViewModels = new List<GameSystemViewModel>();
            SkippedList.Clear();
            //Generate Pinned AnyCore Console
            if (PlatformService.AnyCores != null && PlatformService.AnyCores.Count() > 0)
            {
                foreach (IFileInfo currentFile in PlatformService.AnyCores)
                {
                    try
                    {
                        var fileExtension = Path.GetExtension(currentFile.FullName);
                        if (fileExtension.ToLower().Equals(".dll"))
                        {
                            if (GameSystemSelectionViewModel.isPinnedCore(currentFile.FullName))
                            {
                                var chechCore = MonitorLoadingStart(currentFile.Name);
                                if (chechCore)
                                {
                                    SkippedList.Add($"{currentFile.Name} Skipped due compatibility issues");
                                    continue;
                                }
                                MonitorLoadingEnd(currentFile.Name, true);
                                gameSystemViewModels.Add(GameConsoleAnyCore(currentFile.FullName.Replace(".dll", ""), fileSystem));
                                MonitorLoadingEnd(currentFile.Name, false);
                            }
                        }
                    }catch(Exception ex)
                    {

                    }
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            foreach (KeyValuePair<string, int> item in ConsolesSequence.OrderByDescending(key => key.Value))
            {
                try
                {
                    var chechCore = MonitorLoadingStart(item.Key);
                    if (chechCore)
                    {
                        SkippedList.Add($"{item.Key} Skipped due compatibility issues");
                        continue;
                    }
                    MonitorLoadingEnd(item.Key, true);
                    gameSystemViewModels.Add(GameConsole(item.Key, fileSystem));
                    MonitorLoadingEnd(item.Key, false);
                }catch(Exception ex)
                {

                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            //Generate AnyCore Console
            if (PlatformService.AnyCores != null && PlatformService.AnyCores.Count() > 0)
            {
                foreach (IFileInfo currentFile in PlatformService.AnyCores)
                {
                    try
                    {
                        var fileExtension = Path.GetExtension(currentFile.FullName);
                        if (fileExtension.ToLower().Equals(".dll"))
                        {
                            if (!GameSystemSelectionViewModel.isPinnedCore(currentFile.FullName))
                            {
                                var chechCore = MonitorLoadingStart(currentFile.Name);
                                if (chechCore)
                                {
                                    SkippedList.Add($"{currentFile.Name} Skipped due compatibility issues");
                                    continue;
                                }
                                MonitorLoadingEnd(currentFile.Name, true);
                                gameSystemViewModels.Add(GameConsoleAnyCore(currentFile.FullName.Replace(".dll", ""), fileSystem));
                                MonitorLoadingEnd(currentFile.Name, false);
                            }
                        }
                    }catch(Exception ex)
                    {

                    }
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
            PlatformService.isCoresLoaded = true;
            return gameSystemViewModels;
        }
        private bool MonitorLoadingStart(string systemName)
        {
            return Plugin.Settings.CrossSettings.Current.GetValueOrDefault(systemName, false);
        }
        private void MonitorLoadingEnd(string systemName, bool state)
        {
            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(systemName, state);
        }
        private void AddNewConsole(string ConsoleName)
        {
            string CountConsoleName = ConsoleName;
            if (ConsoleName.Contains(" Old"))
            {
                CountConsoleName = "ROCore" + ConsoleName.Replace(" Old", "");
            }
            ConsolesSequence.Add(ConsoleName, platformService.GetPlaysCount(CountConsoleName));
        }
        private GameSystemViewModel GameConsole(string ConsoleName, IFileSystem fileSystem)
        {
            GameSystemViewModel CreatedConsole = null;

            switch (ConsoleName)
            {
                case "NES":
                    CreatedConsole = GameSystemViewModel.MakeNES(LibRetriX.FCEUMM.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "NES (Nestopia)":
                    CreatedConsole = GameSystemViewModel.MakeNESNestopia(LibRetriX.Nestopia.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Super Nintendo":
                    CreatedConsole = GameSystemViewModel.MakeSNES(LibRetriX.Snes9X.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Nintendo 64":
                    CreatedConsole = GameSystemViewModel.MakeN64(LibRetriX.ParallelN64.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Game Boy":
                    CreatedConsole = GameSystemViewModel.MakeGB(LibRetriX.VBAM.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Game Boy Color":
                    CreatedConsole = GameSystemViewModel.MakeGBC(LibRetriX.VBAMColor.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Game Boy Advance":
                    CreatedConsole = GameSystemViewModel.MakeGBA(LibRetriX.VBAMAdvance.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Game Boy Micro":
                    CreatedConsole = GameSystemViewModel.MakeGBM(LibRetriX.VBAMMicro.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Game Boy Advance SP":
                    CreatedConsole = GameSystemViewModel.MakeGBASP(LibRetriX.VBAM.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Nintendo DS":
                    CreatedConsole = GameSystemViewModel.MakeDS(LibRetriX.MelonDS.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "SG-1000":
                    CreatedConsole = GameSystemViewModel.MakeSG1000(LibRetriX.GenesisPlusGX.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Master System":
                    CreatedConsole = GameSystemViewModel.MakeMasterSystem(LibRetriX.GenesisPlusGXMasterSystem.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Game Gear":
                    CreatedConsole = GameSystemViewModel.MakeGameGear(LibRetriX.GenesisPlusGXGameGear.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Mega Drive":
                    CreatedConsole = GameSystemViewModel.MakeMegaDrive(LibRetriX.GenesisPlusGXMegaDrive.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Mega CD":
                    CreatedConsole = GameSystemViewModel.MakeMegaCD(LibRetriX.GenesisPlusGXCD.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "32X":
                    CreatedConsole = GameSystemViewModel.Make32X(LibRetriX.PicoDrive.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Saturn":
                    CreatedConsole = GameSystemViewModel.MakeSaturn(LibRetriX.BeetleSaturn.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Saturn (Yabause)":
                    CreatedConsole = GameSystemViewModel.MakeSaturnYabause(LibRetriX.BeetleSaturnYabause.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "PlayStation":
                    CreatedConsole = GameSystemViewModel.MakePlayStation(LibRetriX.BeetlePSX.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "PlayStation Old":
                    CreatedConsole = GameSystemViewModel.MakePlayStationOld(LibRetriX.BeetlePSXOld.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "PlayStation Portable":
                    CreatedConsole = GameSystemViewModel.MakePlayStationPortable(LibRetriX.PPSSPP.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "PC Engine":
                    CreatedConsole = GameSystemViewModel.MakePCEngine(LibRetriX.BeetlePCEFast.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "PC Engine CD":
                    CreatedConsole = GameSystemViewModel.MakePCEngineCD(LibRetriX.BeetlePCEFast.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "PC-FX":
                    CreatedConsole = GameSystemViewModel.MakePCFX(LibRetriX.BeetlePCFX.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "WonderSwan Color":
                    CreatedConsole = GameSystemViewModel.MakeWonderSwan(LibRetriX.BeetleWSwan.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Arcade":
                    CreatedConsole = GameSystemViewModel.MakeArcade(LibRetriX.FBAlpha.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Neo Geo Pocket Color":
                    CreatedConsole = GameSystemViewModel.MakeNeoGeoPocket(LibRetriX.BeetleNGP.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Neo Geo":
                    CreatedConsole = GameSystemViewModel.MakeNeoGeo(LibRetriX.FBNeo.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "PolyGame Master":
                    CreatedConsole = GameSystemViewModel.MakePolyGameMaster(LibRetriX.FBPoly.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Lynx":
                    CreatedConsole = GameSystemViewModel.MakeLynx(LibRetriX.Lynx.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "3DO":
                    CreatedConsole = GameSystemViewModel.Make3DO(LibRetriX.The3DO.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Vectrex":
                    CreatedConsole = GameSystemViewModel.MakeVectrex(LibRetriX.Vectrex.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Atari7800":
                    CreatedConsole = GameSystemViewModel.MakeAtari7800(LibRetriX.Atari7800.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Jaguar":
                    CreatedConsole = GameSystemViewModel.MakeAtarJaguar(LibRetriX.AtariJaguar.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "MSX":
                    CreatedConsole = GameSystemViewModel.MakeMSX(LibRetriX.MSX.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Virtual Boy":
                    CreatedConsole = GameSystemViewModel.MakeVirtualBoy(LibRetriX.VirtualBoy.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;


                case "ColecoVision":
                    CreatedConsole = GameSystemViewModel.MakeColecoVision(LibRetriX.BlueMSX.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Atari 5200":
                    CreatedConsole = GameSystemViewModel.MakeAtari5200(LibRetriX.Atari800.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Cannonball":
                    CreatedConsole = GameSystemViewModel.MakeCannonball(LibRetriX.Cannonball.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "DOSBox":
                    CreatedConsole = GameSystemViewModel.MakeDOSBox(LibRetriX.DOSBox.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Fairchild ChannelF":
                    CreatedConsole = GameSystemViewModel.MakeFairchildChannelF(LibRetriX.FreeChaf.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "SEGA Wide":
                    CreatedConsole = GameSystemViewModel.MakeSegaWide(LibRetriX.GenesisPlusGXWide.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Game Music":
                    CreatedConsole = GameSystemViewModel.MakeGameMusicEmu(LibRetriX.GME.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Lynx (Handy)":
                    CreatedConsole = GameSystemViewModel.MakeHandy(LibRetriX.Handy.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "LowresNX":
                    CreatedConsole = GameSystemViewModel.MakeLowResNX(LibRetriX.LowResNX.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Magnavox Odyssey 2":
                    CreatedConsole = GameSystemViewModel.MakeO2M(LibRetriX.O2EM.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "PocketCDG":
                    CreatedConsole = GameSystemViewModel.MakePocketCDG(LibRetriX.PocketCDG.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Pokemon Mini":
                    CreatedConsole = GameSystemViewModel.MakePokeMini(LibRetriX.PocketMini.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Watara Supervision":
                    CreatedConsole = GameSystemViewModel.MakeWataraSupervision(LibRetriX.Potator.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Doom":
                    CreatedConsole = GameSystemViewModel.MakeDoom(LibRetriX.PrBoom.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "PC 8000-8800":
                    CreatedConsole = GameSystemViewModel.MakePC8800(LibRetriX.Quasi88.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "NES (QuickNES)":
                    CreatedConsole = GameSystemViewModel.MakeNESQuick(LibRetriX.QuickNES.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Neo Geo Pocket (RACE)":
                    CreatedConsole = GameSystemViewModel.MakeNEOGEOPockectRace(LibRetriX.RACE.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "REminiscence Flashback":
                    CreatedConsole = GameSystemViewModel.MakeReminiscence(LibRetriX.Reminiscence.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Atari 2600":
                    CreatedConsole = GameSystemViewModel.MakeAtari2600Stella(LibRetriX.Stella.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "GameBoy (TGB Dual)":
                    CreatedConsole = GameSystemViewModel.MakeGBDoual(LibRetriX.TGBDual.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "TIC-80":
                    CreatedConsole = GameSystemViewModel.MakeTIC80(LibRetriX.TIC80.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Quake":
                    CreatedConsole = GameSystemViewModel.MakeQuake(LibRetriX.TryQuake.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                case "Rick Dangerous":
                    CreatedConsole = GameSystemViewModel.MakeRickDangerous(LibRetriX.XRick.Core.Instance(CrossFileSystem.Current.LocalStorage.FullName), fileSystem, platformService);
                    break;

                default:
                    break;
            }
            CreatedConsole.Core.FreeLibretroCore();
            return CreatedConsole;
        }
        private GameSystemViewModel GameConsoleAnyCore(string DllFile, IFileSystem fileSystem)
        {
            GameSystemViewModel CreatedConsole = null;
            var BiosFiles = GameSystemSelectionViewModel.GetAnyCoreBIOSFiles(DllFile);
            bool CDSupport = GameSystemSelectionViewModel.isPinnedCore(DllFile);
            CreatedConsole = GameSystemViewModel.MakeAnyCore(new LibRetriX.AnyCore.Core(BiosFiles).GetCoreInstance(DllFile), fileSystem, platformService, CDSupport);
            return CreatedConsole;
        }
        public static bool isARM()
        {
            try
            {
                Package package = Package.Current;
                string systemArchitecture = package.Id.Architecture.ToString();
                return systemArchitecture.ToLower().Contains("arm") || systemArchitecture.ToLower().Contains("ARM");
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static bool isX64()
        {
            try
            {
                Package package = Package.Current;
                string systemArchitecture = package.Id.Architecture.ToString();
                return systemArchitecture.ToLower().Contains("x64") || systemArchitecture.ToLower().Contains("amd64");
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static bool DeviceIsPhone()
        {
            try
            {
                EasClientDeviceInformation info = new EasClientDeviceInformation();
                string system = info.OperatingSystem;
                if (system.Equals("WindowsPhone"))
                {
                    return true;
                }
            }
            catch (Exception e)
            {

            }
            return false;
        }
    }
}
