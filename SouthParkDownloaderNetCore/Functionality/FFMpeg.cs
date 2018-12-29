using System;
using System.Diagnostics;
using SouthParkDownloaderNetCore.Logic;
using SouthParkDownloaderNetCore.Types;

namespace SouthParkDownloaderNetCore.Functionality
{
    class FFMpeg
    {
        public static Boolean Mux( String directory, String filename )
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = ApplicationLogic.Instance.m_ffmpeg;
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
