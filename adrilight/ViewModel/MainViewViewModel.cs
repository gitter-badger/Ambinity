using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using adrilight.Resources;
using adrilight.Spots;
using adrilight.Util;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using OpenRGB.NET.Models;
using Un4seen.BassWasapi;
using BO;
using Application = System.Windows.Forms.Application;
using GalaSoft.MvvmLight;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows.Media;

namespace adrilight.ViewModel
{
    public class MainViewViewModel : BaseViewModel
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");

        private string JsonDeviceFileNameAndPath => Path.Combine(JsonPath, "adrilight-deviceInfos.json");

        #region constant string
        public const string ImagePathFormat = "pack://application:,,,/adrilight;component/View/Images/{0}";
        public const string dashboard = "Dashboard";
        public const string deviceSetting = "Device Settings";
        public const string appSetting = "App Settings";
        public const string faq = "FAQ";
        public const string general = "General";
        public const string lighting = "Lighting";
        public const string canvasLighting = "Canvas Lighting";
        #endregion
        #region property
        private ObservableCollection<VerticalMenuItem> _menuItems;
        public ObservableCollection<VerticalMenuItem> MenuItems {
            get { return _menuItems; }
            set
            {
                if (_menuItems == value) return;
                _menuItems = value;
                RaisePropertyChanged();
            }
        }
        private VerticalMenuItem _selectedVerticalMenuItem;
        public VerticalMenuItem SelectedVerticalMenuItem {
            get { return _selectedVerticalMenuItem; }
            set
            {
                if (_selectedVerticalMenuItem == value) return;
                _selectedVerticalMenuItem = value;
                RaisePropertyChanged();

            }
        }
        private bool _isDashboardType = true;
        public bool IsDashboardType {
            get { return _isDashboardType; }
            set
            {
                if (_isDashboardType == value) return;
                _isDashboardType = value;
                RaisePropertyChanged();
                LoadMenuByType(value);
            }
        }
     

       
        private string _buildVersion = "";
        public string BuildVersion {
            get { return _buildVersion; }
            set
            {
                if (_buildVersion == value) return;
                _buildVersion = value;
                RaisePropertyChanged();

            }
        }
        private DateTime? _lastUpdate;
        public DateTime? LastUpdate {
            get { return _lastUpdate; }
            set
            {
                if (_lastUpdate == value) return;
                _lastUpdate = value;
                RaisePropertyChanged();

            }
        }
        private string _author = "";
        public string Author {
            get { return _author; }
            set
            {
                if (_author == value) return;
                _author = value;
                RaisePropertyChanged();

            }
        }
        private string _git = "";
        public string Git {
            get { return _git; }
            set
            {
                if (_git == value) return;
                _git = value;
                RaisePropertyChanged();

            }
        }
        private string _faq = "";
        public string FAQ {
            get { return _faq; }
            set
            {
                if (_faq == value) return;
                _faq = value;
                RaisePropertyChanged();

            }
        }
        private string _appName = "";
        public string AppName {
            get { return _appName; }
            set
            {
                if (_appName == value) return;
                _appName = value;
                RaisePropertyChanged();

            }
        }
        private Visibility _visibleTabControl;
        public Visibility VisibleTabControl {
            get
            {
                if (CurrentDevice.IsHUB || CurrentDevice.ParrentLocation != 151293)
                {
                    return Visibility.Visible;
                }
                else return Visibility.Collapsed;


            }
            set
            {
                _visibleTabControl = value;

            }
        }

        private Visibility _dFUVisibility;
        public Visibility DFUVisibility {
            get
            {
                if (CurrentDevice.ParrentLocation != 151293)
                {
                    return Visibility.Collapsed;
                }
                else return Visibility.Visible;


            }
            set
            {
                _dFUVisibility = value;

            }
        }
        private ViewModelBase _currentView;

        private ViewModelBase _detailView;
        private ViewModelBase _deviceSettingView;
        private ViewModelBase _appSettingView;
        private ViewModelBase _faqSettingView;
        public ViewModelBase CurrentView {
            get { return _currentView; }
            set
            {
                _currentView = value;
                RaisePropertyChanged("CurrentView");
            }
        }
        private IDeviceSettings _currentDevice;
        public IDeviceSettings CurrentDevice {
            get { return _currentDevice; }
            set
            {
                if (_currentDevice == value) return;
                if (_currentDevice != null) _currentDevice.PropertyChanged -= _currentDevice_PropertyChanged;
                _currentDevice = value;
                if (_currentDevice != null) _currentDevice.PropertyChanged += _currentDevice_PropertyChanged;
                RaisePropertyChanged("CurrentDevice");

            }
        }

        private void _currentDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!IsDashboardType)
            {
                WriteDeviceInfoJson();
            }
            if (CurrentDevice.DeviceID == 151293)
            {

            }
            else
            {
                foreach (var spotset in SpotSets)
                    if (spotset.ID == CurrentDevice.DeviceID)
                    {
                        PreviewSpots = spotset.Spots;
                    }

            }
        }

        public ICommand SelectMenuItem { get; set; }
        public ICommand BackCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand SnapshotCommand { get; set; }
        #endregion
        private ObservableCollection<IDeviceSettings> _cards;
        public ObservableCollection<IDeviceSettings> Cards {
            get { return _cards; }
            set
            {
                if (_cards == value) return;
                _cards = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<IDeviceSettings> _displayCards;
        public ObservableCollection<IDeviceSettings> DisplayCards {
            get { return _displayCards; }
            set
            {
                if (_displayCards == value) return;
                _displayCards = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<IDeviceSpotSet> _spotSets;
        public ObservableCollection<IDeviceSpotSet> SpotSets {
            get { return _spotSets; }
            set
            {
                if (_spotSets == value) return;
                _spotSets = value;
                RaisePropertyChanged();
            }
        }
        // [Inject, Named("0")]
        // public IDeviceSettings Card1 { get; set; }

        public ICommand SelectCardCommand { get; set; }
        public ICommand ShowAddNewCommand { get; set; }
        public ICommand RefreshDeviceCommand { get; set; }

       
        private string JsonDeviceNameAndPath => Path.Combine(JsonPath, "adrilight-deviceInfos.json");
        public IList<String> _AvailableComPorts;
        public IList<String> AvailableComPorts {
            get
            {


                _AvailableComPorts = SerialPort.GetPortNames().Concat(new[] { "Không có" }).ToList();
                _AvailableComPorts.Remove("COM1");

                return _AvailableComPorts;
            }
        }
        private ObservableCollection<string> _caseEffects;
        public ObservableCollection<string> CaseEffects {
            get { return _caseEffects; }
            set
            {
                if (_caseEffects == value) return;
                _caseEffects = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<CollectionItem> _collectionItm;
        public ObservableCollection<CollectionItem> CollectionItems {
            get { return _collectionItm; }
            set
            {
                if (_collectionItm == value) return;
                _collectionItm = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<string> _availableEffects;
        public ObservableCollection<string> AvailableEffects {
            get { return _availableEffects; }
            set
            {
                if (_availableEffects == value) return;
                _availableEffects = value;
                RaisePropertyChanged();
            }
        }
        public IDeviceSpot[] _previewSpots;
        public IDeviceSpot[] PreviewSpots {
            get => _previewSpots;
            set
            {
                _previewSpots = value;
                RaisePropertyChanged();
            }
        }
        public IGeneralSpot[] _shaderSpots;
        public IGeneralSpot[] ShaderSpots {
            get => _shaderSpots;
            set
            {
                _shaderSpots = value;
                RaisePropertyChanged();
            }
        }

        public  WriteableBitmap _shaderBitmap;
        public  WriteableBitmap ShaderBitmap {
            get => _shaderBitmap;
            set
            {
                Set(ref _shaderBitmap, value);
            }
        }

        private int _parrentLocation;
        public int ParrentLocation {
            get => _parrentLocation;
            set
            {
                _parrentLocation = value;
                RaisePropertyChanged();
            }
        }

        public int _deviceType;
        public int DeviceType {
            get
            {
                if (CurrentDevice.DeviceType == "Strip")
                    return 0;
                else if (CurrentDevice.DeviceType == "Square")
                    return 1;
                else
                    return 0;
            }
            set
            {
                _deviceType = value;
                switch (value)
                {
                    case 0://strip type
                        CurrentDevice.DeviceType = "Strip";
                        CurrentDevice.SpotsX = CurrentDevice.NumLED;
                        CurrentDevice.SpotsY = 1;
                        RaisePropertyChanged(() => CurrentDevice.SpotsX);
                        RaisePropertyChanged(() => CurrentDevice.SpotsY);
                        RaisePropertyChanged(() => CurrentDevice.DeviceType);
                        break;
                    case 1://square type
                        CurrentDevice.DeviceType = "Square";
                        CurrentDevice.SpotsX = CurrentDevice.NumLED / 4 + 1;
                        CurrentDevice.SpotsY = (CurrentDevice.NumLED - (CurrentDevice.NumLED / 4) * 2) / 2 + 1;
                        RaisePropertyChanged(() => CurrentDevice.SpotsX);
                        RaisePropertyChanged(() => CurrentDevice.SpotsY);
                        RaisePropertyChanged(() => CurrentDevice.DeviceType);
                        break;
                    case 2://matrix type
                        CurrentDevice.DeviceType = "Matrix";
                        break;
                }

            }
        }
        public IList<string> _AvailableAudioDevice = new List<string>();
        public IList<String> AvailableAudioDevice {
            get
            {
                _AvailableAudioDevice.Clear();
                int devicecount = BassWasapi.BASS_WASAPI_GetDeviceCount();
                string[] devicelist = new string[devicecount];
                for (int i = 0; i < devicecount; i++)
                {

                    var devices = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);

                    if (devices.IsEnabled && devices.IsLoopback)
                    {
                        var device = string.Format("{0} - {1}", i, devices.name);

                        _AvailableAudioDevice.Add(device);
                    }

                }

                return _AvailableAudioDevice;
            }
        }
        public int _audioDeviceID = -1;
        public int AudioDeviceID {
            get
            {
                if (CurrentDevice.SelectedAudioDevice > AvailableAudioDevice.Count)
                {
                    System.Windows.MessageBox.Show("Last Selected Audio Device is not Available");
                    return -1;
                }
                else
                {
                    var currentDevice = AvailableAudioDevice.ElementAt(CurrentDevice.SelectedAudioDevice);

                    var array = currentDevice.Split(' ');
                    _audioDeviceID = Convert.ToInt32(array[0]);
                    return _audioDeviceID;
                }

            }



        }
        public ObservableCollection<string> AvailablePalette { get; private set; }
        public IContext Context { get; }
        public IList<String> _AvailableDisplays;
        public IList<String> AvailableDisplays {
            get
            {
                var listDisplay = new List<String>();
                foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                {

                    listDisplay.Add(screen.DeviceName);
                }
                _AvailableDisplays = listDisplay;
                return _AvailableDisplays;
            }
        }
      
        public ObservableCollection<string> AvailableFrequency { get; private set; }

        public ObservableCollection<string> AvailableMusicPalette { get; private set; }
        public ObservableCollection<string> AvailableMusicMode { get; private set; }
        public ICommand SelectGif { get; set; }
        public BitmapImage gifimage;
        public Stream gifStreamSource;
        private static int _gifFrameIndex = 0;
        private BitmapSource _contentBitmap;
        public BitmapSource ContentBitmap {
            get { return _contentBitmap; }
            set
            {
                if (value != _contentBitmap)
                {
                    _contentBitmap = value;
                    RaisePropertyChanged(() => ContentBitmap);

                }
            }
        }
        GifBitmapDecoder decoder;
        public IGeneralSettings GeneralSettings { get; }
        public ISerialStream[] SerialStreams { get; }
        public IOpenRGBClientDevice OpenRGBClientDevice { get; set; }
        public ISerialDeviceDetection SerialDeviceDetection { get; set; }
        public static IShaderEffect ShaderEffect {get;set;}
        public int AddedDevice { get; }

        public MainViewViewModel(IContext context, IDeviceSettings[] cards, IDeviceSpotSet[] deviceSpotSets,IGeneralSpotSet generalSpotSet, IGeneralSettings generalSettings, IOpenRGBClientDevice openRGBDevices, ISerialDeviceDetection serialDeviceDetection, ISerialStream[] serialStreams, IShaderEffect shaderEffect)
        {

            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            SerialStreams = serialStreams ?? throw new ArgumentNullException(nameof(serialStreams));
            Cards = new ObservableCollection<IDeviceSettings>();
            DisplayCards = new ObservableCollection<IDeviceSettings>();
            AddedDevice = cards.Length;
            Context=context ?? throw new ArgumentNullException(nameof(context));
            SpotSets = new ObservableCollection<IDeviceSpotSet>();
            OpenRGBClientDevice = openRGBDevices ?? throw new ArgumentNullException(nameof(openRGBDevices));
            SerialDeviceDetection = serialDeviceDetection ?? throw new ArgumentNullException(nameof(serialDeviceDetection));
            ShaderEffect = shaderEffect ?? throw new ArgumentNullException();
            ShaderEffect.PropertyChanged+= ShaderImageUpdate;
            //ShaderSpots = generalSpotSet.ShaderSpot;
            //ShaderBitmap = shaderEffect.MatrixBitmap;
            foreach (IDeviceSettings card in cards)
            {
                Cards.Add(card);
                if (card.IsVissible)
                    DisplayCards.Add(card);
            }
            foreach (IDeviceSpotSet spotSet in deviceSpotSets)
            {
                SpotSets.Add(spotSet);
            }



            WriteJson();
            //binding settings to settings
            //if(CurrentDevice!=null)
            //{
            //    CurrentDevice.PropertyChanged += (s, e) =>
            //    {
            //        switch (e.PropertyName)
            //        {
            //            case nameof(CurrentDevice.SelectedDisplay):
            //                RaisePropertyChanged(() => PreviewSpots);
            //                break;
            //        };
            //    };
            //}






            GeneralSettings.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(GeneralSettings.ScreenSize):
                        if (GeneralSettings.ScreenSize == 0)
                        {
                            GeneralSettings.SpotsX = 11;
                            GeneralSettings.SpotsY = 7;
                            RaisePropertyChanged(() => GeneralSettings.SpotsX);
                            RaisePropertyChanged(() => GeneralSettings.SpotsY);
                        }
                        else if (GeneralSettings.ScreenSize == 1)
                        {
                            GeneralSettings.SpotsX = 13;
                            GeneralSettings.SpotsY = 7;
                            RaisePropertyChanged(() => GeneralSettings.SpotsX);
                            RaisePropertyChanged(() => GeneralSettings.SpotsY);
                        }
                        else if (GeneralSettings.ScreenSize == 2)
                        {
                            GeneralSettings.SpotsX = 14;
                            GeneralSettings.SpotsY = 7;
                            RaisePropertyChanged(() => GeneralSettings.SpotsX);
                            RaisePropertyChanged(() => GeneralSettings.SpotsY);
                        }
                        else if (GeneralSettings.ScreenSize == 3)
                        {
                            GeneralSettings.SpotsX = 14;
                            GeneralSettings.SpotsY = 9;
                            RaisePropertyChanged(() => GeneralSettings.SpotsX);
                            RaisePropertyChanged(() => GeneralSettings.SpotsY);
                        }
                        else if (GeneralSettings.ScreenSize == 4)
                        {
                            GeneralSettings.SpotsX = 16;
                            GeneralSettings.SpotsY = 7;
                            RaisePropertyChanged(() => GeneralSettings.SpotsX);
                            RaisePropertyChanged(() => GeneralSettings.SpotsY);
                        }

                        GeneralSettings.OffsetLed = GeneralSettings.SpotsX - 1;
                        break;
                    case nameof(GeneralSettings.ScreenSizeSecondary):
                        if (GeneralSettings.ScreenSizeSecondary == 0)
                        {
                            GeneralSettings.SpotsX2 = 11;
                            GeneralSettings.SpotsY2 = 7;
                            RaisePropertyChanged(() => GeneralSettings.SpotsX2);
                            RaisePropertyChanged(() => GeneralSettings.SpotsY2);
                        }
                        else if (GeneralSettings.ScreenSizeSecondary == 1)
                        {
                            GeneralSettings.SpotsX2 = 13;
                            GeneralSettings.SpotsY2 = 7;
                            RaisePropertyChanged(() => GeneralSettings.SpotsX2);
                            RaisePropertyChanged(() => GeneralSettings.SpotsY2);
                        }
                        else if (GeneralSettings.ScreenSizeSecondary == 2)
                        {
                            GeneralSettings.SpotsX2 = 14;
                            GeneralSettings.SpotsY2 = 7;
                            RaisePropertyChanged(() => GeneralSettings.SpotsX2);
                            RaisePropertyChanged(() => GeneralSettings.SpotsY2);
                        }
                        else if (GeneralSettings.ScreenSizeSecondary == 3)
                        {
                            GeneralSettings.SpotsX2 = 14;
                            GeneralSettings.SpotsY2 = 9;
                            RaisePropertyChanged(() => GeneralSettings.SpotsX2);
                            RaisePropertyChanged(() => GeneralSettings.SpotsY2);
                        }
                        else if (GeneralSettings.ScreenSizeSecondary == 4)
                        {
                            GeneralSettings.SpotsX2 = 16;
                            GeneralSettings.SpotsY2 = 7;
                            RaisePropertyChanged(() => GeneralSettings.SpotsX2);
                            RaisePropertyChanged(() => GeneralSettings.SpotsY2);
                        }

                        GeneralSettings.OffsetLed2 = GeneralSettings.SpotsX2 - 1;
                        break;
                    case nameof(GeneralSettings.ScreenSizeThird):
                        if (GeneralSettings.ScreenSizeThird == 0)
                        {
                            GeneralSettings.SpotsX3 = 11;
                            GeneralSettings.SpotsY3 = 7;
                            RaisePropertyChanged(() => GeneralSettings.SpotsX3);
                            RaisePropertyChanged(() => GeneralSettings.SpotsY3);
                        }
                        else if (GeneralSettings.ScreenSizeThird == 1)
                        {
                            GeneralSettings.SpotsX3 = 13;
                            GeneralSettings.SpotsY3 = 7;
                            RaisePropertyChanged(() => GeneralSettings.SpotsX3);
                            RaisePropertyChanged(() => GeneralSettings.SpotsY3);
                        }
                        else if (GeneralSettings.ScreenSizeThird == 2)
                        {
                            GeneralSettings.SpotsX3 = 14;
                            GeneralSettings.SpotsY3 = 7;
                            RaisePropertyChanged(() => GeneralSettings.SpotsX3);
                            RaisePropertyChanged(() => GeneralSettings.SpotsY3);
                        }
                        else if (GeneralSettings.ScreenSizeThird == 3)
                        {
                            GeneralSettings.SpotsX3 = 14;
                            GeneralSettings.SpotsY3 = 9;
                            RaisePropertyChanged(() => GeneralSettings.SpotsX3);
                            RaisePropertyChanged(() => GeneralSettings.SpotsY3);
                        }
                        else if (GeneralSettings.ScreenSizeThird == 4)
                        {
                            GeneralSettings.SpotsX3 = 16;
                            GeneralSettings.SpotsY3 = 7;
                            RaisePropertyChanged(() => GeneralSettings.SpotsX3);
                            RaisePropertyChanged(() => GeneralSettings.SpotsY3);
                        }

                        GeneralSettings.OffsetLed3 = GeneralSettings.SpotsX3 - 1;
                        break;

                       
                        case nameof(GeneralSettings.Autostart):
                                if (GeneralSettings.Autostart)
                                {
                                    StartUpManager.AddApplicationToCurrentUserStartup();
                                }
                                else
                                {
                                    StartUpManager.RemoveApplicationFromCurrentUserStartup();
                                }
                                break;
                         default:
                                break;
                        


                }

            };
        }


        public void LoadCard()
        {
            //Cards = new ObservableCollection<IDeviceSettings>();
            // Cards.Add(Card1);


            //var settingsmanager = new UserSettingsManager();
            //var devices = settingsmanager.LoadDeviceIfExists();
            //if (devices != null)
            //{
            //    foreach (var item in devices)
            //    {
            //        var deviceInfo = new DeviceSettings() {
            //            Brightness = item.Brightness,
            //            SelectedDisplay = item.SelectedDisplay,
            //            WhitebalanceRed = item.WhitebalanceRed,
            //            DeviceId = item.DeviceID,
            //            DeviceName = item.DeviceName,
            //            DevicePort = item.DevicePort,
            //            DeviceSize = item.DeviceSize,
            //            DeviceType = item.DeviceType,
            //            //  FadeEnd = item.fadeend,
            //            //  FadeStart = item.fadestart,
            //            // GifMode = item.gifmode,
            //            // GifSource = item.gifsource,
            //            IsBreathing = item.IsBreathing,
            //            IsConnected = item.IsConnected,
            //            SelectedEffect = item.SelectedEffect,
            //            SelectedMusicMode = item.SelectedMusicMode,
            //            MSens = item.MSens,
            //            SelectedAudioDevice = item.SelectedAudioDevice,
            //            SelectedPalette = item.SelectedPalette,
            //            EffectSpeed = item.EffectSpeed,
            //            StaticColor = item.StaticColor,
            //            AtmosphereStart = item.AtmosphereStart,
            //            AtmosphereStop = item.AtmosphereStop,
            //            BreathingSpeed = item.BreathingSpeed,
            //            ColorFrequency = item.ColorFrequency,

            //            SelectedMusicPalette = item.SelectedMusicPalette,
            //            SpotHeight = item.SpotHeight,
            //            SpotsX = item.SpotsX,
            //            SpotsY = item.SpotsY,
            //            SpotWidth = item.SpotWidth,
            //            UseLinearLighting = item.UseLinearLighting,
            //            WhitebalanceBlue = item.WhitebalanceBlue,

            //            WhitebalanceGreen = item.WhitebalanceGreen
            //        };

            //        deviceInfo.PropertyChanged += DeviceInfo_PropertyChanged;
            //        Cards.Add(deviceInfo);
            //    }
            //}

        }
        private void DeviceInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if (_isAddnew) return;
            //_isAddnew = true;
            //WriteJson();
            //_isAddnew = false;
        }

        private  void ShaderImageUpdate(object sender, PropertyChangedEventArgs e)
        {

           
            Context.Invoke(() =>
            {
                var MatrixBitmap = new WriteableBitmap(70, 70, 96, 96, PixelFormats.Bgr32, null);
                MatrixBitmap.Lock();
                IntPtr pixelAddress = MatrixBitmap.BackBuffer;
                var CurrentFrame = ShaderEffect.Frame;

                Marshal.Copy(FrameToInt32(CurrentFrame), 0, pixelAddress, 70 * 70);

                MatrixBitmap.AddDirtyRect(new Int32Rect(0, 0, 70, 70));

                MatrixBitmap.Unlock();
                ShaderBitmap = MatrixBitmap;
            });
            
        }

        private  Int32[] FrameToInt32(Pixel[] frame)
        {
            Int32[] data = new Int32[70 * 70];

            for (int i = 0; i < 70 * 70; i++)
                data[i] = frame[i].GetBPP24RGB_Int32();


            return data;
        }

        public override void ReadData()
        {
            LoadMenu();
            LoadMenuByType(true);
            ReadDataDevice();
            // ReadFAQ();

            //CurrentView = _allDeviceView.CreateViewModel();
            SelectMenuItem = new RelayCommand<VerticalMenuItem>((p) => {
                return true;
            }, (p) =>
            {
                ChangeView(p);
            });
            SelectedVerticalMenuItem = MenuItems.FirstOrDefault();
            //  SettingInfo = new SettingInfoDTO();
            //var setting=  LoadSettingIfExists();
            //  if (setting != null)
            //  {
            //      SettingInfo.AutoAddNewDevice = setting.autoaddnewdevice;
            //      SettingInfo.AutoConnectNewDevice = setting.autoconnectnewdevice;
            //      SettingInfo.AutoDeleteConfigWhenDisconnected = setting.autodeleteconfigwhendisconected;
            //      SettingInfo.AutoStartWithWindows = setting.autostartwithwindows;
            //      SettingInfo.DefaultName = setting.defaultname;
            //      SettingInfo.DisplayConnectionStatus = setting.displayconnectionstatus;
            //      SettingInfo.DisplayLightingStatus = setting.displaylightingstatus;
            //      SettingInfo.IsDarkMode = setting.isdarkmode;
            //      SettingInfo.PushNotificationWhenNewDeviceConnected = setting.pushnotificationwhennewdeviceconnected;
            //      SettingInfo.PushNotificationWhenNewDeviceDisconnected = setting.pushnotificationwhennewdevicedisconnected;
            //      SettingInfo.StartMinimum = setting.startminimum;
            //      SettingInfo.PrimaryColor=(Color )ColorConverter.ConvertFromString(setting.primarycolor);

            //  }
            //  else
            //  {
            //      SettingInfo.PrimaryColor = Colors.White;
            //  }
            DeleteCommand = new RelayCommand<string>((p) => {
                return true;
            }, (p) =>
            {
                ShowDeleteDialog();
            });

            SnapshotCommand = new RelayCommand<string>((p) => {
                return true;
            }, (p) =>
            {

                SnapShot();
            });



            RefreshDeviceCommand = new RelayCommand<string>((p) => {
                return true;
            }, (p) =>
            {

                RefreshDevice();
            });
            SelectCardCommand = new RelayCommand<IDeviceSettings>((p) => {
                return p != null;
            }, (p) =>
            {
                this.GotoChild(p);
            });


            ShowAddNewCommand = new RelayCommand<IDeviceSettings>((p) => {
                return true;
            }, (p) =>
            {
                ShowAddNewDialog();
            });
            BackCommand = new RelayCommand<string>((p) => {
                return true;
            }, (p) =>
            {
                BackToDashboard();
            });
        }

        public void SnapShot()
        {
            int counter = 0;
            byte[] snapshot = new byte[256];
            foreach (IDeviceSpot spot in PreviewSpots)
            {

                snapshot[counter++] = spot.Red;
                snapshot[counter++] = spot.Green;
                snapshot[counter++] = spot.Blue;
                // counter++;
            }
            CurrentDevice.SnapShot = snapshot;
            RaisePropertyChanged(() => CurrentDevice.SnapShot);
        }
        public void DFU()
        {
            foreach (var serialStream in SerialStreams)
            {
                if (serialStream.ID == CurrentDevice.DeviceID)
                    serialStream.DFU();
            }

        }
        private int _dFUProgress;
        public int DFUProgress {
            get { return _dFUProgress; }
            set
            {
                _dFUProgress = value;
                if (value == 75)
                {
                    DFU();
                }
            }

        }

      

      
        public void RefreshDevice()
        {
            var detectedDevices = SerialDeviceDetection.RefreshDevice();
            var newdevices = new List<string>();
            var openRGBdevices = new List<Device>();
            var oldDeviceNum = Cards.Count;
            if (OpenRGBClientDevice.DeviceList != null)
            {
                foreach (var device in OpenRGBClientDevice.DeviceList)//add openrgb device to list
                {
                    openRGBdevices.Add(device);
                }

                foreach (var device in OpenRGBClientDevice.DeviceList)// check if device already exist
                {
                    foreach (var item in Cards)
                    {
                        if (device.Serial == item.DeviceSerial)
                            openRGBdevices.Remove(device);
                    }
                }
            }

            if (openRGBdevices.Count > 0)
            {
                var result = HandyControl.Controls.MessageBox.Show("Phát hiện " + openRGBdevices.Count + " Thiết bị OpenRGB" + " Nhấn [Confirm] để add vào Dashboard", "OpenRGB Device", MessageBoxButton.OK, MessageBoxImage.Information);
                if (result == MessageBoxResult.OK)//restart app
                {
                    foreach (var device in openRGBdevices)//convert openRGB device to ambino Device
                    {

                        IDeviceSettings newDevice = new DeviceSettings();
                        newDevice.DeviceName = device.Name.ToString();
                        newDevice.DeviceType = device.Type.ToString();
                        newDevice.DevicePort = device.Location.ToString();
                        newDevice.DeviceID = 151293;
                        newDevice.DeviceSerial = device.Serial;
                        Cards.Add(newDevice);
                    }
                }
            }

            foreach (var device in detectedDevices)
            {
                newdevices.Add(device);
            }

            if (detectedDevices.Count > 0)
            {
                foreach (var device in detectedDevices)
                {
                    foreach (var existedDevice in Cards)
                    {
                        if (existedDevice.DevicePort == device)
                            newdevices.Remove(device);
                    }

                }

                if (newdevices.Count == 1)
                {
                    var result = HandyControl.Controls.MessageBox.Show("Phát hiện Ambino Basic Rev 2 đã kết nối ở " + newdevices[0] + " Nhấn [Confirm] để add vào Dashboard", "Ambino Device", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (result == MessageBoxResult.OK)//restart app
                    {
                        foreach (var device in newdevices)
                        {
                            IDeviceSettings newDevice = new DeviceSettings();
                            newDevice.DeviceName = "Auto Detected Device(Ambino Basic)";
                            newDevice.DeviceType = "ABRev2";
                            newDevice.DevicePort = device;
                            newDevice.DeviceID = Cards.Count + 1;
                            newDevice.DeviceSerial = "151293";
                            Cards.Add(newDevice);


                        }

                    }
                }
                else if (newdevices.Count > 1)
                {
                    string delimiter = ",";
                    var alldevices = string.Join(delimiter, newdevices);
                    var result = HandyControl.Controls.MessageBox.Show("Phát hiện Ambino Basic Rev 2 đã kết nối ở " + alldevices + " Nhấn [Confirm] để add vào Dashboard", "Ambino Device", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (result == MessageBoxResult.OK)//restart app
                    {
                        foreach (var device in newdevices)
                        {
                            IDeviceSettings newDevice = new DeviceSettings();
                            newDevice.DeviceName = "Auto Detected Device";
                            newDevice.DeviceType = "ABRev2";
                            newDevice.DevicePort = device;
                            newDevice.DeviceID = Cards.Count + 1;
                            newDevice.DeviceSerial = "151293";
                            Cards.Add(newDevice);


                        }

                    }
                }

                else if (newdevices.Count == 0)//no device detected in the list
                {

                    HandyControl.Controls.MessageBox.Show("Không tìm thấy thiết bị mới nào của Ambino, kiểm tra lại kết nối hoặc thêm thiết bị theo cách thủ công", "Ambino Device", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // return null;
                }





            }
            if (oldDeviceNum != Cards.Count) //there are changes in device list, we simply restart the application to add process
            {
                WriteJson();
                Application.Restart();
                Process.GetCurrentProcess().Kill();
            }

        }

        //public void ReadFAQ()
        //{
        //    AppName = $"adrilight {App.VersionNumber}";
        //    BuildVersion = "xxxxxxxxxxxxxxxxxxxxxxxxxxx";
        //    LastUpdate = new DateTime(2020, 06, 01);
        //    Author = "zOe";
        //    Git = "xxxxxxx";
        //    FAQ = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book.";
        //}
        public void ReadDataDevice()
        {
          
            AvailablePalette = new ObservableCollection<string>
       {
           "Rainbow",
           "Cloud",
           "Forest",
           "Sunset",
           "Scarlet",
           "Aurora",
           "France",
           "Lemon",
           "Badtrip",
           "Police",
           "Ice and Fire",
           "Custom"

        };
    
            AvailableEffects = new ObservableCollection<string>
      {
            "Sáng theo màn hình",
           "Sáng theo dải màu",
           "Sáng màu tĩnh",
           "Sáng theo nhạc",
           "Atmosphere",
           "Pixelation(Alpha)"
        };
            AvailableMusicPalette = new ObservableCollection<string>
{
           "Rainbow",
           "Cafe",
           "Jazz",
           "Party",
           "Custom"


        };
            AvailableFrequency = new ObservableCollection<string>
{
           "1",
           "2",
           "3",
           "4"


        };
            AvailableMusicMode = new ObservableCollection<string>
{
          "Equalizer",
           "VU metter",
           "End to End",
           "Push Pull",
          "Symetric VU",
          "Floating VU",
          "Center VU",
          "Naughty boy"

        };



        }




        public async void ShowAddNewDialog()
        {
            var newdevice = new DeviceSettings();
            var vm = new ViewModel.AddDeviceViewModel(newdevice);
            var view = new View.AddDevice();
            view.DataContext = vm;
            bool addResult = (bool)await DialogHost.Show(view, "mainDialog");
            if (addResult)
            {
                try
                {
                    if (vm.Device.DeviceType != "ABHV2")
                    {
                     
                        vm.Device.DeviceID = Cards.Count() + 1;

                        Cards.Add(vm.Device);
                        WriteJson();
                        
                    }
                    else
                    {

                        
                        vm.Device.DeviceID = Cards.Count() + 1;
                        vm.Device.IsHUB = true;
                        Cards.Add(vm.Device);
                        // WriteJson();
                       
                        if (vm.ARGB1Selected) // ARGB1 output port is in the list
                        {
                            var argb1 = new DeviceSettings();
                            argb1.DeviceType = "Strip";                           //add to device settings
                            argb1.DeviceID = Cards.Count() + 1;
                            argb1.SpotsX = 16;
                            argb1.SpotsY = 1;
                            argb1.NumLED = 16;
                            argb1.DeviceName = "ARGB1(HUBV2)";
                            argb1.ParrentLocation = vm.Device.DeviceID;
                            argb1.OutputLocation = 0;
                            argb1.IsVissible = false;
                            Cards.Add(argb1);
                        }
                        if (vm.ARGB2Selected)
                        {
                            var argb2 = new DeviceSettings();
                            argb2.DeviceType = "Strip";                           //add to device settings
                            argb2.DeviceID = Cards.Count() + 1;
                            argb2.SpotsX = 160;
                            argb2.SpotsY = 1;
                            argb2.NumLED = 160;
                            argb2.DeviceName = "ARGB2(HUBV2)";
                            argb2.ParrentLocation = vm.Device.DeviceID;
                            argb2.OutputLocation = 1;
                            argb2.IsVissible = false;
                            Cards.Add(argb2);
                        }
                        if (vm.PCI1Selected)
                        {
                            var PCI = new DeviceSettings();
                            PCI.DeviceType = "Square";                           //add to device settings
                            PCI.DeviceID = Cards.Count() + 1;
                            PCI.SpotsX = 50;
                            PCI.SpotsY = 1;
                            PCI.NumLED = 50;
                            PCI.DeviceName = "PCI1(HUBV2)";
                            PCI.ParrentLocation = vm.Device.DeviceID;
                            PCI.OutputLocation = 2;
                            PCI.IsVissible = false;
                            Cards.Add(PCI);
                        }
                        if (vm.PCI2Selected)
                        {
                            var PCI = new DeviceSettings();
                            PCI.DeviceType = "Square";                           //add to device settings
                            PCI.DeviceID = Cards.Count() + 1;
                            PCI.SpotsX = 50;
                            PCI.SpotsY = 1;
                            PCI.NumLED = 50;
                            PCI.DeviceName = "PCI2(HUBV2)";
                            PCI.ParrentLocation = vm.Device.DeviceID;
                            PCI.OutputLocation = 3;
                            PCI.IsVissible = false;
                            Cards.Add(PCI);
                        }
                        if (vm.PCI3Selected)
                        {
                            var PCI = new DeviceSettings();
                            PCI.DeviceType = "Square";                           //add to device settings
                            PCI.DeviceID = Cards.Count() + 1;
                            PCI.SpotsX = 50;
                            PCI.SpotsY = 1;
                            PCI.NumLED = 50;
                            PCI.DeviceName = "PCI3(HUBV2)";
                            PCI.ParrentLocation = vm.Device.DeviceID;
                            PCI.OutputLocation = 4;
                            PCI.IsVissible = false;
                            Cards.Add(PCI);
                        }
                        if (vm.PCI4Selected)
                        {
                            var PCI = new DeviceSettings();
                            PCI.DeviceType = "Strip";                           //add to device settings
                            PCI.DeviceID = Cards.Count() + 1;
                            PCI.SpotsX = 22;
                            PCI.SpotsY = 1;
                            PCI.NumLED = 22;
                            PCI.DeviceName = "PCI4(HUBV2)";
                            PCI.ParrentLocation = vm.Device.DeviceID;
                            PCI.OutputLocation = 5;
                            PCI.IsVissible = false;
                            Cards.Add(PCI);
                        }


                        WriteJson();
                        // _isAddnew = false;
                    }

                }
                catch (Exception ex)
                {
                    HandyControl.Controls.MessageBox.Show(ex.Message);
                }
                Application.Restart();
                Process.GetCurrentProcess().Kill();
            }


        }

        
        public Visibility ARGB1Visibility {
            get
            {
                foreach (var device in Cards)
                {
                    if (device.ParrentLocation == ParrentLocation)
                    {
                        if (device.OutputLocation == 0)
                            return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed; ;
            }

           

        }
       
        public Visibility ARGB2Visibility {
            get
            {
                foreach (var device in Cards)
                {
                    if (device.ParrentLocation == ParrentLocation)
                    {
                        if (device.OutputLocation == 1)
                            return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed; ;
            }

           

        }
      
        public Visibility PCI1 {
            get
            {
                foreach (var device in Cards)
                {
                    if (device.ParrentLocation == ParrentLocation)
                    {
                        if (device.OutputLocation == 2)
                            return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed; ;
            }

        }
      
        public Visibility PCI2 {
            get
            {
                foreach (var device in Cards)
                {
                    if (device.ParrentLocation == ParrentLocation)
                    {
                        if (device.OutputLocation == 3)
                            return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed; ;
            }

           

        }
       
        public Visibility PCI3 {
            get
            {
                foreach (var device in Cards)
                {
                    if (device.ParrentLocation == ParrentLocation)
                    {
                        if (device.OutputLocation == 4)
                            return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed; ;
            }

           

        }
     
        public Visibility PCI4 {
            get
            {
                foreach (var device in Cards)
                {
                    if (device.ParrentLocation == ParrentLocation)
                    {
                        if (device.OutputLocation == 5)
                            return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed; ;
            }

           

        }
       
        public Visibility SpotSetEnable {
            get
            {
                if (CurrentDevice.DeviceType == "ABRev2")
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

           

        }



        private bool _aRGB1Checked;
        public bool ARGB1Checked {

            get { return _aRGB1Checked; }
            set
            {
                _aRGB1Checked = value;
                if (value)
                {

                    foreach (var device in Cards)
                        if (device.ParrentLocation == ParrentLocation && device.OutputLocation == 0)
                        {
                            GotoChild(device);
                            RaisePropertyChanged(() => DFUVisibility);

                        }


                    foreach (var spotset in SpotSets)
                    {

                        if (spotset.ID == CurrentDevice.DeviceID)
                        {
                            PreviewSpots = spotset.Spots;
                        }
                    }
                }
            }
        }

        private bool _aRGB2Checked;
        public bool ARGB2Checked {

            get { return _aRGB2Checked; }
            set
            {
                _aRGB2Checked = value;
                if (value)
                {

                    foreach (var device in Cards)
                        if (device.ParrentLocation == ParrentLocation && device.OutputLocation == 1)
                            GotoChild(device);
                    RaisePropertyChanged(() => DFUVisibility);
                    foreach (var spotset in SpotSets)
                    {

                        if (spotset.ID == CurrentDevice.DeviceID)
                        {
                            PreviewSpots = spotset.Spots;
                        }
                    }
                }
            }
        }
        private bool _pCI1Checked;
        public bool PCI1Checked {

            get { return _pCI1Checked; }
            set
            {
                _pCI1Checked = value;
                if (value)
                {

                    foreach (var device in Cards)
                        if (device.ParrentLocation == ParrentLocation && device.OutputLocation == 2)
                            GotoChild(device);
                    RaisePropertyChanged(() => DFUVisibility);
                    foreach (var spotset in SpotSets)
                    {

                        if (spotset.ID == CurrentDevice.DeviceID)
                        {
                            PreviewSpots = spotset.Spots;
                        }
                    }
                }
            }
        }

        private bool _pCI2Checked;
        public bool PCI2Checked {

            get { return _pCI2Checked; }
            set
            {
                _pCI2Checked = value;
                if (value)
                {

                    foreach (var device in Cards)
                        if (device.ParrentLocation == ParrentLocation && device.OutputLocation == 3)
                            GotoChild(device);
                    RaisePropertyChanged(() => DFUVisibility);
                    foreach (var spotset in SpotSets)
                    {

                        if (spotset.ID == CurrentDevice.DeviceID)
                        {
                            PreviewSpots = spotset.Spots;
                        }
                    }
                }
            }
        }
        private bool _pCI3Checked;
        public bool PCI3Checked {

            get { return _pCI3Checked; }
            set
            {
                _pCI3Checked = value;
                if (value)
                {

                    foreach (var device in Cards)
                        if (device.ParrentLocation == ParrentLocation && device.OutputLocation == 4)
                            GotoChild(device);
                    RaisePropertyChanged(() => DFUVisibility);
                    foreach (var spotset in SpotSets)
                    {

                        if (spotset.ID == CurrentDevice.DeviceID)
                        {
                            PreviewSpots = spotset.Spots;
                        }
                    }
                }
            }
        }
        private bool _pCI4Checked;
        public bool PCI4Checked {

            get { return _pCI4Checked; }
            set
            {
                _pCI4Checked = value;
                if (value)
                {

                    foreach (var device in Cards)
                        if (device.ParrentLocation == ParrentLocation && device.OutputLocation == 5)
                            GotoChild(device);
                    RaisePropertyChanged(() => DFUVisibility);
                    foreach (var spotset in SpotSets)
                    {

                        if (spotset.ID == CurrentDevice.DeviceID)
                        {
                            PreviewSpots = spotset.Spots;
                        }
                    }
                }
            }
        }


        public async void ShowDeleteDialog()
        {
            var view = new View.DeleteMessageDialog();
            DeleteMessageDialogViewModel dialogViewModel = new DeleteMessageDialogViewModel(CurrentDevice);
            view.DataContext = dialogViewModel;
            bool addResult = (bool)await DialogHost.Show(view, "mainDialog");
            if (addResult)
            {
                DeleteCard(CurrentDevice);
                int counter = 1;
                foreach (var card in Cards)
                {
                    card.DeviceID = counter;
                    counter++;

                }
                WriteJson();
                Application.Restart();
                Process.GetCurrentProcess().Kill();
            }


        }


        public void DeleteCard(IDeviceSettings deviceInfo)
        {
            var childcards = new ObservableCollection<IDeviceSettings>();
            if (deviceInfo.IsHUB)
            {
                foreach (var device in Cards)
                {
                    if (device.ParrentLocation == deviceInfo.DeviceID)
                        childcards.Add(device);
                }
            }
            foreach (var device in childcards)
            {
                Cards.Remove(device);
            }
            Cards.Remove(deviceInfo);
            WriteJson();
        }

        public void WriteJson()
        {
            var devices = new List<IDeviceSettings>();


            foreach (var item in Cards)
            {
                devices.Add(item);
            }




            // if devices disconnected,change connect status



            var json = JsonConvert.SerializeObject(devices, Formatting.Indented);
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonDeviceNameAndPath, json);
        }
       
        /// <summary>
        /// Change View
        /// </summary>
        /// <param name="menuItem"></param>
        public void ChangeView(VerticalMenuItem menuItem)
        {
            SelectedVerticalMenuItem = menuItem;
            SetMenuItemActiveStatus(menuItem.Text);
        }
        public void WriteDeviceInfoJson()
        {
           
            var devices = new List<IDeviceSettings>();
            foreach (var item in Cards)
            {
                devices.Add(item);
            }
            var json = JsonConvert.SerializeObject(devices, Formatting.Indented);
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonDeviceFileNameAndPath, json);
        }
        public void GotoChild(IDeviceSettings card)
        {
           
            SelectedVerticalMenuItem = MenuItems.FirstOrDefault(t => t.Text == general);
            IsDashboardType = false;
            CurrentDevice = card;
            if (CurrentDevice.IsHUB)
            {
                ParrentLocation = CurrentDevice.DeviceID;
            }
            if (CurrentDevice.DeviceID == 151293)
            {

            }
            else
            {
                foreach (var spotset in SpotSets)
                    if (spotset.ID == CurrentDevice.DeviceID)
                    {
                        PreviewSpots = spotset.Spots;
                    }

            }



            SetMenuItemActiveStatus(lighting);
        }
        public void BackToDashboard()
        {

          
            IsDashboardType = true;
            SelectedVerticalMenuItem = MenuItems.FirstOrDefault();
            SetMenuItemActiveStatus(dashboard);
        }
        public void BackToDashboardAndDelete(IDeviceSettings device)
        {
            Cards.Remove(device);
            IsDashboardType = true;
            SelectedVerticalMenuItem = MenuItems.FirstOrDefault();
            SetMenuItemActiveStatus(dashboard);
        }
        /// <summary>
        /// Load vertical menu
        /// </summary>
        public void LoadMenu()
        {
            MenuItems = new ObservableCollection<VerticalMenuItem>();
            MenuItems.Add(new VerticalMenuItem() { Text = dashboard, IsActive = true, Type = MenuButtonType.Dashboard });
            MenuItems.Add(new VerticalMenuItem() { Text = deviceSetting, IsActive = false, Type = MenuButtonType.Dashboard });
            MenuItems.Add(new VerticalMenuItem() { Text = appSetting, IsActive = false, Type = MenuButtonType.Dashboard });
            MenuItems.Add(new VerticalMenuItem() { Text = canvasLighting, IsActive = false, Type = MenuButtonType.Dashboard });
            MenuItems.Add(new VerticalMenuItem() { Text = general, IsActive = true, Type = MenuButtonType.General });
            MenuItems.Add(new VerticalMenuItem() { Text = lighting, IsActive = false, Type = MenuButtonType.General });

        }
        /// <summary>
        /// set active state
        /// </summary>
        /// <param name="key"></param>
        public void SetMenuItemActiveStatus(string key)
        {
            foreach (var item in MenuItems)
            {
                item.IsActive = item.Text == key;
            }
        }
        /// <summary>
        /// hide show vertical menu item
        /// </summary>
        /// <param name="isDashboard"></param>
        private void LoadMenuByType(bool isDashboard)
        {
            if (MenuItems == null) return;
            foreach (var item in MenuItems)
            {
                item.IsVisible = item.Type == MenuButtonType.Dashboard ? isDashboard : !isDashboard;
            }
            RaisePropertyChanged(nameof(MenuItems));
        }




    }
}
