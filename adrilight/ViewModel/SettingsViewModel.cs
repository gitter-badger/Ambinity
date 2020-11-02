﻿using adrilight.DesktopDuplication;
using adrilight.Resources;
using adrilight.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.ApplicationInsights;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NAudio.CoreAudioApi;
using System.Management;
using System.IO;
using System.Collections.ObjectModel;
using adrilight.Settings;

namespace adrilight.ViewModel
{

    class SettingsViewModel : ViewModelBase
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();

        private const string ProjectPage = "http://ambino.net";
        private const string IssuesPage = "https://www.messenger.com/t/109869992932970";
        private const string LatestReleasePage = "https://github.com/fabsenet/adrilight/releases/latest";
        ManagementEventWatcher watcher = new ManagementEventWatcher();
        public SettingsViewModel(IUserSettings userSettings, IList<ISelectableViewPart> selectableViewParts
            , ISpotSet spotSet, IContext context, TelemetryClient tc, ISerialStream serialStream)
        {
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
            watcher = new ManagementEventWatcher(query);
            watcher.EventArrived += (s, e) => RaisePropertyChanged(() => AvailableComPorts);
            // watcher.Query = query;
            watcher.Start();
            // watcher.WaitForNextEvent();
            if (selectableViewParts == null)
            {
                throw new ArgumentNullException(nameof(selectableViewParts));
            }

            this.Settings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
            this.spotSet = spotSet ?? throw new ArgumentNullException(nameof(spotSet));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            this.serialStream = serialStream ?? throw new ArgumentNullException(nameof(serialStream));
            SelectableViewParts = selectableViewParts.OrderBy(p => p.Order).ToList();
            BackUpView = selectableViewParts.OrderBy(p => p.Order).ToList();
            for (int i = 0; i < 6; i++)
            {
                SelectableViewParts.RemoveAt(SelectableViewParts.Count - 1);
                BackUpView.RemoveAt(BackUpView.Count - 1);
            }

            if (!Settings.Pro11 && !Settings.Pro12
                    && !Settings.Pro13)
            {
                for (int j = 0; j < SelectableViewParts.Count; j++)
                {
                    if (SelectableViewParts[j].Order == 24)
                    {
                        SelectableViewParts.RemoveAt(j);
                    }
                }
            }
            if (!Settings.Pro21)
            {
                for (int j = 0; j < SelectableViewParts.Count; j++)
                {
                    if (SelectableViewParts[j].Order == 26)
                    {
                        SelectableViewParts.RemoveAt(j);
                    }
                }

            }
            if (!Settings.Pro22)
            {
                for (int j = 0; j < SelectableViewParts.Count; j++)
                {
                    if (SelectableViewParts[j].Order == 27)
                    {
                        SelectableViewParts.RemoveAt(j);
                    }
                }

            }
            if (!Settings.Pro31)
            {
                for (int j = 0; j < SelectableViewParts.Count; j++)
                {
                    if (SelectableViewParts[j].Order == 28)
                    {
                        SelectableViewParts.RemoveAt(j);
                    }
                }
            }

            lsScreen = new ObservableCollection<string>();
            lsScreen.Add("1");
            lsScreen.Add("2");
            lsScreen.Add("3");

            lsMode = new ObservableCollection<string>();
            lsMode.Add("Linear");
            lsMode.Add("Non Linear");


            //string readText = "";
            //using (StreamReader readtext = new StreamReader("product.ls"))
            //{
            //    readText = readtext.ReadToEnd();
            //}
            //string[] delim = { Environment.NewLine, "\n" };
            //string pro = readText.Split(delim, StringSplitOptions.None)[1];
            //string[] val = pro.Split(',');


#if DEBUG
            SelectedViewPart = SelectableViewParts.First();
#else
            SelectedViewPart = SelectableViewParts.First();
#endif

            PossibleLedCountsVertical = Enumerable.Range(10, 190).ToList();
            PossibleLedCountsHorizontal = Enumerable.Range(10, 290).ToList();

            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(SelectedViewPart):
                        var name = SelectedViewPart?.ViewPartName ?? "nothing";
                        tc.TrackPageView(name);
                        break;
                }
            };

            Settings.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(Settings.SpotsX):
                        Settings.OffsetLed = Settings.SpotsX - 1;
                        RaisePropertyChanged(() => SpotsXMaximum);
                        RaisePropertyChanged(() => LedCount);
                        RaisePropertyChanged(() => OffsetLedMaximum);
                        break;

                    case nameof(Settings.SpotsY):
                        RaisePropertyChanged(() => SpotsYMaximum);
                        RaisePropertyChanged(() => LedCount);
                        RaisePropertyChanged(() => OffsetLedMaximum);
                        break;

                    case nameof(Settings.LedsPerSpot):
                        RaisePropertyChanged(() => LedCount);
                        RaisePropertyChanged(() => OffsetLedMaximum);
                        break;

                    case nameof(Settings.UseLinearLighting):
                        RaisePropertyChanged(() => UseNonLinearLighting);
                        break;

                    case nameof(Settings.OffsetLed):
                        RaisePropertyChanged(() => OffsetLedMaximum);
                        break;

                    case nameof(Settings.Autostart):
                        if (Settings.Autostart)
                        {
                            StartUpManager.AddApplicationToCurrentUserStartup();
                        }
                        else
                        {
                            StartUpManager.RemoveApplicationFromCurrentUserStartup();
                        }
                        break;

                    case nameof(Settings.ComPort):
                        RaisePropertyChanged(() => TransferCanBeStarted);
                        RaisePropertyChanged(() => TransferCanNotBeStarted);
                        break;
                }
            };


            _insideEffects = new ObservableCollection<IEffect>()
           {
              new IEffect(){ Id=1, Name="Sáng theo hiệu ứng"}
                    ,new IEffect(){Id=2, Name="Sáng theo màn hình"}
                    ,new IEffect(){Id=3, Name="Sáng màu tĩnh"}
                    ,new IEffect(){Id=4, Name="Sáng theo nhạc"}
                    ,new IEffect(){Id=5, Name="Đồng bộ Mainboard"}
                    ,new IEffect(){Id=6, Name="Tắt"}
            };

            _outsideEffects = new ObservableCollection<IEffect>()
          {
              new IEffect(){ Id=1, Name="Sáng theo hiệu ứng"}
                    ,new IEffect(){Id=2, Name="Sáng theo màn hình"}
                    ,new IEffect(){Id=3, Name="Sáng màu tĩnh"}
                    ,new IEffect(){Id=4, Name="Sáng theo nhạc"}
                    ,new IEffect(){Id=5, Name="Đồng bộ Mainboard"}
                    ,new IEffect(){Id=6, Name="Tắt"}
            };

            _tableEffects = new ObservableCollection<IEffect>()
          {
              new IEffect(){ Id=1, Name="Sáng theo hiệu ứng"}
                    ,new IEffect(){Id=2, Name="Sáng theo màn hình"}
                    ,new IEffect(){Id=3, Name="Sáng màu tĩnh"}
                    ,new IEffect(){Id=4, Name="Sáng theo nhạc"}
                    ,new IEffect(){Id=5, Name="Đồng bộ Mainboard"}
                    ,new IEffect(){Id=6, Name="Tắt"}
            };


        }

        //  private IUserSettings UserSettings { get; }

        public string Title { get; } = $"adrilight {App.VersionNumber}";
        public int LedCount => spotSet.CountLeds(Settings.SpotsX, Settings.SpotsY) * Settings.LedsPerSpot;

        public bool TransferCanBeStarted => serialStream.IsValid();
        public bool TransferCanNotBeStarted => !TransferCanBeStarted;
        public bool NotAdvancesettings => !Settings.Advancesettings;

        public bool UseNonLinearLighting {
            get => !Settings.UseLinearLighting;
            set => Settings.UseLinearLighting = !value;

        }

        public IUserSettings Settings { get; }
        public IContext Context { get; }
        public IList<String> _AvailableComPorts;
        public IList<String> AvailableComPorts {
            get
            {
                _AvailableComPorts = SerialPort.GetPortNames().Concat(new[] { "Không có" }).ToList();
                _AvailableComPorts.Remove("COM1");
                return _AvailableComPorts;
            }
        }
        //   private IList<string> RefreshPortNames() => AvailableComPorts;


        // public IList<String> _AllAvailableComPorts;



        //  public IList<String> AvailableComPorts2 { get; } = AvailableComPorts.Remove()

        public List<string> lsDevice = new List<string>();


        public ObservableCollection<IEffect> _insideEffects { get; }
        public ObservableCollection<IEffect> _outsideEffects { get; }
        public ObservableCollection<IEffect> _tableEffects { get; }

        public IList<ISelectableViewPart> BackUpView { get; }

        public IList<ISelectableViewPart> SelectableViewParts { get; }

        public IList<int> PossibleLedCountsHorizontal { get; }
        public IList<int> PossibleLedCountsVertical { get; }

        public ISelectableViewPart _selectedViewPart;
        public ISelectableViewPart SelectedViewPart {
            get => _selectedViewPart;
            set
            {
                Set(ref _selectedViewPart, value);
                _log.Info($"SelectedViewPart is now {_selectedViewPart?.ViewPartName}");

                //IsPreviewTabOpen = _selectedViewPart is View.SettingsWindowComponents.Preview.PreviewSelectableViewPart;
                IsPreviewTabOpen = _selectedViewPart is View.SettingsWindowComponents.LedOutsideCase.LedOutsideCaseSelectableViewPart;
            }
        }

        private bool _isPreviewTabOpen;
        public bool IsPreviewTabOpen {
            get => _isPreviewTabOpen;
            private set
            {
                Set(ref _isPreviewTabOpen, value);
                _log.Info($"IsPreviewTabOpen is now {_isPreviewTabOpen}");
            }
        }

        private bool _isSettingsWindowOpen;
        public bool IsSettingsWindowOpen {
            get => _isSettingsWindowOpen;
            set
            {
                Set(ref _isSettingsWindowOpen, value);
                _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public void SetPreviewImage(Bitmap image)
        {
            Context.Invoke(() =>
            {
                if (PreviewImageSource == null)
                {
                    //first run creates writableimage
                    var imagePtr = image.GetHbitmap();
                    try
                    {
                        var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(imagePtr, IntPtr.Zero, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                        PreviewImageSource = new WriteableBitmap(bitmapSource);
                    }
                    finally
                    {
                        var i = DeleteObject(imagePtr);
                    }
                }
                else
                {
                    //next runs reuse the writable image
                    Rectangle colorBitmapRectangle = new Rectangle(0, 0, image.Width, image.Height);
                    Int32Rect colorBitmapInt32Rect = new Int32Rect(0, 0, PreviewImageSource.PixelWidth, PreviewImageSource.PixelHeight);

                    BitmapData data = image.LockBits(colorBitmapRectangle, ImageLockMode.WriteOnly, image.PixelFormat);

                    PreviewImageSource.WritePixels(colorBitmapInt32Rect, data.Scan0, data.Width * data.Height * 4, data.Stride);

                    image.UnlockBits(data);
                }
            });
        }
        public WriteableBitmap _previewImageSource;
        public WriteableBitmap PreviewImageSource {
            get => _previewImageSource;
            set
            {
                _log.Info("PreviewImageSource created.");
                Set(ref _previewImageSource, value);

                RaisePropertyChanged(() => ScreenWidth);
                RaisePropertyChanged(() => ScreenHeight);
                RaisePropertyChanged(() => CanvasWidth);
                RaisePropertyChanged(() => CanvasHeight);
            }
        }


        public ICommand OpenUrlProjectPageCommand { get; } = new RelayCommand(() => OpenUrl(ProjectPage));
        public ICommand OpenUrlIssuesPageCommand { get; } = new RelayCommand(() => OpenUrl(IssuesPage));
        public ICommand OpenUrlLatestReleaseCommand { get; } = new RelayCommand(() => OpenUrl(LatestReleasePage));
        private static void OpenUrl(string url) => Process.Start(url);

        public ICommand ExitAdrilight { get; } = new RelayCommand(() => App.Current.Shutdown(0));

        private int _spotsXMaximum = 300;
        public int SpotsXMaximum {
            get
            {
                return _spotsXMaximum = Math.Max(Settings.SpotsX, _spotsXMaximum);
            }
        }

        private int _spotsYMaximum = 300;
        private readonly ISpotSet spotSet;
        private readonly ISerialStream serialStream;

        public int SpotsYMaximum {
            get
            {
                return _spotsYMaximum = Math.Max(Settings.SpotsY, _spotsYMaximum);
            }
        }

        public int OffsetLedMaximum => Math.Max(Settings.OffsetLed, LedCount);

        //public int ScreenWidth => (int)System.Windows.SystemParameters.PrimaryScreenWidth;
        //public int ScreenHeight => (int)System.Windows.SystemParameters.PrimaryScreenHeight;

        public int ScreenWidth => PreviewImageSource?.PixelWidth ?? 1000;
        public int ScreenHeight => PreviewImageSource?.PixelHeight ?? 1000;

        public int CanvasPadding => 3 / DesktopDuplicator.ScalingFactor;

        public int CanvasWidth => ScreenWidth + 2 * CanvasPadding;
        public int CanvasHeight => ScreenHeight + 2 * CanvasPadding;


        public ISpot[] _previewSpots;
        public ISpot[] PreviewSpots {
            get => _previewSpots;
            set
            {
                _previewSpots = value;
                RaisePropertyChanged();
            }
        }

        //private bool _Pro11 = true;
        //public bool Pro11 {
        //    get => _Pro11;
        //    set
        //    {
        //        _Pro11 = value;
        //        RaisePropertyChanged("Pro11");
        //    }
        //}

        //private bool _Pro12 = true;
        //public bool Pro12 {
        //    get => _Pro12;
        //    set
        //    {
        //        _Pro12 = value;
        //        RaisePropertyChanged("Pro12");
        //    }
        //}

        //private bool _Pro13 = true;
        //public bool Pro13 {
        //    get => _Pro13;
        //    set
        //    {
        //        _Pro13 = value;
        //        RaisePropertyChanged("Pro13");
        //    }
        //}

        //private bool _Pro14;
        //public bool Pro14 {
        //    get => _Pro14;
        //    set
        //    {
        //        _Pro14 = value;
        //        RaisePropertyChanged("Pro14");
        //    }
        //}

        //private bool _Pro21 = true;
        //public bool Pro21 {
        //    get => _Pro21;
        //    set
        //    {
        //        _Pro21 = value;
        //        RaisePropertyChanged("Pro21");
        //    }
        //}

        //private bool _Pro22 = true;
        //public bool Pro22 {
        //    get => _Pro22;
        //    set
        //    {
        //        _Pro22 = value;
        //        RaisePropertyChanged("Pro22");
        //    }
        //}

        //private bool _Pro31;
        //public bool Pro31 {
        //    get => _Pro31;
        //    set
        //    {
        //        _Pro31 = value;
        //        RaisePropertyChanged("Pro31");
        //    }
        //}

        //private bool _Pro210 = true;
        //public bool Pro210 {
        //    get => _Pro210;
        //    set
        //    {
        //        _Pro210 = value;
        //        RaisePropertyChanged("Pro210");
        //    }
        //}

        //private bool _Pro211;
        //public bool Pro211 {
        //    get => _Pro211;
        //    set
        //    {
        //        _Pro211 = value;
        //        RaisePropertyChanged("Pro211");
        //    }
        //}

        //private bool _Pro220;
        //public bool Pro220 {
        //    get => _Pro220;
        //    set
        //    {
        //        _Pro220 = value;
        //        RaisePropertyChanged("Pro220");
        //    }
        //}

        //private bool _Pro221;
        //public bool Pro221 {
        //    get => _Pro221;
        //    set
        //    {
        //        _Pro221 = value;
        //        RaisePropertyChanged("Pro221");
        //    }
        //}

        public ObservableCollection<string> lsScreen { get; private set; }

        public ObservableCollection<string> lsMode { get; private set; }

        public string aScreen { get; set; }
        public string aMode { get; set; }

        //private ObservableCollection<IEffect> _effects;

        //public ObservableCollection<Effect> Effects {
        //    get { return _effects; }
        //    set { _effects = value; }
        //}
        private IEffect effect;

        public IEffect AEffect {
            get { return effect; }
            set { effect = value; }
        }


        public bool disEffect = false;
        public bool disScreen = false;
        public bool disStatic = false;
        public bool disMusic = false;
        public bool disMain = false;

    }

    //public class Effect
    //{

    //    private int _id;

    //    public int Id {
    //        get { return _id; }
    //        set { _id = value; }
    //    }
    //    private string _name;

    //    public string Name {
    //        get { return _name; }
    //        set { _name = value; }
    //    }

    //    private bool isDisplay;
    //    public bool IsDisplay {
    //        get { return isDisplay; }
    //        set { isDisplay = value; }
    //    }
    //}

}









//using adrilight.DesktopDuplication;
//using adrilight.Resources;
//using adrilight.View;
//using GalaSoft.MvvmLight;
//using GalaSoft.MvvmLight.CommandWpf;
//using Microsoft.ApplicationInsights;
//using NLog;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.IO.Ports;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using NAudio.CoreAudioApi;
//using System.Management;
//using System.IO;
//using System.Collections.ObjectModel;
//using adrilight.Settings;



//namespace adrilight.ViewModel
//{

//    class SettingsViewModel : ViewModelBase
//    {
//        private static ILogger _log = LogManager.GetCurrentClassLogger();

//        private const string ProjectPage = "http://ambino.net";
//        private const string IssuesPage = "https://www.messenger.com/t/109869992932970";
//        private const string LatestReleasePage = "https://github.com/fabsenet/adrilight/releases/latest";
//         ManagementEventWatcher watcher =new ManagementEventWatcher();
//        public SettingsViewModel(IUserSettings userSettings, IList<ISelectableViewPart> selectableViewParts
//            , ISpotSet spotSet, IContext context, TelemetryClient tc, ISerialStream serialStream)
//        {
//            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
//            watcher = new ManagementEventWatcher(query);
//            watcher.EventArrived += (s, e) => RaisePropertyChanged(() => AvailableComPorts);
//            // watcher.Query = query;
//            watcher.Start();
//           // watcher.WaitForNextEvent();
//            if (selectableViewParts == null)
//            {
//                throw new ArgumentNullException(nameof(selectableViewParts));
//            }

//            this.Settings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
//            this.spotSet = spotSet ?? throw new ArgumentNullException(nameof(spotSet));
//            Context = context ?? throw new ArgumentNullException(nameof(context));
//            this.serialStream = serialStream ?? throw new ArgumentNullException(nameof(serialStream));
//            SelectableViewParts = selectableViewParts.OrderBy(p => p.Order)
//                .ToList();


//            BackUpView = selectableViewParts.OrderBy(p => p.Order).ToList();
//            for (int i = 0; i < 6; i++)
//            {
//                SelectableViewParts.RemoveAt(SelectableViewParts.Count - 1);
//                BackUpView.RemoveAt(BackUpView.Count - 1);
//            }

//            lsScreen = new ObservableCollection<string>();
//            lsScreen.Add("1");
//            lsScreen.Add("2");
//            lsScreen.Add("3");

//            lsMode = new ObservableCollection<string>();
//            lsMode.Add("Linear Lighting");
//            lsMode.Add("Non Linear Fading");


//#if DEBUG
//            SelectedViewPart = SelectableViewParts.First();
//#else
//            SelectedViewPart = SelectableViewParts.First();
//#endif

//            PossibleLedCountsVertical = Enumerable.Range(10, 190).ToList();
//            PossibleLedCountsHorizontal = Enumerable.Range(10, 290).ToList();

//            PropertyChanged += (s, e) =>
//            {
//                switch (e.PropertyName)
//                {
//                    case nameof(SelectedViewPart):
//                        var name = SelectedViewPart?.ViewPartName ?? "nothing";
//                        tc.TrackPageView(name);
//                        break;
//                }
//            };

//            Settings.PropertyChanged += (s, e) =>
//            {
//                switch (e.PropertyName)
//                {
//                    case nameof(Settings.SpotsX):
//                        RaisePropertyChanged(() => SpotsXMaximum);
//                        RaisePropertyChanged(() => LedCount);
//                        RaisePropertyChanged(() => OffsetLedMaximum);
//                        break;

//                    case nameof(Settings.SpotsY):
//                        RaisePropertyChanged(() => SpotsYMaximum);
//                        RaisePropertyChanged(() => LedCount);
//                        RaisePropertyChanged(() => OffsetLedMaximum);
//                        break;

//                    case nameof(Settings.LedsPerSpot):
//                        RaisePropertyChanged(() => LedCount);
//                        RaisePropertyChanged(() => OffsetLedMaximum);
//                        break;

//                    case nameof(Settings.UseLinearLighting):
//                        RaisePropertyChanged(() => UseNonLinearLighting);
//                        break;

//                    case nameof(Settings.OffsetLed):
//                        RaisePropertyChanged(() => OffsetLedMaximum);
//                        break;

//                    case nameof(Settings.Autostart):
//                        if (Settings.Autostart)
//                        {
//                            StartUpManager.AddApplicationToCurrentUserStartup();
//                        }
//                        else
//                        {
//                            StartUpManager.RemoveApplicationFromCurrentUserStartup();
//                        }
//                        break;

//                    case nameof(Settings.ComPort):
//                        RaisePropertyChanged(() => TransferCanBeStarted);
//                        RaisePropertyChanged(() => TransferCanNotBeStarted);
//                        break;
//                }
//            };
//        }



//        public string Title { get; } = $"adrilight {App.VersionNumber}";
//        public int LedCount => spotSet.CountLeds(Settings.SpotsX, Settings.SpotsY) * Settings.LedsPerSpot;

//        public bool TransferCanBeStarted => serialStream.IsValid();
//        public bool TransferCanNotBeStarted => !TransferCanBeStarted;
//        public bool NotAdvancesettings => !Settings.Advancesettings;

//        public bool UseNonLinearLighting
//        {
//            get => !Settings.UseLinearLighting;
//            set => Settings.UseLinearLighting = !value;
//        }

//        public IUserSettings Settings { get; }
//        public IContext Context { get; }
//        public IList<String> _AvailableComPorts;
//        public IList<String> AvailableComPorts {
//            get
//            {
//                _AvailableComPorts = SerialPort.GetPortNames().Concat(new[] { "Không có" }).ToList();
//                _AvailableComPorts.Remove("COM1");
//                return _AvailableComPorts;
//            }
//        }
//        //   private IList<string> RefreshPortNames() => AvailableComPorts;


//        // public IList<String> _AllAvailableComPorts;



//        //  public IList<String> AvailableComPorts2 { get; } = AvailableComPorts.Remove()



//        public ObservableCollection<IEffect> _effects { get; }

//        public IList<ISelectableViewPart> BackUpView { get; }



//        public IList<ISelectableViewPart> SelectableViewParts { get; }

//        public IList<int> PossibleLedCountsHorizontal { get; }
//        public IList<int> PossibleLedCountsVertical { get; }

//        public ISelectableViewPart _selectedViewPart;
//        public ISelectableViewPart SelectedViewPart
//        {
//            get => _selectedViewPart;
//            set
//            {
//                Set(ref _selectedViewPart, value);
//                _log.Info($"SelectedViewPart is now {_selectedViewPart?.ViewPartName}");

//                //IsPreviewTabOpen = _selectedViewPart is View.SettingsWindowComponents.Preview.PreviewSelectableViewPart;
//                IsPreviewTabOpen = _selectedViewPart is View.SettingsWindowComponents.LedOutsideCase.LedOutsideCaseSelectableViewPart;
//            }
//        }

//        private bool _isPreviewTabOpen;
//        public bool IsPreviewTabOpen
//        {
//            get => _isPreviewTabOpen;
//            private set
//            {
//                Set(ref _isPreviewTabOpen, value);
//                _log.Info($"IsPreviewTabOpen is now {_isPreviewTabOpen}");
//            }
//        }

//        private bool _isSettingsWindowOpen;
//        public bool IsSettingsWindowOpen
//        {
//            get => _isSettingsWindowOpen;
//            set
//            {
//                Set(ref _isSettingsWindowOpen, value);
//                _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
//            }
//        }

//        [DllImport("gdi32.dll")]
//        public static extern bool DeleteObject(IntPtr hObject);

//        public void SetPreviewImage(Bitmap image)
//        {
//            Context.Invoke(() =>
//            {
//                if (PreviewImageSource == null)
//                {
//                    //first run creates writableimage
//                    var imagePtr = image.GetHbitmap();
//                    try
//                    {
//                        var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(imagePtr, IntPtr.Zero, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
//                        PreviewImageSource = new WriteableBitmap(bitmapSource);
//                    }
//                    finally
//                    {
//                        var i = DeleteObject(imagePtr);
//                    }
//                }
//                else
//                {
//                    //next runs reuse the writable image
//                    Rectangle colorBitmapRectangle = new Rectangle(0, 0, image.Width, image.Height);
//                    Int32Rect colorBitmapInt32Rect = new Int32Rect(0, 0, PreviewImageSource.PixelWidth, PreviewImageSource.PixelHeight);

//                    BitmapData data = image.LockBits(colorBitmapRectangle, ImageLockMode.WriteOnly, image.PixelFormat);

//                    PreviewImageSource.WritePixels(colorBitmapInt32Rect, data.Scan0, data.Width * data.Height * 4, data.Stride);

//                    image.UnlockBits(data);
//                }
//            });
//        }
//        public WriteableBitmap _previewImageSource;
//        public WriteableBitmap PreviewImageSource
//        {
//            get => _previewImageSource;
//            set
//            {
//                _log.Info("PreviewImageSource created.");
//                Set(ref _previewImageSource, value);

//                RaisePropertyChanged(() => ScreenWidth);
//                RaisePropertyChanged(() => ScreenHeight);
//                RaisePropertyChanged(() => CanvasWidth);
//                RaisePropertyChanged(() => CanvasHeight);
//            }
//        }


//        public ICommand OpenUrlProjectPageCommand { get; } = new RelayCommand(() => OpenUrl(ProjectPage));
//        public ICommand OpenUrlIssuesPageCommand { get; } = new RelayCommand(() => OpenUrl(IssuesPage));
//        public ICommand OpenUrlLatestReleaseCommand { get; } = new RelayCommand(() => OpenUrl(LatestReleasePage));
//        private static void OpenUrl(string url) => Process.Start(url);

//        public ICommand ExitAdrilight { get; } = new RelayCommand(() => App.Current.Shutdown(0));

//        private int _spotsXMaximum = 300;
//        public int SpotsXMaximum
//        {
//            get
//            {
//                return _spotsXMaximum = Math.Max(Settings.SpotsX, _spotsXMaximum);
//            }
//        }

//        private int _spotsYMaximum = 300;
//        private readonly ISpotSet spotSet;
//        private readonly ISerialStream serialStream;

//        public int SpotsYMaximum
//        {
//            get
//            {
//                return _spotsYMaximum = Math.Max(Settings.SpotsY, _spotsYMaximum);
//            }
//        }

//        public int OffsetLedMaximum => Math.Max(Settings.OffsetLed, LedCount);

//        //public int ScreenWidth => (int)System.Windows.SystemParameters.PrimaryScreenWidth;
//        //public int ScreenHeight => (int)System.Windows.SystemParameters.PrimaryScreenHeight;

//        public int ScreenWidth => PreviewImageSource?.PixelWidth ?? 1000;
//        public int ScreenHeight => PreviewImageSource?.PixelHeight ?? 1000;

//        public int CanvasPadding => 3 / DesktopDuplicator.ScalingFactor;

//        public int CanvasWidth => ScreenWidth + 2 * CanvasPadding;
//        public int CanvasHeight => ScreenHeight + 2 * CanvasPadding;


//        public ISpot[] _previewSpots;
//        public ISpot[] PreviewSpots
//        {
//            get => _previewSpots;
//            set {
//                _previewSpots = value;
//                RaisePropertyChanged();
//            }
//        }


//        private bool _Pro11 = true;
//        public bool Pro11 {
//            get => _Pro11;
//            set
//            {
//                _Pro11 = value;
//                RaisePropertyChanged("Pro11");
//            }
//        }

//        private bool _Pro12 = true;
//        public bool Pro12 {
//            get => _Pro12;
//            set
//            {
//                _Pro12 = value;
//                RaisePropertyChanged("Pro12");
//            }
//        }

//        private bool _Pro13 = true;
//        public bool Pro13 {
//            get => _Pro13;
//            set
//            {
//                _Pro13 = value;
//                RaisePropertyChanged("Pro13");
//            }
//        }

//        private bool _Pro14;
//        public bool Pro14 {
//            get => _Pro14;
//            set
//            {
//                _Pro14 = value;
//                RaisePropertyChanged("Pro14");
//            }
//        }

//        private bool _Pro21 = true;
//        public bool Pro21 {
//            get => _Pro21;
//            set
//            {
//                _Pro21 = value;
//                RaisePropertyChanged("Pro21");
//            }
//        }

//        private bool _Pro22 = true;
//        public bool Pro22 {
//            get => _Pro22;
//            set
//            {
//                _Pro22 = value;
//                RaisePropertyChanged("Pro22");
//            }
//        }

//        private bool _Pro31;
//        public bool Pro31 {
//            get => _Pro31;
//            set
//            {
//                _Pro31 = value;
//                RaisePropertyChanged("Pro31");
//            }
//        }

//        private bool _Pro210 = true;
//        public bool Pro210 {
//            get => _Pro210;
//            set
//            {
//                _Pro210 = value;
//                RaisePropertyChanged("Pro210");
//            }
//        }

//        private bool _Pro211;
//        public bool Pro211 {
//            get => _Pro211;
//            set
//            {
//                _Pro211 = value;
//                RaisePropertyChanged("Pro211");
//            }
//        }

//        private bool _Pro220;
//        public bool Pro220 {
//            get => _Pro220;
//            set
//            {
//                _Pro220 = value;
//                RaisePropertyChanged("Pro220");
//            }
//        }

//        private bool _Pro221;
//        public bool Pro221 {
//            get => _Pro221;
//            set
//            {
//                _Pro221 = value;
//                RaisePropertyChanged("Pro221");
//            }
//        }

//        public ObservableCollection<string> lsScreen { get; private set; }

//        public ObservableCollection<string> lsMode { get; private set; }

//        public string aScreen { get; set; }
//        public string aMode { get; set; }

//        //private ObservableCollection<IEffect> _effects;

//        //public ObservableCollection<Effect> Effects {
//        //    get { return _effects; }
//        //    set { _effects = value; }
//        //}
//        private IEffect effect;

//        public IEffect AEffect {
//            get { return effect; }
//            set { effect = value; }
//        }


//        public bool disEffect = false;
//        public bool disScreen = false;
//        public bool disStatic = false;
//        public bool disMusic = false;
//        public bool disMain = false;

//    }
//}



