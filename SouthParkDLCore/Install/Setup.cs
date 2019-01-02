using SouthParkDLCore.Config;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;

namespace SouthParkDLCore.Install
{
    public class Setup
    {
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

        public void setUpIndex()
        {
            File.Delete(RuntimeConfig.Instance.m_indexFile);
            webClient.DownloadFile("https://bumbummen99.github.io/southparkdownloader/data.db", RuntimeConfig.Instance.m_indexFile);
        }

        public void setUpYoutubeDL()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Please make sure youtube-dl is installed.");
                Console.WriteLine("You can install it by typing 'sudo apt install youtube-dl'.");
                return;
            }

            webClient.DownloadFile("https://yt-dl.org/downloads/latest/youtube-dl.exe", RuntimeConfig.Instance.m_dependencyDirectory + "/youtube-dl.exe");
        }

        public void setUpFFMpeg()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Please make sure ffmpeg is installed.");
                Console.WriteLine("You can install it by typing 'sudo apt install ffmpeg'.");
                return;
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

            webClient.DownloadFile(url, RuntimeConfig.Instance.m_tempDiretory + "/ffmpeg-3.4.1.zip");
            ZipFile.ExtractToDirectory(RuntimeConfig.Instance.m_tempDiretory + "/ffmpeg-3.4.1.zip", RuntimeConfig.Instance.m_tempDiretory);
            File.Move(RuntimeConfig.Instance.m_tempDiretory + "/ffmpeg-3.4.1-win64-static/bin/ffmpeg.exe", RuntimeConfig.Instance.m_dependencyDirectory + "/ffmpeg.exe");
            File.Move(RuntimeConfig.Instance.m_tempDiretory + "/ffmpeg-3.4.1-win64-static/bin/ffplay.exe", RuntimeConfig.Instance.m_dependencyDirectory + "/ffplay.exe");
            File.Move(RuntimeConfig.Instance.m_tempDiretory + "/ffmpeg-3.4.1-win64-static/bin/ffprobe.exe", RuntimeConfig.Instance.m_dependencyDirectory + "/ffprobe.exe");
        }

        public Boolean IsSetup()
        {
            if ( !HasIndex() )
            {
                if (!HasYoutubeDL() || !HasFFMpeg() || !HasIndex())
                    return false;
                return true;
            }

            return true;
        }

        public Boolean HasIndex()
        {
            return File.Exists(RuntimeConfig.Instance.m_indexFile);
        }

        public Boolean HasYoutubeDL()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return File.Exists(RuntimeConfig.Instance.m_youtubeDL);
            return true;
        }

        public Boolean HasFFMpeg()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return File.Exists(RuntimeConfig.Instance.m_ffmpeg);
            return true;
        }
    }
}
