using RetriX.UWP.Services;
using System;

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

namespace RetriX.Shared.Services
{
    public class FullScreenChangeEventArgs : EventArgs
    {
        public FullScreenChangeType Type { get; private set; }

        public FullScreenChangeEventArgs(FullScreenChangeType type)
        {
            Type = type;
        }
    }

    public class GameStateOperationEventArgs : EventArgs
    {
        public enum GameStateOperationType { Save, Load, Action };

        public GameStateOperationType Type { get; private set; }
        public uint SlotID { get; private set; }

        public GameStateOperationEventArgs(GameStateOperationType type, uint slotID)
        {
            Type = type;
            SlotID = slotID;
        }
    }
}
