using LibRetriX;
using LibRetriX.RetroBindings;
using RetriX.Shared.Services;
using RetriX.UWP;
using RetriX.UWP.Pages;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.UI;
using Windows.UI.Xaml.Media;
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
    public class FileImporterViewModel : BindableBase
    {
        public const string SerachLinkFormat = "https://duckduckgo.com/?q={0}";

        public StorageFolder TargetFolder { get; }
        public string TargetFileName { get; }
        public string TargetSystem { get; }
        public string TargetCore { get; }
        public string TargetDescription { get; }
        public string TargetMD5 { get; }
        public string buttonTitle { get; set; }
        public string ButtonTitle
        {
            get
            {
                return buttonTitle;
            }
            set
            {
                buttonTitle = value;
                RaisePropertyChanged(nameof(buttonColor));
            }
        }
        public bool isOptional { get; }
        public bool DisplayDescriptions { get; }
        public bool DisplayMD5 = true;
        public string bIOSIcon = "ms-appx:///Assets/Icons/Menus/exe.png";
        public string BIOSIcon
        {
            get
            {
                return bIOSIcon;
            }
            set
            {
                var transcodedState = GameSystemSelectionView.GetTranscodedImage(value, ref bIOSIcon);
                if (!transcodedState)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var transcodedImage = await GameSystemSelectionView.ConvertPngToBmp(value);
                            if (!transcodedImage.Equals(bIOSIcon))
                            {
                                bIOSIcon = transcodedImage;
                                RaisePropertyChanged(bIOSIcon);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                }
            }
        }
        public string TargetMD5Lable = "MD5: ";
        public string TargetFileLable = "File: ";
        public SolidColorBrush buttonColor
        {
            get
            {
                if (ButtonTitle.Equals("Delete"))
                {
                    return new SolidColorBrush(Colors.Green);
                }
                else
                {
                    if (isOptional)
                    {
                        return new SolidColorBrush(Colors.Gray);
                    }
                    else
                    {
                        return new SolidColorBrush(Colors.DarkRed);
                    }
                }
            }
        }

        public string SearchLink
        {
            get
            {
                return string.Format(SerachLinkFormat, TargetMD5);
            }
        }

        private bool fileAvailable = false;
        public string FileColor = "Red";
        public bool FileAvailable
        {
            get => fileAvailable;
            private set { fileAvailable = value; }
        }
        public bool isFolder = false;

        public ICommand ImportCommand { get; }
        public ICommand CopyMD5ToClipboardCommand { get; }

        protected FileImporterViewModel(LibretroCore core, StorageFolder folder, string fileName, string description, string MD5, bool optional, bool isfolder)
        {
            try
            {
                TargetFolder = folder;
                TargetFileName = fileName;
                TargetDescription = description.Replace("(Optional)", "");
                TargetSystem = core.OriginalSystemName;
                TargetCore = core.Name;
                TargetMD5 = MD5;
                isFolder = isfolder;
                DisplayMD5 = !isFolder;
                isOptional = optional;

                if (isFolder)
                {
                    BIOSIcon = "ms-appx:///Assets/Icons/Menus/folder-wtools.png";
                    TargetMD5Lable = "Info: ";
                    TargetFileLable = "Folder: ";
                }
                else
                {
                    BIOSIcon = "ms-appx:///Assets/Icons/Menus/exe.png";
                }

                DisplayDescriptions = description.Length > 0 && !description.Equals("-");
                if (isOptional)
                {
                    FileColor = "CornflowerBlue";
                }
                else
                {
                    FileColor = "Red";
                }
                RaisePropertyChanged(nameof(FileColor));
                ImportCommand = new Command(ImportHandlerCommand);
                CopyMD5ToClipboardCommand = new Command(() => PlatformService.CopyToClipboard(TargetMD5));
                RaisePropertyChanged(nameof(TargetFolder));
                RaisePropertyChanged(nameof(TargetFileName));
                RaisePropertyChanged(nameof(TargetDescription));
                RaisePropertyChanged(nameof(TargetSystem));
                RaisePropertyChanged(nameof(TargetMD5));
                RaisePropertyChanged(nameof(DisplayDescriptions));

            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }
        }

        public static async Task<FileImporterViewModel> CreateFileImporterAsync(LibretroCore core, StorageFolder folder, string fileName, string description, string MD5, bool optional, bool isfolder)
        {
            try
            {
                var output = new FileImporterViewModel(core, folder, fileName, description, MD5, optional, isfolder);
                var targetFile = await GetTargetFileAsync(core.Name, folder, fileName, isfolder);
                output.FileAvailable = targetFile != null;
                if (output.FileAvailable)
                {
                    output.ButtonTitle = "Delete";
                    output.RaisePropertyChanged(nameof(ButtonTitle));
                    if (!output.isOptional)
                    {
                        output.FileColor = "Green";
                        output.RaisePropertyChanged(nameof(FileColor));
                    }
                }
                else
                {
                    output.ButtonTitle = "Import";
                    output.RaisePropertyChanged(nameof(ButtonTitle));
                }
                output.RaisePropertyChanged(nameof(buttonColor));
                return output;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static async Task<IStorageItem> GetTargetFileAsync(string core, StorageFolder folder, string file, bool isfolder)
        {
            IStorageItem TargetFileTemp = null;
            try
            {
                //Some cores has many cases for path
                //like FBNeo: files can be directly on system folder
                //or in sub folder called fbneo
                if (isfolder)
                {
                    TargetFileTemp = await folder.TryGetItemAsync(file);
                }
                else
                {
                    if (folder != null)
                    {
                        switch (core)
                        {
                            case "FinalBurn Neo":
                                TargetFileTemp = await folder.TryGetItemAsync(file);
                                if (TargetFileTemp == null)
                                {
                                    var testFolder = (StorageFolder)await folder.TryGetItemAsync("fbneo");
                                    if (testFolder != null)
                                    {
                                        TargetFileTemp = await testFolder.TryGetItemAsync(file);
                                    }
                                }
                                break;

                            case "NeoCD":
                                TargetFileTemp = await folder.TryGetItemAsync(file);
                                if (TargetFileTemp == null)
                                {
                                    var testFolder = (StorageFolder)await folder.TryGetItemAsync("neocd");
                                    if (testFolder != null)
                                    {
                                        TargetFileTemp = await testFolder.TryGetItemAsync(file);
                                    }
                                }
                                break;


                            case "VICE x64":
                            case "VICE x64sc":
                            case "VICE xscpu64":
                            case "VICE x128":
                            case "VICE xcbm2":
                            case "VICE xcbm5x0":
                            case "VICE xpet":
                            case "VICE xplus4":
                            case "VICE xvic":
                                TargetFileTemp = await folder.TryGetItemAsync(file);
                                if (TargetFileTemp == null)
                                {
                                    var testFolder = (StorageFolder)await folder.TryGetItemAsync("vice");
                                    if (testFolder != null)
                                    {
                                        TargetFileTemp = await testFolder.TryGetItemAsync(file);
                                        if (TargetFileTemp == null)
                                        {
                                            //SuperCPU Kernal files usually in system/vice/SCPU64.
                                            var testFolderSCPU = (StorageFolder)await testFolder.TryGetItemAsync("SCPU64");
                                            if (testFolderSCPU != null)
                                            {
                                                TargetFileTemp = await testFolderSCPU.TryGetItemAsync(file);
                                            }
                                        }
                                    }
                                }
                                break;

                            default:
                                TargetFileTemp = await folder.TryGetItemAsync(file);
                                break;
                        }
                    }
                }
                return TargetFileTemp;
            }
            catch (Exception e)
            {
                //PlatformService.ShowErrorMessage(e);
                PlatformService.ShowNotificationMain($"Error: {e.Message}", 3);
                return null;
            }
        }

        public void ImportHandlerCommand()
        {
            ImportHandler();
        }
        public async Task ImportHandler(StorageFolder directFolder = null)
        {
            var autoFinderCall = directFolder != null;

            try
            {
                if (FileAvailable)
                {
                    if (autoFinderCall)
                    {
                        return;
                    }
                    PlatformService.PlayNotificationSound("notice");
                    ConfirmConfig confirmDelete = new ConfirmConfig();
                    confirmDelete.SetTitle("Delete BIOS");
                    confirmDelete.SetMessage("Do you want to delete this BIOS?");
                    confirmDelete.UseYesNo();
                    bool confirmDeletetResult = await UserDialogs.Instance.ConfirmAsync(confirmDelete);
                    if (confirmDeletetResult)
                    {
                        if (isFolder)
                        {
                            var TargetBIOS = (StorageFolder)await TargetFolder.TryGetItemAsync(TargetFileName);
                            await TargetBIOS.DeleteAsync();
                            TargetBIOS = (StorageFolder)await TargetFolder.TryGetItemAsync(TargetFileName);
                            if (TargetBIOS == null)
                            {
                                PlatformService.PlayNotificationSound("alert");
                                PlatformService.ShowNotificationMain("BIOS deleted successfully", 3);
                                FileAvailable = false;
                                ButtonTitle = "Import";
                                RaisePropertyChanged(nameof(ButtonTitle));
                                if (!isOptional)
                                {
                                    FileColor = "Red";
                                    RaisePropertyChanged(nameof(FileColor));
                                }
                            }
                            else
                            {
                                PlatformService.PlayNotificationSound("faild");
                                PlatformService.ShowNotificationMain("Failed to delete BIOS file", 3);
                            }
                        }
                        else
                        {
                            var TargetBIOS = (StorageFile)await TargetFolder.TryGetItemAsync(TargetFileName);
                            await TargetBIOS.DeleteAsync();
                            TargetBIOS = (StorageFile)await TargetFolder.TryGetItemAsync(TargetFileName);
                            if (TargetBIOS == null)
                            {
                                PlatformService.PlayNotificationSound("alert");
                                PlatformService.ShowNotificationMain("BIOS deleted successfully", 3);
                                FileAvailable = false;
                                ButtonTitle = "Import";
                                RaisePropertyChanged(nameof(ButtonTitle));
                                if (!isOptional)
                                {
                                    FileColor = "Red";
                                    RaisePropertyChanged(nameof(FileColor));
                                }
                            }
                            else
                            {
                                PlatformService.PlayNotificationSound("faild");
                                PlatformService.ShowNotificationMain("Failed to delete BIOS file", 3);
                            }
                        }
                    }

                    return;
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
            }


            try
            {
                if (isFolder)
                {
                    if (autoFinderCall)
                    {
                        //Auto find folders could lead to long process, better to avoid
                        return;
                    }
                    if (PlatformService.ImportFolderHandler == null)
                    {
                        PlatformService.PlayNotificationSound("alert");
                        PlatformService.ShowNotificationMain("Use Import folder from Main page", 4);
                    }
                    else
                    {
                        ImportFolderHandler.Invoke(TargetFileName, EventArgs.Empty);
                    }
                    return;
                }
                var fileExt = Path.GetExtension(TargetFileName);
                StorageFile sourceFile = null;
                bool folderSelectionCancelled = false;

                try
                {
                    if (!autoFinderCall)
                    {
                        PlatformService.PlayNotificationSound("alert");
                    }
                    string DialogTitle = "Import file";
                    string DialogMessage = $"Select the file by your self or choose 'Find' to let RetriXGold search into specific folder";
                    string[] DialogButtons = new string[] { $"Find", "Select", "Cancel" };
                    int[] DialogButtonsIds = new int[] { 2, 1, 3 };

                    var ReplacePromptDialog = Helpers.CreateDialog(DialogTitle, DialogMessage, DialogButtons);
                    WinUniversalTool.Views.DialogResultCustom dialogResultCustom = autoFinderCall ? WinUniversalTool.Views.DialogResultCustom.Yes : WinUniversalTool.Views.DialogResultCustom.Nothing;
                    var ReplaceResult = await ReplacePromptDialog.ShowAsync2(dialogResultCustom);
                    if (Helpers.DialogResultCheck(ReplacePromptDialog, 3))
                    {
                        return;
                    }
                    else if (Helpers.DialogResultCheck(ReplacePromptDialog, 2))
                    {
                        FolderPicker picker = new FolderPicker();
                        picker.ViewMode = PickerViewMode.List;
                        picker.SuggestedStartLocation = PickerLocationId.Downloads;
                        picker.FileTypeFilter.Add("*");
                        var folder = autoFinderCall ? directFolder : await picker.PickSingleFolderAsync();
                        if (folder != null)
                        {
                            ButtonTitle = "Searching..";
                            RaisePropertyChanged(nameof(ButtonTitle));
                            var fileFound = false;
                            List<string> fileTypeFilter = new List<string>();
                            fileTypeFilter.Add(fileExt);
                            QueryOptions queryOptions = new QueryOptions(CommonFileQuery.OrderByName, fileTypeFilter);
                            queryOptions.FolderDepth = PlatformService.UseWindowsIndexerSubFolders ? FolderDepth.Deep : FolderDepth.Shallow;
                            if (PlatformService.UseWindowsIndexer)
                            {
                                queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
                            }
                            queryOptions.ApplicationSearchFilter = $"System.FileName:{TargetFileName}";

                            StorageFileQueryResult queryResult = folder.CreateFileQueryWithOptions(queryOptions);
                            var files = await queryResult.GetFilesAsync();
                            if (files != null && files.Count > 0)
                            {
                                foreach (var file in files)
                                {
                                    var md5Test = await CryptographyService.ComputeMD5AsyncDirect(file);
                                    var crc32Test = await GetFileCRC32(file);
                                    if (md5Test.ToLowerInvariant() == TargetMD5.ToLowerInvariant() || crc32Test.ToLowerInvariant() == TargetMD5.ToLowerInvariant())
                                    {
                                        sourceFile = file;
                                        fileFound = true;
                                        break;
                                    }
                                }
                                if (!fileFound)
                                {
                                    sourceFile = files.FirstOrDefault();
                                }
                            }
                            if (!fileFound && sourceFile == null)
                            {
                                if (!autoFinderCall)
                                {
                                    PlatformService.PlayNotificationSound("notice");
                                    PlatformService.ShowNotificationMain("Unable to find the file!", 3);
                                }
                                ButtonTitle = "Import";
                                RaisePropertyChanged(nameof(ButtonTitle));
                                return;
                            }
                        }
                        else
                        {
                            folderSelectionCancelled = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!autoFinderCall)
                    {
                        PlatformService.PlayNotificationSound("notice");
                        PlatformService.ShowNotificationMain("Unable to find the file!", 3);
                    }
                    ButtonTitle = "Import";
                    RaisePropertyChanged(nameof(ButtonTitle));
                    return;
                }

                if (sourceFile == null && !folderSelectionCancelled)
                {
                    sourceFile = await PlatformService.PickSingleFile(new string[] { fileExt });
                }
                if (sourceFile == null)
                {
                    return;
                }
                string filename = sourceFile.Name;
                var md5 = await CryptographyService.ComputeMD5AsyncDirect(sourceFile);
                var crc32 = await GetFileCRC32(sourceFile);
                if (md5.ToLowerInvariant() != TargetMD5.ToLowerInvariant() && crc32.ToLowerInvariant() != TargetMD5.ToLowerInvariant())
                {
                    if ((TargetMD5.Length > 0 && !TargetMD5.Contains(".")))
                    {
                        PlatformService.PlayNotificationSound("notice");
                        ConfirmConfig confirmImport = new ConfirmConfig();
                        confirmImport.SetTitle(GetLocalString("FileHashMismatchTitle"));
                        var extraNote = "";
                        if (isOptional)
                        {
                            extraNote = "\nNote: This BIOS is optional you can ignore it";
                        }
                        if (crc32 != "0x0000000")
                        {
                            confirmImport.SetMessage($"Important: This file ({filename}) doesn't match the requested BIOS\n\nRequired: {TargetMD5.ToLowerInvariant()}\nSelected (MD5):\n{md5.ToLowerInvariant()}\n\nSelected (CRC):\n{crc32}\n\nDo you want to import anyway?{extraNote}");
                        }
                        else
                        {
                            confirmImport.SetMessage($"Important: This file ({filename}) doesn't match the requested BIOS\n\nRequired:\n{TargetMD5.ToLowerInvariant()}\n\nSelected (MD5):\n{md5.ToLowerInvariant()}\n\nDo you want to import anyway?{extraNote}");
                        }
                        confirmImport.UseYesNo();
                        confirmImport.SetOkText("Import");
                        bool confirmImportResult = await UserDialogs.Instance.ConfirmAsync(confirmImport);
                        if (!confirmImportResult)
                        {
                            ButtonTitle = "Import";
                            RaisePropertyChanged(nameof(ButtonTitle));
                            return;
                        }
                    }
                }

                await sourceFile.CopyAsync(TargetFolder, sourceFile.Name, NameCollisionOption.ReplaceExisting);
                FileAvailable = true;
                ButtonTitle = "Delete";
                RaisePropertyChanged(nameof(ButtonTitle));
                if (!isOptional)
                {
                    FileColor = "Green";
                    RaisePropertyChanged(nameof(FileColor));
                }

                if (!autoFinderCall)
                {
                    PlatformService.PlayNotificationSound("alert");
                    PlatformService.ShowNotificationMain("BIOS imported successfully", 3);
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                if (fileAvailable)
                {
                    ButtonTitle = "Delete";
                    FileColor = "Green";
                }
                else
                {
                    ButtonTitle = "Import";
                    FileColor = "Red";
                }
                RaisePropertyChanged(nameof(ButtonTitle));
                RaisePropertyChanged(nameof(FileColor));
            }
        }

        async Task<string> GetFileCRC32(StorageFile sourceFile)
        {
            try
            {
                var FileExtension = Path.GetExtension(sourceFile.Name);
                if (!FileExtension.ToLower().Contains("zip"))
                {
                    return "0x0000000";
                }
                using (var stream = await sourceFile.OpenAsync(FileAccessMode.Read))
                {
                    var outStream = stream.AsStream();
                    var parentArchive = new ZipArchive(outStream);

                    ZipArchiveEntry entry = parentArchive.Entries.OrderByDescending(key => key.Length).ElementAt(0);
                    CRC32 crc32 = new CRC32();
                    String hash = String.Empty;

                    using (var outStreamEntry = entry.Open())
                        foreach (byte b in crc32.ComputeHash(outStreamEntry)) hash += b.ToString("x2").ToLower();

                    parentArchive.Dispose();
                    outStream.Dispose();
                    return hash;

                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessage(e);
                return "0x0000000";
            }
        }

    }
    public class CRC32
    {
        private readonly uint[] ChecksumTable;
        private readonly uint Polynomial = 0xEDB88320;

        public CRC32()
        {
            ChecksumTable = new uint[0x100];

            for (uint index = 0; index < 0x100; ++index)
            {
                uint item = index;
                for (int bit = 0; bit < 8; ++bit)
                    item = ((item & 1) != 0) ? (Polynomial ^ (item >> 1)) : (item >> 1);
                ChecksumTable[index] = item;
            }
        }

        public byte[] ComputeHash(Stream stream)
        {
            uint result = 0xFFFFFFFF;

            int current;
            while ((current = stream.ReadByte()) != -1)
                result = ChecksumTable[(result & 0xFF) ^ (byte)current] ^ (result >> 8);

            byte[] hash = BitConverter.GetBytes(~result);
            Array.Reverse(hash);
            return hash;
        }

        public byte[] ComputeHash(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
                return ComputeHash(stream);
        }
    }
}
