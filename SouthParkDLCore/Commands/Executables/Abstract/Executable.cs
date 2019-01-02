using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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

        public Boolean Run(String arguments, String logFile = null, String errorLogFile = null)
        {
            process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                startInfo.FileName = this.Cmd;
            else
                startInfo.FileName = "/bin/bash";

            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = WorkingDirectory;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                startInfo.Arguments = arguments;
            else
            {
                var escapedArgs = (this.Cmd + ' ' + arguments).Replace("\"", "\\\"");
                startInfo.Arguments = $"-c \"{escapedArgs}\"";
            }

            startInfo.RedirectStandardOutput = logFile != null;
            startInfo.RedirectStandardError = errorLogFile != null;
            startInfo.CreateNoWindow = true;

            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

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

            if (process.ExitCode != 0)
                return false;
            return true;
        }
    }
}
