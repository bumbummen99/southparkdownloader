using System;
using System.Diagnostics;
using SouthParkDownloader.Logic;

namespace SouthParkDownloader.Types
{
    class Episode
    {
        public UInt16 Season { get; set; }
        public UInt16 Number { get; set; }
        public String Address { get; set; }
        public String Name { get; set; }

        public String FileName
        {
            get
            {
                return 'S' + Season + '-' + 'E' + Number + ' ' + Name.Replace('\'', ' ').Replace('"', ' ').Replace("  ", " ");
            }
        }

        public String Extension
        {
            get
            {
                return ".mp4";
            }
        }

        public String SeasonDirectory
        {
            get
            {
                return ApplicationLogic.Instance.m_dataDirectory + '/' + Convert.ToString((int)this.Season);
            }
        }
        

        public String Directory
        {
            get {
                return this.SeasonDirectory + '/' + Convert.ToString((int)this.Number);
            }
            
        }

        public String Path
        {
            get
            {
                return this.Directory + '/' + this.FileName + this.Extension;
            }
        }

        public Boolean YTDL()
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = ApplicationLogic.Instance.m_youtubeDL;
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = this.Directory;
            startInfo.Arguments = this.Address;
            process.StartInfo = startInfo;

            Console.WriteLine("Start downloading Episode " + this.Number + ' ' + this.Name);
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.WriteLine("YoutubeDL failed for some reason.");
                return false;
            }

            return true;
        }
    }
}
