using System;
using System.IO.Ports;

namespace programator
{
    class DeviceConnector
    {
        private DataDisplayer _dataDisplayer;
        private SerialPort serialPort;
        private int _baudrate;
        private readonly Parity _parity;
        private int _dataBits;
        private StopBits _stopBits;

        public DeviceConnector(DataDisplayer dataDisplayer)
        {
            _dataDisplayer = dataDisplayer;
            _baudrate = 9600;
            _parity = (Parity)Enum.Parse(typeof(Parity), "None");
            _dataBits = 8;
            _stopBits = (StopBits)Enum.Parse(typeof(StopBits), "One");
        }
        public void Connect(string portName)
        {
            serialPort = new System.IO.Ports.SerialPort(portName, _baudrate, _parity, _dataBits, _stopBits);

            try
            {
                serialPort.Open();
                _dataDisplayer.Show("Connected");
                serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedCallback);
            }
            catch (Exception ex)
            {
                _dataDisplayer.ShowError(ex.ToString());

            }
        }

        private void DataReceivedCallback(object sender, SerialDataReceivedEventArgs e)
        {
            _dataDisplayer.Show("Received: " + serialPort.ReadLine());
        }


        public void Close()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

        public void Send(string text)
        {
            serialPort.Write(text);
            //sign of end of the message
            serialPort.Write(new byte[] { 13, 10 }, 0, 2);
            _dataDisplayer.Show("Sent: " + text + "\n");
        }
    }
}
