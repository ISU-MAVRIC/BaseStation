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
        Gamepad controller;

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

            while (Gamepad.Gamepads.Count <= 0)
                ;

            controller = Gamepad.Gamepads[0];

            intervalUpdate = new Timer(UpdateRover, null, 1000, 1);
        }

        enum RoverState
        {
            Up = 1, Down = 2
        }

        private RoverState state = 0;
        byte[] packet = new byte[] { (byte)'<', 127, 127, 127, 127, 127, 127, (byte)'>' };
        Stopwatch time = new Stopwatch();
        private void UpdateRover(object unused_variable_here)
        {
            GamepadReading reading = controller.GetCurrentReading();
            double fArmUpper = ((-reading.RightThumbstickY + 1) / 2)*500 + 70;
            if (fArmUpper > 255)
            {
                fArmUpper = 255;
            }

            double fArmLower = ((-reading.RightThumbstickX + 1) / 2) * 650 + 120;
            if (fArmLower > 255)
            {
                fArmLower = 255;
            }

            double fPan = ((-reading.LeftThumbstickY + 1) / 2) * 255;
            if (fPan > 255)
            {
                fPan = 255;
            }
            double fPitch = ((-reading.LeftThumbstickX + 1) / 2) * 255;
            if (fPitch > 255)
            {
                fPitch = 255;
            }
            byte ArmUpper = (byte)fArmUpper;
            byte ArmLower = (byte)fArmLower;
            byte Pan = (byte)fPan;
            byte Pitch = (byte)fPitch;
            //Debug.WriteLine($"{actuator}");
            Debug.WriteLine($"{reading.LeftThumbstickX}, {reading.LeftThumbstickY}, {reading.RightThumbstickX}, {reading.RightThumbstickY}, {reading.RightTrigger}, {reading.LeftTrigger}");
            packet[3] = ArmLower;
            packet[4] = ArmUpper;
            packet[5] = Pan;
            packet[6] = Pitch;
            //Debug.WriteLine($"{ArmUpper}, {ArmLower}");
            var task = comPort.OutputStream.WriteAsync(packet.AsBuffer());
            var vib = controller.Vibration;
            vib.LeftMotor = 0.1;
            controller.Vibration = vib;
        }
    }
}
