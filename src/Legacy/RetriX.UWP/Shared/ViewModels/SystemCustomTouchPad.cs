using System;
using System.Collections.Generic;
using System.Text;

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
    public class SystemCustomTouchPad
    {
        public float leftScaleFactorValueP = 1f;
        public float leftScaleFactorValueW = 1f;
        public float rightScaleFactorValueP = 1f;
        public float rightScaleFactorValueW = 1f;
        public double rightTransformXCurrentP = 0;
        public double rightTransformYCurrentP = 0;
        public double leftTransformXCurrentP = 0;
        public double leftTransformYCurrentP = 0;
        public double actionsTransformXCurrentP = 0;
        public double actionsTransformYCurrentP = 0;
        public float buttonsOpacity = 0.50f;
    }
}
