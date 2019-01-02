using SQLite;
using System;

namespace SouthParkDLCore.Database.Abstracts
{
    public abstract class Database
    {
        protected SQLiteConnection connection;

        public Database(String databaseFile)
        {
            try
            {
                this.connection = new SQLiteConnection(databaseFile);
            }
            catch (Exception e)
            {
                // Handle exception
            }
        }

        public void Close()
        {
            this.connection.Close();
        }
    }
}
