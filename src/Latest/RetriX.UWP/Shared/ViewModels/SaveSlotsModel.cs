using Newtonsoft.Json;
using RetriX.UWP.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
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
    public class SaveSlotsModel : BindableBase
    {
        public int SlotID;

        [JsonIgnore]
        Color[] colors = new Color[] { Colors.DodgerBlue, Colors.Orange, Colors.Gray, Colors.Tomato, Colors.Violet, Colors.DarkGreen, Colors.CornflowerBlue, Colors.RoyalBlue, Colors.DodgerBlue, Colors.Orange, Colors.Gray, Colors.Tomato, Colors.Violet, Colors.DarkGreen, Colors.CornflowerBlue, Colors.RoyalBlue, Colors.DodgerBlue, Colors.Orange, Colors.Gray, Colors.Tomato, Colors.Violet, Colors.DarkGreen, Colors.CornflowerBlue, Colors.RoyalBlue, Colors.DodgerBlue, Colors.Orange, Colors.Gray, Colors.Tomato, Colors.Violet, Colors.DarkGreen, Colors.CornflowerBlue, Colors.RoyalBlue };

        [JsonIgnore]
        public SolidColorBrush GameColor
        {
            get
            {
                var color = Colors.Gray;
                try
                {
                    var RandomIndex = new Random().Next(0, 30);
                    color = colors[RandomIndex];
                }
                catch (Exception ex)
                {

                }
                return new SolidColorBrush(color);
            }
        }

        public string snapshot;
        public string Snapshot
        {
            get
            {
                return snapshot;
            }
            set
            {
                var transcodedState = GameSystemSelectionView.GetTranscodedImage(value, ref snapshot);
                if (!transcodedState)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var transcodedImage = await GameSystemSelectionView.ConvertPngToBmp(value);
                            if (!transcodedImage.Equals(snapshot))
                            {
                                snapshot = transcodedImage;
                                RaisePropertyChanged(snapshot);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                }
            }
        }
        public string GameID;
        public string SaveDate;
        public string SaveTime;
        public string SlotFileName;
        public string SnapshotFileName;
        public string SaveLocation;
        public string SlotType;
        public long CreateDate;

        public Stretch StretchState = Stretch.UniformToFill;
        public SaveSlotsModel(string gameID, int slotID,string slotFileName,DateTimeOffset saveDate,string saveLocation, string coreCleanName, long date)
        {
            GameID = gameID;
            SlotID = slotID;
            CreateDate = date;
            SlotFileName = slotFileName;
            var filext = Path.GetExtension(slotFileName);
            SnapshotFileName = slotFileName.Replace(filext, ".png");
            SaveDate = $"{saveDate.Day} / {saveDate.Month} / {saveDate.Year}";
            SaveTime = $"{saveDate.Hour} : {saveDate.Minute} : {saveDate.Second} {(saveDate.Hour > 11 ? "PM" : "AM")}";
            SaveLocation = saveLocation;
            Snapshot = $@"{saveLocation}\{SnapshotFileName}";
            try
            {
                //Test access in case user set custom folder
                if (!File.Exists(Snapshot))
                {
                    Snapshot = "ms-appx:///Assets/Icons/GN.png";
                    StretchState = Stretch.None;
                }
            }catch(Exception ex)
            {

            }
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
            SlotType = SlotType.ToUpper();
            UpdateValues();
        }
        public void UpdateValues()
        {
            RaisePropertyChanged(nameof(SlotType));
            RaisePropertyChanged(nameof(SaveDate));
            RaisePropertyChanged(nameof(Snapshot));
        }
    }
}
