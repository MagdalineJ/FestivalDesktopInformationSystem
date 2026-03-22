namespace FestivalDesktopInformationSystem.Models
{
    // Represents a genre entity.
    // This class is useful because your updated design uses IDs.
    public class Genre
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; }

        public Genre()
        {
            GenreName = string.Empty;
        }

        public Genre(int genreId, string genreName)
        {
            GenreId = genreId;
            GenreName = genreName;
        }

        public override string ToString()
        {
            return $"{GenreId} - {GenreName}";
        }
    }
}