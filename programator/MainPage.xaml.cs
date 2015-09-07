using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;
//using System.Management; // need to add System.Management to your project references
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace programator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ProgrammatorDevice _programmator;
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPageLoaded;
        }

        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            ProgrammatorDevice.UsbDeviceFound += async (o, args) =>
            {
                _programmator = args.ProgrammatorDevice;
                await _programmator.SendRandomMessage();
            };
            ProgrammatorDevice.SearchForUsbDevices(Dispatcher);
        }

        private static void Button_Click(object sender, RoutedEventArgs e)
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            ManagementObjectCollection collection;
            var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub");
            collection = searcher.Get();

            foreach (var device in collection)
            {
                devices.Add(new USBDeviceInfo(
                (string)device.GetPropertyValue("DeviceID"),
                (string)device.GetPropertyValue("PNPDeviceID"),
                (string)device.GetPropertyValue("Description")
                ));
            }

            collection.Dispose();


//            await _programmator.SendRandomMessage();
        }
    }
    class USBDeviceInfo
    {
        public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
        {
            this.DeviceID = deviceID;
            this.PnpDeviceID = pnpDeviceID;
            this.Description = description;
        }
        public string DeviceID { get; private set; }
        public string PnpDeviceID { get; private set; }
        public string Description { get; private set; }
    }
}
