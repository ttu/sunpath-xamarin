using Microsoft.Devices;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using SunPath.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SunPath.WP
{
    // TODO: Move all frunctinality to ViewModel

    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        private const double Deg2Rad = Math.PI / 180.0;
        private const double Rad2Deg = 180.0 / Math.PI;

        private PhotoCamera _cam;
        private Motion _motion;
        private GeoCoordinateWatcher _watcher;

        private Dictionary<DateTime, PositionData> _source = new Dictionary<DateTime, PositionData>();
        private PointCollection _points = new PointCollection();

        public MainPage()
        {
            this.DataContext = this;

            InitializeComponent();

            var myPolygon = new Polygon();
            myPolygon.Stroke = new SolidColorBrush(Colors.White);
            myPolygon.StrokeThickness = 2;
            myPolygon.HorizontalAlignment = HorizontalAlignment.Left;
            myPolygon.VerticalAlignment = VerticalAlignment.Center;

            myPolygon.Points = _points;

            PaintSurface.Children.Add(myPolygon);

            _deviceFaceVector = new Vector3(0, 0, -10);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private double _viewingAltitude;
        private double _viewingAltitudeDeg;

        public double ViewingAltitudeDeg
        {
            get { return _viewingAltitudeDeg; }
            set
            {
                if (_viewingAltitudeDeg != value)
                {
                    _viewingAltitudeDeg = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _viewingAzimuth;

        private double _viewingAzimuthDeg;

        public double ViewingAzimuthDeg
        {
            get { return _viewingAzimuthDeg; }
            set
            {
                if (_viewingAzimuthDeg != value)
                {
                    _viewingAzimuthDeg = value;
                    OnPropertyChanged();
                }
            }
        }

        private GeoCoordinate _currentLocation;

        private Vector3 _currentDirectionVector;

        private Vector3 _deviceFaceVector;

        private string _debugText;

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

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
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

            if (_cam == null)
            {
                _cam = new Microsoft.Devices.PhotoCamera(CameraType.Primary);
                ViewfinderBrush.SetSource(_cam);
            }

            if (_motion == null)
            {
                _motion = new Motion();
                _motion.TimeBetweenUpdates = TimeSpan.FromMilliseconds(1000);
                _motion.CurrentValueChanged += _motion_CurrentValueChanged;
                _motion.Calibrate += _motion_Calibrate;
            }

            if (_watcher == null)
            {
                _watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                _watcher.PositionChanged += _watcher_PositionChanged;
            }

            try
            {
                _motion.Start();
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
                }
            }
        }

        private void _motion_CurrentValueChanged(object sender, SensorReadingEventArgs<MotionReading> e)
        {
            var attitude = e.SensorReading.Attitude;

            var currentDirectionVector = Vector3.Transform(_deviceFaceVector, attitude.Quaternion);

            double x = currentDirectionVector.X;
            double y = currentDirectionVector.Y;
            double z = currentDirectionVector.Z;

            var azimuth = Math.Atan2(x, y);
            var altitude = Math.Atan2(z, Math.Sqrt(x * x + y * y));

            var azDeg = azimuth * Rad2Deg;
            var altDeg = altitude * Rad2Deg;

            this.Dispatcher.BeginInvoke(() =>
            {
                ViewingAzimuthDeg = azDeg;
                ViewingAltitudeDeg = altDeg;
                UpdatePoints();
            });
        }

        private void _motion_Calibrate(object sender, CalibrationEventArgs e)
        {
            //this.Dispatcher.BeginInvoke(() => { IsCalibrated = false; });
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (_cam != null)
            {
                // Dispose camera to minimize power consumption and to expedite shutdown.
                _cam.Dispose();
            }

            if (_motion != null)
            {
                _motion.Dispose();
            }

            if (_watcher != null)
            {
                _watcher.Dispose();
            }
        }

        // Ensure that the viewfinder is upright in LandscapeRight.
        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (_cam != null)
            {
                int landscapeRightRotation = 180;

                if (e.Orientation == PageOrientation.LandscapeRight)
                {
                    // Rotate for LandscapeRight orientation.
                    ViewfinderBrush.RelativeTransform =
                        new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = landscapeRightRotation };
                }
                else
                {
                    // Rotate for standard landscape orientation.
                    ViewfinderBrush.RelativeTransform =
                        new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = 0 };
                }
            }

            base.OnOrientationChanged(e);
        }

        private void ShutterButton_Click(object sender, RoutedEventArgs e)
        {
            UpdatePoints();
        }

        private void UpdatePoints()
        {
            // TODO: How to calculate what we see?
            double minY = ViewingAltitudeDeg - 20;
            double maxY = ViewingAltitudeDeg + 20;
            double minX = ViewingAzimuthDeg - 30;
            double maxX = ViewingAzimuthDeg + 30;

            var points = SunPath.Core.SunPath.GetPointsInArea(_source, minX, maxX, minY, maxY);

            _points.Clear();

            foreach (var p in points.Values)
            {
                // TODO: How to count corret spot for the point
                var az = p.Azimuth - minX;
                var al = p.Altitude - minY;

                var point = new System.Windows.Point(az, al);
                _points.Add(point);
            }
        }

        private void UpdateSourcePoints()
        {
            var sw = Stopwatch.StartNew();
            Debug.WriteLine("Calc start");
            _source = SunPath.Core.SunPath.GetPath(DateTime.Now, _currentLocation.Longitude, _currentLocation.Latitude, 10);
            Debug.WriteLine("Calc end {0}ms", sw.ElapsedMilliseconds);
        }
    }
}