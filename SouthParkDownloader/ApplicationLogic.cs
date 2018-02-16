using SouthParkDownloader.Core;
using SouthParkDownloader.Functionality;
using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Linq;

namespace SouthParkDownloader
{
  class ApplicationLogic : Logic
  {
    private String m_dependencyDirectory;
    private String m_dataDiretory;
    private String m_tempDiretory;

    public ApplicationLogic( String name ) : base( name )
    {
      /* Setup folder structuren */
      m_dependencyDirectory = Directory.CreateDirectory( m_workingDirectory + @"\dep" ).FullName;
      m_dataDiretory = Directory.CreateDirectory( m_workingDirectory + @"\data" ).FullName;
      m_tempDiretory = Directory.CreateDirectory( m_workingDirectory + @"\tmp" ).FullName;

      Run();
    }

    override public void Tick()
    {
      //Get user Input
      Console.Write( "$" );
      CLParser cmd = new CLParser( Console.ReadLine() );

      //Parse command
      switch ( cmd.Command )
      {
        case "setup":
          Setup();
          break;

        case "index":
          Index();
          break;

        case "download":
          //download video files
          break;

        case "process":
          //Merge video files
          break;

        case "help":
          PostHelp();
          break;

        case "exit":
          m_exit = true;
          break;

        default:
          Console.WriteLine( "Unknown Command \"" + cmd.Command + "\". Enter \"help\" for a list of the available commands." );
          break;
      }
    }

    private void PostHelp()
    {
      Console.WriteLine( "setup       Starts the grabbing process. Place data.csv next to the executable." );
      Console.WriteLine( "index       Index the seasons and episodes available at southpark.de." );
      Console.WriteLine( "download    Download the episode from the current index." );
      Console.WriteLine( "process     Merge the episode parts into single files." );
      Console.WriteLine( "help        Show information about commands" );
      Console.WriteLine( "exit        Exits the application" );
    }

    private void Index()
    {
      File.Delete( m_dataDiretory + @"\data.csv" );
      WebClient webClient = new WebClient();
      webClient.DownloadFile( "https://bumbummen99.github.io/southparkdownloader/data.csv", m_dataDiretory + @"\data.csv" );
    }

    private void Setup()
    {
      CleanDependencys();
      CleanTemp();

      /* Download dependencies */
      WebClient webClient = new WebClient();
      
      //youtbe-dl
      Console.WriteLine( "Downloading youtube-dl" );
      webClient.DownloadFile( "https://yt-dl.org/downloads/latest/youtube-dl.exe", m_dependencyDirectory + @"\youtube-dl.exe" );

      //ffmpeg
      Console.WriteLine( "Downloading ffmpeg" );
      webClient.DownloadFile( "https://ffmpeg.zeranoe.com/builds/win64/static/ffmpeg-3.4.1-win64-static.zip", m_tempDiretory + @"\ffmpeg-3.4.1.zip" );
      Console.WriteLine( "Extracting ffmpeg" );
      ZipFile.ExtractToDirectory( m_tempDiretory + @"\ffmpeg-3.4.1.zip", m_tempDiretory );
      File.Move( m_tempDiretory + @"\ffmpeg-3.4.1-win64-static\bin\ffmpeg.exe", m_dependencyDirectory + @"\ffmpeg.exe" );
      File.Move( m_tempDiretory + @"\ffmpeg-3.4.1-win64-static\bin\ffplay.exe", m_dependencyDirectory + @"\ffplay.exe" );
      File.Move( m_tempDiretory + @"\ffmpeg-3.4.1-win64-static\bin\ffprobe.exe", m_dependencyDirectory + @"\ffprobe.exe" );

      //reindex
      Index();

      Console.WriteLine( "Setup complete, clearing tmp" );
      CleanTemp();
    }

    private void CleanDependencys()
    {
      DirectoryInfo di = new DirectoryInfo( m_dependencyDirectory );

      foreach ( FileInfo file in di.GetFiles() )
      {
        file.Delete();
      }
      foreach ( DirectoryInfo dir in di.GetDirectories() )
      {
        dir.Delete( true );
      }
    }

    private void CleanTemp()
    {
      DirectoryInfo di = new DirectoryInfo( m_tempDiretory );

      foreach ( FileInfo file in di.GetFiles() )
      {
        file.Delete();
      }
      foreach ( DirectoryInfo dir in di.GetDirectories() )
      {
        dir.Delete( true );
      }
    }
  }
}