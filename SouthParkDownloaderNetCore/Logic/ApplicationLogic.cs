using System;
using System.Collections;
using System.IO;
using System.Linq;

using SouthParkDownloaderNetCore.Functionality;
using SouthParkDownloaderNetCore.Types;
using SouthParkDownloaderNetCore.Install;
using SouthParkDownloaderNetCore.Database;
using SouthParkDownloaderNetCore.Helpers;

namespace SouthParkDownloaderNetCore.Logic
{
    class ApplicationLogic : Core.Logic
    {
        private Setup m_setup;
        private EpisodeDatabase episodeDatabase;
        public String m_dependencyDirectory;
        public String m_dataDirectory;
        public String m_tempDiretory;

        public String m_indexFile
        {
            get
            {
                return m_dataDirectory + @"\data.db";
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

        private ApplicationLogic() : base("SouthParkDownloader", "1.1")
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

            /* Setup Database */
            episodeDatabase = new EpisodeDatabase(m_indexFile);

            /* Parse csv file */
            Episode[] results = episodeDatabase.GetAllEpisodes();

            /* Check if we have a result */
            if (results == null || results.Count() <= 0)
                return;

            /* Process services */
            foreach (Episode episode in results)
                    m_episodes.Add(episode); //Add service to internal list
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