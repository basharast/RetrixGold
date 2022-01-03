using Acr.UserDialogs;
using LibRetriX;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Resources;
using RetriX.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RetriX.Shared.ViewModels
{
    public class GameSystemViewModel
    {
        private static readonly IEnumerable<FileDependency> NoDependencies = new FileDependency[0];
        public static HashSet<string> CDImageExtensions { get; } = new HashSet<string> { ".bin", ".cue", ".iso", ".mds", ".mdf", ".pbp", ".ccd", ".gdi" };

        public static GameSystemViewModel MakeNES(ICore core, IFileSystem fileSystem, IPlatformService platformService) {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameNES, Strings.ManufacturerNameNintendo);
        }
        public static GameSystemViewModel MakeNESNestopia(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameNESNestopia, Strings.ManufacturerNameNintendo);
        }

        public static GameSystemViewModel MakeSNES(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameSNES, Strings.ManufacturerNameNintendo); ;
        }
        public static GameSystemViewModel MakeVirtualBoy(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemVirtualBoy, Strings.ManufacturerNameNintendo); ;
        }
        public static GameSystemViewModel MakeN64(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameNintendo64, Strings.ManufacturerNameNintendo); ;
        }
        public static GameSystemViewModel MakeGB(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameGameBoy, Strings.ManufacturerNameNintendo, "", null, new HashSet<string> { ".gb", ".dmg" });
        }
        public static GameSystemViewModel MakeGBC(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameGameBoyColor, Strings.ManufacturerNameNintendo, "", null, new HashSet<string> { ".gbc",".cgb", ".dmg" });
        }
        public static GameSystemViewModel MakeGBA(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameGameBoyAdvance, Strings.ManufacturerNameNintendo, "", null, new HashSet<string> { ".gba", ".sgb", ".dmg" });
        }
        public static GameSystemViewModel MakeGBM(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemGameBoyMicro, Strings.ManufacturerNameNintendo, "", null, new HashSet<string> {".gba", ".sgb", ".dmg" });
        }
        public static GameSystemViewModel MakeGBASP(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemGameBoyAdvanceSP, Strings.ManufacturerNameNintendo, "", null, new HashSet<string> { ".gb", ".gbc", ".cgb", ".gba", ".sgb", ".dmg" });
        }
        public static GameSystemViewModel MakeDS(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameDS, Strings.ManufacturerNameNintendo);
        }
        public static GameSystemViewModel MakeSG1000(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameSG1000, Strings.ManufacturerNameSega, "", NoDependencies, new HashSet<string> { ".sg" });
        }
        public static GameSystemViewModel MakeMasterSystem(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameMasterSystem, Strings.ManufacturerNameSega, "", null, new HashSet<string> { ".sms" });
        }
        public static GameSystemViewModel MakeGameGear(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameGameGear, Strings.ManufacturerNameSega, "", null, new HashSet<string> { ".gg" });
        }
        public static GameSystemViewModel MakeMegaDrive(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameMegaDrive, Strings.ManufacturerNameSega, "", null, new HashSet<string> { ".mds", ".md", ".smd", ".gen", ".bin" });
        }
        public static GameSystemViewModel MakeMegaCD(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameMegaCD, Strings.ManufacturerNameSega, "", null, new HashSet<string> { ".bin", ".cue", ".iso", ".chd" }, CDImageExtensions);
        }
        public static GameSystemViewModel Make32X(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemName32X, Strings.ManufacturerNameSega, "", null, new HashSet<string> { ".32x", ".bin", ".68k" });
        }
        public static GameSystemViewModel MakeSaturn(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameSaturn, Strings.ManufacturerNameSega, "", null, null, CDImageExtensions);
        }
        public static GameSystemViewModel MakeSaturnYabause(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemSegaSaturnYabause, Strings.ManufacturerNameSega, "", null, null, CDImageExtensions);
        }
        public static GameSystemViewModel MakePlayStation(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNamePlayStation, Strings.ManufacturerNameSony, "", null, new HashSet<string> { ".exe", ".cue", ".toc", ".ccd", ".m3u", ".pbp", ".chd", ".img" }, CDImageExtensions); ;
        }
        public static GameSystemViewModel MakePlayStationOld(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNamePlayStation+"*", Strings.ManufacturerNameSony, "", null, new HashSet<string> { ".exe", ".cue", ".toc", ".ccd", ".m3u", ".pbp", ".chd", ".img" }, CDImageExtensions,false,true); ;
        }
        public static GameSystemViewModel MakePlayStationPortable(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNamePlayStationPortable, Strings.ManufacturerNameSony);
        }
        public static GameSystemViewModel MakePCEngine(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNamePCEngine, Strings.ManufacturerNameNEC, "", NoDependencies, new HashSet<string> { ".pce" });
        }
        public static GameSystemViewModel MakePCEngineCD(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNamePCEngineCD, Strings.ManufacturerNameNEC, "", null, new HashSet<string> { ".cue", ".ccd", ".chd" }, CDImageExtensions);
        }
        public static GameSystemViewModel MakePCFX(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNamePCFX, Strings.ManufacturerNameNEC, "", null, null, CDImageExtensions);
        }
        public static GameSystemViewModel MakeWonderSwan(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameWonderSwan, Strings.ManufacturerNameBandai, "");
        }
        public static GameSystemViewModel MakeNeoGeo(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameNeoGeo, Strings.ManufacturerNameSNK);
        }
        public static GameSystemViewModel MakePolyGameMaster(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNamePolyGameMaster, Strings.ManufacturerNameIGS);
        }
        public static GameSystemViewModel MakeNeoGeoPocket(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameNeoGeoPocket, Strings.ManufacturerNameSNK);
        }
        public static GameSystemViewModel MakeArcade(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemNameArcade, Strings.ManufacturerNameFBAlpha);
        }
        public static GameSystemViewModel MakeLynx(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemAtariLynx, Strings.SystemAtari, "");
        }

        public static GameSystemViewModel Make3DO(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.System3DO, Strings.SystemThe3DOCompany, "", new FileDependency[] { core.FileDependencies[0] }, null, CDImageExtensions);
        }

        public static GameSystemViewModel MakeVectrex(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemVectrex, Strings.SystemGCEVectrex, "");
        }
        public static GameSystemViewModel MakeAtari7800(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemAtari7800, Strings.SystemAtari, "");
        }
        public static GameSystemViewModel MakeAtarJaguar(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemJaguar, Strings.SystemAtari, "");
        }
        public static GameSystemViewModel MakeMSX(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, Strings.SystemMSX, Strings.SystemMSXMan, "");
        }
        public static GameSystemViewModel MakeColecoVision(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "ColecoVision", "Coleco Industries", "");
        }
        public static GameSystemViewModel MakeAnyCore(ICore core, IFileSystem fileSystem, IPlatformService platformService, bool CDSupport)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, core.Name, core.SystemName, "", core.FileDependencies, null, (CDSupport ? CDImageExtensions : null), true);
        }

        public static GameSystemViewModel MakeAtari5200(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "Atari 5200", Strings.SystemAtari, "");
        }
        public static GameSystemViewModel MakeCannonball(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "Cannonball", "Chris White", "");
        }
        public static GameSystemViewModel MakeDOSBox(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "DOSBox", "DOSBox Team", "");
        }
        public static GameSystemViewModel MakeFairchildChannelF(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "Fairchild ChannelF", "Fairchild Camera and Instrument", "");
        }
        public static GameSystemViewModel MakeSegaWide(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "SEGA Wide", "SEGA", "");
        }
        public static GameSystemViewModel MakeGameMusicEmu(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "Game Music", "Blargg", "");
        }
        public static GameSystemViewModel MakeHandy(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "Lynx (Handy)", "Atari", "");
        }
        public static GameSystemViewModel MakeLowResNX(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "LowresNX", "Inutilis", "");
        }
        public static GameSystemViewModel MakeO2M(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "Magnavox Odyssey 2", "Philips", "");
        }
        public static GameSystemViewModel MakePocketCDG(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "PocketCDG", "RedBug", "");
        }
        public static GameSystemViewModel MakePokeMini(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "Pokémon Mini", "Nintendo", "");
        }
        public static GameSystemViewModel MakeWataraSupervision(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "Watara Supervision", "Watara", "");
        }
        public static GameSystemViewModel MakeDoom(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "Doom", "id Software", "");
        }
        public static GameSystemViewModel MakePC8800(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "PC 8000-8800", "NEC", "");
        }
        public static GameSystemViewModel MakeNESQuick(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, $"{Strings.SystemNameNES} (QuickNES)", Strings.ManufacturerNameNintendo);
        }
        public static GameSystemViewModel MakeNEOGEOPockectRace(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "Neo Geo Pocket (RACE)", Strings.ManufacturerNameSNK);
        }
        public static GameSystemViewModel MakeReminiscence(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "REminiscence Flashback", "Delphine Software");
        }
        public static GameSystemViewModel MakeAtari2600Stella(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "Atari 2600", "Atari");
        }
        public static GameSystemViewModel MakeGBDoual(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "GameBoy (TGB Dual)", "Nintendo");
        }
        public static GameSystemViewModel MakeTIC80(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "TIC-80", "TIC-80 Computer");
        }
        public static GameSystemViewModel MakeQuake(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "Quake", "id Software");
        }
        public static GameSystemViewModel MakeRickDangerous(ICore core, IFileSystem fileSystem, IPlatformService platformService)
        {
            PlatformService = platformService;
            return new GameSystemViewModel(core, fileSystem, "Rick Dangerous", "Core Design");
        }

        public IFileSystem FileSystem { get; }

        public ICore Core { get; set; }

        public string Name { get; }
        public string TempName { get; }
        public string Descriptions { get; }
        public string ProductionYear { get; }
        public string Manufacturer { get; set; }
        public string ManufacturerTemp { get; }
        public string DLLName { get; }
        public string Symbol { get; }
        public string Version { get; }
        public bool AnyCore { get; }
        public bool OldCore { get; }
        public bool Pinned { get; }
        public List<string> SupportedExtensions { get; }
        public IEnumerable<string> MultiFileExtensions { get; }
        public static IPlatformService PlatformService { get; set; }
        private IUserDialogs DialogsService { get; }
        private IEnumerable<FileDependency> Dependencies { get; }
        public bool FailedToLoad = false;

        private static Dictionary<string, string> AnyCoreIconsMap = new Dictionary<string, string>();
        private GameSystemViewModel(ICore core, IFileSystem fileSystem, string name, string manufacturer, string symbol = "", IEnumerable<FileDependency> dependenciesOverride = null, IEnumerable<string> supportedExtensionsOverride = null, IEnumerable<string> multiFileExtensions = null,bool anyCore=false,bool oldCore=false)
        {
            try
            {
                FileSystem = fileSystem;
                Core = core;
                Name = name;
                TempName = name;
                if (anyCore)
                {
                    TempName = $"RACore{name}";
                }
                if (oldCore)
                {
                    TempName = $"ROCore{name}";
                }
                TempName = TempName.Replace("*","");
                Core.SystemName = TempName;
                Core.OriginalSystemName = Name;
                Version = Core.Version;
                Manufacturer = manufacturer!=null? manufacturer:"";
                Symbol = symbol.Length > 0 ? symbol : (GetSystemIconByName(TempName));
                AnyCore = anyCore;
                OldCore = oldCore;
                Pinned = false;
                DLLName = Path.GetFileName(Core.DLLName);
                GetSystemDirectoryAsync();
                
                SupportedExtensions = supportedExtensionsOverride != null ? supportedExtensionsOverride.ToList() : Core.SupportedExtensions.ToList();
                if (!SupportedExtensions.Contains(".7z"))
                {
                    SupportedExtensions.Add(".7z");
                }
                if (!SupportedExtensions.Contains(".rar"))
                {
                    SupportedExtensions.Add(".rar");
                }
                if (!SupportedExtensions.Contains(".gz"))
                {
                    SupportedExtensions.Add(".gz");
                }
                if (!SupportedExtensions.Contains(".tar"))
                {
                    SupportedExtensions.Add(".tar");
                }
                MultiFileExtensions = multiFileExtensions == null ? new string[0] : multiFileExtensions;
                Dependencies = dependenciesOverride != null ? dependenciesOverride : Core.FileDependencies;
                
                
                if (anyCore)
                {
                    GameSystemAnyCore testCore = null;
                    if (!GameSystemSelectionViewModel.SystemsAnyCore.TryGetValue(TempName, out testCore))
                    {
                        GameSystemSelectionViewModel.SystemsAnyCore.Add(TempName, new GameSystemAnyCore(Name, Manufacturer, Symbol, "", "", false, DLLName, multiFileExtensions !=null));
                    }
                    else
                    {
                       if(testCore.CoreName?.Length>0)Name = testCore.CoreName;
                       if(testCore.CoreSystem?.Length>0) Manufacturer = testCore.CoreSystem;
                       testCore.DLLName = DLLName;
                       Pinned = testCore.Pinned;
                        
                        if (testCore.CoreIcon?.Length>0 && File.Exists(testCore.CoreIcon))
                        {
                            Symbol = testCore.CoreIcon;
                            try { 
                            AnyCoreIconsMap.Add(TempName, Symbol);
                            }catch(Exception ex)
                            {

                            }
                        }
                        
                    }
                    
                }
                ManufacturerTemp = Manufacturer;

                FailedToLoad = Core.FailedToLoad;
                if (FailedToLoad)
                {
                    Symbol = "ms-appx:///Assets/Icons/GNFail.png";
                }
                Core.OriginalSystemName = Name;
                CoresOptions testOptions = null;
                if (!GameSystemSelectionViewModel.SystemsOptions.TryGetValue(TempName, out testOptions))
                {
                    GameSystemSelectionViewModel.SystemsOptions.Add(TempName, new CoresOptions(Core));
                    GameSystemSelectionViewModel.SystemsOptionsTemp.Add(TempName, new CoresOptions(Core));
                }
                UpdateOpenCounts();
                Core.FreeLibretroCore();
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public void UpdateOpenCounts()
        {
            Task.Run((Action)(async () =>
            {
                try
                {
                    if (PlatformService != null)
                    {
                        int OpenCounter = PlatformService.GetPlaysCount(TempName);
                        Manufacturer = ManufacturerTemp + (OpenCounter > 0 ? " (" + OpenCounter + ")" : "");
                    }
                }catch(Exception ex)
                {

                }
            }));
        }

        public bool CheckRootFolderRequired(IFileInfo file)
        {
            UpdateOpenCounts();
            try
            {
                var extension = Path.GetExtension(file.Name);
                return MultiFileExtensions.Contains(extension);
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return false;
            }
        }

        List<string> OptionalList = new List<string>() {
        "bios_MD.bin",
        "bios_E.sms",
        "bios_U.sms",
        "bios_J.sms",
        "bios.gg",
        "disksys.rom",
        "sk.bin",
        "sk2chip.bin",
        "areplay.bin",
        "ggenie.bin",
        "gb_bios.bin",
        "gbc_bios.bin",
        "gba_bios.bin",
        "panafz10.bin",
        "panafz10-norsa.bin",
        "panafz10e-anvil.bin",
        "panafz10e-anvil-norsa.bin",
        "panafz1j.bin",
        "panafz1j-norsa.bin",
        "goldstar.bin",
        "sanyotry.bin",
        "3do_arcade_saot.bin",
        "panafz1-kanji.bin",
        "panafz10ja-anvil-kanji.bin",
        "panafz1j-kanji.bin",
        "7800 BIOS (U).rom",
        "7800 BIOS (E).rom",
        "bubsys.zip",
        "cchip.zip",
        "coleco.zip",
        "decocass.zip",
        "fdsbios.zip",
        "isgsm.zip",
        "midssio.zip",
        "neocdz.zip",
        "nmk004.zip",
        "skns.zip",
        "spec128.zip",
        "spectrum.zip",
        "ym2608.zip",
        "jagboot.zip",
        "msx.zip",
        "mpr-18811-mx.ic1",
        "mpr-19367-mx.ic1",
        "NstDatabase.xml",
        "bios7.bin",
        "bios9.bin",
        "firmware.bin",
        "DISK.ROM",
        "FMPAC.ROM",
        "MSXDOS2.ROM",
        "PAINTER.ROM",
        "KANJI.ROM",
        "bios.min"
        };
        public async Task<bool> CheckDependenciesMetAsync()
        {
            try
            {
                var systemFolder = await GetSystemDirectoryAsync();
                foreach (var i in Dependencies)
                {
                    if (OptionalList.IndexOf(i.Name) > -1) continue;
                    var file = await systemFolder.GetFileAsync(i.Name);
                    if (file == null)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return false;
            }
        }

        public Task<IDirectoryInfo> GetSystemDirectoryAsync()
        {
            return GetCoreStorageDirectoryAsync($"{Core.Name} - System");
        }

        public Task<IDirectoryInfo> GetSaveDirectoryAsync()
        {
            return GetCoreStorageDirectoryAsync($"{Core.Name} - Saves");
        }

        private async Task<IDirectoryInfo> GetCoreStorageDirectoryAsync(string directoryName)
        {
            try
            {
                IDirectoryInfo output = null;
                output = await FileSystem.LocalStorage.GetDirectoryAsync(directoryName);
                if (output == null)
                {
                    output = await FileSystem.LocalStorage.CreateDirectoryAsync(directoryName);
                    try
                    {
                        await SyncBIOSFiles(directoryName, output);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                return output;
            }
            catch (Exception e)
            {
                //PlatformService.ShowErrorMessage(e);
                return null;
            }
        }

        
        Dictionary<string, string[]>  BIOSDictionary = new Dictionary<string, string[]>();
        private void CoresMap()
        {
            if (BIOSDictionary.Count == 0)
            {
                BIOSDictionary.Add("Beetle PSX - System", new string[] { "scph5500.bin", "scph5501.bin", "scph5502.bin" });
                BIOSDictionary.Add("FB Alpha - System", new string[] { "neogeo.zip", "pgm.zip", "bubsys.zip", "cchip.zip", "coleco.zip", "decocass.zip","fdsbios.zip","isgsm.zip","midssio.zip","neocdz.zip","nmk004.zip","skns.zip","spec128.zip","spectrum.zip","ym2608.zip" });
                BIOSDictionary.Add("FinalBurn Neo - System", new string[] { "neogeo.zip", "pgm.zip", "bubsys.zip", "cchip.zip", "coleco.zip", "decocass.zip","fdsbios.zip","isgsm.zip","midssio.zip","neocdz.zip","nmk004.zip","skns.zip","spec128.zip","spectrum.zip","ym2608.zip", "msx.zip" });
                BIOSDictionary.Add("Genesis Plus GX - System", new string[] { "BIOS_CD_E.bin", "BIOS_CD_J.bin", "BIOS_CD_U.bin","bios_MD.bin","bios_E.sms","bios_U.sms","bios_J.sms","bios.gg","sk.bin","sk2chip.bin","areplay.bin","ggenie.bin" });
                BIOSDictionary.Add("Mednafen Saturn - System", new string[] { "sega_101.bin", "mpr-17933.bin", "mpr-18811-mx.ic1", "mpr-19367-mx.ic1" });
                BIOSDictionary.Add("Beetle Saturn - System", new string[] { "sega_101.bin", "mpr-17933.bin", "mpr-18811-mx.ic1", "mpr-19367-mx.ic1" });
                BIOSDictionary.Add("Mednafen PCE Fast - System", new string[] { "syscard1.pce", "syscard2.pce", "syscard3.pce",  "gexpress.pce" });
                BIOSDictionary.Add("Beetle PCE Fast - System", new string[] { "syscard1.pce", "syscard2.pce", "syscard3.pce",  "gexpress.pce" });
                BIOSDictionary.Add("Mednafen PC-FX - System", new string[] { "pcfx.rom" });
                BIOSDictionary.Add("Beetle PC-FX - System", new string[] { "pcfx.rom" });
                BIOSDictionary.Add("melonDS - System", new string[] { "bios7.bin", "bios9.bin", "firmware.bin" });
                BIOSDictionary.Add("DeSmuME - System", new string[] { "bios7.bin", "bios9.bin", "firmware.bin" });
                BIOSDictionary.Add("Nestopia - System", new string[] { "disksys.rom", "NstDatabase.xml" });
                BIOSDictionary.Add("FCEUmm - System", new string[] { "disksys.rom" });
                BIOSDictionary.Add("Gambatte - System", new string[] { "gb_bios.bin", "gbc_bios.bin" });
                BIOSDictionary.Add("ProSystem - System", new string[] { "7800 BIOS (E).rom", "7800 BIOS (U).rom" });
                BIOSDictionary.Add("VBA-M - System", new string[] { "gb_bios.bin", "gbc_bios.bin", "gba_bios.bin" });
                BIOSDictionary.Add("PicoDrive - System", new string[] { "BIOS_CD_E.bin", "BIOS_CD_J.bin", "BIOS_CD_U.bin" });
                BIOSDictionary.Add("Beetle Lynx - System", new string[] { "lynxboot.img" });
                BIOSDictionary.Add("Handy - System", new string[] { "lynxboot.img" });
                BIOSDictionary.Add("Mednafen Lynx - System", new string[] { "lynxboot.img" });
                BIOSDictionary.Add("Virtual Jaguar - System", new string[] { "jagboot.zip" });
                BIOSDictionary.Add("Yabause - System", new string[] { "sega_101.bin", "mpr-17933.bin", "saturn_bios.bin" });
                BIOSDictionary.Add("fMSX - System", new string[] { "DISK.ROM", "FMPAC.ROM", "KANJI.ROM", "MSX.ROM", "MSX2.ROM", "MSX2EXT.ROM", "MSX2P.ROM", "MSX2PEXT.ROM", "MSXDOS2.ROM", "PAINTER.ROM" });
                BIOSDictionary.Add("Opera - System", new string[] { "panafz1.bin", "panafz10.bin", "panafz10-norsa.bin", "panafz10e-anvil.bin", "panafz10e-anvil-norsa.bin", "panafz1j.bin", "panafz1j-norsa.bin", "goldstar.bin", "sanyotry.bin", "3do_arcade_saot.bin", "panafz1-kanji.bin", "panafz10ja-anvil-kanji.bin", "panafz1j-kanji.bin" });
            }
        }
        public async Task SyncBIOSFiles(string directoryName,IDirectoryInfo output, IDirectoryInfo customLocation=null)
        {
            try
            {
                CoresMap();
                if (directoryName.Contains("System"))
                {
                    var targetFolderName = @"Bios\";
                    var BIOSFiles = await FileSystem.InstallLocation.GetDirectoryAsync(targetFolderName + directoryName);
                    if (customLocation != null)
                    {
                        BIOSFiles = await customLocation.GetDirectoryAsync(targetFolderName + directoryName);
                    }
                    if (BIOSFiles != null) { 
                    string[] files;
                    if (BIOSDictionary.TryGetValue(directoryName, out files))
                    {
                        foreach (string biosfile in files)
                        {
                                try
                                {
                                    var BIOSFileTarget = await BIOSFiles.GetFileAsync(biosfile);
                                    if (BIOSFileTarget != null && await output.GetFileAsync(BIOSFileTarget.Name) == null)
                                    {
                                        var targetFile = await output.CreateFileAsync(BIOSFileTarget.Name);
                                        using (var inStream = await BIOSFileTarget.OpenAsync(FileAccess.Read))
                                        {
                                            using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                                            {
                                                await inStream.CopyToAsync(outStream);
                                                await outStream.FlushAsync();
                                            }
                                        }
                                    }
                                }catch(Exception ee)
                                {

                                }
                        }
                    }
                    }
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public async Task SyncAllBIOSFiles(IDirectoryInfo customLocation = null)
        {
            try
            {
                CoresMap();
                int copiedItems = 0; 
                foreach(var directoryName in BIOSDictionary.Keys) {
                    try
                    {
                        if (directoryName.Contains("System"))
                        {
                            IDirectoryInfo output = null;
                            output = await FileSystem.LocalStorage.GetDirectoryAsync(directoryName);
                            if (output == null)
                            {
                                output = await FileSystem.LocalStorage.CreateDirectoryAsync(directoryName);
                                output = await FileSystem.LocalStorage.GetDirectoryAsync(directoryName);
                            }
                            var targetFolderName = @"BFiles\";
                            var BIOSFiles = await FileSystem.InstallLocation.GetDirectoryAsync(targetFolderName + directoryName);
                            if (customLocation != null)
                            {
                                BIOSFiles = await customLocation.GetDirectoryAsync(targetFolderName + directoryName);
                            }
                            if (BIOSFiles != null)
                            {
                                string[] files;
                                if (BIOSDictionary.TryGetValue(directoryName, out files))
                                {
                                    foreach (string biosfile in files)
                                    {
                                        try
                                        {
                                            var BIOSFileTarget = await BIOSFiles.GetFileAsync(biosfile);
                                            if (BIOSFileTarget != null && await output.GetFileAsync(BIOSFileTarget.Name) == null)
                                            {
                                                var targetFile = await output.CreateFileAsync(BIOSFileTarget.Name);
                                                using (var inStream = await BIOSFileTarget.OpenAsync(FileAccess.Read))
                                                {
                                                    using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                                                    {
                                                        await inStream.CopyToAsync(outStream);
                                                        await outStream.FlushAsync();
                                                    }
                                                }
                                                copiedItems++;
                                            }
                                        }catch(Exception et)
                                        {

                                        }
                                    }
                                }
                            }
                        }
                    }catch(Exception ee)
                    {

                    }
                }
                if (copiedItems > 0)
                {
                    PlatformService.PlayNotificationSound("gamestarted.mp3");
                    await UserDialogs.Instance.AlertAsync("Nice Trick!, Congratulations", "B.Files");
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    //PlatformService.ShowErrorMessage(e);
                }
            }
        }
        public static string GetSystemIconByName(string ConsoleName)
        {
            string SystemIcon = "ms-appx:///Assets/Icons/GN.png";
            switch (ConsoleName)
            {
                case "NES":
                case "NES (QuickNES)":
                    SystemIcon = "ms-appx:///Assets/Icons/NES.png";
                    break;
                case "NES (Nestopia)":
                    SystemIcon = "ms-appx:///Assets/Icons/NES.png";
                    break;
                case "Super Nintendo":
                    SystemIcon = "ms-appx:///Assets/Icons/SNES.png";
                    break;
                case "Virtual Boy":
                    SystemIcon = "ms-appx:///Assets/Icons/VirtualBoy.png";
                    break;
                case "Nintendo 64":
                    SystemIcon = "ms-appx:///Assets/Icons/N64.png";
                    break;
                case "Game Boy":
                    SystemIcon = "ms-appx:///Assets/Icons/GB.gif";
                    break;
                case "Game Boy Color":
                case "GameBoy (TGB Dual)":
                    SystemIcon = "ms-appx:///Assets/Icons/GBC.gif";
                    break;
                case "Game Boy Advance":
                    SystemIcon = "ms-appx:///Assets/Icons/GBA2.gif";
                    break;
                case "Game Boy Advance SP":
                    SystemIcon = "ms-appx:///Assets/Icons/GBASP.png";
                    break;
                case "Game Boy Micro":
                    SystemIcon = "ms-appx:///Assets/Icons/GBM.gif";
                    break;
                case "Nintendo DS":
                    SystemIcon = "ms-appx:///Assets/Icons/NDS.png";
                    break;
                case "SG-1000":
                    SystemIcon = "ms-appx:///Assets/Icons/SG1000.png";
                    break;
                case "Master System":
                    SystemIcon = "ms-appx:///Assets/Icons/MasterSystem.png";
                    break;
                case "Game Gear":
                    SystemIcon = "ms-appx:///Assets/Icons/GameGear.png";
                    break;
                case "Mega Drive":
                    SystemIcon = "ms-appx:///Assets/Icons/MegaDrive.png";
                    break;
                case "Mega CD":
                    SystemIcon = "ms-appx:///Assets/Icons/MegaCD.png";
                    break;
                case "32X":
                    SystemIcon = "ms-appx:///Assets/Icons/32X.png";
                    break;
                case "Saturn":
                    SystemIcon = "ms-appx:///Assets/Icons/Saturn.png";
                    break;
                case "Saturn (Yabause)":
                    SystemIcon = "ms-appx:///Assets/Icons/Saturn.png";
                    break;
                case "PlayStation":
                case "ROCorePlayStation":
                    SystemIcon = "ms-appx:///Assets/Icons/Playstation.png";
                    break;
                case "Playstation Portable":
                    SystemIcon = "ms-appx:///Assets/Icons/PSP.png";
                    break;
                case "PC Engine":
                    SystemIcon = "ms-appx:///Assets/Icons/PCE.png";
                    break;
                case "PC Engine CD":
                    SystemIcon = "ms-appx:///Assets/Icons/PCECD.png";
                    break;
                case "PC-FX":
                    SystemIcon = "ms-appx:///Assets/Icons/PCEFX.png";
                    break;
                case "WonderSwan Color":
                    SystemIcon = "ms-appx:///Assets/Icons/WonderSwan.png";
                    break;
                case "Arcade":
                    SystemIcon = "ms-appx:///Assets/Icons/Arcade.png";
                    break;
                case "Neo Geo Pocket Color":
                case "Neo Geo Pocket (RACE)":
                    SystemIcon = "ms-appx:///Assets/Icons/NeoGeo Pocket.png";
                    break;
                case "Neo Geo":
                    SystemIcon = "ms-appx:///Assets/Icons/NeoGeo.png";
                    break;
                case "PolyGame Master":
                    SystemIcon = "ms-appx:///Assets/Icons/Arcade 2.png";
                    break;
                case "Game Cube":
                    SystemIcon = "ms-appx:///Assets/Icons/GameCube.png";
                    break;
                case "Dreamcast":
                    SystemIcon = "ms-appx:///Assets/Icons/Dreamcast.png";
                    break;
                case "Jaguar":
                    SystemIcon = "ms-appx:///Assets/Icons/Atari Jaguar.png";
                    break;
                case "Nintendo 3DS":
                    SystemIcon = "ms-appx:///Assets/Icons/3DS.png";
                    break;
                case "Playstation 2":
                    SystemIcon = "ms-appx:///Assets/Icons/PS2.png";
                    break;
                case "Playstation 3":
                    SystemIcon = "ms-appx:///Assets/Icons/PS3.png";
                    break;
                case "Playstation 4":
                    SystemIcon = "ms-appx:///Assets/Icons/PS4.png";
                    break;
                case "Nintendo Switch":
                    SystemIcon = "ms-appx:///Assets/Icons/Switch.png";
                    break;
                case "Nintendo Wii":
                    SystemIcon = "ms-appx:///Assets/Icons/Wii.png";
                    break;
                case "XBox":
                    SystemIcon = "ms-appx:///Assets/Icons/XBOX.png";
                    break;
                case "XBox 360":
                    SystemIcon = "ms-appx:///Assets/Icons/XBOX360.png";
                    break;
                case "XBox One":
                    SystemIcon = "ms-appx:///Assets/Icons/XBOXOne.png";
                    break;
                case "Lynx":
                case "Lynx (Handy)":
                    SystemIcon = "ms-appx:///Assets/Icons/Lynx.png";
                    break;
                case "3DO":
                    SystemIcon = "ms-appx:///Assets/Icons/3DO.png";
                    break;
                case "Vectrex":
                    SystemIcon = "ms-appx:///Assets/Icons/Vectrex.png";
                    break;
                case "Atari 7800":
                    SystemIcon = "ms-appx:///Assets/Icons/Atari7800.png";
                    break;
                case "Atari 5200":
                    SystemIcon = "ms-appx:///Assets/Icons/Atari5200.png";
                    break;
                case "MSX":
                    SystemIcon = "ms-appx:///Assets/Icons/MSX.png";
                    break;
                case "ColecoVision":
                    SystemIcon = "ms-appx:///Assets/Icons/ColecoVision.png";
                    break;
                case "Cannonball":
                    SystemIcon = "ms-appx:///Assets/Icons/Cannonball.png";
                    break;
                case "DOSBox":
                    SystemIcon = "ms-appx:///Assets/Icons/DOSBox.png";
                    break;
                case "Fairchild ChannelF":
                    SystemIcon = "ms-appx:///Assets/Icons/ChannelF.png";
                    break;
                case "SEGA Wide":
                    SystemIcon = "ms-appx:///Assets/Icons/MegaDrive.png";
                    break;
                case "Game Music":
                    SystemIcon = "ms-appx:///Assets/Icons/MusicEmu.png";
                    break;
                case "LowresNX":
                    SystemIcon = "ms-appx:///Assets/Icons/LowResNX.png";
                    break;
                case "Magnavox Odyssey 2":
                    SystemIcon = "ms-appx:///Assets/Icons/Odyssey2.png";
                    break;
                case "PocketCDG":
                    SystemIcon = "ms-appx:///Assets/Icons/PocketCDG.png";
                    break;
                case "Pokémon Mini":
                    SystemIcon = "ms-appx:///Assets/Icons/PokeMini.png";
                    break;
                case "Watara Supervision":
                    SystemIcon = "ms-appx:///Assets/Icons/SuperVision.png";
                    break;
                case "Doom":
                    SystemIcon = "ms-appx:///Assets/Icons/Doom.png";
                    break;
                case "PC 8000-8800":
                    SystemIcon = "ms-appx:///Assets/Icons/PC8800.png";
                    break;
                case "REminiscence Flashback":
                    SystemIcon = "ms-appx:///Assets/Icons/Reminiscence.png";
                    break;
                case "Atari 2600":
                    SystemIcon = "ms-appx:///Assets/Icons/Atari2600.png";
                    break;
                case "TIC-80":
                    SystemIcon = "ms-appx:///Assets/Icons/TIC80.png";
                    break;
                case "Quake":
                    SystemIcon = "ms-appx:///Assets/Icons/Quake.png";
                    break;
                case "Rick Dangerous":
                    SystemIcon = "ms-appx:///Assets/Icons/RickDangrous.png";
                    break;
                default:
                    if (AnyCoreIconsMap.Count > 0)
                    {
                        string testIcon = "";
                        if(AnyCoreIconsMap.TryGetValue(ConsoleName, out testIcon))
                        {
                            SystemIcon = testIcon;
                        }
                        else
                        {
                            SystemIcon = "ms-appx:///Assets/Icons/GN.png";
                        }
                    }
                    else
                    {
                        SystemIcon = "ms-appx:///Assets/Icons/GN.png";
                    }
                    
                    break;
            }
            return SystemIcon;
        }
    }
}
