using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SouthParkDLCore.Commands.Executables.Abstract
{
    abstract class Executable
    {
        protected abstract String GetLinuxCommand();
        protected abstract String GetWindowsCmdOrExe();

        protected Process process;

        public String Cmd
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return this.GetWindowsCmdOrExe();
                else //Linux and OSX
                    return this.GetLinuxCommand();
            }
        }

        protected String WorkingDirectory;

        public Executable( String workingDirectory )
        {
            this.WorkingDirectory = workingDirectory;
        }

        ~Executable()  // finalizer
        {
            if (process != null)
                process.Close();
        }

        public Boolean Run(String arguments, String logFile = null, String errorLogFile = null, int timeout = 10)
        {
            process = new Process();
            process.startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                process.startInfo.FileName = this.Cmd;
            else
                process.startInfo.FileName = "/bin/bash";

            process.startInfo.UseShellExecute = false;
            process.startInfo.WorkingDirectory = WorkingDirectory;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                process.startInfo.Arguments = arguments;
            else
            {
                var escapedArgs = (this.Cmd + ' ' + arguments).Replace("\"", "\\\"");
                process.startInfo.Arguments = $"-c \"{escapedArgs}\"";
            }

            process.startInfo.RedirectStandardOutput = true;
            processstartInfo.RedirectStandardError = true;
            processstartInfo.CreateNoWindow = true;
            
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();
            
            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
            {
                process.OutputDataReceived += (sender, e) => {
                    if (e.Data == null)
                    {
                        outputWaitHandle.Set();
                    }
                    else
                    {
                        output.AppendLine(e.Data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        errorWaitHandle.Set();
                    }
                    else
                    {
                        error.AppendLine(e.Data);
                    }
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                bool success = process.WaitForExit(timeout) && outputWaitHandle.WaitOne(timeout) && errorWaitHandle.WaitOne(timeout);
                
                if (logFile != null)
                {
                    String output = process.StandardOutput.ReadToEnd();
                    File.WriteAllText(logFile, output);
                }


                if (errorLogFile != null)
                {
                    String error = process.StandardError.ReadToEnd();
                    File.WriteAllText(errorLogFile, error);
                }
                
                return success ? process.ExitCode != 0 : false;
            }
            
            return false;
        }
    }
}
