using SouthParkDownloaderNetCore.Database.Models;
using SouthParkDownloaderNetCore.Types;
using SQLite;
using System;

namespace SouthParkDownloaderNetCore.Database
{
    class EpisodeDatabase
    {
        private SQLiteConnection connection;

        public EpisodeDatabase( String databasePath )
        {
            this.connection = new SQLiteConnection(databasePath);
        }

        public Episode[] GetAllEpisodes()
        {
            return Array.ConvertAll(connection.Table<Episodes>().ToArray(), item => (Episode)item);
        }

    }
}
