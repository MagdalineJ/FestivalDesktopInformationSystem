using System.Collections.Generic;
using FestivalDesktopInformationSystem.Models;

namespace FestivalDesktopInformationSystem.Views
{
    // Defines all console interaction methods used by the controller.
    // This keeps the controller independent from a specific UI implementation.
    public interface IFestivalView
    {
        void DisplayMenu();
        void ShowMessage(string message);
        void ShowError(string message);

        string GetInput(string prompt);
        int GetIntInput(string prompt);
        decimal GetDecimalInput(string prompt);
        bool GetConfirmation(string prompt);

        // Displays people using lookup dictionaries so genre/category names
        // can be shown instead of only numeric IDs.
        void DisplayPeople(
            List<Person> people,
            Dictionary<int, string> genreMap,
            Dictionary<int, string> categoryMap);

        void DisplayReport(string report);

        void DisplayGenres(List<Genre> genres);
        void DisplayCategories(List<Category> categories);
    }
}