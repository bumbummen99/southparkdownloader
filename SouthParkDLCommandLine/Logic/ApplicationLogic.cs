using System;
using System.Collections;
using System.IO;
using System.Linq;
using SouthParkDLCommandLine.Functionality;
using System.Threading;
using SouthParkDLCore.Install;
using SouthParkDLCore.Database;
using SouthParkDLCore.Types;
using SouthParkDLCore.Config;
using SouthParkDLCore.Helpers.IO;

namespace SouthParkDLCommandLine.Logic
{
    class ApplicationLogic : Core.Logic
    {
        private Setup m_setup;
        private RuntimeConfig runtimeConfig;
        private EpisodeDatabase episodeDatabase;

        private ArrayList m_episodes;

        private static int workingCounter = 0;
        private static int workingLimit = 4;
        private static int processedCounter = 0;

        private static readonly Lazy<ApplicationLogic> lazy = new Lazy<ApplicationLogic>(() => new ApplicationLogic());
        public static ApplicationLogic Instance

        {
            get
            {
                return lazy.Value;
            }
        }

        private ApplicationLogic() : base("SouthParkDL")
        {
            m_episodes = new ArrayList();
            runtimeConfig = new RuntimeConfig();
        }

        override protected void BeforeRun()
        {
            /* Setup */
            m_setup = new Setup(Array.Exists<string>(m_args, element => element == "ytdlc"));

            if (!m_setup.IsSetup())
                Setup();

            if (m_episodes.Count == 0)
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
            Console.WriteLine("setup       Downloads and installs/updates the dependencies and the episode database from the GIT repository.");
            Console.WriteLine("index       Load the index file containing episodes from southpark.de");
            Console.WriteLine("  update    Updates and reindexes the index file.\n");
            Console.WriteLine("download    Download the episode from the current index.");
            Console.WriteLine("process     Merge the episode parts into single files.");
            Console.WriteLine("help        Show information about commands");
            Console.WriteLine("exit        Exits the application");
        }

        private void Download()
        {
            int checkCount = m_episodes.Count;
            foreach (Episode episode in m_episodes)
            {
                //wait for free limit...
                while (workingCounter >= workingLimit)
                {
                    Thread.Sleep(100);
                }
                workingCounter += 1;
                ParameterizedThreadStart pts = new ParameterizedThreadStart(ProcessEpisode);
                Thread th = new Thread(pts);
                th.Start(episode);
            }

            //wait for all threads to complete...
            while (processedCounter < checkCount)
            {
                Thread.Sleep(100);
            }
        }

        private void ProcessEpisode( object obj )
        {
            Episode episode = (Episode)obj;
            try
            {
                episode.Download();
                episode.Merge();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Processing episode error: " + ex.Message);
            }
            finally
            {
                Interlocked.Decrement(ref workingCounter);
                Interlocked.Increment(ref processedCounter);
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
            if (episodeDatabase != null)
                episodeDatabase.Close();

            if (update)
            {
                if (File.Exists(RuntimeConfig.Instance.m_indexFile))
                    File.Delete(RuntimeConfig.Instance.m_indexFile);
                m_setup.setUpIndex();
            }

            if (!File.Exists(RuntimeConfig.Instance.m_indexFile))
            {
                Console.WriteLine("No index data found!");
                return;
            }

            /* Setup Database */
            episodeDatabase = new EpisodeDatabase(RuntimeConfig.Instance.m_indexFile);

            /* Parse csv file */
            Episode[] results = episodeDatabase.GetAllEpisodes();

            /* Check if we have a result */
            if (results == null || results.Count() <= 0)
                return;

            /* Process services */
            foreach (Episode episode in results)
                    m_episodes.Add(episode); //Add service to internal list
            Console.WriteLine("Loaded episode index.");
        }

        private void Setup()
        {
            Console.WriteLine("Setting up dependencies.");
            DirectoryHelper.DeleteContents(RuntimeConfig.Instance.m_dependencyDirectory);
            DirectoryHelper.DeleteContents(RuntimeConfig.Instance.m_tempDiretory);

            //youtbe-dl
            Console.WriteLine("Downloading youtube-dl...");
            m_setup.setUpYoutubeDL();

            //ffmpeg
            Console.WriteLine("Downloading & extracting ffmpeg...");
            m_setup.setUpFFMpeg();

            //reindex
            Console.WriteLine("Updating episode index...");
            ReadIndexData(true);

            DirectoryHelper.DeleteContents(RuntimeConfig.Instance.m_tempDiretory);
            Console.WriteLine("Setup complete!");
        }
    }
}