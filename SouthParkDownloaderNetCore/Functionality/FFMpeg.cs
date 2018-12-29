using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SouthParkDownloaderNetCore.Logic;
using SouthParkDownloaderNetCore.Types;

namespace SouthParkDownloaderNetCore.Functionality
{
    class FFMpeg
    {
        public static String Executable
        {
            get {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return ApplicationLogic.Instance.m_ffmpeg;
                else //Linux and OSX
                    return "ffmpeg";
            }
        }

        public static Boolean Mux( String directory, String filename )
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = FFMpeg.Executable;
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = directory;
            startInfo.Arguments = "-f concat -safe 0 -i files.txt -c copy \"" + filename +  '"';
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
                return false;
            return true;
        }
    }
}
