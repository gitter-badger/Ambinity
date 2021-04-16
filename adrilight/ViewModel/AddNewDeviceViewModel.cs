﻿using BO;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.ViewModel
{
  public  class AddNewDeviceViewModel : BaseViewModel
    {
        private DeviceCard _device;
        public DeviceCard Device {
            get { return _device; }
            set
            {
                if (_device == value) return;
                _device = value;
                RaisePropertyChanged();
            }
        }
        private ViewModelBase _currentView;
        private ViewModelBase _allDeviceView;
        private ViewModelBase _changePortView;
        private ViewModelBase _changeNameView;
        public ViewModelBase CurrentView {
            get { return _currentView; }
            set
            {
                _currentView = value;
                RaisePropertyChanged("CurrentView");
            }
        }
        
        public override void ReadData()
        {
            _allDeviceView =new AllNewDeviceViewModel(this);
            CurrentView = _allDeviceView;
        }
        public void GoAllDeviceView()
        {
            _allDeviceView = new AllNewDeviceViewModel(this);
            CurrentView = _allDeviceView;
        }
        public void GoToChangeNameView(DeviceCard device)
        {
            Device = device;
            _changeNameView = new ChangeDeviceNameViewModel(this,Device);
            CurrentView = _changeNameView;
        }
        public void GoToChangePort(DeviceCard device)
        {
            Device = device;
            _changePortView = new ChangePortViewModel(this,device);
            CurrentView = _changePortView;           
        }
    }
}
