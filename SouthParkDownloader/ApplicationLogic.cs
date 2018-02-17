using SouthParkDownloader.Core;
using SouthParkDownloader.Functionality;
using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Linq;
using TinyCsvParser;
using SouthParkDownloader.Types;
using System.Text;
using System.Diagnostics;

namespace SouthParkDownloader
{
  class ApplicationLogic : Logic
  {
    private String m_dependencyDirectory;
    private String m_dataDiretory;
    private String m_tempDiretory;

    private String m_indexFile
    {
      get
      {
        return m_dataDiretory + @"\data.csv";
      }
    }
    private String m_youtubeDL
    {
      get
      {
        return m_dependencyDirectory + @"\youtube-dl.exe";
      }
    }
    private String m_ffmpeg
    {
      get
      {
        return m_dependencyDirectory + @"\ffmpeg.exe";
      }
    }

    private ArrayList m_episodes;

    public ApplicationLogic( String name ) : base( name )
    {
      /* Setup folder structuren */
      m_dependencyDirectory = Directory.CreateDirectory( m_workingDirectory + @"\dep" ).FullName;
      m_dataDiretory = Directory.CreateDirectory( m_workingDirectory + @"\data" ).FullName;
      m_tempDiretory = Directory.CreateDirectory( m_workingDirectory + @"\tmp" ).FullName;

      if ( !IsSetup() )
        Setup();

      m_episodes = new ArrayList();

      if ( HasIndex() )
        ReadIndexData();

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
          ReadIndexData();
          break;

        case "updateindex":
          DownloadIndex();
          ReadIndexData();
          break;

        case "download":
          Download();
          break;

        case "process":
          Merge();
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
      Console.WriteLine( "index       Load the index file containing episodes from southpark.de" );
      Console.WriteLine( "updateindex Updates and reindexes the index file." );
      Console.WriteLine( "download    Download the episode from the current index." );
      Console.WriteLine( "process     Merge the episode parts into single files." );
      Console.WriteLine( "help        Show information about commands" );
      Console.WriteLine( "exit        Exits the application" );
    }

    private void Download()
    {
      foreach ( Episode episode in m_episodes )
      {
        DownloadEpisode( episode );
        MergeEpisode( episode );
      }
    }

    private void DownloadEpisode( Episode episode, Boolean overwrite = false )
    {
      String seasonDir = m_dataDiretory + '/' + episode.Season;
      String episodeDir = seasonDir + '/' + episode.Number;

      if ( !overwrite && File.Exists( episodeDir + "/dlfinish" ) )
        return;

      Console.WriteLine( "Start downloading Episode " + episode.Number + ' ' + episode.Name );

      Directory.CreateDirectory( seasonDir );
      Directory.CreateDirectory( episodeDir );

      String command = '"' + m_youtubeDL + "\" " + episode.Address;

      Process process = new Process();
      ProcessStartInfo startInfo = new ProcessStartInfo();
      startInfo.WindowStyle = ProcessWindowStyle.Hidden;
      startInfo.FileName = "cmd.exe";
      startInfo.RedirectStandardInput = true;
      startInfo.UseShellExecute = false;
      process.StartInfo = startInfo;
      process.Start();

      using ( StreamWriter sw = process.StandardInput )
      {
        if ( sw.BaseStream.CanWrite )
        {
          sw.WriteLine( "cd " + episodeDir );
          sw.WriteLine( command );
        }
      }

      process.WaitForExit();

      if ( process.ExitCode != 0 )
      {
        CleanDirectory( episodeDir );
        Console.WriteLine( "YoutubeDL failed for some reason." );
      }

      File.Create( episodeDir + "/dlfinish" );
#if RELEASE
      File.SetAttributes( episodeDir + "/dlfinish", File.GetAttributes( episodeDir + "/dlfinish" ) | FileAttributes.Hidden );
#endif
    }

    private void Merge()
    {
      foreach ( Episode episode in m_episodes )
      {
        MergeEpisode( episode );
      }
    }

    private void MergeEpisode( Episode episode )
    {
      String seasonDir = m_dataDiretory + '/' + episode.Season;
      String episodeDir = seasonDir + '/' + episode.Number;

      if ( !File.Exists( episodeDir + "/dlfinish" ) )
      {
        Console.WriteLine( "No video files to merge!" );
        return;
      }

      if ( File.Exists( episodeDir + "/mergefinish" ) )
        return;

      String[] files = Directory.GetFiles( episodeDir );
      ArrayList videoParts = new ArrayList();
      foreach ( String file in files )
      {
        String ext = Path.GetExtension( file );
        if ( Path.GetExtension( file ) != ".mp4" )
          continue;

        videoParts.Add( file );
      }

      String[] sortedParts = new String[videoParts.Count];
      foreach ( String part in videoParts )
      {
        Int32 index = Int32.Parse( part.Substring( part.IndexOf( ". A" ) - 1, 1 ) );
        if ( index == 0 )
          return; //English episode

        sortedParts[index - 1] = part;
      }

      StreamWriter fileList = File.CreateText( episodeDir + "/files.txt" );
      foreach ( String filePath in sortedParts )
      {
        fileList.Write( "file '" + filePath + "'" + fileList.NewLine );
      }
      fileList.Close();

      String command = '"' + m_ffmpeg + "\" -f concat -safe 0 -i files.txt -c copy \"" + RemoveSpecialCharacters( episode.Name ) + ".mp4\"";

      Process process = new Process();
      ProcessStartInfo startInfo = new ProcessStartInfo();
      startInfo.WindowStyle = ProcessWindowStyle.Hidden;
      startInfo.FileName = "cmd.exe";
      startInfo.RedirectStandardInput = true;
      startInfo.RedirectStandardOutput = true;
      startInfo.UseShellExecute = false;
      process.StartInfo = startInfo;
      process.Start();

      using ( StreamWriter sw = process.StandardInput )
      {
        if ( sw.BaseStream.CanWrite )
        {
          sw.WriteLine( "cd " + episodeDir );
          sw.WriteLine( command );
        }
      }

      while ( !process.StandardOutput.EndOfStream )
      {
        String line = process.StandardOutput.ReadLine();
        if ( line.ToLower().Contains( "invalid" ) )
        {
          Console.WriteLine( "FFMPEG failed for some reason!" );
          return;
        }
      }

      process.WaitForExit();

      File.Create( episodeDir + "/mergefinish" );
#if RELEASE
      File.SetAttributes( episodeDir + "/mergefinish", File.GetAttributes( episodeDir + "/mergefinish" ) | FileAttributes.Hidden );
#endif

    }

    private Boolean HasIndex()
    {
      if ( !File.Exists( m_indexFile ) )
        return false;
      return true;
    }

    private void DownloadIndex()
    {
      File.Delete( m_indexFile );
      WebClient webClient = new WebClient();
      webClient.DownloadFile( "https://bumbummen99.github.io/southparkdownloader/data.csv", m_indexFile );
    }

    private void ReadIndexData()
    {
      if ( !File.Exists( m_indexFile ) )
      {
        Console.WriteLine( "No index data found!" );
        return;
      }

      /* Setup csv parser options */
      CsvParserOptions csvParserOptions = new CsvParserOptions( false, ';' );
      CsvEpisodeMapping csvMapper = new CsvEpisodeMapping();
      CsvParser<Episode> csvParser = new CsvParser<Episode>( csvParserOptions, csvMapper );

      /* Parse csv file */
      var results = csvParser.ReadFromFile( m_indexFile, Encoding.ASCII ).ToList();

      /* Check if we have a result */
      if ( results != null && results.Count <= 0 )
        return;

      /* Process services */
      foreach ( TinyCsvParser.Mapping.CsvMappingResult<Episode> episode in results )
      {
        if ( episode.Result != null )
          m_episodes.Add( episode.Result ); //Add service to internal list
      }
      Console.WriteLine( "Index data read successfully" );
    }

    private Boolean IsSetup()
    {
      if ( !File.Exists( m_youtubeDL ) || !File.Exists( m_ffmpeg ) || !HasIndex() )
        return false;
      return true;
    }

    private void Setup()
    {
      CleanDirectory( m_dependencyDirectory );
      CleanDirectory( m_tempDiretory );

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
      DownloadIndex();

      Console.WriteLine( "Setup complete, clearing tmp" );
      CleanDirectory( m_tempDiretory );
    }

    private void CleanDirectory( String directory )
    {
      DirectoryInfo di = new DirectoryInfo( directory );

      foreach ( FileInfo file in di.GetFiles() )
      {
        file.Delete();
      }
      foreach ( DirectoryInfo dir in di.GetDirectories() )
      {
        dir.Delete( true );
      }
    }

    public static String RemoveSpecialCharacters( String str )
    {
      StringBuilder sb = new StringBuilder();
      foreach ( Char c in str )
      {
        if ( (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == ' ' )
        {
          sb.Append( c );
        }
      }
      return sb.ToString();
    }
  }
}