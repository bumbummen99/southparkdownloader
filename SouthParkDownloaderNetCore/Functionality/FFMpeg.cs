using System;
using System.Runtime.InteropServices;
using SouthParkDownloaderNetCore.Logic;

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
            String arguments = "-f concat -safe 0 -i files.txt -c copy \"" + filename + '"';
            if (ProcessHelper.Run(directory, Executable, arguments))
                return true;
            return false;
        }
    }
}
