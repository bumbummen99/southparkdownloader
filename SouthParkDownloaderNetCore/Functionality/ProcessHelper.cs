using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SouthParkDownloaderNetCore.Functionality
{
    class ProcessHelper
    {
        public static Boolean Run(String workingDirectory, String exeOrCommand, String arguments)
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
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
                return false;
            return true;
        }
    }
}
