using System;
using SouthParkDLCore.Database.Models;
using SouthParkDLCore.Types;

namespace SouthParkDLCore.Database
{
    public class EpisodeDatabase : Abstracts.Database
    {
        public EpisodeDatabase( String databaseFile ) : base(databaseFile)
        {
            //
        }

        public Episode[] GetAllEpisodes()
        {
            return Array.ConvertAll(connection.Table<Episodes>().ToArray(), item => (Episode)item);
        }

    }
}
