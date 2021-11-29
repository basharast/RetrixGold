using RetriX.Shared.Services;
using RetriX.Shared.ViewModels;
using RetriX.UWP.Services;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RetriX.UWP.Controls
{
    public sealed partial class VirtualPad : UserControl
    {

        public GamePlayerViewModel ViewModel
        {
            get { return (GamePlayerViewModel)GetValue(VMProperty); }
            set { SetValue(VMProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpButtonInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty = DependencyProperty.Register(nameof(ViewModel), typeof(GamePlayerViewModel), typeof(PlayerOverlay), new PropertyMetadata(null));

        public InjectedInputTypes UpButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(UpButtonInputTypeProperty); }
            set { SetValue(UpButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpButtonInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpButtonInputTypeProperty = DependencyProperty.Register(nameof(UpButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadUp));

        public InjectedInputTypes DownButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(DownButtonInputTypeProperty); }
            set { SetValue(DownButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DownButtonInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DownButtonInputTypeProperty = DependencyProperty.Register(nameof(DownButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadDown));

        public InjectedInputTypes LeftButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(LeftButtonInputTypeProperty); }
            set { SetValue(LeftButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftButtonInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftButtonInputTypeProperty = DependencyProperty.Register(nameof(LeftButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadLeft));

        public InjectedInputTypes RightButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(RightButtonInputTypeProperty); }
            set { SetValue(RightButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightButtonInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightButtonInputTypeProperty = DependencyProperty.Register(nameof(RightButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadRight));

        public InjectedInputTypes PointerPressedInputType
        {
            get { return (InjectedInputTypes)GetValue(PointerPressedTypeProperty); }
            set { SetValue(PointerPressedTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpButtonInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointerPressedTypeProperty = DependencyProperty.Register(nameof(PointerPressedInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdPointerPressed));
        
        public InjectedInputTypes MouseRightInputType
        {
            get { return (InjectedInputTypes)GetValue(MouseRightTypeProperty); }
            set { SetValue(MouseRightTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpButtonInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseRightTypeProperty = DependencyProperty.Register(nameof(MouseRightInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdMouseRight));
        
        public InjectedInputTypes MouseLeftInputType
        {
            get { return (InjectedInputTypes)GetValue(MouseLeftTypeProperty); }
            set { SetValue(MouseLeftTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpButtonInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseLeftTypeProperty = DependencyProperty.Register(nameof(MouseLeftInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdMouseLeft));


        TextBox PointerPositionText;
        double defaultXPosition = 0;
        double defaultYPosition = 0;
        public VirtualPad()
        {
            this.InitializeComponent(); 
            try
            {
                defaultXPosition = AnalogControlButtonTransform.TranslateX;
                defaultYPosition = AnalogControlButtonTransform.TranslateY;
                PrepareAccelerometer();
            }catch(Exception ex)
            {

            }
        }

        private Accelerometer _accelerometer;
        private Gyrometer _gryometer;

        // This event handler writes the current accelerometer reading to
        // the three acceleration text blocks on the app' s main page.

        bool AccMeterErrorCatched = false;
        bool ButtonsReseted = false;
        bool ReadingInProgress = false;
        int AccSkipper = 0;
        private async void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            try {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try { 
                    if (!ReadingInProgress && (ViewModel.SensorsMovement || ViewModel.ShowSensorsInfo) && !ViewModel.GameIsPaused && !ViewModel.GameStopInProgress)
                    {

                        if (ViewModel.ShowSensorsInfo)
                        {
                            try
                            {
                                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                 {
                                     try
                                     {
                                         double xAxisAcc = 0;
                                         double yAxisAcc = 0;
                                         double zAxisAcc = 0;
                                         AccelerometerReading reading = e.Reading;

                                         DisplayInformation displayInfo = DisplayInformation.GetForCurrentView();
                                         switch (displayInfo.CurrentOrientation)
                                         {
                                             case DisplayOrientations.Landscape:
                                                 xAxisAcc = reading.AccelerationX * 10;
                                                 yAxisAcc = reading.AccelerationY * 10;
                                                 zAxisAcc = reading.AccelerationZ * 10;
                                                 break;
                                             case DisplayOrientations.Portrait:
                                                 xAxisAcc = reading.AccelerationY * 10;
                                                 yAxisAcc = (-1 * reading.AccelerationX) * 10;
                                                 zAxisAcc = reading.AccelerationZ * 10;
                                                 break;
                                             case DisplayOrientations.LandscapeFlipped:
                                                 xAxisAcc = (-1 * reading.AccelerationX) * 10;
                                                 yAxisAcc = (-1 * reading.AccelerationY) * 10;
                                                 zAxisAcc = reading.AccelerationZ * 10;
                                                 break;
                                             case DisplayOrientations.PortraitFlipped:
                                                 xAxisAcc = (-1 * reading.AccelerationY) * 10;
                                                 yAxisAcc = reading.AccelerationX * 10;
                                                 zAxisAcc = reading.AccelerationZ * 10;
                                                 break;
                                         }
                                         txtXAxis.Text = String.Format("{0,5:0.00}", xAxisAcc);
                                         txtYAxis.Text = String.Format("{0,5:0.00}", yAxisAcc);
                                         txtZAxis.Text = String.Format("{0,5:0.00}", zAxisAcc);
                                     }
                                     catch (Exception ee)
                                     {

                                     }
                                 });
                            }
                            catch (Exception ec)
                            {

                            }
                        }
                        else
                        {

                            if (AccSkipper > 1000)
                            {
                                AccSkipper = 0;
                            }
                            ReadingInProgress = true;
                            double xAxisAcc = 0;
                            double yAxisAcc = 0;
                            double zAxisAcc = 0;
                            AccelerometerReading reading = e.Reading;

                            DisplayInformation displayInfo = DisplayInformation.GetForCurrentView();
                            switch (displayInfo.CurrentOrientation)
                            {
                                case DisplayOrientations.Landscape:
                                    xAxisAcc = reading.AccelerationX * 10;
                                    yAxisAcc = reading.AccelerationY * 10;
                                    zAxisAcc = reading.AccelerationZ * 10;
                                    break;
                                case DisplayOrientations.Portrait:
                                    xAxisAcc = reading.AccelerationY * 10;
                                    yAxisAcc = (-1 * reading.AccelerationX) * 10;
                                    zAxisAcc = reading.AccelerationZ * 10;
                                    break;
                                case DisplayOrientations.LandscapeFlipped:
                                    xAxisAcc = (-1 * reading.AccelerationX) * 10;
                                    yAxisAcc = (-1 * reading.AccelerationY) * 10;
                                    zAxisAcc = reading.AccelerationZ * 10;
                                    break;
                                case DisplayOrientations.PortraitFlipped:
                                    xAxisAcc = (-1 * reading.AccelerationY) * 10;
                                    yAxisAcc = reading.AccelerationX * 10;
                                    zAxisAcc = reading.AccelerationZ * 10;
                                    break;
                            }

                            try
                            {

                                if (yAxisAcc < -0.7 && yAxisAcc > -0.9 && AccSkipper == 0)
                                {
                                    CallRight();
                                }
                                else if (yAxisAcc < -0.9 && yAxisAcc > -1.1 && (AccSkipper == 0 || AccSkipper == 1000))
                                {
                                    CallRight();
                                }
                                else if (yAxisAcc < -1.1 && yAxisAcc > -1.4 && (AccSkipper == 0 || AccSkipper == 500 || AccSkipper == 1000))
                                {
                                    CallRight();
                                }
                                else if (yAxisAcc < -1.4 && yAxisAcc > -2.1 && (AccSkipper == 0 || AccSkipper == 250 || AccSkipper == 500 || AccSkipper == 1000))
                                {
                                    CallRight();
                                }
                                else if (yAxisAcc < -2.1 && yAxisAcc > -3 && (AccSkipper == 0 || AccSkipper == 200 || AccSkipper == 350 || AccSkipper == 650 || AccSkipper == 100))
                                {
                                    CallRight();
                                }
                                else if (yAxisAcc < -3 && yAxisAcc > -3.5 && (AccSkipper == 0 || AccSkipper == 150 || AccSkipper == 300 || AccSkipper == 500 || AccSkipper == 600 || AccSkipper == 750 || AccSkipper == 850 || AccSkipper == 1000))
                                {
                                    CallRight();
                                }
                                else if (yAxisAcc < -3.5 && yAxisAcc > -9 && (AccSkipper != 0 || AccSkipper != 1000))
                                {
                                    CallRight();
                                }
                                else
                                {
                                    _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                     {
                                         RightButton.Background = (SolidColorBrush)Resources["InActive"];
                                     });
                                }


                                if (yAxisAcc > 0.7 && yAxisAcc < 0.8 && AccSkipper == 0)
                                {
                                    CallLeft();
                                }
                                else if (yAxisAcc > 0.8 && yAxisAcc < 1.0 && (AccSkipper == 0 || AccSkipper == 1000))
                                {
                                    CallLeft();
                                }
                                else if (yAxisAcc > 1.0 && yAxisAcc < 1.4 && (AccSkipper == 0 || AccSkipper == 500 || AccSkipper == 1000))
                                {
                                    CallLeft();
                                }
                                else if (yAxisAcc > 1.4 && yAxisAcc < 2.1 && (AccSkipper == 0 || AccSkipper == 250 || AccSkipper == 500 || AccSkipper == 1000))
                                {
                                    CallLeft();
                                }
                                else if (yAxisAcc > 2.1 && yAxisAcc < 3 && (AccSkipper == 0 || AccSkipper == 200 || AccSkipper == 350 || AccSkipper == 650 || AccSkipper == 100))
                                {
                                    CallLeft();
                                }
                                else if (yAxisAcc > 3 && yAxisAcc < 3.5 && (AccSkipper == 0 || AccSkipper == 150 || AccSkipper == 300 || AccSkipper == 500 || AccSkipper == 600 || AccSkipper == 750 || AccSkipper == 850 || AccSkipper == 1000))
                                {
                                    CallLeft();
                                }
                                else if (yAxisAcc > 3.5 && yAxisAcc < 9 && (AccSkipper != 0 || AccSkipper != 1000))
                                {
                                    CallLeft();
                                }
                                else
                                {
                                    _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                     {
                                         LeftButton.Background = (SolidColorBrush)Resources["InActive"];
                                     });
                                }

                                if (xAxisAcc > -2.4)
                                {
                                    ViewModel.InjectInputCommand.Execute(UpButtonInputType);
                                    _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                     {
                                         UpButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                                     });
                                }
                                else
                                {
                                    _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                     {
                                         UpButton.Background = (SolidColorBrush)Resources["InActive"];
                                     });
                                }
                            }
                            catch (Exception ee)
                            {

                            }

                            ButtonsReseted = false;
                        }
                    }
                    else
                    {
                        if (!ButtonsReseted)
                        {
                            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                             {
                                 UpButton.Background = (SolidColorBrush)Resources["InActive"];
                                 RightButton.Background = (SolidColorBrush)Resources["InActive"];
                                 LeftButton.Background = (SolidColorBrush)Resources["InActive"];
                             });
                            ButtonsReseted = true;
                        }
                    }
                    ReadingInProgress = false;
                    AccSkipper += 50;
                    }catch(Exception ex)
                    {

                    }
                });

            }
            catch(Exception ex)
            {
                if (!AccMeterErrorCatched) { 
                PlatformService.ShowErrorMessageDirect(ex);
                    AccMeterErrorCatched = true;
                    ReadingInProgress = false;
                    ButtonsReseted = false;
                    AccSkipper = 0;
                }
            }
        }
        private void CallLeft()
        {
            try { 
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ViewModel.InjectInputCommand.Execute(LeftButtonInputType);
                RightButton.Background = (SolidColorBrush)Resources["InActive"];
                LeftButton.Background = (SolidColorBrush)Resources["ActiveButton"];
            });
            }catch(Exception ee)
            {

            }
        }
        private void CallRight()
        {
            try { 
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ViewModel.InjectInputCommand.Execute(RightButtonInputType);
                RightButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                LeftButton.Background = (SolidColorBrush)Resources["InActive"];
            });
            }catch(Exception ee)
            {

            }
        }
        bool GyrMeterErrorCatched = false;
        double xAxix = 0;
        double yAxix = 0;
        double zAxix = 0;
        private async void ReadingChanged(object sender, GyrometerReadingChangedEventArgs e)
        {
            try {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try { 
            double x_Axis = 0;
            double y_Axis = 0;
            double z_Axis = 0;

            GyrometerReading reading = e.Reading;

            // Calculate the gyrometer axes based on
            // the current display orientation.
            DisplayInformation displayInfo = DisplayInformation.GetForCurrentView();
            switch (displayInfo.CurrentOrientation)
            {
                case DisplayOrientations.Landscape:
                    x_Axis = reading.AngularVelocityX;
                    y_Axis = reading.AngularVelocityY;
                    z_Axis = reading.AngularVelocityZ;
                    break;
                case DisplayOrientations.Portrait:
                    x_Axis = reading.AngularVelocityY;
                    y_Axis = -1 * reading.AngularVelocityX;
                    z_Axis = reading.AngularVelocityZ;
                    break;
                case DisplayOrientations.LandscapeFlipped:
                    x_Axis = -1 * reading.AngularVelocityX;
                    y_Axis = -1 * reading.AngularVelocityY;
                    z_Axis = reading.AngularVelocityZ;
                    break;
                case DisplayOrientations.PortraitFlipped:
                    x_Axis = -1 * reading.AngularVelocityY;
                    y_Axis = reading.AngularVelocityX;
                    z_Axis = reading.AngularVelocityZ;
                    break;
            }
                    xAxix += x_Axis;
                    yAxix += y_Axis;
                    zAxix += z_Axis;
                    if (xAxix > 5)
                    {

                    }else if(xAxix < -5)
                    {

                    }
                        /*txtXAxis.Text = String.Format("{0,5:0.00}", xAxix);
                        txtYAxis.Text = String.Format("{0,5:0.00}", yAxix);
                        txtZAxis.Text = String.Format("{0,5:0.00}", zAxix);*/
                    }catch(Exception ex)
                    {

                    }
                });
            }
            catch(Exception ex)
            {
                if (!GyrMeterErrorCatched)
                {
                    PlatformService.ShowErrorMessageDirect(ex);
                    GyrMeterErrorCatched = true;
                }
            }
        }
        private async void PrepareAccelerometer()
        {
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    _accelerometer = Accelerometer.GetDefault();

                    if (_accelerometer != null)
                    {
                        while (ViewModel == null || ViewModel.CoreName == null)
                        {
                            await Task.Delay(650);
                        }
                        // Establish the report interval
                        uint minReportInterval = _accelerometer.MinimumReportInterval;
                        uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                        _accelerometer.ReportInterval = reportInterval;

                        // Assign an event handler for the reading-changed event
                        UnlinkAccelerometer(null, EventArgs.Empty);
                        _accelerometer.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);

                        ViewModel.SetSensorMovementActive();
                        try { 
                        ViewModel.UnlinkSensorsHandler += UnlinkAccelerometer;
                        }catch(Exception eh)
                        {
                            PlatformService.ShowErrorMessageDirect(eh);
                        }
                    }
                });
            }catch(Exception e)
            {
                PlatformService.ShowErrorMessageDirect(e);
            }

            /*try
            {
                _gryometer = Gyrometer.GetDefault();

                if (_gryometer != null)
                {
                    // Establish the report interval
                    uint minReportInterval = _gryometer.MinimumReportInterval;
                    uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                    _gryometer.ReportInterval = reportInterval;

                    // Assign an event handler for the reading-changed event
                    _gryometer.ReadingChanged += new TypedEventHandler<Gyrometer, GyrometerReadingChangedEventArgs>(ReadingChanged);
                }
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessageDirect(e);
            }*/
        }

        private void UnlinkAccelerometer(object sender, EventArgs eventArgs)
        {
            try { 
            if (_accelerometer != null)
            {
                _accelerometer.ReadingChanged -= ReadingChanged;
            }
            }catch(Exception e)
            {

            }
            try
            {
                ViewModel.UnlinkSensorsHandler -= UnlinkAccelerometer;
            }
            catch (Exception e)
            {
                PlatformService.ShowErrorMessageDirect(e);
            }
        }

        double CurrentXTemp = 0;
        double CurrentYTemp = 0;
        private void AnalogControlSetup(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try { 
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            //PointerPoint CurrentPosition = e.GetCurrentPoint(sender as RepeatButton);
            var PositionX = e.Delta.Translation.X / ViewModel.LeftScaleFactorValue;
            var PositionY = e.Delta.Translation.Y / ViewModel.LeftScaleFactorValue;
             var CurrentX = AnalogControlButtonTransform.TranslateX;
             var CurrentY = AnalogControlButtonTransform.TranslateY;
            CurrentXTemp = CurrentX;
            CurrentYTemp = CurrentY;


            if ((CurrentY > 55 && PositionY > 0) || (CurrentY < -55 && PositionY < 0))
            {
                PositionY = 0;
            }
            if ((CurrentX > 55 && PositionX > 0) || (CurrentX < -55 && PositionX < 0))
            {
                PositionX = 0;
            }

            var AnalogValueX = CurrentX / (55 * ViewModel.LeftScaleFactorValue);
            var AnalogValueY = -CurrentY / (55 * ViewModel.LeftScaleFactorValue);

            InputService.CanvasPointerPosition[0] = AnalogValueX>1?1: AnalogValueX<-1?-1:AnalogValueX;
            AnalogControlButtonTransform.TranslateX += PositionX;
            
            InputService.CanvasPointerPosition[1] = AnalogValueY>1?1: AnalogValueY<-1?-1:AnalogValueY;
            AnalogControlButtonTransform.TranslateY += PositionY;
            }catch(Exception ex)
            {

            }
        }
        private new void ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            try { 
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            AnalogControlButtonTransform.TranslateX = defaultXPosition;
            AnalogControlButtonTransform.TranslateY = defaultYPosition;
            InputService.CanvasPointerPosition[0] = 0;
            InputService.CanvasPointerPosition[1] = 0;
                CurrentYTemp = 0;
                CurrentXTemp = 0;
                SkipperCounterH = 0;
                SkipperCounterV = 0;
            if (ViewModel.UseAnalogDirections)
            {
                LeftButton.Background = (SolidColorBrush)Resources["InActive"];
                RightButton.Background = (SolidColorBrush)Resources["InActive"];
                DownButton.Background = (SolidColorBrush)Resources["InActive"];
                UpButton.Background = (SolidColorBrush)Resources["InActive"];
            }
            if (ViewModel.TabSoundEffect)
            {
                PlatformService.PlayNotificationSoundDirect("analog-01.wav");
            }
            }catch(Exception ex)
            {

            }
        }
    

        private void UpButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            ViewModel.InjectInputCommand.Execute(UpButtonInputType);
        }
        private void UpButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            try { 
            ViewModel.AddActionButton("Up", "Up", UpButtonInputType,ViewModel.ActionsCustomDelay?"+":",");
            }
            catch (Exception ee)
            {

            }
            Button04Clicked(sender, e);
            
        }

        private void DownButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            ViewModel.InjectInputCommand.Execute(DownButtonInputType);
        }
        private void DownButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            try { 
            ViewModel.AddActionButton("Down", "Down", DownButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
            }
            catch (Exception ee)
            {

            }
            Button04Clicked(sender, e);
            
        }

        private void LeftButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            ViewModel.InjectInputCommand.Execute(LeftButtonInputType);
        }
        private void LeftButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            try { 
            ViewModel.AddActionButton("Left", "Left", LeftButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
            }
            catch (Exception ee)
            {

            }
            Button04Clicked(sender, e);
            
        }

        private void RightButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            ViewModel.InjectInputCommand.Execute(RightButtonInputType);
        }
        private void RightButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            try { 
            ViewModel.AddActionButton("Right", "Right", RightButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
            }catch(Exception ee)
            {

            }
            Button04Clicked(sender, e); 
        }

        private void UpRightButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            ViewModel.InjectInputCommand.Execute(UpButtonInputType);
            ViewModel.InjectInputCommand.Execute(RightButtonInputType);
        }
        private void UpRightButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            Button04Clicked(sender, e);
            try { 
            ViewModel.ActionsCustomDelay = true;
            ViewModel.AddActionButton("Up", "Up", UpButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
            ViewModel.AddActionButton("Right", "Right", RightButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
            }
            catch (Exception ee)
            {
                ViewModel.ActionsCustomDelay = false;
            }

        }
        private void UpLeftButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            ViewModel.InjectInputCommand.Execute(UpButtonInputType);
            ViewModel.InjectInputCommand.Execute(LeftButtonInputType);
        }
        private void UpLeftButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            Button04Clicked(sender, e);
            try { 
            ViewModel.ActionsCustomDelay = true;
            ViewModel.AddActionButton("Up", "Up", UpButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
            ViewModel.AddActionButton("Left", "Left", LeftButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
            }
            catch (Exception ee)
            {
                ViewModel.ActionsCustomDelay = false;
            }

        }
        private void DownRightButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            ViewModel.InjectInputCommand.Execute(DownButtonInputType);
            ViewModel.InjectInputCommand.Execute(RightButtonInputType);
        }
        private void DownRightButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            Button04Clicked(sender, e);
            try { 
            ViewModel.ActionsCustomDelay = true;
            ViewModel.AddActionButton("Down", "Down", DownButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
            ViewModel.AddActionButton("Right", "Right", RightButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
            }
            catch (Exception ee)
            {
                ViewModel.ActionsCustomDelay = false;
            }


        }
        private void DownLeftButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            ViewModel.InjectInputCommand.Execute(DownButtonInputType);
            ViewModel.InjectInputCommand.Execute(LeftButtonInputType);
        }
        private void DownLeftButtonTapped(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            Button04Clicked(sender, e);
            try { 
            ViewModel.ActionsCustomDelay = true;
            ViewModel.AddActionButton("Down", "Down", DownButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
            ViewModel.AddActionButton("Left", "Left", LeftButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
            }
            catch (Exception ee)
            {
                ViewModel.ActionsCustomDelay = false;
            }


        }

        private async void ActiveButton(object sender)
        {
            try { 
            var ObjectName = (sender as RepeatButton).Name;
                (sender as RepeatButton).Background = (SolidColorBrush)Resources["ActiveButton"];
                await Task.Delay(10);
                (sender as RepeatButton).Background = (SolidColorBrush)Resources["InActive"];
            }
            catch(Exception ee)
            {

            }
        }
        private void Button04Clicked(object sender, RoutedEventArgs e)
        {
            try { 
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            if (ViewModel.TabSoundEffect)
            {
                PlatformService.PlayNotificationSoundDirect("button-04.mp3");
            }
            ActiveButton(sender);
            }catch(Exception ee)
            {

            }
        }

        
        private void PointerPressedTapped(object sender, RoutedEventArgs e)
        {
            try { 
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            //ViewModel.InjectInputCommand.Execute(PointerPressedInputType);
            ViewModel.InjectInputCommand.Execute(MouseLeftInputType);
            ViewModel.AddActionButton("Tap", "Tap", MouseLeftInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
            Button04Clicked(sender, e);
            ActiveButton(sender);
            }catch(Exception ee)
            {

            }
        }

        int SkipperCounterH = 0;
        int SkipperCounterV = 0;
        private void AnalogControlButton_Click(object sender, RoutedEventArgs e)
        {
            try { 
            if (ViewModel.UseAnalogDirections)
            {
                    if (SkipperCounterH > 15)
                    {
                        SkipperCounterH = 0;
                    }

                    if (SkipperCounterV > 15)
                    {
                        SkipperCounterV = 0;
                    }
                if (CurrentXTemp > 25)
                {
                    ViewModel.InjectInputCommand.Execute(RightButtonInputType);
                    RightButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                    LeftButton.Background = (SolidColorBrush)Resources["InActive"];
                }
                else if (CurrentXTemp < -25)
                {
                    ViewModel.InjectInputCommand.Execute(LeftButtonInputType);
                    LeftButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                    RightButton.Background = (SolidColorBrush)Resources["InActive"];
                }else if (CurrentXTemp > 17 && (SkipperCounterH == 0 || SkipperCounterH == 5))
                    {
                        ViewModel.InjectInputCommand.Execute(RightButtonInputType);
                        RightButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                        LeftButton.Background = (SolidColorBrush)Resources["InActive"];
                    }
                    else if (CurrentXTemp < -17 && (SkipperCounterH == 0 || SkipperCounterH == 5))
                    {
                        ViewModel.InjectInputCommand.Execute(LeftButtonInputType);
                        LeftButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                        RightButton.Background = (SolidColorBrush)Resources["InActive"];
                    }
                    
                else if (CurrentXTemp > 9 && SkipperCounterH == 0 && (CurrentYTemp > -25 && CurrentYTemp < 25))
                {
                    ViewModel.InjectInputCommand.Execute(RightButtonInputType);
                    RightButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                    LeftButton.Background = (SolidColorBrush)Resources["InActive"];
                }
                else if (CurrentXTemp < -9 && SkipperCounterH == 0 && (CurrentYTemp > -25 && CurrentYTemp < 25))
                {
                     ViewModel.InjectInputCommand.Execute(LeftButtonInputType);
                     LeftButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                     RightButton.Background = (SolidColorBrush)Resources["InActive"];
                }
                else
                {
                    LeftButton.Background = (SolidColorBrush)Resources["InActive"];
                    RightButton.Background = (SolidColorBrush)Resources["InActive"];
                }
                
                if (CurrentYTemp < -25)
                {
                    ViewModel.InjectInputCommand.Execute(UpButtonInputType);
                    UpButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                    DownButton.Background = (SolidColorBrush)Resources["InActive"];
                }
                else if (CurrentYTemp > 25)
                {
                    ViewModel.InjectInputCommand.Execute(DownButtonInputType);
                    DownButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                    UpButton.Background = (SolidColorBrush)Resources["InActive"];
                }else if (CurrentYTemp < -20 && (SkipperCounterV == 0 || SkipperCounterV == 5))
                    {
                        ViewModel.InjectInputCommand.Execute(UpButtonInputType);
                        UpButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                        DownButton.Background = (SolidColorBrush)Resources["InActive"];
                    }
                    else if (CurrentYTemp > 20 && (SkipperCounterV == 0 || SkipperCounterV == 5))
                    {
                        ViewModel.InjectInputCommand.Execute(DownButtonInputType);
                        DownButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                        UpButton.Background = (SolidColorBrush)Resources["InActive"];
                    }
                    else
                {
                    DownButton.Background = (SolidColorBrush)Resources["InActive"];
                    UpButton.Background = (SolidColorBrush)Resources["InActive"];
                }
                    SkipperCounterH++;
                    SkipperCounterV++;
                }
            }catch(Exception ee)
            {

            }
        }
    }
}
