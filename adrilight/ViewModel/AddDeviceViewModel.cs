﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Windows.Controls;
using HandyControl.Controls;

namespace adrilight.ViewModel
{
    public class AddDeviceViewModel : BaseViewModel
    {
        private IDeviceSettings _device;
        public IDeviceSettings Device {
            get { return _device; }
            set
            {
                if (_device == value) return;
                _device = value;
                RaisePropertyChanged();
            }
        }

        private int _stepIndex;

        public int StepIndex {
            get => _stepIndex;
#if NET40
            set => Set(nameof(StepIndex), ref _stepIndex, value);
#else
            set => Set(ref _stepIndex, value);
#endif
        }
        public RelayCommand<Panel> NextCmd => new(Next);

        /// <summary>
        ///     上一步
        /// </summary>
        public RelayCommand<Panel> PrevCmd => new(Prev);

        private void Next(Panel panel)
        {
            foreach (var stepBar in panel.Children.OfType<StepBar>())
            {
                stepBar.Next();
            }
        }

        private void Prev(Panel panel)
        {
            foreach (var stepBar in panel.Children.OfType<StepBar>())
            {
                stepBar.Prev();
            }
        }
        public IList<String> _AvailableComPorts;
        public IList<String> AvailableComPorts {
            get
            {


                _AvailableComPorts = SerialPort.GetPortNames().Concat(new[] { "Không có" }).ToList();
                _AvailableComPorts.Remove("COM1");

                return _AvailableComPorts;
            }
        }

        public AddDeviceViewModel(IDeviceSettings device)
        {
            Device = device;
        }
        //private ViewModelBase _currentView;
        //private ViewModelBase _allDeviceView;
        ////private ViewModelBase _changePortView;
        ////private ViewModelBase _changeNameView;
        //public ViewModelBase CurrentView {
        //    get { return _currentView; }
        //    set
        //    {
        //        _currentView = value;
        //        RaisePropertyChanged("CurrentView");
        //    }
        //}
        private bool _basicRev1Checked;
        public bool BasicRev1Checked {

            get { return _basicRev1Checked; }
            set
            {
                _basicRev1Checked = value;
                if(value)
                {
                    Device.DeviceType = "ABRev1";
                    Device.RGBOrder = 0;
                    IsNextable = true;
                    RaisePropertyChanged(() => Device.DeviceType);
                    RaisePropertyChanged(() => Device.RGBOrder);
                    RaisePropertyChanged(() => IsNextable);
                }
              
            }
        }
        private bool _basicRev2Checked;
        public bool BasicRev2Checked {

            get { return _basicRev2Checked; }
            set
            {
                _basicRev2Checked = value;
                if (value)
                {
                    Device.DeviceType = "ABRev2";
                    Device.RGBOrder = 1;
                    IsNextable = true;
                    RaisePropertyChanged(() => Device.DeviceType);
                    RaisePropertyChanged(() => Device.RGBOrder);
                    RaisePropertyChanged(() => IsNextable);

                }
                

            }
        }
        private bool _isNextable;
        public bool IsNextable {

            get { return _isNextable; }
            set
            {
                
                    _isNextable = value;
                

            }
        }
        private bool _eDGEChecked;
        public bool EDGEChecked {

            get { return _eDGEChecked; }
            set
            {
                _eDGEChecked = value;
                if (value)
                {
                    Device.DeviceType = "ABEDGE";
                    IsNextable = true;
                    RaisePropertyChanged(() => Device.DeviceType);
                    RaisePropertyChanged(() => IsNextable);
                }
                
            }
        }
        private bool _hUBV2Checked;
        public bool HUBV2Checked {

            get { return _hUBV2Checked; }
            set
            {
                _hUBV2Checked = value;
                if (value)
                {
                    IsNextable = true;
                    Device.DeviceType = "ABHV2";
                    RaisePropertyChanged(() => Device.DeviceType);
                    RaisePropertyChanged(() => IsNextable);
                }
                
            }
        }
        public ObservableCollection<string> AvailableDevice { get; private set; }

        /// <summary>
        /// ReadData
        /// </summary>
        public override void ReadData()
        {
            Device = new DeviceSettings();
            AvailableDevice = new ObservableCollection<string>
{
          "Ambino Basic Rev1",
           "Ambino Basic Rev2",
           "Ambino EDGE",
           "Ambino HUBV2",
          "Custom Device"


        };

            //_allDeviceView = new AllNewDeviceViewModel(this);
            //CurrentView = _allDeviceView;
        }


        //public void GoAllDeviceView()
        //{
        //    _allDeviceView = new AllNewDeviceViewModel(this);
        //    CurrentView = _allDeviceView;
        //}
        //public void GoToChangeNameView(IDeviceSettings device)
        //{
        //    Device = device;
        //    _changeNameView = new ChangeDeviceNameViewModel(this, Device);
        //    CurrentView = _changeNameView;
        //}
        //public void GoToChangePort(IDeviceSettings device)
        //{
        //    Device = device;
        //    _changePortView = new ChangePortViewModel(this, device);
        //    CurrentView = _changePortView;
        //}

    }
}
