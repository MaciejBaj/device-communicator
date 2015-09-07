using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Forms;

namespace programator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataDisplayer dataDisplayer;
        private DeviceConnector deviceConnector;
        private AvrdudeProgrammer programmer;
         
        public MainWindow()
        {
            InitializeComponent();
            dataDisplayer = new DataDisplayer(ReceivedMessageBox);
            deviceConnector = new DeviceConnector(dataDisplayer);
            programmer = new AvrdudeProgrammer(new DataDisplayer(MonitorTextBox));

            foreach (String s in SerialPort.GetPortNames())
            {
                SerialPortsComboBox.Items.Add(s);
            }
            ConnectButton.IsEnabled = SerialPortsComboBox.HasItems;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            deviceConnector.Connect(SerialPortsComboBox.Text);
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            deviceConnector.Close();
          
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            deviceConnector.Send(SendMessageTextBox.Text);
        }

        private void OpenFileDialogButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
                    FileNameTextBox.Text = file;
                    FileNameTextBox.ToolTip = file;
                    break;
                default:
                    FileNameTextBox.Text = null;
                    FileNameTextBox.ToolTip = null;
                    break;
            }
        }

        private void SendFileToProgrammatorButton_Click(object sender, RoutedEventArgs e)
        {
            programmer.SendFile(FileNameTextBox.Text);
        }

        private void GetInfoProgrammatorButton_Click(object sender, RoutedEventArgs e)
        {
            programmer.GetInfo();
        }
    }

   
}
