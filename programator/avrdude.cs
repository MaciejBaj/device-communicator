﻿/*
 * Project: AVRDUDESS - A GUI for AVRDUDE
 * Author: Zak Kemble, contact@zakkemble.co.uk
 * Copyright: (C) 2014 by Zak Kemble
 * License: GNU GPL v3 (see License.txt)
 * Web: http://blog.zakkemble.co.uk/avrdudess-a-gui-for-avrdude/
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace avrdudess
{
    abstract class Executable
    {
        private Process p;
        private Action<object> onFinish;
        private object param;
        public event EventHandler OnProcessStart;
        public event EventHandler OnProcessEnd;
        private string binary;
        private bool processOutputStreamOpen;
        private bool processErrorStreamOpen;
        private bool enableConsoleUpdate;
        protected string outputLog { get; private set; }

        public enum OutputTo
        {
            Log,
            Console
        }

        protected void load(string binaryName, string directory, bool enableConsoleWrite = true)
        {
            binary = searchForBinary(binaryName, directory);

            if (binary == null)
                MsgBox.error(binaryName + " is missing!");
            else if (enableConsoleWrite)
            {
                Thread t = new Thread(new ThreadStart(tConsoleUpdate));
                t.IsBackground = true;
                t.Start();
            }
        }

        private string searchForBinary(string binaryName, string directory)
        {
            PlatformID os = Environment.OSVersion.Platform;
            if (os != PlatformID.MacOSX && os != PlatformID.Unix)
                binaryName += ".exe";

            string app;

            // Check user defined directory
            if (!String.IsNullOrEmpty(directory))
            {
                app = Path.Combine(directory, binaryName);
                if (File.Exists(app))
                    return app;
                return null;
            }

            // File exists in application directory (mainly for Windows)
            app = Path.Combine(AssemblyData.directory, binaryName);
            if (File.Exists(app))
                return app;

            // File exists in working directory
            app = Path.Combine(Directory.GetCurrentDirectory(), binaryName);
            if (File.Exists(app))
                return app;

            // Search PATHs
            string[] paths = Environment.GetEnvironmentVariable("PATH").Split(new char[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string path in paths)
            {
                app = Path.Combine(path, binaryName);
                if (File.Exists(app))
                    return app;
            }

            return null;
        }

        protected bool launch(string args, Action<object> onFinish, object param, OutputTo outputTo)
        {
            if (isActive()) // Another process is active
                return false;

            // Clear log
            outputLog = "";
            //Util.consoleClear();

            // Binary is missing
            if (binary == null || !File.Exists(binary))
                return false;

            this.onFinish = onFinish;
            this.param = param;

            return launch(args, outputTo);
        }

        private bool launch(string args, OutputTo outputTo)
        {
            Process tmp = new Process();
            tmp.StartInfo.FileName = binary;
            tmp.StartInfo.Arguments = args;
            tmp.StartInfo.CreateNoWindow = true;
            tmp.StartInfo.UseShellExecute = false;
            tmp.StartInfo.RedirectStandardOutput = true;
            tmp.StartInfo.RedirectStandardError = true;
            tmp.EnableRaisingEvents = true;
            if (outputTo == OutputTo.Log)
            {
                tmp.OutputDataReceived += new DataReceivedEventHandler(outputLogHandler);
                tmp.ErrorDataReceived += new DataReceivedEventHandler(errorLogHandler);
            }
            tmp.Exited += new EventHandler(p_Exited);

            try
            {
                tmp.Start();
            }
            catch (Exception ex)
            {
                MsgBox.error("Error starting process", ex);
                return false;
            }

            if (OnProcessStart != null)
                OnProcessStart(this, EventArgs.Empty);

            enableConsoleUpdate = (outputTo == OutputTo.Console);
            p = tmp;

            if (outputTo == OutputTo.Log)
            {
                processOutputStreamOpen = true;
                processErrorStreamOpen = true;
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }
            else
            {
                processOutputStreamOpen = false;
                processErrorStreamOpen = false;
            }

            return true;
        }

        private void p_Exited(object sender, EventArgs e)
        {
            if (OnProcessEnd != null)
                OnProcessEnd(this, EventArgs.Empty);

            if (onFinish != null)
                onFinish(param);
            onFinish = null;
        }

        // Progress bars don't work using async output, since it only fires when a new line is received
        // Problem: Slow if the process outputs a lot of text
        private void tConsoleUpdate()
        {
            while (true)
            {
                Thread.Sleep(15);

                if (!enableConsoleUpdate)
                    continue;

                try
                {
                    if (p != null)
                    {
                        char[] buff = new char[256];

                        // TODO: read from stdError AND stdOut (AVRDUDE outputs stuff through stdError)
                        if (p.StandardError.Read(buff, 0, buff.Length) > 0)
                        {
                            string s = new string(buff);
                            Util.consoleWrite(s);
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        // These methods are needed to properly capture the process output for logging
        private bool logger(string s)
        {
            if (s != null) // A null is sent when the stream is closed
            {
                outputLog += s.Replace("\0", String.Empty) + Environment.NewLine;
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

        protected bool isActive()
        {
            return (p != null && !p.HasExited);
        }

        public bool kill()
        {
            if (!isActive())
                return false;
            p.Kill();
            return true;
        }

        protected void waitForExit()
        {
            if (isActive())
                p.WaitForExit();

            // There might still be data in a buffer somewhere that needs to be read by the output handler even after the process has ended
            while (processOutputStreamOpen && processErrorStreamOpen)
                Thread.Sleep(15);
        }
    }
}
