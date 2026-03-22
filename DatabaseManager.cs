using MySql.Data.MySqlClient;

namespace FestivalDesktopInformationSystem
{
    // Responsible for creating and testing database connections.
    // No business logic placed here.
    public class DatabaseManager
    {
        private readonly string _connectionString;

        public DatabaseManager()
        {
            _connectionString = DbConfig.ConnectionString;
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public bool TestConnection()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}