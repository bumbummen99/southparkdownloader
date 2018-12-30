using SouthParkDownloaderNetCore.Helpers;
using SouthParkDownloaderNetCore.Logic;
using System;
using System.Runtime.InteropServices;

namespace SouthParkDownloaderNetCore.Functionality
{
    class YouTubeDL
    {
        public static String Executable
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return ApplicationLogic.Instance.m_youtubeDL;
                else //Linux and OSX
                    return "youtube-dl";
            }
        }

        public static Boolean Download( String url, String directory )
        {
            String arguments = "-q " + url;
            String logFile = directory + "/ytdl.log";
            if (ProcessHelper.Run(directory, Executable, arguments, logFile))
                return true;
            return false;
        }
    }
}
