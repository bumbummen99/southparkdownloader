using SouthParkDownloaderNetCore.Logic;
using System;
using System.Diagnostics;

namespace SouthParkDownloaderNetCore.Functionality
{
    class YouTubeDL
    {
        public static Boolean Download( String url, String directory )
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = ApplicationLogic.Instance.m_youtubeDL;
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = directory;
            startInfo.Arguments = url;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
                return false;
            return true;
        }
    }
}
