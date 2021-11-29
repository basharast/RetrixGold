using LibRetriX;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetriX.Shared.ViewModels
{
    public class CoresOptions
    {
        public string SystemName;
        public string OriginalSystemName;
        public Dictionary<string, CoreOptionsValues> OptionsList = new Dictionary<string, CoreOptionsValues>();

        public CoresOptions(ICore SystemCore)
        {
            if (SystemCore != null) { 
            try {
                SystemName = SystemCore.Name;
                OriginalSystemName = SystemCore.OriginalSystemName;
                if (SystemCore.Options != null) { 
                foreach (var OptionKey in SystemCore.Options.Keys) {
                    OptionsList.Add(OptionKey, new CoreOptionsValues(OptionKey, SystemCore.Options[OptionKey]));
                }
                }
            } catch (Exception e)
            {

            }
            }
        }

    }
    public class CoreOptionsValues
    {
        public string OptionsKey;
        public string OptionsDescription;
        public List<string> OptionsValues=new List<string>();
        public uint SelectedIndex;
        public CoreOptionsValues(string Key, CoreOption coreOption)
        {
            if (coreOption != null) { 
            try { 
            OptionsKey = Key;
            OptionsDescription = coreOption.Description;
            SelectedIndex = coreOption.SelectedValueIx;
            foreach(var ValueItem in coreOption.Values)
            {
                OptionsValues.Add(char.ToUpper(ValueItem[0]) + ValueItem.Substring(1));
            }
            }catch(Exception e)
            {

            }
            }
        }
    }
}
