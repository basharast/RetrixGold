using LibRetriX;
using LibRetriX.RetroBindings;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RetriX.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace RetriX.UWP.Cores
{
    public class RetriXGoldCore
    {
        public bool AnyCore = false;
        public bool SkippedCore = false;
        public bool StartupLoading = false;
        public bool ImportedCore = false;
        public bool DLLMissing = false;
        public string CoreDLL = "";
        public string CoreDLLTemp = "";
        public string CoreExtraName = "";
        public Tuple<string, uint>[] Options;
        public FileDependency[] Dependencies;
        public uint?[] InputTypeID = null;

        public RetriXGoldCore(string dll, bool skippedCore, bool startup = false, bool anycore = false, bool importedCore = false, string extraName = "", Tuple<string, uint>[] options = null, FileDependency[] fileDependencies = null, uint?[] inputTypeID = null)
        {
            if (!dll.ToLower().EndsWith(".dll") && !dll.ToLower().EndsWith(".dat"))
            {
                dll = $"{dll}.dll";
            }
            CoreDLL = dll;
            CoreDLLTemp = dll;
            Options = options;
            Dependencies = fileDependencies;
            AnyCore = anycore;
            ImportedCore = importedCore;
            SkippedCore = skippedCore;
            StartupLoading = startup;
            InputTypeID = inputTypeID;
            CoreExtraName = extraName;
        }

        public LibretroCore core = null;

        public async Task<LibretroCore> GetCore()
        {
            if (!AnyCore)
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var PureCoreFolder = (StorageFolder)await localFolder.TryGetItemAsync("PureCore");

                if (PureCoreFolder != null)
                {
                    var testUpdateFile = (StorageFile)await PureCoreFolder.TryGetItemAsync(CoreDLLTemp);
                    if (testUpdateFile != null)
                    {
                        CoreDLL = testUpdateFile.Path;
                    }
                    else
                    {
                        testUpdateFile = (StorageFile)await PureCoreFolder.TryGetItemAsync(CoreDLLTemp.Replace(".dll", ".dat"));
                        if (testUpdateFile != null)
                        {
                            CoreDLL = testUpdateFile.Path;
                        }
                        else
                        {
                            CoreDLL = CoreDLLTemp;
                            try
                            {
                                var mainPath = $@"Assets\Libraries\{CoreDLL}";
                                var testFile = await Package.Current.InstalledLocation.TryGetItemAsync(mainPath);
                                if (testFile == null)
                                {
                                    testFile = await Package.Current.InstalledLocation.TryGetItemAsync(mainPath.Replace(".dll", ".dat"));
                                    if (testFile == null)
                                    {
                                        DLLMissing = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                DLLMissing = true;
                            }
                        }
                    }
                }
                else
                {
                    try
                    {
                        var mainPath = $@"Assets\Libraries\{CoreDLL}";
                        var testFile = await Package.Current.InstalledLocation.TryGetItemAsync(mainPath);
                        if (testFile == null)
                        {
                            testFile = await Package.Current.InstalledLocation.TryGetItemAsync(mainPath.Replace(".dll", ".dat"));
                            if (testFile == null)
                            {
                                DLLMissing = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DLLMissing = true;
                    }
                }
            }
            else
            {

            }

            var DLLName = $"{Path.GetFileNameWithoutExtension(CoreDLL)}{(AnyCore ? "-anycore" : "")}";
            if (CoreExtraName.Length > 0)
            {
                DLLName = $"{DLLName}-{CoreExtraName}";
            }
            var localCacheFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("CoreCache", CreationCollisionOption.OpenIfExists);
            bool cachedCore = false;
            try
            {
                var testFile = (StorageFile)await localCacheFolder.TryGetItemAsync($"{DLLName}.rxc");
                if (testFile != null)
                {
                    var fileAttr = await testFile.GetBasicPropertiesAsync();
                    if (fileAttr.Size > 0)
                    {
                        Encoding unicode = Encoding.Unicode;
                        byte[] result;
                        using (var stream = await testFile.OpenAsync(FileAccessMode.Read))
                        {
                            var outStream = stream.AsStream();
                            using (var memoryStream = new MemoryStream())
                            {
                                outStream.CopyTo(memoryStream);
                                result = memoryStream.ToArray();
                            }
                            await outStream.FlushAsync();
                        }
                        string CoreContent = unicode.GetString(result);
                        var settings = new JsonSerializerSettings
                        {
                            ContractResolver = (Newtonsoft.Json.Serialization.IContractResolver)IgnorePropertiesOfTypeContractResolver<System.Delegate>.Instance,
                        };
                        core = JsonConvert.DeserializeObject<LibretroCore>(CoreContent, settings);
                        if (core != null)
                        {
                            SkippedCore = false;
                            core.LibretroCoreCall(StartupLoading, CoreDLL, Dependencies, Options, InputTypeID, CoreDLL, SkippedCore, AnyCore, ImportedCore, false);
                            cachedCore = true;
                        }
                    }
                    else
                    {
                        await testFile.DeleteAsync();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            if (core == null)
            {
                core = new LibretroCore(StartupLoading, CoreDLL, Dependencies, Options, InputTypeID, CoreDLL, SkippedCore, AnyCore, ImportedCore);
            }
            if (!SkippedCore && !cachedCore)
            {
                core.Initialize();

                //I will consider all core imported as new core
                //This related to recents games records issue
                core.IsNewCore = AnyCore;

                if (!cachedCore)
                {
                    await CacheCoreData();
                }
            }
            return core;
        }
        public async Task CacheCoreData()
        {
            try
            {
                var DLLName = $"{Path.GetFileNameWithoutExtension(CoreDLL)}{(AnyCore ? "-anycore" : "")}";
                if (CoreExtraName.Length > 0)
                {
                    DLLName = $"{DLLName}-{CoreExtraName}";
                }
                var localCacheFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("CoreCache", CreationCollisionOption.OpenIfExists);
                if (!SkippedCore && !core.SkippedCore && !core.FailedToLoad)
                {
                    var testFile = (StorageFile)await localCacheFolder.CreateFileAsync($"{DLLName}.rxc", CreationCollisionOption.ReplaceExisting);
                    if (testFile != null)
                    {
                        Encoding unicode = Encoding.Unicode;
                        var settings = new JsonSerializerSettings
                        {
                            ContractResolver = (Newtonsoft.Json.Serialization.IContractResolver)IgnorePropertiesOfTypeContractResolver<System.Delegate>.Instance,
                        };
                        byte[] dictionaryListBytes = unicode.GetBytes(JsonConvert.SerializeObject(core, settings));
                        using (var stream = await testFile.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            var outStream = stream.AsStream();
                            await outStream.WriteAsync(dictionaryListBytes, 0, dictionaryListBytes.Length);
                            await outStream.FlushAsync();
                        }
                        var fileAttr = await testFile.GetBasicPropertiesAsync();
                        if (fileAttr.Size == 0)
                        {
                            await testFile.DeleteAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task CacheCoreClean()
        {
            try
            {
                var DLLName = $"{Path.GetFileNameWithoutExtension(CoreDLL)}{(AnyCore ? "-anycore" : "")}";
                if (CoreExtraName.Length > 0)
                {
                    DLLName = $"{DLLName}-{CoreExtraName}";
                }
                var localCacheFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("CoreCache", CreationCollisionOption.OpenIfExists);
                if (!SkippedCore && !core.SkippedCore && !core.FailedToLoad)
                {
                    var testFile = (StorageFile)await localCacheFolder.TryGetItemAsync($"{DLLName}.rxc");
                    if (testFile != null)
                    {
                        await testFile.DeleteAsync();
                    }
                }

                try
                {
                    var removeOptionsCacheFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("CoresOptions", CreationCollisionOption.OpenIfExists);
                    if (!SkippedCore && !core.SkippedCore && !core.FailedToLoad)
                    {
                        var testFile = (StorageFile)await removeOptionsCacheFolder.TryGetItemAsync($"{core.Name}_{core.SystemName}.rto");
                        if (testFile != null)
                        {
                            await testFile.DeleteAsync();
                            var expectedName = $"{core.Name}_{core.SystemName}";
                            CoresOptions TargetSystem;
                            if (GameSystemSelectionViewModel.SystemsOptions.TryGetValue(expectedName, out TargetSystem))
                            {
                                GameSystemSelectionViewModel.SystemsOptions.Remove(expectedName);
                                GameSystemSelectionViewModel.SystemsOptionsTemp.Remove(expectedName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {

            }
        }
    }
    public class IgnorePropertiesOfTypeContractResolver<T> : IgnorePropertiesOfTypeContractResolver
    {
        // As of 7.0.1, Json.NET suggests using a static instance for "stateless" contract resolvers, for performance reasons.
        // http://www.newtonsoft.com/json/help/html/ContractResolver.htm
        // http://www.newtonsoft.com/json/help/html/M_Newtonsoft_Json_Serialization_DefaultContractResolver__ctor_1.htm
        // "Use the parameterless constructor and cache instances of the contract resolver within your application for optimal performance."
        static IgnorePropertiesOfTypeContractResolver<T> instance;

        static IgnorePropertiesOfTypeContractResolver() { instance = new IgnorePropertiesOfTypeContractResolver<T>(); }

        public static IgnorePropertiesOfTypeContractResolver<T> Instance { get { return instance; } }

        public IgnorePropertiesOfTypeContractResolver() : base(new[] { typeof(T) }) { }
    }

    /// <summary>
    /// Contract resolver to ignore properties of any number of given types.
    /// </summary>
    public class IgnorePropertiesOfTypeContractResolver : DefaultContractResolver
    {
        readonly HashSet<Type> toIgnore;

        public IgnorePropertiesOfTypeContractResolver(IEnumerable<Type> toIgnore)
        {
            if (toIgnore == null)
                throw new ArgumentNullException();
            this.toIgnore = new HashSet<Type>(toIgnore);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType.BaseTypesAndSelf().Any(t => toIgnore.Contains(t)))
            {
                property.Ignored = true;
            }

            return property;
        }
    }

    public static class TypeExtensions
    {
        public static IEnumerable<Type> BaseTypesAndSelf(this Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }
    }
}
