using System;
using System.Windows;
using System.Windows.Controls;

namespace programator
{
    class DataDisplayer
    {
        private TextBox _receivedMessageBox;
        public DataDisplayer(TextBox receivedMessageBox)
        {
            _receivedMessageBox = receivedMessageBox;
        }

        public void Show(string message)
        {
            String dateTime = DateTime.Now.ToShortTimeString();
            Action update = () => _receivedMessageBox.AppendText("[" + dateTime + "] " + message + "\n");
            _receivedMessageBox.Dispatcher.Invoke(update);
        }

        public void ShowError(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Error");
        }
    }
}
