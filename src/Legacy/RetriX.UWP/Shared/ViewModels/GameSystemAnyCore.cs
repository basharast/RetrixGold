using Newtonsoft.Json;
using RetriX.UWP.Pages;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
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
    public class GameSystemAnyCore : BindableBase
    {
        public string DLLName = "";
        public string CoreName = "";
        public string CoreSystem = "";
        public string coreIcon = "";
        public string CoreIcon
        {
            get
            {
                return coreIcon;
            }
            set
            {
                var transcodedState = GameSystemSelectionView.GetTranscodedImage(value, ref coreIcon);
                if (!transcodedState)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var transcodedImage = await GameSystemSelectionView.ConvertPngToBmp(value);
                            if (!transcodedImage.Equals(coreIcon))
                            {
                                coreIcon = transcodedImage;
                                RaisePropertyChanged(coreIcon);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                }
            }
        }
        public string CoreYear = "";
        public string CoreDescription = "";
        public bool Pinned = false;
        public bool CDSupport = false;

        public Dictionary<string, string[]> BiosFiles = new Dictionary<string, string[]>();

        public GameSystemAnyCore(string coreName, string coreSystem = "", string coreIcon = "", string coreYear = "", string coreDescription = "", bool pinned = false, string dllName = "", bool cdSupport = false)
        {
            CoreName = coreName;
            CoreSystem = coreSystem;
            CoreIcon = coreIcon;
            CoreYear = coreYear;
            CoreDescription = coreDescription;
            Pinned = pinned;
            DLLName = dllName;
            CDSupport = cdSupport;
            RetriveBIOSMap();
        }
        public async Task RetriveBIOSMap()
        {
            try
            {
                var CoreFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("AnyCore");
                if (CoreFolder != null)
                {
                    var testBIOSMap = (StorageFile)await CoreFolder.TryGetItemAsync($"{DLLName.Replace(".dll", "")}.rab");
                    if (testBIOSMap != null)
                    {
                        Encoding unicode = Encoding.Unicode;
                        byte[] result;
                        using (var stream = await testBIOSMap.OpenAsync(FileAccessMode.Read))
                        {
                            var outStream = stream.AsStream();
                            using (var memoryStream = new MemoryStream())
                            {
                                outStream.CopyTo(memoryStream);
                                result = memoryStream.ToArray();
                            }
                            await outStream.FlushAsync();
                        }
                        string CoreFileContent = "";
                        Dictionary<string, string[]> dictionaryList = null;
                        try
                        {
                            CoreFileContent = unicode.GetString(result);
                            dictionaryList = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(CoreFileContent);
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                unicode = Encoding.UTF8;
                                CoreFileContent = unicode.GetString(result);
                                dictionaryList = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(CoreFileContent);
                            }
                            catch
                            {
                                unicode = Encoding.ASCII;
                                CoreFileContent = unicode.GetString(result);
                                dictionaryList = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(CoreFileContent);
                            }
                        }

                        if (dictionaryList != null)
                        {
                            BiosFiles = dictionaryList;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                try
                {
                    //PlatformService.ShowErrorMessageDirect(e);
                    var CoreFolder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("AnyCore");
                    if (CoreFolder != null)
                    {
                        var testBIOSMap = (StorageFile)await CoreFolder.TryGetItemAsync($"{DLLName.Replace(".dll", "")}.rab");
                        if (testBIOSMap != null)
                        {
                            await testBIOSMap.DeleteAsync();
                        }
                    }
                }
                catch (Exception ea)
                {

                }
            }
        }
    }
}
