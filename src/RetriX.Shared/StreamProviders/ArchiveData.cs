using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RetriX.Shared.StreamProviders
{
    public class ArchiveData
    {
        public long entrySize;
        public long streamSize;
        public string entryKey;
        public Action<Stream, IProgress<double>> WriteEntryTo;
     
        public ArchiveData(Action<Stream, IProgress<double>> writeEntryTo, long EntrySize, long StreamSize, string EntryKey)
        {
            WriteEntryTo = writeEntryTo;
            entrySize = EntrySize;
            streamSize = StreamSize;
            entryKey = EntryKey;
        }
    }
}
