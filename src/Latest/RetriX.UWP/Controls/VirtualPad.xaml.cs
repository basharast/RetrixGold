using RetriX.Shared.Services;
using RetriX.Shared.ViewModels;
using RetriX.UWP.Services;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
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

        public static DependencyProperty UpButtonInputTypeProperty
        {
            get
            {
                return VirtualPadActions.UpButtonInputTypeProperty;
            }
            set
            {
                VirtualPadActions.UpButtonInputTypeProperty = value;
            }
        }
        public InjectedInputTypes DownButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(DownButtonInputTypeProperty); }
            set { SetValue(DownButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DownButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty DownButtonInputTypeProperty
        {
            get
            {
                return VirtualPadActions.DownButtonInputTypeProperty;
            }
            set
            {
                VirtualPadActions.DownButtonInputTypeProperty = value;
            }
        }
        public InjectedInputTypes LeftButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(LeftButtonInputTypeProperty); }
            set { SetValue(LeftButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty LeftButtonInputTypeProperty
        {
            get
            {
                return VirtualPadActions.LeftButtonInputTypeProperty;
            }
            set
            {
                VirtualPadActions.LeftButtonInputTypeProperty = value;
            }
        }
        public InjectedInputTypes RightButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(RightButtonInputTypeProperty); }
            set { SetValue(RightButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty RightButtonInputTypeProperty
        {
            get
            {
                return VirtualPadActions.RightButtonInputTypeProperty;
            }
            set
            {
                VirtualPadActions.RightButtonInputTypeProperty = value;
            }
        }
        public InjectedInputTypes PointerPressedInputType
        {
            get { return (InjectedInputTypes)GetValue(PointerPressedTypeProperty); }
            set { SetValue(PointerPressedTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty PointerPressedTypeProperty
        {
            get
            {
                return VirtualPadActions.PointerPressedTypeProperty;
            }
            set
            {
                VirtualPadActions.PointerPressedTypeProperty = value;
            }
        }
        public InjectedInputTypes MouseRightInputType
        {
            get { return (InjectedInputTypes)GetValue(MouseRightTypeProperty); }
            set { SetValue(MouseRightTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty MouseRightTypeProperty
        {
            get
            {
                return VirtualPadActions.MouseRightTypeProperty;
            }
            set
            {
                VirtualPadActions.MouseRightTypeProperty = value;
            }
        }
        public InjectedInputTypes MouseLeftInputType
        {
            get { return (InjectedInputTypes)GetValue(MouseLeftTypeProperty); }
            set { SetValue(MouseLeftTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpButtonInput.  This enables animation, styling, binding, etc...
        public static DependencyProperty MouseLeftTypeProperty
        {
            get
            {
                return VirtualPadActions.MouseLeftTypeProperty;
            }
            set
            {
                VirtualPadActions.MouseLeftTypeProperty = value;
            }
        }

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
            }
            catch (Exception ex)
            {

            }
            PlatformService.UpdateMouseButtonsState = UpdateMouseButtonsState;
            PlatformService.RightClick = RightClick;
            PlatformService.LeftClick = LeftClick;
            PlatformService.PrepareLeftControls = PrepareLeftControls;
            PrepareLeftControls(null, null);
        }

        private void PrepareLeftControls(object sender, EventArgs args)
        {
            try
            {
                lock (InputService.LeftControls)
                {
                    InputService.LeftControls.Clear();
                    InputService.LeftControls.Add(new TouchControl() { InjectedInput = UpButtonInputType, GetState = GetUpState });
                    InputService.LeftControls.Add(new TouchControl() { InjectedInput = DownButtonInputType, GetState = GetDownState });
                    InputService.LeftControls.Add(new TouchControl() { InjectedInput = LeftButtonInputType, GetState = GetLeftState });
                    InputService.LeftControls.Add(new TouchControl() { InjectedInput = RightButtonInputType, GetState = GetRightState });
                    InputService.LeftControls.Add(new TouchControl() { InjectedInput = MouseRightInputType, GetState = GetMouseRightState });
                    InputService.LeftControls.Add(new TouchControl() { InjectedInput = MouseLeftInputType, GetState = GetMouseLeftState });
                    InputService.LeftControls.Add(new TouchControl() { InjectedInput = PointerPressedInputType, GetState = GetPointerState });
                }
            }
            catch (Exception ex)
            {

            }
        }

        bool UpActiveState = false;
        public void UpActive()
        {
            UpActiveState = true;
        }
        public void UpInActive()
        {
            UpActiveState = false;
        }
        public bool GetUpState()
        {
            try
            {
                return UpActiveState;
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        bool DownActiveState = false;
        public void DownActive()
        {
            DownActiveState = true;
        }
        public void DownInActive()
        {
            DownActiveState = false;
        }
        public bool GetDownState()
        {
            try
            {
                return DownActiveState;
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        bool LeftActiveState = false;
        public void LeftActive()
        {
            LeftActiveState = true;
        }
        public void LeftInActive()
        {
            LeftActiveState = false;
        }
        public bool GetLeftState()
        {
            try
            {
                return LeftActiveState;
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        bool RightActiveState = false;
        public void RightActive()
        {
            RightActiveState = true;
        }
        public void RightInActive()
        {
            RightActiveState = false;
        }
        public bool GetRightState()
        {
            try
            {
                return RightActiveState;
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        bool MouseRightActiveState = false;
        public void UpRightActive()
        {
            if (PlatformService.MouseSate)
            {
                MouseRightActiveState = true;
            }
            else
            {
                UpActive();
                RightActive();
            }
        }
        public void UpRightInActive()
        {
            if (PlatformService.MouseSate)
            {
                MouseRightActiveState = false;
            }
            else
            {
                UpInActive();
                RightInActive();
            }
        }
        public void MouseRightActive()
        {
            MouseRightActiveState = true;
        }
        public void MouseRightInActive()
        {
            MouseRightActiveState = false;
        }
        public bool GetMouseRightState()
        {
            try
            {
                return PlatformService.MouseSate && MouseRightActiveState;
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        bool MouseLeftActiveState = false;
        public void UpLeftActive()
        {
            if (PlatformService.MouseSate)
            {
                MouseLeftActiveState = true;
            }
            else
            {
                UpActive();
                LeftActive();
            }
        }
        public void UpLeftInActive()
        {
            if (PlatformService.MouseSate)
            {
                MouseLeftActiveState = false;
            }
            else
            {
                UpInActive();
                LeftInActive();
            }
        }
        public void MouseLeftActive()
        {
            MouseLeftActiveState = true;
        }
        public void MouseLeftInActive()
        {
            MouseLeftActiveState = false;
        }
        public void AMouseLeftActive()
        {
            if (PlatformService.TapEvent)
            {
                MouseLeftActiveState = true;
            }
        }
        public void AMouseLeftInActive()
        {
            if (PlatformService.TapEvent)
            {
                MouseLeftActiveState = false;
            }
        }
        public bool GetMouseLeftState()
        {
            try
            {
                return PointerActiveState || (PlatformService.MouseSate && MouseLeftActiveState);
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        bool PointerActiveState = false;
        public void PointerActive()
        {
            if (PlatformService.TapEvent)
            {
                PointerActiveState = true;
            }
        }
        public void PointerInActive()
        {
            if (PlatformService.TapEvent)
            {
                PointerActiveState = false;
            }
        }
        public bool GetPointerState()
        {
            try
            {
                return PointerActiveState;
            }
            catch (Exception ex)
            {

            }
            return false;
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
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
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
                                             ViewModel.txtXAxis = String.Format("{0,5:0.00}", xAxisAcc);
                                             ViewModel.txtYAxis = String.Format("{0,5:0.00}", yAxisAcc);
                                             ViewModel.txtZAxis = String.Format("{0,5:0.00}", zAxisAcc);
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
                                             RightInActive();
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
                                             LeftInActive();
                                             LeftButton.Background = (SolidColorBrush)Resources["InActive"];
                                         });
                                    }

                                    if (xAxisAcc > -2.4)
                                    {
                                        //ViewModel.InjectInputCommand(UpButtonInputType);
                                        UpActive();
                                        _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                         {
                                             UpButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                                         });
                                    }
                                    else
                                    {
                                        UpInActive();
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
                    }
                    catch (Exception ex)
                    {

                    }
                });

            }
            catch (Exception ex)
            {
                if (!AccMeterErrorCatched)
                {
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
            try
            {
                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //ViewModel.InjectInputCommand(LeftButtonInputType);
                    LeftActive();
                    RightButton.Background = (SolidColorBrush)Resources["InActive"];
                    LeftButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                });
            }
            catch (Exception ee)
            {

            }
        }
        private void CallRight()
        {
            try
            {
                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //ViewModel.InjectInputCommand(RightButtonInputType);
                    RightActive();
                    RightButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                    LeftButton.Background = (SolidColorBrush)Resources["InActive"];
                });
            }
            catch (Exception ee)
            {

            }
        }
        bool GyrMeterErrorCatched = false;
        double xAxix = 0;
        double yAxix = 0;
        double zAxix = 0;
        private async void ReadingChanged(object sender, GyrometerReadingChangedEventArgs e)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
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

                        }
                        else if (xAxix < -5)
                        {

                        }
                        /*txtXAxis.Text = String.Format("{0,5:0.00}", xAxix);
                        txtYAxis.Text = String.Format("{0,5:0.00}", yAxix);
                        txtZAxis.Text = String.Format("{0,5:0.00}", zAxix);*/
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
            catch (Exception ex)
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
                        try
                        {
                            ViewModel.UnlinkSensorsHandler += UnlinkAccelerometer;
                        }
                        catch (Exception eh)
                        {
                            PlatformService.ShowErrorMessageDirect(eh);
                        }
                    }
                });
            }
            catch (Exception e)
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
            try
            {
                if (_accelerometer != null)
                {
                    _accelerometer.ReadingChanged -= ReadingChanged;
                }
            }
            catch (Exception e)
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
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                PlatformService.DPadActive = false;
                var PositionX = e.Delta.Translation.X / ViewModel.LeftScaleFactorValue;
                var PositionY = e.Delta.Translation.Y / ViewModel.LeftScaleFactorValue;
                var CurrentX = AnalogControlButtonTransform.TranslateX;
                var CurrentY = AnalogControlButtonTransform.TranslateY;

                if ((CurrentY > 55 && PositionY > 0) || (CurrentY < -55 && PositionY < 0))
                {
                    PositionY = 0;
                }
                if ((CurrentX > 55 && PositionX > 0) || (CurrentX < -55 && PositionX < 0))
                {
                    PositionX = 0;
                }

                var AnalogValueX = (CurrentX + (PlatformService.DynamicPosition ? CurrentXTemp : 0)) / (55 * ViewModel.LeftScaleFactorValue);
                var AnalogValueY = -(CurrentY + (PlatformService.DynamicPosition ? CurrentYTemp : 0)) / (55 * ViewModel.LeftScaleFactorValue);

                if (!PlatformService.DynamicPosition)
                {
                    CurrentXTemp = CurrentX;
                    CurrentYTemp = CurrentY;
                }
                else
                {
                    CurrentXTemp += CurrentX * 0.001;
                    CurrentYTemp += CurrentY * 0.001;
                    if ((CurrentYTemp > 55) || (CurrentYTemp < -55))
                    {
                        CurrentYTemp = 0;
                    }
                    if ((CurrentXTemp > 55) || (CurrentXTemp < -55))
                    {
                        CurrentXTemp = 0;
                    }
                }

                var X = AnalogValueX > 1 ? 1 : AnalogValueX < -1 ? -1 : AnalogValueX;
                var Y = AnalogValueY > 1 ? 1 : AnalogValueY < -1 ? -1 : AnalogValueY;

                if (PlatformService.MouseSate)
                {
                    ReportAnalogAsMouse(X, Y);
                }
                else
                {
                    InputService.CanvasPointerPosition[0] = X;
                    InputService.CanvasPointerPosition[1] = Y;
                }
                AnalogControlButtonTransform.TranslateX += PositionX;
                AnalogControlButtonTransform.TranslateY += PositionY;
                AnalogControlButtonUpdate();
            }
            catch (Exception ex)
            {

            }
        }
        private void ReportAnalogAsMouse(double xDistance, double yDistance)
        {
            try
            {
                if (PlatformService.MouseSate)
                {
                    InputService.MousePointerPosition[0] = xDistance * 5;
                    InputService.MousePointerPosition[1] = -yDistance * 5;
                }
            }
            catch (Exception ex)
            {
                //PlatformService.ShowErrorMessageDirect(ex);
            }
        }
        private async void Action1Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            if (ViewModel.ActionsToSave)
            {
                ViewModel.SaveStateSlot1.Execute(null);
            }
            else
            {
                await ViewModel.ExcuteActionsAsync(1);
            }
        }

        private async void Action2Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            if (ViewModel.ActionsToSave)
            {
                ViewModel.SaveStateSlot2.Execute(null);
            }
            else
            {
                await ViewModel.ExcuteActionsAsync(2);
            }
        }

        private async void Action3Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            if (ViewModel.ActionsToSave)
            {
                ViewModel.SaveStateSlot3.Execute(null);
            }
            else
            {
                await ViewModel.ExcuteActionsAsync(3);
            }
        }

        private void Action1Button_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            if (ViewModel.ActionsToSave)
            {
                ViewModel.LoadStateSlot1.Execute(null);
            }
        }

        private void Action2Button_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            if (ViewModel.ActionsToSave)
            {
                ViewModel.LoadStateSlot2.Execute(null);
            }
        }

        private void Action3Button_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
            if (ViewModel.ActionsToSave)
            {
                ViewModel.LoadStateSlot3.Execute(null);
            }
        }
        private new void ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                AnalogControlButtonTransform.TranslateX = defaultXPosition;
                AnalogControlButtonTransform.TranslateY = defaultYPosition;
                if (!PlatformService.DynamicPosition)
                {
                    InputService.CanvasPointerPosition[0] = 0;
                    InputService.CanvasPointerPosition[1] = 0;
                    ReportAnalogAsMouse(InputService.CanvasPointerPosition[0], InputService.CanvasPointerPosition[1]);
                    CurrentYTemp = 0;
                    CurrentXTemp = 0;
                    SkipperCounterH = 0;
                    SkipperCounterV = 0;
                }

                if (ViewModel.UseAnalogDirections)
                {
                    LeftButton.Background = (SolidColorBrush)Resources["InActive"];
                    RightButton.Background = (SolidColorBrush)Resources["InActive"];
                    DownButton.Background = (SolidColorBrush)Resources["InActive"];
                    UpButton.Background = (SolidColorBrush)Resources["InActive"];
                }
                if (ViewModel.TabSoundEffect)
                {
                    PlatformService.PlayNotificationSoundDirect("analog-01");
                }
                AnalogControlButtonUpdate();
            }
            catch (Exception ex)
            {

            }
        }


        private void UpButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                //ViewModel.InjectInputCommand(UpButtonInputType);
                PlatformService.DPadActive = false;
            }
            catch (Exception ex)
            {

            }
        }
        private void UpButtonTapped(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                try
                {
                    ViewModel.AddActionButton("Up", "Up", UpButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                }
                catch (Exception ee)
                {

                }
                Button04Clicked(sender, e);
                ReleaseKey(UpButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void DownButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                //ViewModel.InjectInputCommand(DownButtonInputType);
                PlatformService.DPadActive = false;
            }
            catch (Exception ex)
            {

            }
        }
        private void DownButtonTapped(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                try
                {
                    ViewModel.AddActionButton("Down", "Down", DownButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                }
                catch (Exception ee)
                {

                }
                Button04Clicked(sender, e);
                ReleaseKey(DownButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void LeftButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                //ViewModel.InjectInputCommand(LeftButtonInputType);
                PlatformService.DPadActive = false;
            }
            catch (Exception ex)
            {

            }
        }
        private void LeftButtonTapped(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                try
                {
                    ViewModel.AddActionButton("Left", "Left", LeftButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                }
                catch (Exception ee)
                {

                }
                Button04Clicked(sender, e);
                ReleaseKey(LeftButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void RightButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                //ViewModel.InjectInputCommand(RightButtonInputType);
                PlatformService.DPadActive = false;
            }
            catch (Exception ex)
            {

            }
        }
        private void RightButtonTapped(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                try
                {
                    ViewModel.AddActionButton("Right", "Right", RightButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                }
                catch (Exception ee)
                {

                }
                Button04Clicked(sender, e);
                ReleaseKey(RightButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }
        private void BindingsUpdate()
        {
            try
            {
                if (PlatformService.GamePlayPageUpdateBindings != null)
                {
                    PlatformService.GamePlayPageUpdateBindings.Invoke(null, null);
                }
            }
            catch (Exception e)
            {

            }
        }
        int Delay = 5;
        int Interval = 5;
        public async void UpdateMouseButtonsState(object sender, EventArgs args)
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(CoreDispatcherPriority.High, async () =>
                {
                    try
                    {
                        if (PlatformService.MouseSate)
                        {
                            var brushLeft = new SolidColorBrush(Colors.Orange);
                            //brushLeft.Opacity = ViewModel.ButtonsSubOpacity;
                            UpLeftButton.Background = brushLeft;

                            var brushRight = new SolidColorBrush(Colors.Green);
                            //brushRight.Opacity = ViewModel.ButtonsSubOpacity;
                            UpRightButton.Background = brushRight;

                            //Mouse left click should have some delay
                            Delay = 500;
                        }
                        else
                        {
                            UpRightButton.Background = (SolidColorBrush)Resources["InActive"];
                            UpLeftButton.Background = (SolidColorBrush)Resources["InActive"];
                            Delay = 5;
                        }
                        BindingsUpdate();
                    }
                    catch (Exception e)
                    {

                    }
                });
            }
            catch (Exception ex)
            {

            }
        }


        private async void RightClick(object sender, EventArgs e)
        {
            try
            {
                if (ViewModel != null)
                {
                    ViewModel.InjectInputCommand(MouseRightInputType, true);
                    await Task.Delay(50);
                    ViewModel.InjectInputCommand(MouseRightInputType, false);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void LeftClick(object sender, EventArgs e)
        {
            try
            {
                if (ViewModel != null)
                {
                    ViewModel.InjectInputCommand(MouseLeftInputType, true);
                    await Task.Delay(50);
                    ViewModel.InjectInputCommand(MouseLeftInputType, false);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void UpRightButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                if (PlatformService.MouseSate)
                {
                    //ViewModel.InjectInputCommand(MouseRightInputType);
                }
                else
                {
                    //ViewModel.InjectInputCommand(UpButtonInputType);
                    //ViewModel.InjectInputCommand(RightButtonInputType);
                }
                PlatformService.DPadActive = false;
            }
            catch (Exception ex)
            {

            }
        }
        private async void UpRightButtonTapped(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                if (PlatformService.MouseSate)
                {
                    UpRightButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                    await Task.Delay(10);
                    var brushRight = new SolidColorBrush(Colors.Green);
                    //brushRight.Opacity = ViewModel.ButtonsSubOpacity;
                    UpRightButton.Background = brushRight;
                }
                else
                {
                    Button04Clicked(sender, e);

                    try
                    {
                        ViewModel.ActionsCustomDelay = true;
                        ViewModel.AddActionButton("Up", "Up", UpButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                        ViewModel.AddActionButton("Right", "Right", RightButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                    }
                    catch (Exception ee)
                    {
                        ViewModel.ActionsCustomDelay = false;
                    }
                    ReleaseKey(UpButtonInputType);
                    ReleaseKey(RightButtonInputType);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void UpLeftButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                if (PlatformService.MouseSate)
                {
                    //ViewModel.InjectInputCommand(MouseLeftInputType);
                }
                else
                {
                    //ViewModel.InjectInputCommand(UpButtonInputType);
                    //ViewModel.InjectInputCommand(LeftButtonInputType);
                }
                PlatformService.DPadActive = false;
            }
            catch (Exception ex)
            {

            }
        }
        private async void UpLeftButtonTapped(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                if (PlatformService.MouseSate)
                {
                    UpLeftButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                    await Task.Delay(10);
                    var brushLeft = new SolidColorBrush(Colors.Orange);
                    //brushLeft.Opacity = ViewModel.ButtonsSubOpacity;
                    UpLeftButton.Background = brushLeft;
                }
                else
                {
                    Button04Clicked(sender, e);
                    try
                    {
                        ViewModel.ActionsCustomDelay = true;
                        ViewModel.AddActionButton("Up", "Up", UpButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                        ViewModel.AddActionButton("Left", "Left", LeftButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                    }
                    catch (Exception ee)
                    {
                        ViewModel.ActionsCustomDelay = false;
                    }
                    ReleaseKey(UpButtonInputType);
                    ReleaseKey(LeftButtonInputType);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void DownRightButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                //ViewModel.InjectInputCommand(DownButtonInputType);
                //ViewModel.InjectInputCommand(RightButtonInputType);
                PlatformService.DPadActive = false;
            }
            catch (Exception ex)
            {

            }
        }
        private void DownRightButtonTapped(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                Button04Clicked(sender, e);
                try
                {
                    ViewModel.ActionsCustomDelay = true;
                    ViewModel.AddActionButton("Down", "Down", DownButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                    ViewModel.AddActionButton("Right", "Right", RightButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                }
                catch (Exception ee)
                {
                    ViewModel.ActionsCustomDelay = false;
                }
                ReleaseKey(DownButtonInputType);
                ReleaseKey(RightButtonInputType);
            }
            catch (Exception ex)
            {

            }

        }
        private void DownLeftButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                //ViewModel.InjectInputCommand(DownButtonInputType);
                //ViewModel.InjectInputCommand(LeftButtonInputType);
                PlatformService.DPadActive = false;
            }
            catch (Exception ex)
            {

            }
        }
        private void DownLeftButtonTapped(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                Button04Clicked(sender, e);
                try
                {
                    ViewModel.ActionsCustomDelay = true;
                    ViewModel.AddActionButton("Down", "Down", DownButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                    ViewModel.AddActionButton("Left", "Left", LeftButtonInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                }
                catch (Exception ee)
                {
                    ViewModel.ActionsCustomDelay = false;
                }
                ReleaseKey(DownButtonInputType);
                ReleaseKey(LeftButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private async void ActiveButton(object sender)
        {
            try
            {
                /*var ObjectName = (sender as RepeatButton).Name;
                (sender as RepeatButton).Background = (SolidColorBrush)Resources["ActiveButton"];
                await Task.Delay(10);
                (sender as RepeatButton).Background = (SolidColorBrush)Resources["InActive"];*/
            }
            catch (Exception ee)
            {

            }
        }
        private void Button04Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                if (ViewModel.TabSoundEffect)
                {
                    PlatformService.PlayNotificationSoundDirect("button-04");
                }
                ActiveButton(sender);
            }
            catch (Exception ee)
            {

            }
        }

        private void PointerPressedTapped(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.GameStopStarted || ViewModel.ScaleFactorVisible) return;
                if (PlatformService.ThumbstickSate)
                {
                    //ViewModel.InjectInputCommand(MouseLeftInputType);
                    //ViewModel.InjectInputCommand(PointerPressedInputType);
                }
                else
                {
                    //ViewModel.InjectInputCommand(MouseLeftInputType);
                    //ViewModel.InjectInputCommand(PointerPressedInputType);
                }
                PlatformService.DPadActive = false;
                ViewModel.AddActionButton("Tap", "Tap", MouseLeftInputType, ViewModel.ActionsCustomDelay ? "+" : ",");
                Button04Clicked(sender, e);
                ActiveButton(sender);
            }
            catch (Exception ee)
            {

            }
        }

        int SkipperCounterH = 0;
        int SkipperCounterV = 0;
        private void AnalogControlButtonUpdate()
        {
            try
            {
                if (ViewModel.UseAnalogDirections)
                {
                    if ((CurrentXTemp != 0 || CurrentYTemp != 0))
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
                            //ViewModel.InjectInputCommand(RightButtonInputType);
                            RightActive();
                            LeftInActive();
                            RightButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                            LeftButton.Background = (SolidColorBrush)Resources["InActive"];
                        }
                        else if (CurrentXTemp < -25)
                        {
                            LeftActive();
                            RightInActive();
                            //ViewModel.InjectInputCommand(LeftButtonInputType);
                            LeftButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                            RightButton.Background = (SolidColorBrush)Resources["InActive"];
                        }
                        else if (CurrentXTemp > 17 && (SkipperCounterH == 0 || SkipperCounterH == 5))
                        {
                            RightActive();
                            LeftInActive();
                            //ViewModel.InjectInputCommand(RightButtonInputType);
                            RightButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                            LeftButton.Background = (SolidColorBrush)Resources["InActive"];
                        }
                        else if (CurrentXTemp < -17 && (SkipperCounterH == 0 || SkipperCounterH == 5))
                        {
                            LeftActive();
                            RightInActive();
                            //ViewModel.InjectInputCommand(LeftButtonInputType);
                            LeftButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                            RightButton.Background = (SolidColorBrush)Resources["InActive"];
                        }

                        else if (CurrentXTemp > 9 && SkipperCounterH == 0 && (CurrentYTemp > -25 && CurrentYTemp < 25))
                        {
                            RightActive();
                            LeftInActive();
                            //ViewModel.InjectInputCommand(RightButtonInputType);
                            RightButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                            LeftButton.Background = (SolidColorBrush)Resources["InActive"];
                        }
                        else if (CurrentXTemp < -9 && SkipperCounterH == 0 && (CurrentYTemp > -25 && CurrentYTemp < 25))
                        {
                            LeftActive();
                            RightInActive();
                            //ViewModel.InjectInputCommand(LeftButtonInputType);
                            LeftButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                            RightButton.Background = (SolidColorBrush)Resources["InActive"];
                        }
                        else
                        {
                            LeftInActive();
                            RightInActive();
                            LeftButton.Background = (SolidColorBrush)Resources["InActive"];
                            RightButton.Background = (SolidColorBrush)Resources["InActive"];
                        }

                        if (CurrentYTemp < -25)
                        {
                            UpActive();
                            DownInActive();
                            //ViewModel.InjectInputCommand(UpButtonInputType);
                            UpButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                            DownButton.Background = (SolidColorBrush)Resources["InActive"];
                        }
                        else if (CurrentYTemp > 25)
                        {
                            UpInActive();
                            DownActive();
                            //ViewModel.InjectInputCommand(DownButtonInputType);
                            DownButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                            UpButton.Background = (SolidColorBrush)Resources["InActive"];
                        }
                        else if (CurrentYTemp < -20 && (SkipperCounterV == 0 || SkipperCounterV == 5))
                        {
                            UpActive();
                            DownInActive();
                            //ViewModel.InjectInputCommand(UpButtonInputType);
                            UpButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                            DownButton.Background = (SolidColorBrush)Resources["InActive"];
                        }
                        else if (CurrentYTemp > 20 && (SkipperCounterV == 0 || SkipperCounterV == 5))
                        {
                            UpInActive();
                            DownActive();
                            //ViewModel.InjectInputCommand(DownButtonInputType);
                            DownButton.Background = (SolidColorBrush)Resources["ActiveButton"];
                            UpButton.Background = (SolidColorBrush)Resources["InActive"];
                        }
                        else
                        {
                            UpInActive();
                            DownInActive();
                            DownButton.Background = (SolidColorBrush)Resources["InActive"];
                            UpButton.Background = (SolidColorBrush)Resources["InActive"];
                        }
                        SkipperCounterH++;
                        SkipperCounterV++;
                    }
                    else
                    {
                        LeftInActive();
                        RightInActive();
                        UpInActive();
                        DownInActive();
                    }
                }
            }
            catch (Exception ee)
            {

            }
        }

        private void UpButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(UpButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void LeftButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(LeftButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void RightButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(RightButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void DownButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(DownButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void DownLeftButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(DownButtonInputType);
                ReleaseKey(LeftButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void UpLeftButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(UpButtonInputType);
                ReleaseKey(LeftButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void DownRightButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(DownButtonInputType);
                ReleaseKey(RightButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void UpRightButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(UpButtonInputType);
                ReleaseKey(RightButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void UpButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(UpButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void LeftButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(LeftButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void RightButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(RightButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void DownButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(DownButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void DownLeftButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(DownButtonInputType);
                ReleaseKey(LeftButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void UpLeftButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(UpButtonInputType);
                ReleaseKey(LeftButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void DownRightButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(DownButtonInputType);
                ReleaseKey(RightButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }

        private void UpRightButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                ReleaseKey(UpButtonInputType);
                ReleaseKey(RightButtonInputType);
            }
            catch (Exception ex)
            {

            }
        }
        private void ReleaseKey(InjectedInputTypes input)
        {
            try
            {
                //After 3.0.22 the way to detect buttons changed
                /*
                lock (InputService.InjectedInputPressed)
                {
                    InputService.InjectedInputPressed.Remove((uint)input);
                }
                */
            }
            catch (Exception ex)
            {

            }
        }

        private void AnalogControlButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
                //ViewModel.InjectInputCommand(MouseLeftInputType);
                //ViewModel.InjectInputCommand(PointerPressedInputType);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
