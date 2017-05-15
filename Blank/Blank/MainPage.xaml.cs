using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Gaming.Input;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Popups;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Blank
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Timer intervalUpdate;
        SerialDevice comPort;
        Gamepad ArmController;
        Gamepad DriveController;

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (comPort != null)
            {
                comPort.Dispose();
                comPort = null;
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            IEnumerable<DeviceInformation> UARTs;
            do
            {
                do
                {
                    var serialSelector = SerialDevice.GetDeviceSelector();
                    var devices_Task = DeviceInformation.FindAllAsync(serialSelector);
                    while (devices_Task.Status != AsyncStatus.Completed) ;
                    var devices = devices_Task.GetResults();
                    UARTs = devices.Where(x => x.Name.Contains("UART"));
                } while (UARTs.Count() <= 0);

                var comPort_Task = SerialDevice.FromIdAsync(UARTs.First().Id);
                while (comPort_Task.Status != AsyncStatus.Completed) ;
                comPort = comPort_Task.GetResults();
            } while (comPort == null);
            comPort.BaudRate = 9600;
            comPort.DataBits = 8;
            comPort.StopBits = SerialStopBitCount.One;
            comPort.Parity = SerialParity.None;

            CameraStepTime = new Stopwatch();
            Task.Run(()=> {
                while (true)
                {
                    var delay = Task.Delay(5);
                    while (!delay.IsCompleted) ;
                    UpdateRover();
                }
            });
            //intervalUpdate = new Timer(UpdateRover, null, 1000, 6);
        }

        enum RoverState
        {
            Up = 1, Down = 2
        }

        private void DoArm()
        {
            if (ArmController != null)
            {
                GamepadReading reading = ArmController.GetCurrentReading();
                if (reading.LeftThumbstickX == 0 &&
                    reading.LeftThumbstickY == 0 &&
                    reading.RightThumbstickX == 0 &&
                    reading.RightThumbstickY == 0)
                {
                    return;
                }
                double fArmUpper = ((-reading.RightThumbstickY + 1) / 2) * 500 + 70;
                if (fArmUpper > 255)
                {
                    fArmUpper = 255;
                }

                double fArmLower = ((-reading.RightThumbstickX + 1) / 2) * 650-70;
                if (fArmLower > 255)
                {
                    fArmLower = 255;
                }

                double fPan = ((reading.LeftThumbstickY + 1) / 2) * 400;
                if (fPan > 255)
                {
                    fPan = 255;
                }
                double fPitch = ((reading.LeftThumbstickX + 1) / 2) * 400 + 15;
                if (fPitch > 255)
                {
                    fPitch = 255;
                }
                byte ArmUpper = (byte)fArmUpper;
                byte ArmLower = (byte)fArmLower;
                byte Pan = (byte)fPan;
                byte Pitch = (byte)fPitch;
                //Debug.WriteLine($"{actuator}");
                //Debug.WriteLine($"{Pan}");
                //Debug.WriteLine($"{reading.LeftThumbstickX}, {reading.LeftThumbstickY}, {reading.RightThumbstickX}, {reading.RightThumbstickY}, {reading.RightTrigger}, {reading.LeftTrigger}");
                packet[3] = ArmLower;
                packet[4] = ArmUpper;
                packet[5] = Pan;
                packet[6] = Pitch;
                //Debug.WriteLine($"{ArmLower} {ArmUpper}");
                //Debug.WriteLine($"{Pitch}");
            }
            else
            {
                // Do nothing to arm
            }
        }

        GamepadReading prevDriveReading;
        private void DoDrive()
        {
            if (DriveController != null)
            {
                GamepadReading reading = DriveController.GetCurrentReading();
                //if (reading.LeftThumbstickX == 0 &&
                //    reading.LeftThumbstickY == 0 &&
                //    reading.RightThumbstickX == 0 &&
                //    reading.RightThumbstickY == 0)
                //{
                //    return;
                //}
                double speed = reading.LeftThumbstickY;
                double turn = reading.LeftThumbstickX;
                double right = reading.RightThumbstickY;
                double left = reading.LeftThumbstickY;
                if (left > 255)
                {
                    left = 255;
                }
                if (right > 255)
                {
                    left = 255;
                }
                if (Math.Abs(left) < 0.10)
                {
                    left = 0;
                }
                if (Math.Abs(right) < 0.10)
                {
                    right = 0;
                }
                packet[1] = (byte)(((right + 1) / 2) * 255);
                packet[2] = (byte)(((left + 1) / 2) * 255);
                //Debug.WriteLine($"{packet[1]}, {packet[2]}");

                if (CameraStepTime.ElapsedMilliseconds >= 10)
                {
                    if (reading.Buttons.HasFlag(GamepadButtons.DPadLeft))
                    {
                        packet[7]++;
                        CameraStepTime.Restart();
                    }
                    if (reading.Buttons.HasFlag(GamepadButtons.DPadRight))
                    {
                        packet[7]--;
                        CameraStepTime.Restart();
                    }
                    if (reading.Buttons.HasFlag(GamepadButtons.DPadDown))
                    {
                        packet[8]--;
                        CameraStepTime.Restart();
                    }
                    if (reading.Buttons.HasFlag(GamepadButtons.DPadUp))
                    {
                        packet[8]++;
                        CameraStepTime.Restart();
                    }
                }
                if (!CameraStepTime.IsRunning)
                {
                    CameraStepTime.Start();
                }

                if (reading.Buttons.HasFlag(GamepadButtons.A) && !prevDriveReading.Buttons.HasFlag(GamepadButtons.A))
                { // A pressed
                    if (packet[10] <= 64)
                    {
                        packet[10] = 255;
                    }
                    else
                    {
                        packet[10] = 64;
                    }
                }

                //Debug.WriteLine($"{packet[7]}, {packet[8]}");

                prevDriveReading = reading;
            }
            else
            {
                packet[1] = 127;
                packet[2] = 127;
            }
        }

        Stopwatch CameraStepTime;
        byte[] packet = new byte[] { (byte)'<', 127, 127, 255, 255, 127, 127, 127, 127, 127, 127, 127, 127, 127, (byte)'>' };
        enum PacketIndex : int
        {
            Right = 1,
            Left = 2,
            ArmLower = 3,
            ArmUpper = 4,
            ClawPan = 5,
            ClawPitch = 6,

        }
        private void UpdateRover()
        {
            if (ArmController == null && DriveController == null)
            {
                GetControllers();
            }
            DoArm();
            DoDrive();
            //foreach (byte b in packet)
            //{
            //    var task = comPort.OutputStream.WriteAsync(new byte[] { b }.AsBuffer());
            //    while (task.Status != AsyncStatus.Completed) ;
            //}
            var task = comPort.OutputStream.WriteAsync(packet.AsBuffer());
            while (task.Status != AsyncStatus.Completed) ;
        }

        private void GetControllers()
        {
            lock (this)
            {
                if (DriveController != null || ArmController != null)
                {
                    return;
                }
                while (Gamepad.Gamepads.Count < 1)
                    ;
                Task.Delay(100).Wait();
                Gamepad g1 = Gamepad.Gamepads[0];
                if (Gamepad.Gamepads.Count > 1)
                {
                    Gamepad g2 = Gamepad.Gamepads[1];

                    while (true)
                    {
                        GamepadReading r1 = g1.GetCurrentReading();
                        GamepadReading r2 = g2.GetCurrentReading();
                        Debug.WriteLine($"{r1.Buttons}, {r2.Buttons}");
                        if (r1.Buttons != 0)
                        {
                            ArmController = g2;
                            DriveController = g1;
                            break;
                        }
                        else if (r2.Buttons != 0)
                        {
                            ArmController = g1;
                            DriveController = g2;
                            break;
                        }
                    }
                }
                else
                {
                    ArmController = g1;
                    for (int i = 0; i < 200; i++)
                    {
                        if (g1.GetCurrentReading().Buttons != 0)
                        {
                            ArmController = null;
                            DriveController = g1;
                            break;
                        }
                        Task.Delay(10).Wait();
                    }
                }
            }
        }

        private void PowerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(0));
            }
            catch (Exception ex)
            {
                new MessageDialog("Are you running on an IoT core device? Shutdown only works there.\n" + ex.Message, "Could not shhutdown").ShowAsync().AsTask().Wait();
            }
        }

        private void DifferentialSlider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Slider source = sender as Slider;
            if (source != null)
            {
                source.Value = 127;
            }
        }

        private void ArmUpperSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            packet[4] = (byte)ArmUpperSlider.Value;
        }

        private void ArmLowerSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            packet[3] = (byte)ArmLowerSlider.Value;
        }

        private void SSArmSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            packet[11] = (byte)SSArmSlider.Value;
        }

        private void SSDepthSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            packet[12] = (byte)SSDepthSlider.Value;
        }

        private void SSDrillSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            packet[13] = (byte)SSDrillSlider.Value;
        }
    }
}
