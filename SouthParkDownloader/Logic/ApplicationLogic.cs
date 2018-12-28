using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Linq;
using System.Text;
using System.Diagnostics;

using TinyCsvParser;

using SouthParkDownloader.Functionality;
using SouthParkDownloader.Types;
using SouthParkDownloader.CSVMappings;

namespace SouthParkDownloader.Logic
{
    class ApplicationLogic : Core.Logic
    {
        public String m_dependencyDirectory;
        public String m_dataDirectory;
        public String m_tempDiretory;
        private SystemInfo m_systemInfo;

        public String m_indexFile
        {
            get
            {
                return m_dataDirectory + @"\data.csv";
            }
        }
        public String m_youtubeDL
        {
            get
            {
                return m_dependencyDirectory + @"\youtube-dl.exe";
            }
        }
        public String m_ffmpeg
        {
            get
            {
                return m_dependencyDirectory + @"\ffmpeg.exe";
            }
        }

        private ArrayList m_episodes;

        private static ApplicationLogic instance;
        public static ApplicationLogic Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ApplicationLogic();
                }
                return instance;
            }
        }

        private ApplicationLogic() : base("SouthParkDownlaoder", "1.1")
        {
            /* Get information about the system */
            m_systemInfo = (new SystemAnalyzer()).GetInfo();

            /* Setup folder structures */
            m_dependencyDirectory = Directory.CreateDirectory(m_workingDirectory + @"\dep").FullName;
            m_dataDirectory = Directory.CreateDirectory(m_workingDirectory + @"\data").FullName;
            m_tempDiretory = Directory.CreateDirectory(m_workingDirectory + @"\tmp").FullName;

            if (!IsSetup())
                Setup();

            m_episodes = new ArrayList();

            if (HasIndex())
                ReadIndexData();
        }

        override public void Tick()
        {
            //Get user Input
            Console.Write("$");
            CLParser cmd = new CLParser(Console.ReadLine());

            //Parse command
            switch (cmd.Command)
            {
                case "setup":
                    Setup();
                    break;

                case "index":
                    ReadIndexData(cmd.HasArgument("update"));
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
                    Console.WriteLine("Unknown Command \"" + cmd.Command + "\". Enter \"help\" for a list of the available commands.");
                    break;
            }
        }

        private void PostHelp()
        {
            Console.WriteLine("setup       Starts the grabbing process. Place data.csv next to the executable.");
            Console.WriteLine("index       Load the index file containing episodes from southpark.de");
            Console.WriteLine("  update    Updates and reindexes the index file.\n");
            Console.WriteLine("download    Download the episode from the current index.");
            Console.WriteLine("process     Merge the episode parts into single files.");
            Console.WriteLine("help        Show information about commands");
            Console.WriteLine("exit        Exits the application");
        }

        private void Download()
        {
            foreach (Episode episode in m_episodes)
            {
                DownloadEpisode(episode);
                MergeEpisode(episode);
            }
        }

        private void DownloadEpisode(Episode episode, Boolean overwrite = false)
        {
            Directory.CreateDirectory(episode.SeasonDirectory); // Create directories in case they dont exist
            Directory.CreateDirectory(episode.Directory);

            if (File.Exists(episode.Directory + "/dlfinish") && !overwrite)
                return; // Skip already downloaded episode

            if (!this.YTDLEpisode(episode))
                return;

            String[] files = Directory.GetFiles(episode.Directory);
            ArrayList videoParts = new ArrayList();
            foreach (String _file in files)
            {
                String extension = Path.GetExtension(_file);
                String filename = Path.GetFileNameWithoutExtension(_file);
                if (extension != ".mp4")
                    continue;

                Int32 index = 0;
                if (filename.Contains(". Ak-") || filename.Contains(". Akt")) //Deutsch
                    index = Int32.Parse(filename.Substring(filename.IndexOf(". Ak") - 1, 1));
                else if (filename.Contains("Akt"))
                    index = Int32.Parse(filename.Substring(filename.IndexOf("Akt ") + 4, 1));
                else if (filename.Contains("Teil "))
                    index = Int32.Parse(filename.Substring(filename.IndexOf("Teil ") + 5, 1));
                else //Englisch
                    index = Int32.Parse(filename.Substring(filename.IndexOf("Act ") + 4, 1));

                System.IO.File.Move(_file, Path.GetDirectoryName(_file) + "/part" + index + extension);
                videoParts.Add(_file);
            }

            File.Create(episode.Directory + "/dlfinish");
#if RELEASE
            File.SetAttributes( episode.Directory + "/dlfinish", File.GetAttributes( episode.Directory + "/dlfinish" ) | FileAttributes.Hidden );
#endif
        }

        private bool YTDLEpisode(Episode episode)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = m_youtubeDL;
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = episode.Directory;
            startInfo.Arguments = episode.Address;
            process.StartInfo = startInfo;

            Console.WriteLine("Start downloading Episode " + episode.Number + ' ' + episode.Name);
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                CleanDirectory(episode.Directory);
                Console.WriteLine("YoutubeDL failed for some reason.");
                return false;
            }

            return true;
        }

        private void Merge()
        {
            foreach (Episode episode in m_episodes)
            {
                MergeEpisode(episode);
            }
        }

        private void MergeEpisode(Episode episode)
        {
            if (!File.Exists(episode.Directory + "/dlfinish"))
            {
                Console.WriteLine("No video files to merge!");
                return;
            }

            if (File.Exists(episode.Directory + "/mergefinish"))
                return;

            String[] files = Directory.GetFiles(episode.Directory);
            ArrayList videoParts = new ArrayList();
            foreach (String _file in files)
            {
                String ext = Path.GetExtension(_file);
                if (Path.GetExtension(_file) != ".mp4")
                    continue;

                videoParts.Add(_file);
            }

            /* Sort paths */
            String[] sortedParts = new String[videoParts.Count];
            foreach (String part in videoParts)
            {
                Int32 index = Int32.Parse(part.Substring(part.IndexOf("part") + 4, 1));
                if (index == 0)
                    return; //English episode

                if (index - 1 > 0 && sortedParts[index - 2] == null)
                {
                    Console.WriteLine("Missing part " + (index - 1));
                    return;
                }
                sortedParts[index - 1] = part;
            }

            /* Output parts into files.txt for ffmpeg */
            StreamWriter fileList = File.CreateText(episode.Directory + "/files.txt");
            foreach (String filePath in sortedParts)
            {
                fileList.Write("file '" + filePath + '\'' + fileList.NewLine);
            }
            fileList.Close();

            String outputFileName = RemoveSpecialCharacters(episode.Name) + ".mp4";

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = m_ffmpeg;
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = episode.Directory;
            startInfo.Arguments = "-f concat -safe 0 -i files.txt -c copy \"" + outputFileName + '"';
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                //CleanDirectory( episodeDir );
                Console.WriteLine("ffmpeg failed for some reason.");
                return;
            }

            /* Delete previous data after successfull concat */
            foreach (String oldFile in sortedParts)
            {
                File.Delete(oldFile);
            }
            File.Delete(episode.Directory + "/files.txt");

            /* Add meta data */
            TagLib.File file = TagLib.File.Create(episode.Directory + '/' + outputFileName); // Change file path accordingly.
            file.Tag.Title = episode.Name;
            file.Save();

            File.Create(episode.Directory + "/mergefinish");
#if RELEASE
            File.SetAttributes( episode.Directory + "/mergefinish", File.GetAttributes( episode.Directory + "/mergefinish" ) | FileAttributes.Hidden );
#endif

        }

        private Boolean HasIndex()
        {
            if (!File.Exists(m_indexFile))
                return false;
            return true;
        }

        private void DownloadIndex()
        {
            File.Delete(m_indexFile);
            WebClient webClient = new WebClient();
            webClient.DownloadFile("https://bumbummen99.github.io/southparkdownloader/data.csv", m_indexFile);
        }

        private void ReadIndexData(Boolean update = false)
        {
            if (update)
            {
                if (File.Exists(m_indexFile))
                    File.Delete(m_indexFile);
                DownloadIndex();
            }

            if (!File.Exists(m_indexFile))
            {
                Console.WriteLine("No index data found!");
                return;
            }

            /* Setup csv parser options */
            CsvParserOptions csvParserOptions = new CsvParserOptions(false, ';');
            CsvEpisodeMapping csvMapper = new CsvEpisodeMapping();
            CsvParser<Episode> csvParser = new CsvParser<Episode>(csvParserOptions, csvMapper);

            /* Parse csv file */
            var results = csvParser.ReadFromFile(m_indexFile, Encoding.ASCII).ToList();

            /* Check if we have a result */
            if (results != null && results.Count <= 0)
                return;

            /* Process services */
            foreach (TinyCsvParser.Mapping.CsvMappingResult<Episode> episode in results)
            {
                if (episode.Result != null)
                    m_episodes.Add(episode.Result); //Add service to internal list
            }
            Console.WriteLine("Index data read successfully");
        }

        private Boolean IsSetup()
        {
            if (!File.Exists(m_youtubeDL) || !File.Exists(m_ffmpeg) || !HasIndex())
                return false;
            return true;
        }

        private void Setup()
        {
            CleanDirectory(m_dependencyDirectory);
            CleanDirectory(m_tempDiretory);

            /* Download dependencies */
            WebClient webClient = new WebClient();

            //youtbe-dl
            Console.WriteLine("Downloading youtube-dl");
            webClient.DownloadFile("https://yt-dl.org/downloads/latest/youtube-dl.exe", m_dependencyDirectory + @"\youtube-dl.exe");

            //ffmpeg
            Console.WriteLine("Downloading ffmpeg");
            if (m_systemInfo.CPUArchitecture == SystemInfo.Architecture.x86_64)
                webClient.DownloadFile("https://ffmpeg.zeranoe.com/builds/win64/static/ffmpeg-3.4.1-win64-static.zip", m_tempDiretory + @"\ffmpeg-3.4.1.zip");
            else
                webClient.DownloadFile("https://ffmpeg.zeranoe.com/builds/win64/static/ffmpeg-3.4.1-win32-static.zip", m_tempDiretory + @"\ffmpeg-3.4.1.zip");
            Console.WriteLine("Extracting ffmpeg");
            ZipFile.ExtractToDirectory(m_tempDiretory + @"\ffmpeg-3.4.1.zip", m_tempDiretory);
            File.Move(m_tempDiretory + @"\ffmpeg-3.4.1-win64-static\bin\ffmpeg.exe", m_dependencyDirectory + @"\ffmpeg.exe");
            File.Move(m_tempDiretory + @"\ffmpeg-3.4.1-win64-static\bin\ffplay.exe", m_dependencyDirectory + @"\ffplay.exe");
            File.Move(m_tempDiretory + @"\ffmpeg-3.4.1-win64-static\bin\ffprobe.exe", m_dependencyDirectory + @"\ffprobe.exe");

            //reindex
            ReadIndexData(true);

            Console.WriteLine("Setup complete, clearing tmp");
            CleanDirectory(m_tempDiretory);
        }

        private void CleanDirectory(String directory)
        {
            DirectoryInfo di = new DirectoryInfo(directory);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public static String RemoveSpecialCharacters(String str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}