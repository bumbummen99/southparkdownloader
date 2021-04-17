using SouthParkDLCore.Config;
using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

namespace SouthParkDLCore.Install
{
  public class Setup
  {
    private WebClient _webClient;
    private Boolean m_useYoutubeDlCommunity = false;

    private WebClient webClient
    {
      get
      {
        if (_webClient == null)
          _webClient = new WebClient();
        return _webClient;
      }
    }

    public Setup(Boolean useYoutubeDLCommunity = false)
    {
      m_useYoutubeDlCommunity = useYoutubeDLCommunity;
    }

    public void setUpIndex()
    {
      Console.WriteLine("Updating episode index...");

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

      String youtubeDLDownloadURI = "https://yt-dl.org/downloads/latest/youtube-dl.exe";
      if (m_useYoutubeDlCommunity)
        youtubeDLDownloadURI = "https://github.com/blackjack4494/youtube-dlc/releases/latest/download/youtube-dlc.exe";

      Console.WriteLine("Downloading youtube-dl" + (m_useYoutubeDlCommunity ? "c" : "") + "...");

      webClient.DownloadFile(youtubeDLDownloadURI, RuntimeConfig.Instance.m_dependencyDirectory + "/youtube-dl.exe");
    }

    public void setUpFFMpeg()
    {
      Console.WriteLine("Downloading & extracting ffmpeg...");

      if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        Console.WriteLine("Unsupported operating detected, can not install ffmpeg automatically!");
        Console.WriteLine("Please make sure ffmpeg is installed or install it manually.");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
          Console.WriteLine("You can install it by installing the ffmpeg packe using your PM. For Debian devirates typing 'sudo apt install ffmpeg' should suffice.");
        }
        
        return;
      }

      try {
        /* Get the latest FFMPEG release version */
        String version = webClient.DownloadString("https://www.gyan.dev/ffmpeg/builds/release-version");

        /* Try to download the release from GitHub */
        webClient.DownloadFile("https://github.com/GyanD/codexffmpeg/releases/download/" + version + "/ffmpeg-" + version + "-full_build.zip", RuntimeConfig.Instance.m_tempDiretory + "/ffmpeg.zip");
        ZipFile.ExtractToDirectory(RuntimeConfig.Instance.m_tempDiretory + "/ffmpeg.zip", RuntimeConfig.Instance.m_tempDiretory);
      } catch (Exception e) {
        Console.WriteLine("Could not download ffmpeg, please contact the developer and/or make sure that the hardcoded uri is still valid. Error: " + e.Message);
        return;
      }

      try {
        string path = Directory.GetDirectories(RuntimeConfig.Instance.m_tempDiretory).Where(p => p.Replace(RuntimeConfig.Instance.m_tempDiretory + Path.DirectorySeparatorChar, "").StartsWith("ffmpeg")).First();
        File.Move(BuildPath(new string[] { path, "bin", "ffmpeg.exe" }), BuildPath(new string[] { RuntimeConfig.Instance.m_dependencyDirectory, "ffmpeg.exe" }));
        File.Move(BuildPath(new string[] { path, "bin", "ffplay.exe" }), BuildPath(new string[] { RuntimeConfig.Instance.m_dependencyDirectory, "ffplay.exe" }));
        File.Move(BuildPath(new string[] { path, "bin", "ffprobe.exe" }), BuildPath(new string[] { RuntimeConfig.Instance.m_dependencyDirectory, "ffprobe.exe" }));
      } catch (Exception e) {
        Console.WriteLine("Could not complete installation of ffmpeg, there was an error during extraction / installation. Error: " + e.Message);
      }
    }

    public Boolean IsSetup()
    {
      return HasIndex() && HasYoutubeDL() && HasFFMpeg();
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

    private string BuildPath(string[] parts)
    {
      return String.Join(Path.DirectorySeparatorChar.ToString(), parts);
    }
}
}
