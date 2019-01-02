using SouthParkDLCore.Config;
using SouthParkDLCore.Database;
using SouthParkDLCore.Helpers.IO;
using SouthParkDLCore.Install;
using SouthParkDLCore.Types;
using SouthParkDLFrontend.Functionality;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SouthParkDLFrontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Setup m_setup;
        private RuntimeConfig runtimeConfig;
        private EpisodeDatabase episodeDatabase;

        private ArrayList m_episodes;

        private static int workingCounter = 0;
        private static int workingLimit = 4;
        private static int processedCounter = 0;

        public MainWindow()
        {
            InitializeComponent();

            BtnStopDownload.Visibility = Visibility.Collapsed;
            VersionString.Text = 'v' + AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString();

            m_episodes = new ArrayList();
            runtimeConfig = new RuntimeConfig();

            Console.SetOut(new ControlWriter(ConsoleBox));

            /* Setup */
            m_setup = new Setup();

            if (!m_setup.IsSetup())
                Setup();

            if (m_episodes.Count == 0)
                ReadIndexData();
        }

        CancellationTokenSource source;
        CancellationToken token;

        private async void Button_Click(object sender, RoutedEventArgs e)
        { //Download
            source = new CancellationTokenSource();
            token = source.Token;

            BtnDownload.Visibility = Visibility.Collapsed;
            BtnStopDownload.Visibility = Visibility.Visible;

            await Task.Run(() => Download(), token);

            BtnStopDownload.Visibility = Visibility.Collapsed;
            BtnDownload.Visibility = Visibility.Visible;
        }

        private async void Button_Click_Stop(object sender, RoutedEventArgs e)
        { //Cancel download
            if (source != null)
                source.Cancel();

            BtnStopDownload.Visibility = Visibility.Collapsed;
            BtnDownload.Visibility = Visibility.Visible;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        { //Reindex
            BtnReloadIndex.IsEnabled = false;
            await Task.Run(() => ReadIndexData());
            BtnReloadIndex.IsEnabled = true;
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        { //Merge
            await Task.Run(() => Merge());
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        { //Help
            //Todo
        }

        private void Button_Click_Open(object sender, RoutedEventArgs e)
        { //Open Folder
            Process.Start(runtimeConfig.m_dataDirectory);
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
                Task.Run(() => ProcessEpisode(episode), token);
            }

            //wait for all threads to complete...
            while (processedCounter < checkCount)
            {
                Thread.Sleep(100);
            }
        }

        private void Merge()
        {
            foreach (Episode episode in m_episodes)
            {
                episode.Merge();
            }
        }

        private void ProcessEpisode(object obj)
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
            ReadIndexData();

            DirectoryHelper.DeleteContents(RuntimeConfig.Instance.m_tempDiretory);
            Console.WriteLine("Setup complete!");
        }

        private void ReadIndexData()
        {
            if (episodeDatabase != null)
                episodeDatabase.Close();

            if (File.Exists(RuntimeConfig.Instance.m_indexFile))
                File.Delete(RuntimeConfig.Instance.m_indexFile);
            m_setup.setUpIndex();

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
    }
}
