using System;
using System.Collections.Generic;
using System.Text;

namespace RetriX.Shared.ViewModels
{
    public class SaveSlotsModel
    {
        public int SlotID;
        public string Snapshot;
        public string GameID;
        public string SaveDate;
        public string SlotFileName;
        public string SnapshotFileName;
        public string SaveLocation;
        public string SlotType;
        public SaveSlotsModel(string gameID, int slotID,string slotFileName,string saveDate,string saveLocation)
        {
            GameID = gameID;
            SlotID = slotID;
            SlotFileName = slotFileName;
            SnapshotFileName = $"{GameID}_S{SlotID}.png";
            SaveDate = saveDate;
            SaveLocation = saveLocation;
            Snapshot = $@"{saveLocation}\{SnapshotFileName}";
            if (slotID < 11)
            {
                SlotType = $"Slot {slotID}";
            }else if (slotID < 21)
            {
                SlotType = "Quick";
            }else if(slotID < 36)
            {
                SlotType = "Auto";
            }
        }
    }
}
