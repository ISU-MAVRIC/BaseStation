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

            var serialSelector = SerialDevice.GetDeviceSelector();
            var devices_Task = DeviceInformation.FindAllAsync(serialSelector);
            while (devices_Task.Status != AsyncStatus.Completed) ;
            var devices = devices_Task.GetResults();
            var UARTs = devices.Where(x => x.Name.Contains("UART"));
            var comPort_Task = SerialDevice.FromIdAsync(UARTs.First().Id);
            while (comPort_Task.Status != AsyncStatus.Completed) ;
            comPort = comPort_Task.GetResults();
            comPort.BaudRate = 9600;
            comPort.DataBits = 8;
            comPort.StopBits = SerialStopBitCount.One;

            intervalUpdate = new Timer(UpdateRover, null, 1000, 1);
        }

        enum RoverState
        {
            Up = 1, Down = 2
        }

        byte[] packet = new byte[] { (byte)'<', 127, 127, 127, 127, 127, 127, (byte)'>' };
        private void UpdateRover(object unused_variable_here)
        {
            if (ArmController == null && DriveController == null)
            {
                GetControllers();
            }
            if (ArmController != null)
            {
                GamepadReading reading = ArmController.GetCurrentReading();
                double fArmUpper = ((-reading.RightThumbstickY + 1) / 2) * 500 + 70;
                if (fArmUpper > 255)
                {
                    fArmUpper = 255;
                }

                double fArmLower = ((-reading.RightThumbstickX + 1) / 2) * 650 + 120;
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
                Debug.WriteLine($"{Pan}");
                //Debug.WriteLine($"{reading.LeftThumbstickX}, {reading.LeftThumbstickY}, {reading.RightThumbstickX}, {reading.RightThumbstickY}, {reading.RightTrigger}, {reading.LeftTrigger}");
                packet[3] = ArmLower;
                packet[4] = ArmUpper;
                packet[5] = Pan;
                packet[6] = Pitch;

            }
            else
            {
                // Do nothing to arm
            }
            if (DriveController != null)
            {
                GamepadReading reading = DriveController.GetCurrentReading();
                double speed = reading.LeftThumbstickY;
                double turn = reading.LeftThumbstickX;
                double right = speed + turn;
                double left = speed - turn;
                packet[1] = (byte)(((right + 1) / 2) * 255);
                packet[2] = (byte)(((left + 1) / 2) * 255);
            }
            else
            {
                packet[1] = 127;
                packet[2] = 127;
            }
            //Debug.WriteLine($"{ArmUpper}, {ArmLower}");
            var task = comPort.OutputStream.WriteAsync(packet.AsBuffer());
            var vib = ArmController.Vibration;
            vib.LeftMotor = 0.1;
            ArmController.Vibration = vib;
        }

        private void GetControllers()
        {
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
                for (int i = 0; i < 100; i++)
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
}
