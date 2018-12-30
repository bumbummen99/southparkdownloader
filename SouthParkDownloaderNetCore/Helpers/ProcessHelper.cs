using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SouthParkDownloaderNetCore.Helpers
{
    class ProcessHelper
    {
        public static Boolean Run(String workingDirectory, String exeOrCommand, String arguments, String logFile = null, String errorLogFile = null)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                startInfo.FileName = exeOrCommand;
            else
                startInfo.FileName = "/bin/bash";

            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = workingDirectory;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                startInfo.Arguments = arguments;
            else
            {
                String cmd = exeOrCommand + ' ' + arguments;
                var escapedArgs = cmd.Replace("\"", "\\\"");
                startInfo.Arguments = $"-c \"{escapedArgs}\"";
            }

            startInfo.RedirectStandardOutput = logFile != null;
            startInfo.RedirectStandardError = errorLogFile != null;

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
