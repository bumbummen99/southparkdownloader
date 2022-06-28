using SouthParkDLCore.Commands.Executables.Abstract;
using SouthParkDLCore.Config;
using System;

namespace SouthParkDLCore.Commands.Executables
{
    class YouTubeDL : Executable
    {
        protected override string GetLinuxCommand()
        {
            return "youtube-dl";
        }

        protected override string GetWindowsCmdOrExe()
        {
            return RuntimeConfig.Instance.m_youtubeDL;
        }

        public YouTubeDL( String workingDirectory ) : base(workingDirectory)
        {
            //
        }

        public Boolean Download(String url)
        {
            String arguments = "-q " + url;
            String logFile = WorkingDirectory + "/ytdl.log";

            return Run(arguments, logFile, null, 0);
        }
    }
}
