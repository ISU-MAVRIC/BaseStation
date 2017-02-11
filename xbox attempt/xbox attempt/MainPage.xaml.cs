using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace xbox_attempt
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Gamepad pad;
        SerialDevice comPort;

        public MainPage()
        {
            this.InitializeComponent();
            while (Gamepad.Gamepads.Count <= 0)
            {

            }
            pad = Gamepad.Gamepads[0];

            Task.Run(async () => { while (true) {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        await Update(pad);
                    });
                } });
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var selector = SerialDevice.GetDeviceSelector();
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);
            DeviceInformation info = devices[0];
            comPort = await SerialDevice.FromIdAsync(info.Id);
            comPort.BaudRate = 9600;
            comPort.Parity = SerialParity.None;
            comPort.DataBits = 8;
        }

        public async Task Update(Gamepad pad)
        {
            GamepadReading state = pad.GetCurrentReading();
            outputLSX.Text = state.LeftThumbstickX.ToString();
            outputLSY.Text = state.LeftThumbstickY.ToString();
            outputRSX.Text = state.RightThumbstickX.ToString();
            outputRSY.Text = state.RightThumbstickY.ToString();
            byte[] packet = { (byte)'<', 127, 127, (byte)((state.LeftThumbstickX+1)*128), (byte)((state.RightThumbstickX + 1) * 128), 127, 127, (byte)'>' };
            await comPort.OutputStream.WriteAsync(packet.AsBuffer());
        }

        
    }
}


