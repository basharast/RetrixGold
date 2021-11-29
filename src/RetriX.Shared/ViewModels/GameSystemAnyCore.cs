using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RetriX.Shared.ViewModels
{
    public class GameSystemAnyCore
    {
        public string DLLName="";
        public string CoreName="";
        public string CoreSystem = "";
        public string CoreIcon = "";
        public string CoreYear = "";
        public string CoreDescription = "";
        public bool Pinned = false;
        public bool CDSupport = false;
        public Dictionary<string, string[]> BiosFiles = new Dictionary<string, string[]>();
        public Dictionary<string, string[]> BiosFilesSample = new Dictionary<string, string[]>();
        
        public GameSystemAnyCore(string coreName, string coreSystem="", string coreIcon = "", string coreYear="", string coreDescription="", bool pinned=false, string dllName="", bool cdSupport=false)
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
            BiosFilesSample.Add("biosfile1.bin",new string[] { "bios 1 description", "md5"});
            BiosFilesSample.Add("biosfile2.bin",new string[] { "bios 2 description", "md5"});
            BiosFilesSample.Add("biosfile3.bin",new string[] { "bios 3 description (Optional)", "md5"});
        }
        private async void RetriveBIOSMap()
        {
            try
            {
                var CoreFolder = await Plugin.FileSystem.CrossFileSystem.Current.LocalStorage.GetDirectoryAsync("AnyCore");
                if (CoreFolder != null)
                {
                    var testBIOSMap = await CoreFolder.GetFileAsync($"{DLLName}.rab");
                    if (testBIOSMap != null)
                    {
                        Encoding unicode = Encoding.Unicode;
                        byte[] result;
                        using (var outStream = await testBIOSMap.OpenAsync(FileAccess.Read))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                outStream.CopyTo(memoryStream);
                                result = memoryStream.ToArray();
                            }
                            await outStream.FlushAsync();
                        }
                        string CoreFileContent = unicode.GetString(result);
                        var dictionaryList = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(CoreFileContent);

                        if (dictionaryList != null)
                        {
                            BiosFiles = dictionaryList;
                        }
                    }
                }
            }catch(Exception e)
            {
                try
                {
                    var CoreFolder = await Plugin.FileSystem.CrossFileSystem.Current.LocalStorage.GetDirectoryAsync("AnyCore");
                    if (CoreFolder != null)
                    {
                        var testBIOSMap = await CoreFolder.GetFileAsync($"{DLLName}.rab");
                        if (testBIOSMap != null)
                        {
                            await testBIOSMap.DeleteAsync();
                        }
                    }
                    }catch(Exception ea)
                {

                }
            }
        }
        public void AddNewBios(string fileName,string description, string md5)
        {
            BiosFiles.Add(fileName, new string[] { description, md5 });
        }
    }
}
