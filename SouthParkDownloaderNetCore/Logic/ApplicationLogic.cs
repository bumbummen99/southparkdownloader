using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;

using TinyCsvParser;

using SouthParkDownloaderNetCore.Functionality;
using SouthParkDownloaderNetCore.Types;
using SouthParkDownloaderNetCore.CSVMappings;
using SouthParkDownloaderNetCore.Install;

namespace SouthParkDownloaderNetCore.Logic
{
    class ApplicationLogic : Core.Logic
    {
        private Setup m_setup;
        public String m_dependencyDirectory;
        public String m_dataDirectory;
        public String m_tempDiretory;

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
            /* Setup */
            m_setup = new Setup(this);

            /* Setup folder structures */
            m_setup.setUpFolderStructure();

            m_episodes = new ArrayList();

            if (!m_setup.IsSetup())
                Setup();

            if (m_setup.HasIndex())
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
                episode.Download();
                episode.Merge();
            }
        }

        private void Merge()
        {
            foreach (Episode episode in m_episodes)
            {
                episode.Merge();
            }
        }

        private void ReadIndexData(Boolean update = false)
        {
            if (update)
            {
                if (File.Exists(m_indexFile))
                    File.Delete(m_indexFile);
                m_setup.setUpIndex();
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

        private void Setup()
        {
            FSHelper.CleanDirectory(m_dependencyDirectory);
            FSHelper.CleanDirectory(m_tempDiretory);

            //youtbe-dl
            Console.WriteLine("Downloading youtube-dl");
            m_setup.setUpYoutubeDL();

            //ffmpeg
            Console.WriteLine("Downloading & extracting ffmpeg");
            m_setup.setUpFFMpeg();

            //reindex
            ReadIndexData(true);

            Console.WriteLine("Setup complete, clearing tmp directory");
            FSHelper.CleanDirectory(m_tempDiretory);
        }
    }
}