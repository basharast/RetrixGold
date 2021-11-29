using Acr.UserDialogs;
using LibRetriX;
using MvvmCross.Core.ViewModels;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Services;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace RetriX.Shared.ViewModels
{
    public class FileImporterViewModel : MvxViewModel
    {
        public const string SerachLinkFormat = "https://www.google.com/search?q={0}";

        private readonly IFileSystem FileSystem;
        private readonly IUserDialogs DialogsService;
        private readonly IPlatformService PlatformService;
        private readonly ICryptographyService CryptographyService;

        public IDirectoryInfo TargetFolder { get; }
        public string TargetFileName { get; }
        public string TargetSystem { get; }
        public string TargetDescription { get; }
        public string TargetDescriptionOptional { get; }
        public string TargetMD5 { get; }
        public string ButtonTitle { get; set; }
        public bool DisplayDescriptions { get; }
        public bool DisplayDescriptionsOptional { get; }

        public string SearchLink => string.Format(SerachLinkFormat, TargetMD5);

        private bool fileAvailable = false;
        public string FieldColor = "Red";
        public bool FileAvailable
        {
            get => fileAvailable;
            private set { if (SetProperty(ref fileAvailable, value)) { ImportCommand.RaiseCanExecuteChanged(); } }
        }

        public IMvxCommand ImportCommand { get; }
        public IMvxCommand CopyMD5ToClipboardCommand { get; }

        protected FileImporterViewModel(ICore core, IFileSystem fileSystem, IUserDialogs dialogsService, IPlatformService platformService, ICryptographyService cryptographyService, IDirectoryInfo folder, string fileName, string description, string MD5)
        {
            try
            {
                FileSystem = fileSystem;
                DialogsService = dialogsService;
                PlatformService = platformService;
                CryptographyService = cryptographyService;

                TargetFolder = folder;
                TargetFileName = fileName;
                TargetDescription = description.Contains("Optional")?"": description;
                TargetDescriptionOptional = description.Contains("Optional") ? description : "";
                TargetSystem = core.OriginalSystemName;
                TargetMD5 = MD5;

                DisplayDescriptions = !description.Contains("Optional");
                DisplayDescriptionsOptional = !DisplayDescriptions;
                if (DisplayDescriptionsOptional)
                {
                    FieldColor = "CornflowerBlue";
                }
                else
                {
                    FieldColor = "Red";
                }
                RaisePropertyChanged(nameof(FieldColor));
                ImportCommand = new MvxCommand(ImportHandler);
                CopyMD5ToClipboardCommand = new MvxCommand(() => PlatformService.CopyToClipboard(TargetMD5));
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        public static async Task<FileImporterViewModel> CreateFileImporterAsync(ICore core,IFileSystem fileSystem, IUserDialogs dialogsService, IPlatformService platformService, ICryptographyService cryptographyService, IDirectoryInfo folder, string fileName, string description, string MD5)
        {
            try
            {
                var output = new FileImporterViewModel(core, fileSystem, dialogsService, platformService, cryptographyService, folder, fileName, description, MD5);
                var targetFile = await output.GetTargetFileAsync();
                output.FileAvailable = targetFile != null;
                if (output.FileAvailable)
                {
                    output.ButtonTitle = "Delete";
                    output.RaisePropertyChanged(nameof(ButtonTitle));
                    if (!output.DisplayDescriptionsOptional) { 
                    output.FieldColor = "Green";
                    output.RaisePropertyChanged(nameof(FieldColor));
                    }

                }
                else
                {
                    output.ButtonTitle = "Import";
                    output.RaisePropertyChanged(nameof(ButtonTitle));
                }
                return output;
            }catch(Exception e)
            {
                return null;
            }
        }

        public Task<IFileInfo> GetTargetFileAsync()
        {
            try
            {
                var TargetFileTemp = TargetFolder.GetFileAsync(TargetFileName);
                return TargetFileTemp;
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return null;
            }
        }

        string[] IgnoreVerificationArray = new string[] { "neogeo.zip", "pgm.zip", "NstDatabase.zip", "bubsys.zip", "cchip.zip", "coleco.zip", "decocass.zip", "fdsbios.zip", "isgsm.zip", "midssio.zip", "neocdz.zip", "nmk004.zip", "skns.zip", "spec128.zip", "spectrum.zip", "ym2608.zip", "msx.zip" };
        private async void ImportHandler()
        {
            try
            {
                if (FileAvailable)
                {
                    PlatformService?.PlayNotificationSound("notice.mp3");
                    ConfirmConfig confirmDelete = new ConfirmConfig();
                    confirmDelete.SetTitle("Delete BIOS");
                    confirmDelete.SetMessage("Do you want to delete this BIOS?");
                    confirmDelete.UseYesNo();
                    bool confirmDeletetResult = await UserDialogs.Instance.ConfirmAsync(confirmDelete);
                    if (confirmDeletetResult)
                    {
                        var TargetBIOS = await TargetFolder.GetFileAsync(TargetFileName);
                        await TargetBIOS.DeleteAsync();
                        TargetBIOS = await TargetFolder.GetFileAsync(TargetFileName);
                        if (TargetBIOS==null) { 
                        PlatformService.PlayNotificationSound("alert.wav");
                        await DialogsService.AlertAsync("BIOS deleted successfully", "Delete BIOS");
                            FileAvailable = false;
                            ButtonTitle = "Import";
                            RaisePropertyChanged(nameof(ButtonTitle));
                            if (!DisplayDescriptionsOptional)
                            {
                                FieldColor = "Red";
                                RaisePropertyChanged(nameof(FieldColor));
                            }
                        }
                        else
                        {
                            PlatformService.PlayNotificationSound("faild.wav");
                            await DialogsService.AlertAsync("Failed to delete BIOS file", "Delete BIOS");
                        }
                    }

                    return;
                }
            }catch(Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
            }


            try
            {
                var fileExt = Path.GetExtension(TargetFileName);
                var sourceFile = await FileSystem.PickFileAsync(new string[] { fileExt });
                if (sourceFile == null)
                {
                    return;
                }
                string filename = sourceFile.Name;
                var md5 = await CryptographyService.ComputeMD5Async(sourceFile);
                var crc32 = await GetFileCRC32(sourceFile);
                bool IgnoreVerification = IgnoreVerificationArray.Contains(filename); //Not in use anymore all bios allowed be force import
                if (md5.ToLowerInvariant() != TargetMD5.ToLowerInvariant() && crc32.ToLowerInvariant() != TargetMD5.ToLowerInvariant())
                {
                    //if (IgnoreVerification)
                    //{
                        PlatformService?.PlayNotificationSound("notice.mp3");
                        ConfirmConfig confirmImport = new ConfirmConfig();
                        confirmImport.SetTitle(Resources.Strings.FileHashMismatchTitle);
                        confirmImport.SetMessage("This file doesn't match the requested BIOS, do you want to import anyway?");
                        confirmImport.UseYesNo();
                        bool confirmImportResult = await UserDialogs.Instance.ConfirmAsync(confirmImport);
                        if (!confirmImportResult)
                        {
                            return;
                        }
                    //}
                    /*else { 
                    var title = Resources.Strings.FileHashMismatchTitle;
                    var message = Resources.Strings.FileHashMismatchMessage;
                    PlatformService.PlayNotificationSound("faild.wav");
                    await DialogsService.AlertAsync(message, title);
                    return;
                    }*/
                }

                using (var inStream = await sourceFile.OpenAsync(FileAccess.Read))
                {
                    var targetFile = await TargetFolder.CreateFileAsync(TargetFileName);
                    using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                    {
                        await inStream.CopyToAsync(outStream);
                        await outStream.FlushAsync();
                    }

                    FileAvailable = true;
                    ButtonTitle = "Delete";
                    RaisePropertyChanged(nameof(ButtonTitle));
                    if (!DisplayDescriptionsOptional)
                    {
                        FieldColor = "Green";
                        RaisePropertyChanged(nameof(FieldColor));
                    }
                }

                PlatformService.PlayNotificationSound("alert.wav");
                await DialogsService.AlertAsync("BIOS imported successfully", "Import BIOS");
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
            }
        }

        async Task<string> GetFileCRC32(IFileInfo sourceFile)
        {
            try {
                var FileExtension = Path.GetExtension(sourceFile.Name);
                if (!FileExtension.ToLower().Contains("zip"))
                {
                    return "0x0000000";
                }
            using (var outStream = await sourceFile.OpenAsync(FileAccess.Read))
            {
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
            }catch(Exception e)
            {
                PlatformService?.ShowErrorMessage(e);
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
