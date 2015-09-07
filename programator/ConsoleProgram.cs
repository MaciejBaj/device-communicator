using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace programator
{
    class ConsoleProgram
    {
        private Process process;
        private bool processOutputStreamOpen;
        private bool processErrorStreamOpen;
        public event EventHandler OnProcessEnd;
        private Action<object> onFinish;
        private object param;
        private DataDisplayer dataDisplayer;
        private ProcessStartInfo processStartInfo;

        public ConsoleProgram(DataDisplayer dataDisplayerArgument, string applicationName = "avrdude", string commandLineArguments = "")
        {
            dataDisplayer = dataDisplayerArgument;
            string binary = getBinaryPath(applicationName);
            processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = binary;
            processStartInfo.Arguments = commandLineArguments;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
        }

        public void launch(string commandLineArguments = "")
        {
            processStartInfo.Arguments = commandLineArguments;
            process = new Process {EnableRaisingEvents = true, StartInfo = processStartInfo};
            process.OutputDataReceived += outputLogHandler;
            process.ErrorDataReceived += errorLogHandler;
            process.Exited += processExited;
            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                dataDisplayer.ShowError("Error starting process");
            }
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        private string getBinaryPath(string applicationName)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                applicationName += ".exe";

                string[] paths = Environment.GetEnvironmentVariable("PATH").Split(new char[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string app in paths.Select(path => Path.Combine(path, applicationName)).Where(File.Exists))
                {
                    return app;
                }
                throw new Exception("console application " + applicationName + " is not installed correctly");
            }
            throw new Exception("your operating system is not supported");
        }

        private bool logger(string s)
        {
            if (s != null) // A null is sent when the stream is closed
            {
                dataDisplayer.Show(s.Replace("\0", String.Empty) + Environment.NewLine);
                return true;
            }

            return false;
        }

        private void outputLogHandler(object sender, DataReceivedEventArgs e)
        {
            processOutputStreamOpen = logger(e.Data);
        }

        private void errorLogHandler(object sender, DataReceivedEventArgs e)
        {
            processErrorStreamOpen = logger(e.Data);
        }

        private void processExited(object sender, EventArgs e)
        {
            dataDisplayer.ShowError("attempt to exit");
            if (OnProcessEnd != null)
                OnProcessEnd(this, EventArgs.Empty);

            if (onFinish != null)
                onFinish(param);
            onFinish = null;
            SafeExit();
        }

        protected void SafeExit()
        {
            process.WaitForExit();
            process.Close();
            process = null;

            // There might still be data in a buffer somewhere that needs to be read by the output handler even after the process has ended
            while (processOutputStreamOpen && processErrorStreamOpen)
                Thread.Sleep(15);
        }

    }
}
