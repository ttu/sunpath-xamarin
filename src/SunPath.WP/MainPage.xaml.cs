using GART.BaseControls;
using GART.Data;
using Microsoft.Devices;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;
using SunPath.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SunPath.WP
{
    // TODO: Move all frunctinality to ViewModel

    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        private GeoCoordinateWatcher _watcher;

        private Dictionary<DateTime, PositionData> _source;
        private ObservableCollection<ARItem> _locations;

        private double _screenWidth;
        private double _screenHeight;
        private double _maxDimension;

        private GeoCoordinate _currentLocation;

        private string _debugText;

        public MainPage()
        {
            this.DataContext = this;

            InitializeComponent();

            _locations = new ObservableCollection<ARItem>();
            _source = new Dictionary<DateTime, PositionData>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string DebugText
        {
            get { return _debugText; }
            set
            {
                if (_debugText != value)
                {
                    _debugText = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Start AR services
            ArDisplay.StartServices();

            if (!PhotoCamera.IsCameraTypeSupported(CameraType.Primary))
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    DebugText = "A Camera is not available on this phone.";
                });

                return;
            }

            if (!Motion.IsSupported)
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    DebugText = "The Motion API is not supported on this device.";
                });

                return;
            }

            if (_watcher == null)
            {
                _watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                _watcher.PositionChanged += _watcher_PositionChanged;
            }

            try
            {
                _watcher.Start();
            }
            catch (Exception)
            {
                MessageBox.Show("unable to start the Motion API.");
            }
        }

        private void _watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (!e.Position.Location.IsUnknown)
            {
                if (_currentLocation != e.Position.Location)
                {
                    _currentLocation = e.Position.Location;

                    UpdateSourcePoints();

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        var locations = new ObservableCollection<ARItem>
                        {
                            new ARItem(){GeoLocation = new GeoCoordinate(22.20, 114.11),Content = "Hong Kong"},
                            new ARItem(){GeoLocation = new GeoCoordinate(59.17, 18.03),Content = "Stockholm"},
                            new ARItem(){GeoLocation = new GeoCoordinate(35.40, 139.45),Content = "Tokyo"},
                            new ARItem(){GeoLocation = new GeoCoordinate(47.30, 19.05),Content = "Budapest"},
                            new ARItem(){GeoLocation = new GeoCoordinate(40.42, 74),Content = "A"},
                            new ARItem(){GeoLocation = new GeoCoordinate(55.45, 37.36),Content = "Moscow"},
                            new ARItem(){GeoLocation = new GeoCoordinate(60.1, 25),Content = "Helsinki"}
                        };

                        ArDisplay.ARItems = locations;
                    });
                }
            }
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            // Stop AR services
            ArDisplay.StopServices();

            if (_watcher != null)
            {
                _watcher.Dispose();
            }
        }

        // Ensure that the viewfinder is upright in LandscapeRight.
        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (ArDisplay != null)
            {
                var orientation = ControlOrientation.Default;

                switch (e.Orientation)
                {
                    case PageOrientation.LandscapeLeft:
                        orientation = ControlOrientation.Clockwise270Degrees;
                        ArDisplay.Visibility = Visibility.Visible;
                        break;

                    case PageOrientation.LandscapeRight:
                        orientation = ControlOrientation.Clockwise90Degrees;
                        ArDisplay.Visibility = Visibility.Visible;
                        break;
                }

                ArDisplay.Orientation = orientation;
            }

            base.OnOrientationChanged(e);
        }

        private void UpdateSourcePoints()
        {
            _source = SunPath.Core.SunPath.GetPath(DateTime.Now, _currentLocation.Longitude, _currentLocation.Latitude, 10);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Save the screen dimensions
            _screenWidth = this.ActualWidth;
            _screenHeight = this.ActualHeight;
            _maxDimension = Math.Max(_screenWidth, _screenHeight);
        }
    }
}