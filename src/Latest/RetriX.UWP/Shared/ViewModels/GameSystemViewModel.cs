using LibRetriX;
using LibRetriX.RetroBindings;
using LibRetriX.RetroBindings.Tools;
using RetriX.UWP;
using RetriX.UWP.Cores;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using WinUniversalTool;
using static RetriX.UWP.Services.PlatformService;


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
    public class GameSystemViewModel : BindableBase
    {
        private static readonly IEnumerable<FileDependency> NoDependencies = new FileDependency[0];
        public static HashSet<string> CDImageExtensions { get; } = new HashSet<string> { ".bin", ".cue", ".iso", ".mds", ".mdf", ".pbp", ".ccd", ".gdi", ".scummvm" };

        public static async Task<GameSystemViewModel> MakeGearColeco(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("colecovision.rom", "ColecoVision BIOS", "2c66f5911e5b42b8ebe113403548eee7"),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "ColecoVision", "Coleco");
        }

        public static async Task<GameSystemViewModel> MakeC64(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("vice", "BIOS Collection", "", false, true),
                new FileDependency("JiffyDOS_C64.bin", "JiffyDOS C64 Kernal", "be09394f0576cf81fa8bacf634daf9a2", true),
                new FileDependency("JiffyDOS_1541-II.bin", "JiffyDOS 1541 drive BIOS", "1b1e985ea5325a1f46eb7fd9681707bf", true),
                new FileDependency("JiffyDOS_1571_repl310654.bin", "JiffyDOS 1571 drive BIOS", "41c6cc528e9515ffd0ed9b180f8467c0", true),
                new FileDependency("JiffyDOS_1581.bin", "JiffyDOS 1581 drive BIOS", "20b6885c6dc2d42c38754a365b043d71", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "C64", "Commodore");
        }

        public static async Task<GameSystemViewModel> MakeC64SuperCPU(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("vice", "BIOS Collection", "", false, true),
                new FileDependency("JiffyDOS_C64.bin", "JiffyDOS C64 Kernal", "be09394f0576cf81fa8bacf634daf9a2", true),
                new FileDependency("JiffyDOS_1541-II.bin", "JiffyDOS 1541 drive BIOS", "1b1e985ea5325a1f46eb7fd9681707bf", true),
                new FileDependency("JiffyDOS_1571_repl310654.bin", "JiffyDOS 1571 drive BIOS", "41c6cc528e9515ffd0ed9b180f8467c0", true),
                new FileDependency("JiffyDOS_1581.bin", "JiffyDOS 1581 drive BIOS", "20b6885c6dc2d42c38754a365b043d71", true),
                new FileDependency("scpu-dos-1.4.bin", "CMD SuperCPU Kernal 1.4", "cda2fcd2e1f0412029383e51dd472095", true),
                new FileDependency("scpu-dos-2.04.bin", "CMD SuperCPU Kernal 2.04", "b2869f8678b8b274227f35aad26ba509", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "C64 SuperCPU", "Commodore");
        }

        public static async Task<GameSystemViewModel> MakeC64SuperCPU2(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("vice", "BIOS Collection", "", false, true),
                new FileDependency("JiffyDOS_C64.bin", "JiffyDOS C64 Kernal", "be09394f0576cf81fa8bacf634daf9a2", true),
                new FileDependency("JiffyDOS_1541-II.bin", "JiffyDOS 1541 drive BIOS", "1b1e985ea5325a1f46eb7fd9681707bf", true),
                new FileDependency("JiffyDOS_1571_repl310654.bin", "JiffyDOS 1571 drive BIOS", "41c6cc528e9515ffd0ed9b180f8467c0", true),
                new FileDependency("JiffyDOS_1581.bin", "JiffyDOS 1581 drive BIOS", "20b6885c6dc2d42c38754a365b043d71", true),
                new FileDependency("scpu-dos-1.4.bin", "CMD SuperCPU Kernal 1.4", "cda2fcd2e1f0412029383e51dd472095", true),
                new FileDependency("scpu-dos-2.04.bin", "CMD SuperCPU Kernal 2.04", "b2869f8678b8b274227f35aad26ba509", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "C64 SuperCPU", "Commodore");
        }

        public static async Task<GameSystemViewModel> MakeC128(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("vice", "BIOS Collection", "", false, true),
                new FileDependency("JiffyDOS_C128.bin", "JiffyDOS C128 Kernal", "cbbd1bbcb5e4fd8046b6030ab71fc021", true),
                new FileDependency("JiffyDOS_C64.bin", "JiffyDOS C64 Kernal", "be09394f0576cf81fa8bacf634daf9a2", true),
                new FileDependency("JiffyDOS_1541-II.bin", "JiffyDOS 1541 drive BIOS", "1b1e985ea5325a1f46eb7fd9681707bf", true),
                new FileDependency("JiffyDOS_1571_repl310654.bin", "JiffyDOS 1571 drive BIOS", "41c6cc528e9515ffd0ed9b180f8467c0", true),
                new FileDependency("JiffyDOS_1581.bin", "JiffyDOS 1581 drive BIOS", "20b6885c6dc2d42c38754a365b043d71", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "C128", "Commodore");
        }

        public static async Task<GameSystemViewModel> MakeCBMII(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("vice", "BIOS Collection", "", false, true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "CBM-II", "Commodore");
        }

        public static async Task<GameSystemViewModel> MakeCBMII2(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("vice", "BIOS Collection", "", false, true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "CBM-II", "Commodore");
        }
        public static async Task<GameSystemViewModel> MakeCPET(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("vice", "BIOS Collection", "", false, true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "CPET", "Commodore");
        }
        public static async Task<GameSystemViewModel> MakePlus4(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("vice", "BIOS Collection", "", false, true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Plus4", "Commodore");
        }
        public static async Task<GameSystemViewModel> MakeVIC20(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("vice", "BIOS Collection", "", false, true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "VIC-20", "Commodore");
        }

        public static async Task<GameSystemViewModel> MakeOberon(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Oberon RISC", "Niklaus Wirth");
        }
        public static async Task<GameSystemViewModel> Make2048(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;


            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "2048", "Gabriele Cirulli");
        }
        public static async Task<GameSystemViewModel> MakeNXEngine(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;


            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Cave Story", "Studio Pixel");
        }
        public static async Task<GameSystemViewModel> MakeCaprice32(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;


            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Amstrad CPC", "Amstrad");
        }
        public static async Task<GameSystemViewModel> MakeMiniVMac(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("MacII.rom", "", "66223be14974601f1e60885eeb35e03cc"),
            };


            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "vMac", "Apple");
        }
        public static async Task<GameSystemViewModel> MakeFreeIntv(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("exec.bin", "Executive ROM", "62e761035cb657903761800f4437b8af"),
                new FileDependency("grom.bin", "Graphics ROM", "0cd5946c6473e42e8e4c2137785e427f"),
            };


            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Intellivision", "Mattel Electronics");
        }
        public static async Task<GameSystemViewModel> MakeFlycast(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("dc", "Dreamcast BIOS", "", false, true)
            };


            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Dreamcast", "SEGA");
        }
        public static async Task<GameSystemViewModel> MakeZX81(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Sinclair ZX81", "Timex Corporation");
        }
        public static async Task<GameSystemViewModel> MakeNES(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("disksys.rom", "Disk System BIOS (Optional)", "ca30b50f880eb660a320674ed365ef7a", true)
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameNES"), GetLocalString("ManufacturerNameNintendo"));
        }
        public static async Task<GameSystemViewModel> MakeNESNestopia(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("disksys.rom", "Famicom Disk System BIOS (Optional)", "ca30b50f880eb660a320674ed365ef7a", true),
                new FileDependency("NstDatabase.xml", "Nestopia UE Database file", "", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "NES", GetLocalString("ManufacturerNameNintendo"));
        }

        public static async Task<GameSystemViewModel> MakeSNES(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameSNES"), GetLocalString("ManufacturerNameNintendo"));
        }
        public static async Task<GameSystemViewModel> MakeSNES2005(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameSNES"), GetLocalString("ManufacturerNameNintendo"));
        }
        public static async Task<GameSystemViewModel> MakeVirtualBoy(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemVirtualBoy"), GetLocalString("ManufacturerNameNintendo"));
        }
        public static async Task<GameSystemViewModel> MakeN64(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = new Tuple<string, uint>[]{
                Tuple.Create("parallel-n64-audio-buffer-size", 1U),
                Tuple.Create("parallel-n64-gfxplugin-accuracy", 3U),
                Tuple.Create("parallel-n64-send_allist_to_hle_rsp", 1U),
                Tuple.Create("parallel-n64-cpucore", 0U),
                Tuple.Create("parallel-n64-skipframes", 1U),
                Tuple.Create("parallel-n64-screensize", 9U),
                Tuple.Create("parallel-n64-angrylion-multithread", 1U),
                Tuple.Create("parallel-n64-angrylion-vioverlay", 4U),
                Tuple.Create("parallel-n64-dithering", 1U),
                Tuple.Create("parallel-n64-gfxplugin", 0U)
            };

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameNintendo64"), GetLocalString("ManufacturerNameNintendo"));
        }
        public static async Task<GameSystemViewModel> MakeGW(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameGW"), GetLocalString("ManufacturerNameNintendo"));
        }
        public static async Task<GameSystemViewModel> MakeJNB(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameJNB"), GetLocalString("SystemBrain"));
        }
        public static async Task<GameSystemViewModel> MakeGB(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = new Tuple<string, uint>[]{
                Tuple.Create("vbam_gbHardware", 1U),
                Tuple.Create("vbam_showborders", 2U),
            };

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("gb_bios.bin", "Game Boy BIOS (Optional)", "32fbbd84168d3482956eb3c5051637f5", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "GameBoy", GetLocalString("ManufacturerNameNintendo"), "", null, new HashSet<string> { ".gb", ".dmg" });
        }
        public static async Task<GameSystemViewModel> MakeGBC(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("gbc_bios.bin", "Game Boy Color BIOS (Optional)", "dbfce9db9deaa2567f6a84fde55f9680", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "GameBoy Color", GetLocalString("ManufacturerNameNintendo"), "", null, new HashSet<string> { ".gbc", ".cgb", ".dmg" });
        }
        public static async Task<GameSystemViewModel> MakeGambatte(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[] {
                new FileDependency("gb_bios.bin", "Game Boy BIOS (Optional)", "32fbbd84168d3482956eb3c5051637f5", true),
                new FileDependency("gbc_bios.bin", "Game Boy Color BIOS (Optional)", "dbfce9db9deaa2567f6a84fde55f9680", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "GameBoy+", GetLocalString("ManufacturerNameNintendo"), "", null, new HashSet<string> { ".gb", ".gbc", ".dmg" });
        }
        public static async Task<GameSystemViewModel> MakeGearboy(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[] {
                new FileDependency("dmg_boot.bin", "Game Boy boot ROM", "32fbbd84168d3482956eb3c5051637f5", true),
                new FileDependency("cgb_boot.bin", "Game Boy Color boot ROM", "dbfce9db9deaa2567f6a84fde55f9680", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "GameBoy+", GetLocalString("ManufacturerNameNintendo"), "", null, new HashSet<string> { ".gb", ".gbc", ".cgb", ".sgb", ".dmg" });
        }
        public static async Task<GameSystemViewModel> MakeGBA(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("gba_bios.bin", "Game Boy Advance BIOS (Optional)", "a860e8c0b6d573d191e4ec7db1b1e4f6", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "GameBoy Advance", GetLocalString("ManufacturerNameNintendo"), "", null, new HashSet<string> { ".gba", ".sgb", ".dmg" });
        }
        public static async Task<GameSystemViewModel> MakeGBAMeteor(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemGBAM"), GetLocalString("ManufacturerNameNintendo"), "", null, new HashSet<string> { ".gba", ".sgb", ".dmg" });
        }
        public static async Task<GameSystemViewModel> MakeGBM(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("gb_bios.bin", "Game Boy BIOS (Optional)", "32fbbd84168d3482956eb3c5051637f5", true),
                new FileDependency("gbc_bios.bin", "Game Boy Color BIOS (Optional)", "dbfce9db9deaa2567f6a84fde55f9680", true),
                new FileDependency("gba_bios.bin", "Game Boy Advance BIOS (Optional)", "a860e8c0b6d573d191e4ec7db1b1e4f6", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemGameBoyMicro"), GetLocalString("ManufacturerNameNintendo"), "", null, new HashSet<string> { ".gba", ".sgb", ".dmg" });
        }
        public static async Task<GameSystemViewModel> MakeGameCube(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("dolphin-emu", "Dolphin Data", "", false, true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "GameCube", GetLocalString("ManufacturerNameNintendo"));
        }
        public static async Task<GameSystemViewModel> MakeWii(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("dolphin-emu", "Dolphin Data", "", false, true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Nintendo Wii", GetLocalString("ManufacturerNameNintendo"));
        }
        public static async Task<GameSystemViewModel> MakeGBASP(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemGameBoyAdvanceSP"), GetLocalString("ManufacturerNameNintendo"), "", null, new HashSet<string> { ".gb", ".gbc", ".cgb", ".gba", ".sgb", ".dmg" });
        }
        public static async Task<GameSystemViewModel> MakeDS(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = new Tuple<string, uint>[]{
                Tuple.Create("desmume_num_cores", 1U),
                Tuple.Create("desmume_frameskip", 1U),
                Tuple.Create("desmume_gfx_linehack", 1U),
                Tuple.Create("desmume_cpu_mode", 1U),
                Tuple.Create("desmume_load_to_memory", 0U),
                Tuple.Create("desmume_pointer_type", (!PlatformService.isXBOX && !PlatformService.DPadActive?1U:0U)),
                Tuple.Create("desmume_pointer_device_l", 2U),
                Tuple.Create("desmume_mouse_speed", 7U),
                Tuple.Create("desmume_pointer_colour", 0U),
                Tuple.Create("desmume_gfx_edgemark", 1U),
                Tuple.Create("desmume_use_external_bios", 0U),
                Tuple.Create("desmume_advanced_timing", 1U)
            };

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("bios7.bin", "ARM7 BIOS (Optional)", "df692a80a5b1bc90728bc3dfc76cd948", true),
                new FileDependency("bios9.bin", "ARM9 BIOS (Optional)", "a392174eb3e572fed6447e956bde4b25", true),
                new FileDependency("firmware.bin", "Firmware (Optional)", "145eaef5bd3037cbc247c213bb3da1b3", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameDS"), GetLocalString("ManufacturerNameNintendo"));
        }

        public static async Task<GameSystemViewModel> MakeMelonDS(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("bios7.bin", "ARM7 BIOS", "df692a80a5b1bc90728bc3dfc76cd948"),
                new FileDependency("bios9.bin", "ARM9 BIOS", "a392174eb3e572fed6447e956bde4b25"),
                new FileDependency("firmware.bin", "Firmware", "145eaef5bd3037cbc247c213bb3da1b3"),
                new FileDependency("dsi_bios7.bin", "DSi ARM7 BIOS", "", true),
                new FileDependency("dsi_bios9.bin", "DSi ARM9 BIOS", "", true),
                new FileDependency("dsi_firmware.bin", "DSi Firmware", "", true),
                new FileDependency("dsi_nand.bin", "DSi NAND", "", true),
                new FileDependency("dsi_sd_card.bin", "DSi SD Card", "", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameDS"), GetLocalString("ManufacturerNameNintendo"));
        }
        public static async Task<GameSystemViewModel> MakeSG1000(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameSG1000"), GetLocalString("ManufacturerNameSega"), "", NoDependencies, new HashSet<string> { ".sg" });
        }
        public static async Task<GameSystemViewModel> MakeMasterSystem(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]
            {
                new FileDependency("bios_E.sms", "MasterSystem EU (Optional)", "840481177270d5642a14ca71ee72844c", true),
                new FileDependency("bios_U.sms", "MasterSystem US (Optional)", "840481177270d5642a14ca71ee72844c", true),
                new FileDependency("bios_J.sms", "MasterSystem JP (Optional)", "24a519c53f67b00640d0048ef7089105", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameMasterSystem"), GetLocalString("ManufacturerNameSega"), "", null, new HashSet<string> { ".sms" });
        }
        public static async Task<GameSystemViewModel> MakeGameGear(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = new Tuple<string, uint>[]{
                Tuple.Create("genesis_plus_gx_system_hw", 6U),
            };

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
              new FileDependency("bios.gg", "GameGear BIOS (Optional)", "672e104c3be3a238301aceffc3b23fd6", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameGameGear"), GetLocalString("ManufacturerNameSega"), "", null, new HashSet<string> { ".gg" });
        }
        public static async Task<GameSystemViewModel> MakeMegaDrive(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = new Tuple<string, uint>[]{
                Tuple.Create("genesis_plus_gx_system_hw", 7U),
            };

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("bios_MD.bin", "MegaDrive TMSS (Optional)", "45e298905a08f9cfb38fd504cd6dbc84", true),
                new FileDependency("ggenie.bin", "Game Genie ROM (Optional)", "e8af7fe115a75c849f6aab3701e7799b", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameMegaDrive"), GetLocalString("ManufacturerNameSega"), "", null, new HashSet<string> { ".mds", ".md", ".smd", ".gen", ".bin" });
        }
        public static async Task<GameSystemViewModel> MakeMegaCD(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("BIOS_CD_E.bin", "Mega-CD (Model 1 1.00 Europe) BIOS", "e66fa1dc5820d254611fdcdba0662372"),
                new FileDependency("BIOS_CD_J.bin", "Mega-CD (Model 1 1.00 Japan) BIOS", "278a9397d192149e84e820ac621a8edd"),
                new FileDependency("BIOS_CD_U.bin", "Mega-CD (Model 1 1.00 USA) BIOS", "2efd74e3232ff260e371b99f84024f7f"),
                new FileDependency("sk.bin", "Sonic & Knuckles ROM (Optional)", "4ea493ea4e9f6c9ebfccbdb15110367e", true),
                new FileDependency("sk2chip.bin", "Sonic & Knuckles UPMEM (Optional)", "b4e76e416b887f4e7413ba76fa735f16", true),
                new FileDependency("areplay.bin", "Action Replay (Optional)", "a0028b3043f9d59ceeb03da5b073b30d", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameMegaCD"), GetLocalString("ManufacturerNameSega"), "", null, new HashSet<string> { ".bin", ".cue", ".iso", ".chd" }, CDImageExtensions);
        }
        public static async Task<GameSystemViewModel> Make32X(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("BIOS_CD_E.bin", "Mega-CD (Model 1 1.00 Europe) BIOS", "e66fa1dc5820d254611fdcdba0662372"),
                new FileDependency("BIOS_CD_J.bin", "Mega-CD (Model 1 1.00 Japan) BIOS", "278a9397d192149e84e820ac621a8edd"),
                new FileDependency("BIOS_CD_U.bin", "Mega-CD (Model 1 1.00 USA) BIOS", "2efd74e3232ff260e371b99f84024f7f"),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemName32X"), GetLocalString("ManufacturerNameSega"), "", null, new HashSet<string> { ".32x", ".bin", ".68k" });
        }
        public static async Task<GameSystemViewModel> MakeSaturn(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("sega_101.bin", "Saturn JP BIOS", "85ec9ca47d8f6807718151cbcca8b964"),
                new FileDependency("mpr-17933.bin", "Saturn US.mdEU BIOS", "3240872c70984b6cbfda1586cab68dbe")
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameSaturn"), GetLocalString("ManufacturerNameSega"), "", null, null, CDImageExtensions);
        }
        public static async Task<GameSystemViewModel> MakeSaturnYabause(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("saturn_bios.bin", "Saturn BIOS", "af5828fdff51384f99b3c4926be27762"),
                new FileDependency("sega_101.bin", "Saturn JP BIOS", "85ec9ca47d8f6807718151cbcca8b964"),
                new FileDependency("mpr-17933.bin", "Saturn US.mdEU BIOS (Optional)", "3240872c70984b6cbfda1586cab68dbe", true)
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameSaturn"), GetLocalString("ManufacturerNameSega"), "", null, null, CDImageExtensions);
        }
        public static async Task<GameSystemViewModel> MakePlayStation2(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("pcsx2", "PlayStation 2 Data", "", false, true)
            };

            //Input settings
            xcore.InputTypeID = new uint?[] { Converter.GenerateDeviceSubclass(Constants.RETRO_DEVICE_ANALOG, 0) };

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "PlayStation 2", GetLocalString("ManufacturerNameSony"), "", null, null, CDImageExtensions); ;
        }
        public static async Task<GameSystemViewModel> MakePlayStation(RetriXGoldCore xcore)
        {
            //Options override
            if (PlatformService.DeviceIsPhone())
            {
                xcore.Options = new Tuple<string, uint>[]{
                /*Tuple.Create("beetle_psx_frame_duping", 1U),*/
                Tuple.Create("beetle_psx_analog_calibration", 1U),
                Tuple.Create("beetle_psx_cpu_freq_scale", 2U),
                Tuple.Create("beetle_psx_line_render", 0U),
                Tuple.Create("beetle_psx_gte_overclock", 0U),
                Tuple.Create("beetle_psx_dither_mode", 2U)
            };
            }

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("scph5500.bin", "PlayStation (v3.0 09/09/96 J) BIOS", "8dd7d5296a650fac7319bce665a6a53c"),
                new FileDependency("scph5501.bin", "PlayStation (v3.0 11/18/96 A) BIOS", "490f666e1afb15b7362b406ed1cea246"),
                new FileDependency("scph5502.bin", "PlayStation (v3.0 01/06/97 E) BIOS", "32736f17079d0b2b7024407c39bd3050"),
            };

            //Input settings
            xcore.InputTypeID = new uint?[] { Converter.GenerateDeviceSubclass(Constants.RETRO_DEVICE_ANALOG, 0) };

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNamePlayStation"), GetLocalString("ManufacturerNameSony"), "", null, null, CDImageExtensions); ;
        }
        public static async Task<GameSystemViewModel> MakePlayStationReARMed(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("scph5500.bin", "PlayStation (v3.0 09/09/96 J) BIOS", "8dd7d5296a650fac7319bce665a6a53c", true),
                new FileDependency("scph5501.bin", "PlayStation (v3.0 11/18/96 A) BIOS", "490f666e1afb15b7362b406ed1cea246", true),
                new FileDependency("scph5502.bin", "PlayStation (v3.0 01/06/97 E) BIOS", "32736f17079d0b2b7024407c39bd3050", true),
            };

            //Input settings
            xcore.InputTypeID = new uint?[] { Converter.GenerateDeviceSubclass(Constants.RETRO_DEVICE_ANALOG, 0) };

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNamePlayStation"), GetLocalString("ManufacturerNameSony"), "", null, null, CDImageExtensions); ;
        }
        public static async Task<GameSystemViewModel> MakePlayStationDuckStation(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = new Tuple<string, uint>[]{
                //Tuple.Create("beetle_psx_frame_duping", 1U),
                Tuple.Create("duckstation_Debug.ShowVRAM", 1U),
                Tuple.Create("duckstation_CPU.ExecutionMode", 2U),
                Tuple.Create("duckstation_CPU.Overclock", 1U),
            };

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("scph5500.bin", "PlayStation (v3.0 09/09/96 J) BIOS", "8dd7d5296a650fac7319bce665a6a53c"),
                new FileDependency("scph5501.bin", "PlayStation (v3.0 11/18/96 A) BIOS", "490f666e1afb15b7362b406ed1cea246"),
                new FileDependency("scph5502.bin", "PlayStation (v3.0 01/06/97 E) BIOS", "32736f17079d0b2b7024407c39bd3050"),
            };

            //Input settings
            xcore.InputTypeID = new uint?[] { Converter.GenerateDeviceSubclass(Constants.RETRO_DEVICE_ANALOG, 0) };

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNamePlayStation"), GetLocalString("ManufacturerNameSony"), "", null, null, CDImageExtensions); ;
        }
        public static async Task<GameSystemViewModel> MakePlayStationHW(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = new Tuple<string, uint>[]{
                //Tuple.Create("beetle_psx_frame_duping", 1U),
                Tuple.Create("beetle_psx_analog_calibration", 1U),
                Tuple.Create("beetle_psx_cpu_freq_scale", 1U),
                Tuple.Create("beetle_psx_line_render", 0U),
                Tuple.Create("beetle_psx_gte_overclock", 0U),
                Tuple.Create("beetle_psx_dither_mode", 2U)
            };

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("scph5500.bin", "PlayStation (v3.0 09/09/96 J) BIOS", "8dd7d5296a650fac7319bce665a6a53c"),
                new FileDependency("scph5501.bin", "PlayStation (v3.0 11/18/96 A) BIOS", "490f666e1afb15b7362b406ed1cea246"),
                new FileDependency("scph5502.bin", "PlayStation (v3.0 01/06/97 E) BIOS", "32736f17079d0b2b7024407c39bd3050"),
            };

            //Input settings
            xcore.InputTypeID = new uint?[] { Converter.GenerateDeviceSubclass(Constants.RETRO_DEVICE_ANALOG, 0) };

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNamePlayStation"), GetLocalString("ManufacturerNameSony"), "", null, new HashSet<string> { ".exe", ".cue", ".toc", ".ccd", ".m3u", ".pbp", ".chd", ".img" }, CDImageExtensions); ;
        }

        public static async Task<GameSystemViewModel> MakePlayStationPortable(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNamePlayStationPortable"), GetLocalString("ManufacturerNameSony"));
        }
        public static async Task<GameSystemViewModel> MakePCEngine(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNamePCEngine"), GetLocalString("ManufacturerNameNEC"), "", NoDependencies, new HashSet<string> { ".pce" });
        }

        public static async Task<GameSystemViewModel> MakePCEngineFast(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("syscard3.pce", "PC Engine CD BIOS", "ff1a674273fe3540ccef576376407d1d"),
                new FileDependency("syscard2.pce", "CD-ROM System V2.xx", "3cdd6614a918616bfc41c862e889dd79", true),
                new FileDependency("syscard1.pce", "CD-ROM System V1.xx", "2b7ccb3d86baa18f6402c176f3065082", true),
                new FileDependency("gexpress.pce", "Game Express CD Card", "6d2cb14fc3e1f65ceb135633d1694122", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "PCEngine Fast", GetLocalString("ManufacturerNameNEC"));
        }

        public static async Task<GameSystemViewModel> MakePCEngineCD(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("syscard3.pce", "PC Engine CD BIOS", "ff1a674273fe3540ccef576376407d1d"),
                new FileDependency("syscard2.pce", "CD-ROM System V2.xx", "3cdd6614a918616bfc41c862e889dd79", true),
                new FileDependency("syscard1.pce", "CD-ROM System V1.xx", "2b7ccb3d86baa18f6402c176f3065082", true),
                new FileDependency("gexpress.pce", "Game Express CD Card", "6d2cb14fc3e1f65ceb135633d1694122", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNamePCEngineCD"), GetLocalString("ManufacturerNameNEC"), "", null, new HashSet<string> { ".cue", ".ccd", ".chd" }, CDImageExtensions);
        }
        public static async Task<GameSystemViewModel> MakePCFX(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("pcfx.rom", "PC-FX BIOS", "08e36edbea28a017f79f8d4f7ff9b6d7"),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNamePCFX"), GetLocalString("ManufacturerNameNEC"), "", null, null, CDImageExtensions);
        }
        public static async Task<GameSystemViewModel> MakeWonderSwanColor(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameWonderSwan"), GetLocalString("ManufacturerNameBandai"), "");
        }
        public static async Task<GameSystemViewModel> MakeWonderSwan(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "WonderSwan", GetLocalString("ManufacturerNameBandai"), "");
        }
        public static async Task<GameSystemViewModel> MakeNeoGeo(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameNeoGeo"), GetLocalString("ManufacturerNameSNK"));
        }
        public static async Task<GameSystemViewModel> MakeNeoGeoCD(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("neocd", "BIOS Collection", "", false, true),
                new FileDependency("neocd_f.rom", "Front Loader BIOS", "a5f4a7a627b3083c979f6ebe1fabc5d2df6d083b", true),
                new FileDependency("neocd_t.rom", "Top Loader BIOS", "cc92b54a18a8bff6e595aabe8e5c360ba9e62eb5", true),
                new FileDependency("neocd_z.rom", "CDZ BIOS", "b0f1c4fa8d4492a04431805f6537138b842b549f", true),
                new FileDependency("neocd_sf.rom", "Front Loader BIOS(2010)", "4a94719ee5d0e3f2b981498f70efc1b8f1cef325", true),
                new FileDependency("neocd_st.rom", "Top Loader BIOS(2010)", "19729b51bdab60c42aafef6e20ea9234c7eb8410", true),
                new FileDependency("neocd_sz.rom", "CDZ BIOS(2010)", "6a947457031dd3a702a296862446d7485aa89dbb", true),
                new FileDependency("front-sp1.bin", "Front Loader BIOS (MAME)", "53bc1f283cdf00fa2efbb79f2e36d4c8038d743a", true),
                new FileDependency("top-sp1.bin", "Top Loader BIOS (MAME)", "235f4d1d74364415910f73c10ae5482d90b4274f", true),
                new FileDependency("neocd.bin", "CDZ BIOS (MAME)", "7bb26d1e5d1e930515219cb18bcde5b7b23e2eda", true),
                new FileDependency("uni-bioscd.rom", "Universe BIOS CD 3.3", "5142f205912869b673a71480c5828b1eaed782a8", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "NeoGeo CD", GetLocalString("ManufacturerNameSNK"));
        }
        public static async Task<GameSystemViewModel> MakePolyGameMaster(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("pgm.zip", "IGS PolyGame Master BIOS", "581cc172db39bb5007642405adf25b6e")
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNamePolyGameMaster"), GetLocalString("ManufacturerNameIGS"));
        }
        public static async Task<GameSystemViewModel> MakeNeoGeoPocket(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "NeoGeo Pocket", GetLocalString("ManufacturerNameSNK"));
        }
        public static async Task<GameSystemViewModel> MakeArcade(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("neogeo.zip", "NeoGeo BIOS", "93adcaa22d652417cbc3927d46b11806"),
                new FileDependency("fbneo", "FBNeo BIOS collection", "", true, true)
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameArcade"), GetLocalString("ManufacturerNameFBAlpha"));
        }
        public static async Task<GameSystemViewModel> MakeMAME2000(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "MAME", "Global");
        }
        public static async Task<GameSystemViewModel> MakeMAME2003(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "MAME", "Global");
        }
        public static async Task<GameSystemViewModel> MakeFBAlpha(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("neogeo.zip", "NeoGeo BIOS collection", "93adcaa22d652417cbc3927d46b11806"),
                new FileDependency("bubsys.zip", "Bubble System (Optional)", "f81298afd68a1a24a49a1a2d9f087964", true),
                new FileDependency("cchip.zip", "C-Chip Internal ROM (Optional)", "df6f8a3d83c028a5cb9f2f2be60773f3", true),
                new FileDependency("coleco.zip", "ColecoVision System (Optional)", "140f47a7fe0cc6f7ff9634c4bb6014b4", true),
                new FileDependency("decocass.zip", "DECO Cassette System (Optional)", "b7e1189b341bf6a8e270017c096d21b0", true),
                new FileDependency("fdsbios.zip", "FDS System BIOS (Optional)", "d5e59c76bd6fb0668c79ecfa934cbc66", true),
                new FileDependency("isgsm.zip", "ISG Selection Master 2006 (Optional)", "4a56d56e2219c5e2b006b66a4263c01c", true),
                new FileDependency("midssio.zip", "Midway SSIO Sound Board (Optional)", "5904b0de768d1d506e766aa7e18994c1", true),
                new FileDependency("neocdz.zip", "Neo Geo CDZ System (Optional)", "eed0134ebf619aebb81bdc4f53b1084e", true),
                new FileDependency("nmk004.zip", "NMK004 Internal (Optional)", "bfacf1a68792d5348f93cf724d2f1dda", true),
                new FileDependency("skns.zip", "Super Kaneko Nova System (Optional)", "3f956c4e7008804cb47cbde49bd5b908", true),
                new FileDependency("spec128.zip", "ZX Spectrum 128 (Optional)", "b1b94f4e4cd645515fd42e6e61836f35", true),
                new FileDependency("spectrum.zip", "ZX Spectrum (Optional)", "c5f6e525ec21b8b3bef52e9f8416be24", true),
                new FileDependency("ym2608.zip", "YM2608 Internal (Optional)", "79ae0d2bb1901b7e606b6dc339b79a97", true),
                new FileDependency("msx.zip", "MSX1 System (Optional)", "a317e6b4", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemNameArcade"), GetLocalString("ManufacturerNameFBAlpha"));
        }
        public static async Task<GameSystemViewModel> MakeLynx(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("lynxboot.img", "Lynx Boot Image", "fcd403db69f54290b51035d82f835e7b"),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemAtariLynx"), GetLocalString("SystemAtari"), "");
        }
        public static async Task<GameSystemViewModel> Make3DO(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("panafz1.bin", "Panasonic FZ-1", "f47264dd47fe30f73ab3c010015c155b"),
                new FileDependency("panafz10.bin", "Panasonic FZ-10 (Optional)", "51f2f43ae2f3508a14d9f56597e2d3ce", true),
                new FileDependency("panafz10-norsa.bin", "Panasonic FZ-10 [RSA] (Optional)", "1477bda80dc33731a65468c1f5bcbee9", true),
                new FileDependency("panafz10e-anvil.bin", "Panasonic FZ-10-E [Anvil] (Optional)", "a48e6746bd7edec0f40cff078f0bb19f", true),
                new FileDependency("panafz10e-anvil-norsa.bin", "FZ-10-E [Anvil,RSA] (Optional)", "cf11bbb5a16d7af9875cca9de9a15e09", true),
                new FileDependency("panafz1j.bin", "Panasonic FZ-1J (Optional)", "a496cfdded3da562759be3561317b605", true),
                new FileDependency("panafz1j-norsa.bin", "Panasonic FZ-1J [RSA] (Optional)", "f6c71de7470d16abe4f71b1444883dc8", true),
                new FileDependency("goldstar.bin", "Goldstar GDO-101M (Optional)", "8639fd5e549bd6238cfee79e3e749114", true),
                new FileDependency("sanyotry.bin", "Sanyo IMP-21J TRY (Optional)", "35fa1a1ebaaeea286dc5cd15487c13ea", true),
                new FileDependency("3do_arcade_saot.bin", "Shootout At Old Tucson (Optional)", "8970fc987ab89a7f64da9f8a8c4333ff", true),
                new FileDependency("panafz1-kanji.bin", "Panasonic FZ-1 Kanji (Optional)", "b8dc97f778a6245c58e064b0312e8281", true),
                new FileDependency("panafz10ja-anvil-kanji.bin", "FZ-10JA Kanji (Optional)", "428577250f43edc902ea239c50d2240d", true),
                new FileDependency("panafz1j-kanji.bin", "Panasonic FZ-1J Kanji (Optional)", "c23fb5d5e6bb1c240d02cf968972be37", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("System3DO"), GetLocalString("SystemThe3DOCompany"), "", null, null, CDImageExtensions);
        }
        public static async Task<GameSystemViewModel> MakeVectrex(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = new Tuple<string, uint>[]{
                Tuple.Create("vecx_res_multi", 0U),
            };

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemVectrex"), GetLocalString("SystemGCEVectrex"), "");
        }
        public static async Task<GameSystemViewModel> MakeAtari7800(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("7800 BIOS (U).rom", "7800 BIOS (U) (Optional)", "0763f1ffb006ddbe32e52d497ee848ae", true),
                new FileDependency("7800 BIOS (E).rom", "7800 BIOS (E) (Optional)", "397bb566584be7b9764e7a68974c4263", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemAtari7800"), GetLocalString("SystemAtari"), "");
        }
        public static async Task<GameSystemViewModel> MakeAtarJaguar(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = new Tuple<string, uint>[]{
                Tuple.Create("virtualjaguar_usefastblitter", 1U),
            };

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("jagboot.zip", "BIOS Loading (Optional)", "2c35353f766c4448632df176c320f3a9", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, GetLocalString("SystemJaguar"), GetLocalString("SystemAtari"), "");
        }
        public static async Task<GameSystemViewModel> MakeMSX(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("MSX.ROM", " MSX BIOS", "364a1a579fe5cb8dba54519bcfcdac0d"),
                new FileDependency("MSX2.ROM", " MSX2 BIOS", "ec3a01c91f24fbddcbcab0ad301bc9ef"),
                new FileDependency("MSX2EXT.ROM", "MSX2 ExtROM", "2183c2aff17cf4297bdb496de78c2e8a"),
                new FileDependency("MSX2P.ROM", "MSX2+ BIOS", "847cc025ffae665487940ff2639540e5"),
                new FileDependency("MSX2PEXT.ROM", "MSX2+ ExtROM", "7c8243c71d8f143b2531f01afa6a05dc"),
                new FileDependency("RS232.ROM", "Microsoft MSX", "279efd1eae0d358eecd4edc7d9adedf3", true),
                new FileDependency("DISK.ROM", "DiskROM/BDOS (Optional)", "80dcd1ad1a4cf65d64b7ba10504e8190", true),
                new FileDependency("FMPAC.ROM", "FMPAC BIOS (Optional)", "6f69cc8b5ed761b03afd78000dfb0e19", true),
                new FileDependency("MSXDOS2.ROM", "MSX-DOS 2 (Optional)", "6418d091cd6907bbcf940324339e43bb", true),
                new FileDependency("PAINTER.ROM", "Yamaha Painter (Optional)", "403cdea1cbd2bb24fae506941f8f655e", true),
                new FileDependency("KANJI.ROM", "Kanji Font (Optional)", "febe8782b466d7c3b16de6d104826b34", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "MSX Computer", "Microsoft", "");
        }
        public static async Task<GameSystemViewModel> MakeMSXComputer(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("Databases", "MSX machines databases", "", false, true),
                new FileDependency("Machines", "MSX machines configuration", "", false, true)
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "MSX Computer", "Microsoft");
        }
        public static async Task<GameSystemViewModel> MakeAnyCore(RetriXGoldCore xcore, bool CDSupport, bool skippedCore = false)
        {
            //I shouldn't override anything for AnyCore they will be linked from frontend in future
            //Options override
            //xcore.Options = null;

            //Dependencies override (related to BIOSMap, user can assign this from his side)
            //xcore.Dependencies = null;

            //Input settings
            //xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, core.Name, core.SystemName, "", core.FileDependencies, null, (CDSupport ? CDImageExtensions : null), true, skippedCore);
        }

        public static async Task<GameSystemViewModel> MakeAtari5200(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = new Tuple<string, uint>[]{
                Tuple.Create("atari800_system", 3U),
            };

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("5200.rom", "5200 BIOS", "281f20ea4320404ec820fb7ec0693b38"),
                new FileDependency("ATARIXL.ROM", "Atari XL/XE OS", "06daac977823773a3eea3422fd26a703"),
                new FileDependency("ATARIBAS.ROM", "BASIC interpreter", "0bac0c6a50104045d902df4503a4c30b"),
                new FileDependency("ATARIOSA.ROM", "Atari 400/800 PAL", "eb1f32f5d9f382db1bbfb8d7f9cb343a"),
                new FileDependency("ATARIOSB.ROM", "BIOS for Atari 400/800 NTSC", "a3e8d617c95d08031fe1b20d541434b2"),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Atari 5200", GetLocalString("SystemAtari"), "");
        }
        public static async Task<GameSystemViewModel> MakeCannonball(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "OutRun", "SEGA", "");
        }
        public static async Task<GameSystemViewModel> MakeDOSBox(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "DOSBox", "DOSBox Team", "");
        }
        public static async Task<GameSystemViewModel> MakeDOSBoxPure(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "DOSBox Pure", "DOSBox Team", "");
        }
        public static async Task<GameSystemViewModel> MakeDOSBoxSVN(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "DOSBox SVN", "DOSBox Team", "");
        }
        public static async Task<GameSystemViewModel> MakeFairchildChannelF(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("sl31253.bin", "ChannelF BIOS (PSU 1)", "ac9804d4c0e9d07e33472e3726ed15c3"),
                new FileDependency("sl31254.bin", "ChannelF BIOS (PSU 2)", "da98f4bb3242ab80d76629021bb27585"),
                new FileDependency("sl90025.bin", "ChannelF II BIOS (PSU 1)", "95d339631d867c8f1d15a5f2ec26069d"),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Fairchild ChannelF", "Fairchild Camera and Instrument", "");
        }
        public static async Task<GameSystemViewModel> MakeSegaWide(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("BIOS_CD_E.bin", "Mega-CD (Model 1 1.00 Europe) BIOS", "e66fa1dc5820d254611fdcdba0662372"),
                new FileDependency("BIOS_CD_J.bin", "Mega-CD (Model 1 1.00 Japan) BIOS", "278a9397d192149e84e820ac621a8edd"),
                new FileDependency("BIOS_CD_U.bin", "Mega-CD (Model 1 1.00 USA) BIOS", "2efd74e3232ff260e371b99f84024f7f"),
                new FileDependency("bios_MD.bin", "MegaDrive TMSS (Optional)", "45e298905a08f9cfb38fd504cd6dbc84", true),
                new FileDependency("ggenie.bin", "Game Genie ROM (Optional)", "e8af7fe115a75c849f6aab3701e7799b", true),
                new FileDependency("bios.gg", "GameGear BIOS (Optional)", "672e104c3be3a238301aceffc3b23fd6", true),
                new FileDependency("bios_E.sms", "MasterSystem EU (Optional)", "840481177270d5642a14ca71ee72844c", true),
                new FileDependency("bios_U.sms", "MasterSystem US (Optional)", "840481177270d5642a14ca71ee72844c", true),
                new FileDependency("bios_J.sms", "MasterSystem JP (Optional)", "24a519c53f67b00640d0048ef7089105", true),
                new FileDependency("sk.bin", "Sonic & Knuckles ROM (Optional)", "4ea493ea4e9f6c9ebfccbdb15110367e", true),
                new FileDependency("sk2chip.bin", "Sonic & Knuckles UPMEM (Optional)", "b4e76e416b887f4e7413ba76fa735f16", true),
                new FileDependency("areplay.bin", "Action Replay (Optional)", "a0028b3043f9d59ceeb03da5b073b30d", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "SEGA Wide", "SEGA", "");
        }
        public static async Task<GameSystemViewModel> MakeGameMusicEmu(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Game Music", "Blargg", "");
        }
        public static async Task<GameSystemViewModel> MakeHandy(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("lynxboot.img", "Lynx Boot Image", "fcd403db69f54290b51035d82f835e7b"),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Lynx", "Atari", "");
        }
        public static async Task<GameSystemViewModel> MakeLowResNX(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "LowresNX", "Inutilis", "");
        }
        public static async Task<GameSystemViewModel> MakeO2M(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("o2rom.bin", "Odyssey2 BIOS - G7000", "562d5ebf9e030a40d6fabfc2f33139fd"),
                new FileDependency("c52.bin", "Videopac+ French BIOS - G7000", "f1071cdb0b6b10dde94d3bc8a6146387"),
                new FileDependency("g7400.bin", "Videopac+ European BIOS - G7400", "c500ff71236068e0dc0d0603d265ae76"),
                new FileDependency("jopac.bin", "Videopac+ French BIOS - G7400", "279008e4a0db2dc5f1c048853b033828"),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Magnavox Odyssey 2", "Philips", "");
        }
        public static async Task<GameSystemViewModel> MakePocketCDG(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "PocketCDG", "RedBug", "");
        }
        public static async Task<GameSystemViewModel> MakePokeMini(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("bios.min", "Pokémon Mini BIOS (Optional)", "1e4fb124a3a886865acb574f388c803d", true),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Pokémon Mini", "Nintendo", "");
        }
        public static async Task<GameSystemViewModel> MakeWataraSupervision(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Watara Supervision", "Watara", "");
        }
        public static async Task<GameSystemViewModel> MakeDoom(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("prboom.wad", "Inside the ROM directory too", ""),
                new FileDependency("doom1.wad", "Test content", "", true),
            };

            //Input settings
            xcore.InputTypeID = new uint?[] { Converter.GenerateDeviceSubclass(Constants.RETRO_DEVICE_ANALOG, 0) };

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Doom", "id Software", "");
        }
        public static async Task<GameSystemViewModel> MakePC8800(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("n88.rom", "-", "4f984e04a99d56c4cfe36115415d6eb8"),
                new FileDependency("n88n.rom", "PC-8000 series emulation", "2ff07b8769367321128e03924af668a0"),
                new FileDependency("disk.rom", "Loading disk images", "793f86784e5608352a5d7f03f03e0858"),
                new FileDependency("n88knj1.rom", "Viewing kanji", "d81c6d5d7ad1a4bbbd6ae22a01257603"),
                new FileDependency("n88_0.rom", "-", "d675a2ca186c6efcd6277b835de4c7e5"),
                new FileDependency("n88_1.rom", "-", "e844534dfe5744b381444dbe61ef1b66"),
                new FileDependency("n88_2.rom", "-", "6548fa45061274dee1ea8ae1e9e93910"),
                new FileDependency("n88_3.rom", "-", "fc4b76a402ba501e6ba6de4b3e8b4273"),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "PC 8000-8800", "NEC", "");
        }
        public static async Task<GameSystemViewModel> MakeNESQuick(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "NES", GetLocalString("ManufacturerNameNintendo"));
        }
        public static async Task<GameSystemViewModel> MakeNEOGEOPockectRace(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "NeoGeo Pocket", GetLocalString("ManufacturerNameSNK"));
        }

        static readonly HashSet<string> IndexFiles = new HashSet<string>(new[] { ".0", ".1", ".2", ".3", ".5", ".6", ".8", ".16", ".25", ".99", ".101", ".102", ".418", ".455", ".512", ".scummvm", ".scumm", ".gam", ".z5", ".dat", ".blb", ".z6", ".RAW", ".ROM", ".taf", ".zblorb", ".dcp", ".(a)", ".cup", ".HE0", ".(A)", ".D$$", ".STK", ".z8", ".hex", ".VMD", ".TGA", ".ITK", ".SCN", ".INF", ".pic", ".Z5", ".z3", ".blorb", ".ulx", ".DAT", ".cas", ".PIC", ".acd", ".006", ".SYS", ".alr", ".t3", ".gblorb", ".tab", ".AP", ".CRC", ".EXE", ".z4", ".W32", ".MAC", ".mac", ".WIN", ".001", ".003", ".000", ".bin", ".exe", ".asl", ".AVD", ".INI", ".SND", ".cat", ".ANG", ".CUP", ".SYS16", ".img", ".LB", ".TLK", ".MIX", ".VQA", ".RLB", ".FNT", ".win", ".HE1", ".DMU", ".FON", ".SCR", ".TEX", ".HEP", ".DIR", ".DRV", ".MAP", ".a3c", ".GRV", ".CUR", ".OPT", ".gfx", ".ASK", ".LNG", ".ini", ".RSC", ".SPP", ".CC", ".BND", ".LA0", ".TRS", ".add", ".HRS", ".DFW", ".DR1", ".ALD", ".004", ".002", ".005", ".R02", ".R00", ".C00", ".D00", ".GAM", ".IDX", ".ogg", ".TXT", ".GRA", ".BMV", ".H$$", ".MSG", ".VGA", ".PKD", ".OUT", ".99 (PG)", ".SAV", ".PAK", ".BIN", ".CPS", ".SHP", ".DXR", ".dxr", ".gmp", ".SNG", ".C35", ".C06", ".WAV", ".SMK", ".wav", ".CAB", ".game", ".Z6", ".(b)", ".slg", ".he2", ".he1", ".HE2", ".SYN", ".PAT", ".NUT", ".nl", ".PRC", ".V56", ".SEQ", ".P56", ".AUD", ".FKR", ".EX1", ".rom", ".LIC", ".$00", ".ALL", ".LTK", ".txt", ".acx", ".VXD", ".ACX", ".mpc", ".msd", ".ADF", ".nib", ".HELLO", ".dsk", ".xfd", ".woz", ".d$$", ".SET", ".SOL", ".Pat", ".CFG", ".BSF", ".RES", ".IMD", ".LFL", ".SQU", ".rsc", ".BBM", ".2 US", ".OVL", ".OVR", ".007", ".PNT", ".pat", ".CHK", ".MDT", ".EMC", ".ADV", ".FDT", ".GMC", ".FMC", ".info", ".HPF", ".hpf", ".INE", ".RBT", ".CSC", ".HEB", ".MID", ".lfl", ".LEC", ".HNM", ".QA", ".009", ".PRF", ".EGA", ".MHK", ".d64", ".prg", ".LZC", ".flac", ".IMS", ".REC", ".MOR", ".doc", ".HAG", ".AGA", ".BLB", ".TABLE", ".PAL", ".PRG", ".CLG", ".ORB", ".BRO", ".bro", ".PH1", ".DEF", ".IN", ".jpg", ".TOC", ".j2", ".Text", ".CEL", ".he0", ".AVI", ".1C", ".1c", ".BAK", ".L9", ".CGA", ".HRC", ".mhk", ".RED", ".SM0", ".SM1", ".SOU", ".RRM", ".LIB", ". Seuss's  ABC", ".CNV", ".VOC", ".OGG", ".GME", ".GERMAN", ".SHR", ".FRENCH", ".DNR", ".DSK", ".dnr", ".CAT", ".V16", ".cab", ".CLU", ".b25c", ".RL", ".mp3", ".FRM", ".SOG", ".HEX", ".mma", ".st", ".MPC", ".IMG", ".ENC", ".SPR", ".AD", ".C", ".CON", ".PGM", ".Z", ".RL2", ".MMM", ".OBJ", ".ZFS", ".zfs", ".STR", ".z2", ".z1" }, StringComparer.OrdinalIgnoreCase);

        public static async Task<GameSystemViewModel> MakeScummVM(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("MT32_PCM.ROM", "MT-32 PCM (Optional)", "89e42e386e82e0cacb4a2704a03706ca", true),
                new FileDependency("MT32_CONTROL.ROM", "MT-32 Control (Optional)", "5626206284b22c2734f3e9efefcd2675", true)
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "ScummVM", "ScummVM Team", "", null, IndexFiles, IndexFiles);
        }
        public static async Task<GameSystemViewModel> MakeReminiscence(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Flashback", "Delphine Software");
        }
        public static async Task<GameSystemViewModel> MakeAtari2600Stella(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Atari 2600", "Atari");
        }
        public static async Task<GameSystemViewModel> MakeAtari2600Stella2014(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Atari 2600", "Atari");
        }
        public static async Task<GameSystemViewModel> MakeGBDual(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "GameBoy+", "Nintendo");
        }
        public static async Task<GameSystemViewModel> MakeTIC80(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "TIC-80", "TIC-80 Computer");
        }
        public static async Task<GameSystemViewModel> MakeQuake(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = null;

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Quake", "id Software");
        }
        public static async Task<GameSystemViewModel> MakeRickDangerous(RetriXGoldCore xcore)
        {
            //Options override
            xcore.Options = null;

            //Dependencies override
            xcore.Dependencies = new FileDependency[]{
                new FileDependency("data.zip", "XRick Data", "A471E64E9F69AFBE59C10CC94ED1B184"),
            };

            //Input settings
            xcore.InputTypeID = null;

            //Generate core
            var core = await xcore.GetCore();
            return new GameSystemViewModel(xcore, "Rick Dangerous", "Core Design");
        }

        public LibretroCore Core { get; set; }

        public string Name { get; }
        public string TempName { get; }
        string folderPickerTokenName
        {
            get
            {
                return TempName;
            }
        }
        public string Descriptions { get; }
        public string ProductionYear { get; }
        public string ConsoleName { get; set; }
        public string Manufacturer { get; set; }
        public int openTimes { get; set; }
        public int OpenTimes
        {
            get
            {
                return openTimes;
            }
            set
            {
                openTimes = value;
                RaisePropertyChanged(nameof(OpenTimes));
            }
        }
        public string ManufacturerTemp { get; }
        public string DLLName { get; }
        string symbol;
        public string Symbol
        {
            get
            {
                if (SkippedCore)
                {
                    return "ms-appx:///Assets/Icons/GNSkipped.png";
                }
                else
                if (FailedToLoad)
                {
                    return "ms-appx:///Assets/Icons/GNFail.png";
                }
                else
                {
                    return symbol;
                }
            }
            set
            {
                symbol = value;
            }
        }
        public string Version { get; }
        public bool AnyCore { get; }
        public bool OldCore { get; }
        public bool CDSupport { get; }
        public bool SupportNoGame { get; }
        public List<string> SupportedExtensions { get; }
        public IEnumerable<string> MultiFileExtensions { get; }
        private IEnumerable<FileDependency> Dependencies { get; }
        public bool FailedToLoad = false;
        public bool SkippedCore = false;
        public RetriXGoldCore XCore = null;
        //Imported cores could match with builtin configuration
        //Some functions should be visible like online update
        //because usually AnyCore doesn't allow online update
        //So I have to use this variable to know this case
        public bool ImportedCore = false;

        private static Dictionary<string, string> AnyCoreIconsMap = new Dictionary<string, string>();
        private GameSystemViewModel(RetriXGoldCore xcore, string name, string manufacturer, string symbol = "", IEnumerable<FileDependency> dependenciesOverride = null, IEnumerable<string> supportedExtensionsOverride = null, IEnumerable<string> multiFileExtensions = null, bool anyCore = false, bool oldCore = false, bool skippedCore = false)
        {
            try
            {
                XCore = xcore;
                LibretroCore core = xcore.core;

                Core = core;
                Name = name;
                TempName = name;
                TempName = TempName.Replace("*", "");
                AnyCore = core.AnyCore;
                ImportedCore = core.ImportedCore;

                Core.SystemName = TempName;
                Core.OriginalSystemName = Name;
                Version = Core.Version;
                Manufacturer = manufacturer != null ? manufacturer : "";
                Symbol = symbol.Length > 0 ? symbol : (GetSystemIconByName(TempName, Core.Name));

                OldCore = oldCore;
                CDSupport = false;
                DLLName = Path.GetFileName(Core.DLLName);
                SupportNoGame = Core.SupportNoGame;
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

                if (AnyCore)
                {
                    GameSystemAnyCore testCore = null;
                    if (!GameSystemSelectionViewModel.SystemsAnyCore.TryGetValue(TempName, out testCore))
                    {
                        if (!ImportedCore)
                        {
                            Manufacturer = "";
                        }
                        GameSystemSelectionViewModel.SystemsAnyCore.Add(TempName, new GameSystemAnyCore(Name, Manufacturer, Symbol, "", "", false, DLLName, multiFileExtensions != null));
                    }
                    else
                    {
                        if (testCore.CoreName?.Length > 0) Name = testCore.CoreName;
                        if (testCore.CoreSystem?.Length > 0)
                        {
                            Manufacturer = testCore.CoreSystem;
                        }
                        else
                        {
                            if (!ImportedCore)
                            {
                                Manufacturer = "";
                            }
                        }
                        testCore.DLLName = DLLName;
                        CDSupport = testCore.CDSupport;

                        if (testCore.CoreIcon?.Length > 0 && File.Exists(testCore.CoreIcon))
                        {
                            Symbol = testCore.CoreIcon;
                            try
                            {
                                AnyCoreIconsMap.Add(TempName, Symbol);
                            }
                            catch (Exception ex)
                            {

                            }
                        }

                    }

                }
                ManufacturerTemp = Manufacturer;

                FailedToLoad = Core.FailedToLoad;
                SkippedCore = skippedCore;

                Core.OriginalSystemName = Name;
                CoresOptions testOptions = null;
                var expectedName = $"{core.Name}_{TempName}";
                if (!GameSystemSelectionViewModel.SystemsOptions.TryGetValue(expectedName, out testOptions))
                {
                    if (!core.IsNewCore && !GameSystemSelectionViewModel.SystemsOptions.TryGetValue(TempName, out testOptions))
                    {
                        GameSystemSelectionViewModel.SystemsOptions.Add(expectedName, new CoresOptions(Core));
                        GameSystemSelectionViewModel.SystemsOptionsTemp.Add(expectedName, new CoresOptions(Core));
                    }
                    else
                    {
                        GameSystemSelectionViewModel.SystemsOptions.Add(expectedName, new CoresOptions(Core));
                        GameSystemSelectionViewModel.SystemsOptionsTemp.Add(expectedName, new CoresOptions(Core));
                    }
                }
                if (Core.Options != null && Core.Options.Count > 0)
                {
                    CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await GameSystemSelectionViewModel.CoreOptionsStoreAsyncDirect(Core.Name, Core.SystemName, Core.IsNewCore, "", true);
                    });
                }
                UpdateOpenCounts();
                Core.FreeLibretroCore();
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }
        public static bool SetCoreOptions(LibretroCore core, string systemName)
        {
            bool changesState = false;
            try
            {
                CoresOptions testOptions = null;
                var expectedName = $"{core.Name}_{systemName}";
                if (!GameSystemSelectionViewModel.SystemsOptions.TryGetValue(expectedName, out testOptions))
                {
                    if (!core.IsNewCore && !GameSystemSelectionViewModel.SystemsOptions.TryGetValue(systemName, out testOptions))
                    {
                        GameSystemSelectionViewModel.SystemsOptions.Add(expectedName, new CoresOptions(core));
                        GameSystemSelectionViewModel.SystemsOptionsTemp.Add(expectedName, new CoresOptions(core));
                    }
                    else
                    {
                        GameSystemSelectionViewModel.SystemsOptions.Add(expectedName, new CoresOptions(core));
                        GameSystemSelectionViewModel.SystemsOptionsTemp.Add(expectedName, new CoresOptions(core));
                    }
                }
                else if (testOptions.OptionsList.Count == 0)
                {
                    if (!core.IsNewCore && !GameSystemSelectionViewModel.SystemsOptions.TryGetValue(systemName, out testOptions))
                    {
                        GameSystemSelectionViewModel.SystemsOptions[expectedName] = new CoresOptions(core);
                        GameSystemSelectionViewModel.SystemsOptionsTemp[expectedName] = new CoresOptions(core);
                    }
                    else
                    {
                        GameSystemSelectionViewModel.SystemsOptions[expectedName] = new CoresOptions(core);
                        GameSystemSelectionViewModel.SystemsOptionsTemp[expectedName] = new CoresOptions(core);
                    }
                }
                else
                {
                    //Check for possible changes on some options by the core
                    var currentCoreOptions = core.Options;
                    foreach (var oItem in testOptions.OptionsList)
                    {
                        var key = oItem.Key;
                        var originalOption = oItem.Value;
                        var currentOption = currentCoreOptions[key];
                        if (originalOption.OptionsValues.Count != currentOption.Values.Count)
                        {
                            //The core changed values
                            //I should update
                            testOptions.OptionsList[key] = new CoreOptionsValues(key, core.Options[key]);
                            changesState = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            //It will return false when there is no changes on the current options
            return changesState;
        }
        public void UpdateOpenCounts()
        {
            try
            {
                int OpenCounter = PlatformService.GetPlaysCount(Core.Name, TempName, Core.IsNewCore);
                OpenTimes = OpenCounter;
                Manufacturer = ManufacturerTemp;
            }
            catch (Exception ex)
            {

            }
        }

        public bool CheckRootFolderRequired(StorageFile file)
        {
            UpdateOpenCounts();
            try
            {
                var extension = Path.GetExtension(file.Name);
                return MultiFileExtensions.Contains(extension);
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                return false;
            }
        }

        public async Task<bool> CheckDependenciesMetAsync()
        {
            try
            {
                var systemFolder = await GetSystemDirectoryAsync();

                foreach (var i in Dependencies)
                {
                    if (i.Optional)
                    {
                        continue;
                    }
                    var file = await FileImporterViewModel.GetTargetFileAsync(Core.Name, systemFolder, i.Name, i.IsFolder);
                    if (file == null)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                return false;
            }
        }

        public Task<StorageFolder> GetSystemDirectoryAsync()
        {
            string coreName = Core.Name;
            if (Core == null || coreName == null || coreName.Length == 0 || coreName.EndsWith(".dll"))
            {
                coreName = "Unknown";
            }
            else
            {
                //VICE cores will cause a lot of folders while they use almost the same BIOS files
                //it's better to make one shared folder for all of them
                switch (coreName)
                {
                    case "VICE x64":
                    case "VICE x64sc":
                    case "VICE xscpu64":
                    case "VICE x128":
                    case "VICE xcbm2":
                    case "VICE xcbm5x0":
                    case "VICE xpet":
                    case "VICE xplus4":
                    case "VICE xvic":
                        coreName = "VICE shared";
                        break;
                }
            }
            return GetCoreStorageDirectoryAsync($"{coreName} - System");
        }

        public Task<StorageFolder> GetSaveDirectoryAsync()
        {
            string coreName = Core.Name;
            if (Core == null || coreName == null || coreName.Length == 0 || coreName.EndsWith(".dll"))
            {
                coreName = "Unknown";
            }
            else
            {
                //VICE cores will cause a lot of folders while they use almost the same BIOS files
                //it's better to make one shared folder for all of them
                switch (coreName)
                {
                    case "VICE x64":
                    case "VICE x64sc":
                    case "VICE xscpu64":
                    case "VICE x128":
                    case "VICE xcbm2":
                    case "VICE xcbm5x0":
                    case "VICE xpet":
                    case "VICE xplus4":
                    case "VICE xvic":
                        coreName = "VICE shared";
                        break;
                }
            }
            return GetCoreStorageDirectoryAsync($"{coreName} - Saves");
        }
        public async Task<StorageFolder> GetGamesDirectoryAsync()
        {
            try
            {
                string coreName = Core.Name;
                if (Core == null || coreName == null || coreName.Length == 0 || coreName.EndsWith(".dll"))
                {
                    coreName = "Unknown";
                }
                var folderState = !await PlatformService.IsCoreGamesFolderAlreadySelected(folderPickerTokenName);

                if (folderState)
                {
                    if (PlatformService.IsCoreRequiredGamesFolder(coreName))
                    {
                        var ignoreGamesFolder = Core.ignoreGamesFolderSelection;

                        if (!ignoreGamesFolder)
                        {
                            var globalState = false;
                            try
                            {
                                var globalTest = await PlatformService.PickDirectory(folderPickerTokenName, false, true, true);
                                if (globalTest != null)
                                {
                                    globalState = true;
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                            if (!globalState)
                            {
                                try
                                {
                                    PlatformService.PlayNotificationSound("alert");
                                    await UserDialogs.Instance.AlertAsync("This core requires games folder\nIf you skipped this step 'system' folder will be used instead for this session.", "Games Folder", "Choose");
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }

                        var systemRootFolder = !ignoreGamesFolder ? await PlatformService.PickDirectory(folderPickerTokenName, false) : null;

                        if (systemRootFolder == null)
                        {
                            try
                            {
                                var globalTest = await PlatformService.PickDirectory(folderPickerTokenName, false, true, true);
                                if (globalTest != null)
                                {
                                    systemRootFolder = globalTest;
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }

                        if (systemRootFolder == null)
                        {
                            systemRootFolder = await GetSystemDirectoryAsync();
                        }
                        return systemRootFolder;
                    }
                    else
                    {
                        try
                        {
                            var globalTest = await PlatformService.PickDirectory(folderPickerTokenName, false, true, true);
                            if (globalTest != null)
                            {
                                return globalTest;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        return await GetSystemDirectoryAsync();
                    }
                }
                else
                {
                    var systemRootFolder = await PlatformService.PickDirectory(folderPickerTokenName, false);

                    if (systemRootFolder == null)
                    {
                        systemRootFolder = await GetSystemDirectoryAsync();
                    }
                    return systemRootFolder;
                }
            }
            catch (Exception ex)
            {
                return await GetSystemDirectoryAsync();
            }
        }

        public async Task<bool> SetGamesDirectoryAsync(bool reselect = false)
        {
            try
            {
                var systemRootFolder = await PlatformService.PickDirectory(folderPickerTokenName, reselect);

                if (systemRootFolder != null)
                {
                    StorageFolder testFolder = null;
                    if (GameSystemSelectionViewModel.SystemRoots.TryGetValue(folderPickerTokenName, out testFolder))
                    {
                        GameSystemSelectionViewModel.SystemRoots[folderPickerTokenName] = systemRootFolder;
                    }
                    else
                    {
                        GameSystemSelectionViewModel.SystemRoots.Add(folderPickerTokenName, systemRootFolder);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<StorageFolder> GetCoreStorageDirectoryAsync(string directoryName)
        {
            try
            {
                StorageFolder output = null;
                if (!Core.FailedToLoad && !Core.SkippedCore)
                {
                    output = await PlatformService.CreateLocalFolderDirectoryAsync(directoryName, true);
                }
                if (directoryName.ToLower().EndsWith("system"))
                {
                    //Check if user selected any custom location
                    var testSystemCustomFolder = await PlatformService.GetCustomFolder(Core.Name, "system");
                    if (testSystemCustomFolder != null)
                    {
                        output = testSystemCustomFolder;
                    }
                }
                else
                {
                    //Check if user selected any custom location
                    var testSavesCustomFolder = await PlatformService.GetCustomFolder(Core.Name, "saves");
                    if (testSavesCustomFolder != null)
                    {
                        output = testSavesCustomFolder;
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

        public static string GetSystemIconByName(string ConsoleName, string CoreName = "")
        {
            string SystemIcon = "ms-appx:///Assets/Icons/GN.png";
            switch (ConsoleName)
            {
                case "Amstrad CPC":
                case "Caprice32":
                    SystemIcon = "ms-appx:///Assets/Icons/Amstrad CPC.png";
                    break;
                case "MSX Computer":
                    switch (CoreName)
                    {
                        case "blueMSX":
                            SystemIcon = "ms-appx:///Assets/Icons/MSXCom.png";
                            break;

                        case "fMSX":
                            SystemIcon = "ms-appx:///Assets/Icons/MSX.png";
                            break;

                        default:
                            SystemIcon = "ms-appx:///Assets/Icons/MSXCom.png";
                            break;
                    }
                    break;
                case "Intellivision":
                    SystemIcon = "ms-appx:///Assets/Icons/Intellivision.png";
                    break;
                case "Sinclair ZX81":
                    SystemIcon = "ms-appx:///Assets/Icons/ZX81.png";
                    break;
                case "NES":
                    switch (CoreName)
                    {
                        case "QuickNES":
                            SystemIcon = "ms-appx:///Assets/Icons/NESQ.png";
                            break;

                        case "Nestopia":
                            SystemIcon = "ms-appx:///Assets/Icons/NESN.png";
                            break;

                        default:
                            SystemIcon = "ms-appx:///Assets/Icons/NES.png";
                            break;
                    }
                    break;
                case "Super Nintendo":
                    switch (CoreName)
                    {
                        case "Snes9x 2005":
                            SystemIcon = "ms-appx:///Assets/Icons/SNES2005.png";
                            break;

                        default:
                            SystemIcon = "ms-appx:///Assets/Icons/SNES.png";
                            break;
                    }
                    break;
                case "Virtual Boy":
                    SystemIcon = "ms-appx:///Assets/Icons/VirtualBoy.png";
                    break;
                case "NeoGeo CD":
                    SystemIcon = "ms-appx:///Assets/Icons/NeoGeo.png";
                    break;
                case "Nintendo 64":
                    SystemIcon = "ms-appx:///Assets/Icons/N64.png";
                    break;
                case "GameBoy":
                case "GameBoy+":
                    switch (CoreName)
                    {
                        case "Gearboy":
                            SystemIcon = "ms-appx:///Assets/Icons/GBC2.png";
                            break;
                        case "TGB Dual":
                            SystemIcon = "ms-appx:///Assets/Icons/GBC3.png";
                            break;
                        case "Gambatte":
                            SystemIcon = "ms-appx:///Assets/Icons/GBC1.png";
                            break;
                        case "VBA-M":
                            SystemIcon = "ms-appx:///Assets/Icons/GB.gif";
                            break;
                        default:
                            SystemIcon = "ms-appx:///Assets/Icons/GBC.png";
                            break;
                    }
                    break;
                case "MAME":
                    switch (CoreName)
                    {
                        case "MAME 2000":
                        case "MAME2000":
                            SystemIcon = "ms-appx:///Assets/Icons/MAME2000.png";
                            break;
                        default:
                            SystemIcon = "ms-appx:///Assets/Icons/MAME.png";
                            break;
                    }
                    break;
                case "GameBoy Color":
                    SystemIcon = "ms-appx:///Assets/Icons/GBC.gif";
                    break;
                case "GameBoy Advance":
                    SystemIcon = "ms-appx:///Assets/Icons/GBA2.gif";
                    break;
                case "Game Boy Advance (Meteor)":
                    SystemIcon = "ms-appx:///Assets/Icons/GBA.gif";
                    break;
                case "GameBoy Advance SP":
                    SystemIcon = "ms-appx:///Assets/Icons/GBASP.png";
                    break;
                case "GameBoy Micro":
                    SystemIcon = "ms-appx:///Assets/Icons/GBM.gif";
                    break;
                case "Nintendo DS":
                    switch (CoreName)
                    {
                        case "MelonDS":
                        case "Melon DS":
                            SystemIcon = "ms-appx:///Assets/Icons/NDS2.png";
                            break;

                        default:
                            SystemIcon = "ms-appx:///Assets/Icons/NDS4.png";
                            break;
                    }
                    break;

                case "Game & Watch":
                    SystemIcon = "ms-appx:///Assets/Icons/GW.png";
                    break;

                case "2048":
                    SystemIcon = "ms-appx:///Assets/Icons/2048.png";
                    break;

                case "Oberon RISC":
                case "Oberon":
                    SystemIcon = "ms-appx:///Assets/Icons/Oberon.png";
                    break;

                case "Jump n' Bump":
                case "Jump n Bump":
                case "JNB":
                    SystemIcon = "ms-appx:///Assets/Icons/JNB.png";
                    break;

                case "C64":
                    SystemIcon = "ms-appx:///Assets/Icons/vice-c64.png";
                    break;

                case "C128":
                    SystemIcon = "ms-appx:///Assets/Icons/vice-c128.png";
                    break;

                case "CPET":
                    SystemIcon = "ms-appx:///Assets/Icons/vice-pet.png";
                    break;

                case "Plus4":
                    SystemIcon = "ms-appx:///Assets/Icons/vice-plus4.png";
                    break;

                case "VIC-20":
                    SystemIcon = "ms-appx:///Assets/Icons/vice-vic20.png";
                    break;

                case "CBM-II":
                    switch (CoreName)
                    {
                        case "VICE xcbm5x0":
                            SystemIcon = "ms-appx:///Assets/Icons/vice-c6102.png";
                            break;

                        default:
                            SystemIcon = "ms-appx:///Assets/Icons/vice-c610.png";
                            break;
                    }
                    break;

                case "C64 SuperCPU":
                    switch (CoreName)
                    {
                        case "VICE xscpu64":
                            SystemIcon = "ms-appx:///Assets/Icons/vice-c64su2.png";
                            break;

                        default:
                            SystemIcon = "ms-appx:///Assets/Icons/vice-c64su1.png";
                            break;
                    }
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
                    switch (CoreName)
                    {
                        case "Yabause":
                            SystemIcon = "ms-appx:///Assets/Icons/SaturnYabause.png";
                            break;

                        default:
                            SystemIcon = "ms-appx:///Assets/Icons/Saturn.png";
                            break;
                    }
                    break;
                case "PlayStation 2":
                case "PCSX2":
                    SystemIcon = "ms-appx:///Assets/Icons/Playstation2.png";
                    break;
                case "PlayStation":
                case "ROCorePlayStation":
                    switch (CoreName)
                    {
                        case "PCSX ReARMed":
                        case "PCSX-ReARMed":
                            SystemIcon = "ms-appx:///Assets/Icons/PlaystationReARMed.png";
                            break;

                        case "DuckStation":
                            SystemIcon = "ms-appx:///Assets/Icons/PlaystationDuck.png";
                            break;

                        default:
                            SystemIcon = "ms-appx:///Assets/Icons/Playstation.png";
                            break;
                    }
                    break;
                case "Playstation Portable":
                    SystemIcon = "ms-appx:///Assets/Icons/PSP.png";
                    break;
                case "PCEngine Fast":
                    SystemIcon = "ms-appx:///Assets/Icons/PCEFast.png";
                    break;
                case "PC Engine":
                    switch (CoreName)
                    {
                        case "Beetle PCE Fast":
                        case "Mednafen PCE Fast":
                            SystemIcon = "ms-appx:///Assets/Icons/PCEFast.png";
                            break;

                        default:
                            SystemIcon = "ms-appx:///Assets/Icons/PCE.png";
                            break;
                    }
                    break;
                case "PC Engine CD":
                    SystemIcon = "ms-appx:///Assets/Icons/PCECD.png";
                    break;
                case "PC-FX":
                    SystemIcon = "ms-appx:///Assets/Icons/PCEFX.png";
                    break;
                case "WonderSwan Color":
                    SystemIcon = "ms-appx:///Assets/Icons/WonderSwanColor.png";
                    break;
                case "WonderSwan":
                    SystemIcon = "ms-appx:///Assets/Icons/WonderSwan.png";
                    break;
                case "Cave Story":
                    SystemIcon = "ms-appx:///Assets/Icons/Cave Story.gif";
                    break;

                case "MiniVMac":
                case "Mini vMac":
                case "vMac":
                    SystemIcon = "ms-appx:///Assets/Icons/MiniVMac.png";
                    break;

                case "Arcade":
                    switch (CoreName)
                    {
                        case "FinalBurn Neo":
                            SystemIcon = "ms-appx:///Assets/Icons/Arcade.png";
                            break;

                        case "FB Alpha":
                            SystemIcon = "ms-appx:///Assets/Icons/ArcadeAlpha.png";
                            break;
                    }
                    break;
                case "NeoGeo Pocket":
                case "Neo Geo Pocket":
                    switch (CoreName)
                    {
                        case "RACE":
                            SystemIcon = "ms-appx:///Assets/Icons/NeoGeo PocketRACE.png";
                            break;

                        default:
                            SystemIcon = "ms-appx:///Assets/Icons/NeoGeo Pocket.png";
                            break;
                    }
                    break;
                case "Neo Geo":
                    SystemIcon = "ms-appx:///Assets/Icons/NeoGeo.png";
                    break;
                case "PolyGame Master":
                    SystemIcon = "ms-appx:///Assets/Icons/Arcade 2.png";
                    break;
                case "GameCube":
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

                    switch (CoreName)
                    {
                        case "Lynx":
                        case "Mednafen Lynx":
                        case "Beetle Lynx":
                            SystemIcon = "ms-appx:///Assets/Icons/Lynx.png";
                            break;

                        case "Handy":
                            SystemIcon = "ms-appx:///Assets/Icons/LynxHandy.png";
                            break;
                    }
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

                case "ColecoVision":
                    SystemIcon = "ms-appx:///Assets/Icons/ColecoVision.png";
                    break;
                case "OutRun":
                case "Cannonball":
                    SystemIcon = "ms-appx:///Assets/Icons/OutRun.png";
                    break;
                case "DOSBox":
                    SystemIcon = "ms-appx:///Assets/Icons/DOSBox.png";
                    break;
                case "DOSBox Pure":
                    SystemIcon = "ms-appx:///Assets/Icons/DOSBoxPure.png";
                    break;
                case "DOSBox SVN":
                    SystemIcon = "ms-appx:///Assets/Icons/DOSBoxSVN.png";
                    break;
                case "ScummVM":
                    SystemIcon = "ms-appx:///Assets/Icons/ScummVM.png";
                    break;
                case "Fairchild ChannelF":
                case "FreeChaf":
                    SystemIcon = "ms-appx:///Assets/Icons/ChannelF.png";
                    break;
                case "SEGA Wide":
                    SystemIcon = "ms-appx:///Assets/Icons/MegaDriveWide.png";
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
                case "Flashback":
                    SystemIcon = "ms-appx:///Assets/Icons/Reminiscence.png";
                    break;
                case "Atari 2600":
                    switch (CoreName)
                    {
                        case "Stella 2014":
                            SystemIcon = "ms-appx:///Assets/Icons/Atari2600 2014.png";
                            break;

                        default:
                            SystemIcon = "ms-appx:///Assets/Icons/Atari2600.png";
                            break;
                    }
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
                        if (AnyCoreIconsMap.TryGetValue(ConsoleName, out testIcon))
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
