using SouthParkDownloaderNetCore.Logic;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;

namespace SouthParkDownloaderNetCore.Install
{
    class Setup
    {
        private ApplicationLogic applicationLogic;
        private WebClient _webClient;
        private WebClient webClient
        {
            get
            {
                if (_webClient == null)
                    _webClient = new WebClient();
                return _webClient;
            }
        }

        public Setup( ApplicationLogic applicationLogic )
        {
            this.applicationLogic = applicationLogic;
        }

        public void setUpFolderStructure()
        {
            /* Setup folder structures */
            applicationLogic.m_dependencyDirectory = Directory.CreateDirectory(applicationLogic.m_workingDirectory + @"\dep").FullName;
            applicationLogic.m_dataDirectory = Directory.CreateDirectory(applicationLogic.m_workingDirectory + @"\data").FullName;
            applicationLogic.m_tempDiretory = Directory.CreateDirectory(applicationLogic.m_workingDirectory + @"\tmp").FullName;
        }

        public void setUpIndex()
        {
            File.Delete(applicationLogic.m_indexFile);
            webClient.DownloadFile("https://bumbummen99.github.io/southparkdownloader/data.csv", applicationLogic.m_indexFile);
        }

        public void setUpYoutubeDL()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Please make sure youtube-dl is installed.");
                Console.WriteLine("You can install it by typing 'sudo apt install youtube-dl'.");
            }

            webClient.DownloadFile("https://yt-dl.org/downloads/latest/youtube-dl.exe", applicationLogic.m_dependencyDirectory + @"\youtube-dl.exe");
        }

        public void setUpFFMpeg()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Please make sure ffmpeg is installed.");
                Console.WriteLine("You can install it by typing 'sudo apt install ffmpeg'.");
            }

            String url;
            switch( RuntimeInformation.OSArchitecture )
            {
                case Architecture.X86:
                    url = "https://ffmpeg.zeranoe.com/builds/win64/static/ffmpeg-3.4.1-win32-static.zip";
                    break;

                case Architecture.X64:
                    url = "https://ffmpeg.zeranoe.com/builds/win64/static/ffmpeg-3.4.1-win64-static.zip";
                    break;

                default:
                    Console.WriteLine("Unsupported Architecture " + RuntimeInformation.OSArchitecture);
                    return;
            }

            webClient.DownloadFile(url, applicationLogic.m_tempDiretory + @"\ffmpeg-3.4.1.zip");
            ZipFile.ExtractToDirectory(applicationLogic.m_tempDiretory + @"\ffmpeg-3.4.1.zip", applicationLogic.m_tempDiretory);
            File.Move(applicationLogic.m_tempDiretory + @"\ffmpeg-3.4.1-win64-static\bin\ffmpeg.exe", applicationLogic.m_dependencyDirectory + @"\ffmpeg.exe");
            File.Move(applicationLogic.m_tempDiretory + @"\ffmpeg-3.4.1-win64-static\bin\ffplay.exe", applicationLogic.m_dependencyDirectory + @"\ffplay.exe");
            File.Move(applicationLogic.m_tempDiretory + @"\ffmpeg-3.4.1-win64-static\bin\ffprobe.exe", applicationLogic.m_dependencyDirectory + @"\ffprobe.exe");
        }

        public Boolean IsSetup()
        {
            if (!File.Exists(applicationLogic.m_youtubeDL) || !File.Exists(applicationLogic.m_ffmpeg) || !HasIndex())
                return false;
            return true;
        }

        public Boolean HasIndex()
        {
            if (!File.Exists(applicationLogic.m_indexFile))
                return false;
            return true;
        }
    }
}
