using Microsoft.Data.Sqlite;
using System.IO;

namespace Armonia.App.Data
{
    public class LibraryDbContext
    {
        private readonly string _dbPath;

        public LibraryDbContext(string dbDirectory)
        {
            _dbPath = Path.Combine(dbDirectory, "Library.sqlite");
            Initialize();
        }

        private void Initialize()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Media (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FilePath TEXT,
                Type TEXT,
                Duration REAL,
                Tags TEXT,
                ImportedOn TEXT
            );";
            cmd.ExecuteNonQuery();
        }
    }
}
