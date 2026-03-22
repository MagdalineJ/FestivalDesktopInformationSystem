using System.Collections.Generic;
using FestivalDesktopInformationSystem.Models;

namespace FestivalDesktopInformationSystem.Interfaces
{
    // Defines all database operations required by the business layer.
    // This keeps SQL away from the controller and service classes.
    public interface IPersonRepository
    {
        void AddPerson(Person person);
        void UpdatePerson(Person person);
        bool DeletePerson(int personId);
        bool SoftDeletePerson(int personId);
        bool RestorePerson(int personId);

        List<Person> GetAllPeople();
        List<Person> GetPeopleByRole(string role);
        Person? GetPersonById(int personId);

        List<Person> SearchPeople(string keyword);
        List<Person> filterPeople(string filterField);

        bool EmailExists(string email);

        void SavePerformerGenres(int personId, List<int> genres);
        void SaveVendorCategories(int personId, List<int> categories);

        List<int> GetGenresByPerformerId(int personId);
        List<int> GetCategoriesByVendorId(int personId);
        List<Genre> GetAllGenres();
        List<Category> GetAllCategories();
    }
}