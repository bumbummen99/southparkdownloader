using SouthParkDLCore.Commands.Executables.Abstract;
using SouthParkDLCore.Config;
using System;
using System.IO;

namespace SouthParkDLCore.Commands.Executables
{
    class FFMpeg : Executable
    {
        protected override string GetLinuxCommand()
        {
            return "ffmpeg";
        }

        protected override string GetWindowsCmdOrExe()
        {
            return RuntimeConfig.Instance.m_ffmpeg;
        }

        public FFMpeg(String workingDirectory) : base(workingDirectory)
        {
            //
        }

        public Boolean Mux(String[] fileList, String filename)
        {
            CreateFilesList(WorkingDirectory, fileList); //Create file list as ffmpeg requires it

            String arguments = "-f concat -safe 0 -i files.txt -c copy \"" + filename + '"';
            String logFile = WorkingDirectory + "/ffmpeg.log";
            Boolean result = Run(arguments, null, logFile); //ffmpeg for some reason writes to error log

            File.Delete(WorkingDirectory + "/files.txt");
            if (result)
                return true;
            return false;
        }

        public static void CreateFilesList(String targetDirectory, String[] fileList)
        {
            StreamWriter sw = File.CreateText(targetDirectory + "/files.txt");
            foreach (String filePath in fileList)
                sw.Write("file '" + filePath + '\'' + sw.NewLine);
            sw.Close();
        }
    }
}
