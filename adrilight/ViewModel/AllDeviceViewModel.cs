﻿using BO;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace adrilight.ViewModel
{
   public class AllDeviceViewModel : ViewModelBase
    {
        private ObservableCollection<DeviceCard> _cards;
        public ObservableCollection<DeviceCard> Cards {
            get { return _cards; }
            set
            {
                if (_cards == value) return;
                _cards = value;
                RaisePropertyChanged();
            }
        }
        public ICommand SelectCard { get; set; }
        private readonly ViewModelBase _parentVm;
        public AllDeviceViewModel(ViewModelBase parent)
        {
            _parentVm = parent;
            ReadData();
           
        }
        
        public void LoadCard()
        {
            Cards = new ObservableCollection<DeviceCard>();
            Cards.Add(new DeviceCard() {
                Title = "LED màn 1",
                Brightness = 70,
                ComPort = "COM3",
                IsActive = true,
                TypeName = "Ambino Basic",             
        });
            Cards.Add(new DeviceCard() {
                Title = "LED màn 2",
                Brightness = 70,
                ComPort = "COM4",
                IsActive = true,
                TypeName = "Ambino Basic"
            });
           
        }
        public void ReadData()
        {
            SelectCard = new RelayCommand<DeviceCard>((p) => {
                return p != null;
            }, (p) =>
              {
                  (_parentVm as MainViewViewModel).GotoChild(p);
              });
            LoadCard();
        }
    }
}
