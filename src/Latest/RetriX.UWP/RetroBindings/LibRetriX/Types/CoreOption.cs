using RetriX.UWP.RetroBindings.Structs;
using System;
using System.Collections.Generic;

namespace LibRetriX
{
    public class CoreOption
    {
        public string Description { get; private set; }
        public List<string> Values { get; private set; }

        private uint selectedValueIx;
        public uint SelectedValueIx
        {
            get => selectedValueIx;
            set
            {
                if (value >= Values.Count)
                {
                    selectedValueIx = 0;
                }
                else
                {
                    selectedValueIx = value;
                }
            }
        }

        public CoreOption(string description, List<string> values)
        {
            Description = description;
            Values = values;
        }
    }
}
