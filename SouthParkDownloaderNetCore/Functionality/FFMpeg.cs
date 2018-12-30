using System;
using System.IO;
using System.Runtime.InteropServices;
using SouthParkDownloaderNetCore.Helpers;
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

        public static Boolean Mux( String directory, String[] fileList, String filename )
        {
            CreateFilesList(directory, fileList);
            String arguments = "-f concat -safe 0 -i files.txt -c copy \"" + filename + '"';
            String logFile = directory + "/ffmpeg.log";
            Boolean result = ProcessHelper.Run(directory, Executable, arguments, null, logFile); //ffmpeg for some reason writes to error log
            File.Delete(directory + "/files.txt");
            if (result)
                return true;
            return false;
        }

        public static void CreateFilesList( String targetDirectory, String[] fileList )
        {
            StreamWriter sw = File.CreateText(targetDirectory + "/files.txt");
            foreach (String filePath in fileList)
                sw.Write("file '" + filePath + '\'' + sw.NewLine);
            sw.Close();
        }
    }
}
