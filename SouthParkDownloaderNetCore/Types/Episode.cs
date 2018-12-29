using System;
using System.Collections;
using System.IO;
using System.Linq;
using SouthParkDownloaderNetCore.Functionality;
using SouthParkDownloaderNetCore.Logic;

namespace SouthParkDownloaderNetCore.Types
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
                return FSHelper.RemoveSpecialCharacters(this.Name);
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

        public void Download(Boolean overwrite = false)
        {
            System.IO.Directory.CreateDirectory(this.SeasonDirectory); // Create directories in case they dont exist
            System.IO.Directory.CreateDirectory(this.Directory);

            if (File.Exists(this.Directory + "/dlfinish") && !overwrite)
                return; // Skip already downloaded episode

            Console.WriteLine("Start downloading episode " + this.Number + ' ' + this.Name);
            if (!YouTubeDL.Download(this.Address, this.Directory))
            {
                FSHelper.CleanDirectory(this.Directory);
                Console.WriteLine("YoutubeDL failed for some reason.");
                return;
            }
            Console.WriteLine("Finished download of episode " + this.Number);

            /* Sort video parts and rename */
            String[] files = System.IO.Directory.GetFiles(this.Directory);
            ArrayList videoParts = new ArrayList();
            foreach (String _file in files)
            {
                String extension = System.IO.Path.GetExtension(_file);
                String filename = System.IO.Path.GetFileNameWithoutExtension(_file);
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

                File.Move(_file, System.IO.Path.GetDirectoryName(_file) + "/part" + index + extension);
                videoParts.Add(_file);
            }

            File.Create(this.Directory + "/dlfinish");
#if RELEASE
            File.SetAttributes( this.Directory + "/dlfinish", File.GetAttributes( this.Directory + "/dlfinish" ) | FileAttributes.Hidden );
#endif
        }

        public void Merge()
        {
            if (!File.Exists(this.Directory + "/dlfinish"))
            {
                Console.WriteLine("No video files to merge!");
                return;
            }

            if (File.Exists(this.Directory + "/mergefinish"))
                return;

            var videoFiles = System.IO.Directory.GetFiles(this.Directory, "*.*", SearchOption.AllDirectories)
                .Where(s => System.IO.Path.GetExtension(s) == this.Extension)
                .OrderBy(x => Int32.Parse(x.Substring(x.IndexOf("part") + 4, 1)));

            /* Output parts into files.txt for ffmpeg */
            StreamWriter sw = File.CreateText(this.Directory + "/files.txt");
            foreach (String filePath in videoFiles)
            {
                sw.Write("file '" + filePath + '\'' + sw.NewLine);
            }
            sw.Close();

            if (!FFMpeg.Mux(this.Directory, this.FileName + this.Extension))
            {
                Console.WriteLine("ffmpeg failed for some reason.");
                return;
            }

            /* Delete previous data after successfull muxing */
            File.Delete(this.Directory + "/files.txt");
            foreach (String oldFile in videoFiles)
            {
                File.Delete(oldFile);
            }

            File.Create(this.Directory + "/mergefinish");
#if RELEASE
            File.SetAttributes( this.Directory + "/mergefinish", File.GetAttributes( this.Directory + "/mergefinish" ) | FileAttributes.Hidden );
#endif
            this.AddMeta();
        }

        public void AddMeta()
        {
            TagLib.File file = TagLib.File.Create(this.Path); // Change file path accordingly.
            file.Tag.Title = this.Name;
            file.Save();
        }
    }
}
