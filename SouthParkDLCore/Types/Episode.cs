using System;
using System.Collections;
using System.IO;
using System.Linq;
using SouthParkDLCore.Commands.Executables;
using SouthParkDLCore.Config;
using SouthParkDLCore.Helpers.IO;

namespace SouthParkDLCore.Types
{
    public class Episode
    {
        public UInt16 Season { get; set; }
        public UInt16 Number { get; set; }
        public String Address { get; set; }
        public String Name { get; set; }

        public String FileName
        {
            get
            {
                return 'S'+Season.ToString()+'E'+Number.ToString() + '-' + FileHelper.EscapeFileName(this.Name);
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
                return RuntimeConfig.Instance.m_dataDirectory + '/' + Convert.ToString((int)this.Season);
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

        public String ConsoleTag
        {
            get
            {
                return "[S" + Season.ToString() + 'E' + Number.ToString() + ']';
            }
        }

        public bool Download(Boolean overwrite = false)
        {
            System.IO.Directory.CreateDirectory(this.SeasonDirectory); // Create directories in case they dont exist
            System.IO.Directory.CreateDirectory(this.Directory);

            if (File.Exists(this.Directory + "/dlfinish") && !overwrite) {
                Console.WriteLine(ConsoleTag + " Already downloaded " + this.Number + ' ' + this.Name);
                return false; // Skip already downloaded episode
            }
            Console.WriteLine(ConsoleTag + " Starting download " + this.Number + ' ' + this.Name);
            YouTubeDL ytdl = new YouTubeDL(this.Directory);
            ytdl.Download(this.Address);
            /*
             * TODO: Return values of run method not working correctly
            if (!ytdl.Download(this.Address))
            {
                DirectoryHelper.DeleteContents(this.Directory);
                Console.WriteLine(ConsoleTag + " YoutubeDL failed.");
                return;
            }
            */
            Console.WriteLine(ConsoleTag + " Finished download");

            SortDownloadedParts();

            File.Create(this.Directory + "/dlfinish");
#if RELEASE
            File.SetAttributes( this.Directory + "/dlfinish", File.GetAttributes( this.Directory + "/dlfinish" ) | FileAttributes.Hidden );
#endif      
            return true;
        }

        private void SortDownloadedParts()
        {
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
                foreach (String partNumberString in new String[] { "Comedy Central S" })
                {
                    if (filename.Contains(partNumberString))
                    {
                        index = Int32.Parse(filename.Substring(filename.IndexOf(partNumberString) + partNumberString.Length, 1));
                        break;
                    }
                }

                File.Move(_file, System.IO.Path.GetDirectoryName(_file) + "/part" + index + extension);
                videoParts.Add(_file);
            }
        }

        public void Merge()
        {
            if (!File.Exists(this.Directory + "/dlfinish"))
            {
                Console.WriteLine(ConsoleTag + " No video files to merge!");
                return;
            }

            if (File.Exists(this.Directory + "/mergefinish"))
                return;

            var videoFiles = System.IO.Directory.GetFiles(this.Directory, "*.*", SearchOption.AllDirectories)
                .Where(s => System.IO.Path.GetExtension(s) == this.Extension)
                .OrderBy(x => Int32.Parse(x.Substring(x.IndexOf("part") + 4, 1)))
                .ToArray<string>();

            Console.WriteLine(ConsoleTag + " Start muxing");
            FFMpeg ffmpeg = new FFMpeg(this.Directory);
            ffmpeg.Mux(videoFiles, this.FileName + this.Extension);
            /*
             * TODO: Return values of run method not working correctly
            if (!ffmpeg.Mux(videoFiles, this.FileName + this.Extension))
            {
                Console.WriteLine(ConsoleTag + " ffmpeg failed for some reason.");
                return;
            }
            */
            Console.WriteLine(ConsoleTag + " Successfully muxed episode to \"" + Path + '"');

            /* Delete video parts after successfully muxing */
            foreach (String oldFile in videoFiles)
                File.Delete(oldFile);

            File.Create(this.Directory + "/mergefinish");
#if RELEASE
            File.SetAttributes( this.Directory + "/mergefinish", File.GetAttributes( this.Directory + "/mergefinish" ) | FileAttributes.Hidden );
#endif
            this.AddMeta();
        }

        public void AddMeta()
        {
            Console.WriteLine(ConsoleTag + " Adding meta information.");
            TagLib.File file = TagLib.File.Create(this.Path); // Change file path accordingly.
            file.Tag.Title = this.Name;
            file.Save();
            Console.WriteLine(ConsoleTag + " Meta information complete.");
        }
    }
}
