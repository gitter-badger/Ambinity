﻿using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight
{
    internal class GeneralSettings : ViewModelBase, IGeneralSettings
    {
       // private bool _autostart = true;
        private int _borderDistanceX = 0;
        private int _borderDistanceY = 0;


        private bool _mirrorX = true;
        private bool _mirrorY = false;
        private int _offsetLed = 10;
        private bool _shouldbeRunning = true;
        //ambilight smooth choice///



        private int _spotHeight = 150;
        private int _spotsX = 11;



        private int _spotsY = 6;

        private int _spotWidth = 150;


  
        private int _selectedDisplay = 0;
        private int _selectedAdapter = 0;
        private byte _saturationTreshold = 10;

        private int _limitFps = 100;

        private bool _useLinearLighting = true;




















        public int BorderDistanceX { get => _borderDistanceX; set { Set(() => BorderDistanceX, ref _borderDistanceX, value); } }
        public int BorderDistanceY { get => _borderDistanceY; set { Set(() => BorderDistanceY, ref _borderDistanceY, value); } }

        public bool ShouldbeRunning { get => _shouldbeRunning; set { Set(() => ShouldbeRunning, ref _shouldbeRunning, value); } }

        public bool MirrorX { get => _mirrorX; set { Set(() => MirrorX, ref _mirrorX, value); } }
        public bool MirrorY { get => _mirrorY; set { Set(() => MirrorY, ref _mirrorY, value); } }
        public int OffsetLed { get => _offsetLed; set { Set(() => OffsetLed, ref _offsetLed, value); } }

        public bool UseLinearLighting { get => _useLinearLighting; set { Set(() => UseLinearLighting, ref _useLinearLighting, value); } }

        public int SpotHeight { get => _spotHeight; set { Set(() => SpotHeight, ref _spotHeight, value); } }
        public int SpotsX { get => _spotsX; set { Set(() => SpotsX, ref _spotsX, value); } }

        public int SpotsY { get => _spotsY; set { Set(() => SpotsY, ref _spotsY, value); } }

        public int SpotWidth { get => _spotWidth; set { Set(() => SpotWidth, ref _spotWidth, value); } }


        public byte SaturationTreshold { get => _saturationTreshold; set { Set(() => SaturationTreshold, ref _saturationTreshold, value); } }


        public int LimitFps { get => _limitFps; set { Set(() => LimitFps, ref _limitFps, value); }  }


        public int SelectedDisplay { get => _selectedDisplay; set { Set(() => SelectedDisplay, ref _selectedDisplay, value); } }
        public int SelectedAdapter { get => _selectedAdapter; set { Set(() => SelectedAdapter, ref _selectedAdapter, value); } }
       

       




    }
}