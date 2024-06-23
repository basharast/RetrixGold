using Microsoft.Graphics.Canvas.Effects;
using RetriX.Shared.Components;
using RetriX.Shared.ViewModels;
using RetriX.UWP.Pages;
using RetriX.UWP.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

/**
  Copyright (c) RetriX Developer Alberto Fustinoni
  Copyright (c) RetriXGold Bashar Astifan (Since 2019)
  Legal Note:
  -This software is free and open source, provided without any warranty
  -If you want to make your own copy keep it open source and free
  -Don't ever add any tracking or ads as per the license
*/

namespace RetriX.UWP.Controls
{
    public sealed partial class PlayerOverlay : UserControl
    {
        public GamePlayerViewModel ViewModel
        {
            get { return (GamePlayerViewModel)GetValue(VMProperty); }
            set { SetValue(VMProperty, value); }
        }

       
        public VirtualPadActions VA { get; } = new VirtualPadActions();
        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty = DependencyProperty.Register(nameof(ViewModel), typeof(GamePlayerViewModel), typeof(PlayerOverlay), new PropertyMetadata(null));

        public PlayerOverlay()
        {
            InitializeComponent();
            //PlatformService.GameOverlaysUpdateBindings = BindingsUpdate;
        }

     
    }
}
