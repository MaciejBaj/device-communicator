using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;

namespace programator
{
    class ProgrammatorDevice
    {
        private HidDevice _hidDevice;
        private ProgrammatorDevice(HidDevice hidDevice)
        {
            _hidDevice = hidDevice;
        }

         private async Task SendOutputMessage(byte[] message)
        {
            if (_hidDevice != null)
            {
                HidOutputReport report = _hidDevice.CreateOutputReport();
                report.Data = message.AsBuffer();

                await _hidDevice.SendOutputReportAsync(report);
            }
        }
         private static readonly byte[] Cmd = { 0, 0, 0, 0, 0, 0, 0, 0, 2 };
         public async Task SendRandomMessage()
         {
             await SendOutputMessage(Cmd);
         }


         private const ushort vid = 8483;
         private const ushort pid = 4112;
         private const ushort uid = 16;
         private const ushort uPage = 1;

        public static EventHandler<ProgrammatorDeviceEventArgs> UsbDeviceFound;

        public static void SearchForUsbDevices(CoreDispatcher dispatcher)
        {
            DeviceWatcher deviceWatcher =
                DeviceInformation.CreateWatcher(HidDevice.GetDeviceSelector(uPage, uid));
            deviceWatcher.Added += (s, a) => dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                HidDevice hidDevice = await HidDevice.FromIdAsync(a.Id, FileAccessMode.ReadWrite);
                var launcher = new ProgrammatorDevice(hidDevice);
                if (UsbDeviceFound != null)
                {
                    UsbDeviceFound(null, new ProgrammatorDeviceEventArgs(launcher));
                }
                  
                deviceWatcher.Stop();
            });
            deviceWatcher.Start();
        }

        public void Dispose()
        {
            _hidDevice.Dispose();
        }

        ~ProgrammatorDevice()
        {
            Dispose();
        }


    }

    internal class ProgrammatorDeviceEventArgs
    {
        public ProgrammatorDeviceEventArgs(ProgrammatorDevice programmator)
        {
            ProgrammatorDevice = programmator;
        }

        public ProgrammatorDevice ProgrammatorDevice { get; set; }
    }
}
