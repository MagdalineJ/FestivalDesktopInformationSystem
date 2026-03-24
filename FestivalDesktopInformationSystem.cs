namespace FestivalDesktopInformationSystem
{
    // Stores all database connection settings in one place.
    // This makes it easier to update the connection later.
    public static class DbConfig
    {
        public static string Server => "localhost";
        public static string Database => "festival_operations";
        public static string User => "root";
        public static string Password => "";

        public const string ConnectionString = 
        "Server=localhost;Port=3306;Database=Festival_desktop_information_system;" + "Uid=root;SslMode=Disabled;";
    }
}