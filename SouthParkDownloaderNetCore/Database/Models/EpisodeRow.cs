using SouthParkDownloaderNetCore.Types;
using System;

namespace SouthParkDownloaderNetCore.Database.Models
{
    class Episodes
    {
        public int season { get; set; }
        public int episode { get; set; }
        public string url { get; set; }
        public string name { get; set; }

        public static implicit operator Episode(Episodes row)
        {
            Episode n = new Episode();
            n.Season = (UInt16)row.season;
            n.Number = (UInt16)row.episode;
            n.Address = row.url;
            n.Name = row.name;

            return n;
        }
    }
}
