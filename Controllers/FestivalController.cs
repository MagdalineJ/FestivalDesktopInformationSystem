using System;
using System.Collections.Generic;
using System.Linq;
using FestivalDesktopInformationSystem.Models;
using FestivalDesktopInformationSystem.Services;
using FestivalDesktopInformationSystem.Views;

namespace FestivalDesktopInformationSystem.Controllers
{
    // Main controller for the console application.
    // It handles menu actions and coordinates between the view and service layer.
    public class FestivalController
    {
        private readonly FestivalService _service;
        private readonly IFestivalView _view;

        public FestivalController(FestivalService service, IFestivalView view)
        {
            _service = service;
            _view = view;
        }

        public void Start()
        {
            // Start() - Runs the main application loop
            bool running = true;

            while (running)
            {
                try
                {
                    _view.DisplayMenu();
                    int choice = _view.GetIntInput("Choose an option: ");
                    running = HandleMenuChoice(choice);
                }
                catch (Exception ex)
                {
                    _view.ShowError(ex.Message);
                }
            }
        }

        private bool HandleMenuChoice(int choice)
        {
            // HandleMenuChoice() - Processes menu selection and triggers corresponding actions
            switch (choice)
            {
                case 1:
                    AddPerson();
                    return true;
                case 2:
                    ViewAllPeople();
                    return true;
                case 3:
                    ViewPeopleByRole();
                    return true;
                case 4:
                    SearchPeople();
                    return true;
                case 5:
                    FilterPeople();
                    return true;
                case 6:
                    EditPerson();
                    return true;
                case 7:
                    DeletePerson();
                    return true;
                case 8:
                    SoftDeletePerson();
                    return true;
                case 9:
                    RestorePerson();
                    return true;
                case 10:
                    ExportToCsv();
                    return true;
                case 11:
                    GenerateReports();
                    return true;
                case 0:
                    _view.ShowMessage("You have exited the Festival Desktop Information System...");
                    return false;
                default:
                    _view.ShowError("Invalid menu choice.");
                    return true;
            }
        }

        private void AddPerson()
        {
            // AddPerson() - Displays role options and routes to specific add methods
            _view.ShowMessage("\nAdd New Record");
            _view.ShowMessage("1. Performer");
            _view.ShowMessage("2. Crew");
            _view.ShowMessage("3. Vendor");

            int roleChoice = _view.GetIntInput("Select type: ");

            switch (roleChoice)
            {
                case 1:
                    AddPerformer();
                    break;
                case 2:
                    AddCrew();
                    break;
                case 3:
                    AddVendor();
                    break;
                default:
                    _view.ShowError("Invalid role choice.");
                    break;
            }
        }

        private void AddPerformer()
        {
            // AddPerformer() - Collects performer input and sends data to service
            string name = _view.GetInput("Name: ");
            string telephone = _view.GetInput("Telephone: ");
            string email = _view.GetInput("Email: ");
            decimal fee = _view.GetDecimalInput("Fee: ");

            var genres = _service.GetAllGenres();
            _view.DisplayGenres(genres);

            string genreInput = _view.GetInput("Enter genre IDs separated by commas: ");
            List<int> selectedGenreIds = ParseIdList(genreInput);

            string result = _service.AddPerformer(name, telephone, email, fee, selectedGenreIds);
            _view.ShowMessage(result);
        }

        private void AddCrew()
        {
            // AddCrew - Collects crew input and sends data to service
            string name = _view.GetInput("Name: ");
            string telephone = _view.GetInput("Telephone: ");
            string email = _view.GetInput("Email: ");
            decimal hourlyRate = _view.GetDecimalInput("Hourly Rate: ");
            string employmentType = _view.GetInput("Employment Type (FullTime/PartTime): ");
            int weeklyHours = _view.GetIntInput("Weekly Hours: ");

            string result = _service.AddCrew(
                name,
                telephone,
                email,
                hourlyRate,
                employmentType,
                weeklyHours);

            _view.ShowMessage(result);
        }

        private void AddVendor()
        {   
            // AddVendor() - Collects vendor input and sends data to service
            string name = _view.GetInput("Contact Name: ");
            string telephone = _view.GetInput("Telephone: ");
            string email = _view.GetInput("Email: ");
            string stallName = _view.GetInput("Stall Name: ");

            var categories = _service.GetAllCategories();
            _view.DisplayCategories(categories);

            string categoryInput = _view.GetInput("Enter category IDs separated by commas: ");
            List<int> selectedCategoryIds = ParseIdList(categoryInput);

            string result = _service.AddVendor(name, telephone, email, stallName, selectedCategoryIds);
            _view.ShowMessage(result);
        }

        private void ViewAllPeople()
        {   
            // ViewAllPeople() - Retrieves and displays all records
            var people = _service.GetAllPeople();
            _view.DisplayPeople(people, GetGenreMap(), GetCategoryMap());
        }

        private void ViewPeopleByRole()
        {
            // ViewPeopleByRole - Displays records Filtered by role
            string role = _view.GetInput("Enter role (Performer/Crew/Vendor): ");
            var people = _service.GetPeopleByRole(role);
            _view.DisplayPeople(people, GetGenreMap(), GetCategoryMap());
        }

        private void SearchPeople()
        {   
            // SearchPeople - Searches records based on keyword input
            string keyword = _view.GetInput("Enter name, email, phone or role keyword: ");
            var results = _service.SearchPeople(keyword);
            _view.DisplayPeople(results, GetGenreMap(), GetCategoryMap());
        }

        private void FilterPeople()
        {   
            // FilterPeople - Filters records based on selected field
            string FilterField = _view.GetInput("Filter by (personId/name/email/role): ").ToLower();

            List<string> validFields = new List<string> { "personid", "name", "email", "role" };

            if (!validFields.Contains(FilterField))
            {
                _view.ShowError("Invalid Filter option. Please choose personId, name, email or role.");
                return;
            }

            var results = _service.FilterPeople(FilterField);
            _view.DisplayPeople(results, GetGenreMap(), GetCategoryMap());
        }

        private void EditPerson()
        {   
            // EditPerson - Updates an existing person’s details
            int personId = _view.GetIntInput("Enter Person ID to edit: ");
            Person? existingPerson = _service.GetPersonById(personId);

            if (existingPerson == null)
            {
                _view.ShowError("Person not found.");
                return;
            }

            _view.ShowMessage($"Editing {existingPerson.GetRole()} record.");
            _view.ShowMessage("Enter updated values.");

            string name = _view.GetInput("Name: ");
            string telephone = _view.GetInput("Telephone: ");
            string email = _view.GetInput("Email: ");

            Person updatedPerson;

            if (existingPerson is Performer)
            {
                decimal fee = _view.GetDecimalInput("Fee: ");

                var genres = _service.GetAllGenres();
                _view.DisplayGenres(genres);

                string genreInput = _view.GetInput("Enter genre IDs separated by commas: ");
                List<int> selectedGenreIds = ParseIdList(genreInput);

                updatedPerson = new Performer(personId, name, telephone, email, fee, selectedGenreIds, existingPerson.GetIsDeleted());
            }
            else if (existingPerson is Crew)
            {
                decimal hourlyRate = _view.GetDecimalInput("Hourly Rate: ");
                string employmentType = _view.GetInput("Employment Type (FullTime/PartTime): ");
                int weeklyHours = _view.GetIntInput("Weekly Hours: ");

                updatedPerson = new Crew(personId, name, telephone, email, hourlyRate, employmentType, weeklyHours, existingPerson.GetIsDeleted());
            }
            else if (existingPerson is Vendor)
            {
                string stallName = _view.GetInput("Stall Name: ");

                var categories = _service.GetAllCategories();
                _view.DisplayCategories(categories);

                string categoryInput = _view.GetInput("Enter category IDs separated by commas: ");
                List<int> selectedCategoryIds = ParseIdList(categoryInput);

                updatedPerson = new Vendor(personId, name, telephone, email, stallName, selectedCategoryIds, existingPerson.GetIsDeleted());
            }
            else
            {
                _view.ShowError("Unsupported person type.");
                return;
            }

            string result = _service.EditPerson(updatedPerson);
            _view.ShowMessage(result);
        }

        private void DeletePerson()
        {   
            // DeletePerson() - Permanently deletes a person record
            int personId = _view.GetIntInput("Enter Person ID to delete permanently: ");
            bool confirmed = _view.GetConfirmation("Are you sure you want to permanently delete this record?");

            if (!confirmed)
            {
                _view.ShowMessage("Delete cancelled.");
                return;
            }

            string result = _service.DeletePerson(personId);
            _view.ShowMessage(result);
        }

        private void SoftDeletePerson()
        {   
            // SoftDeletePerson() - Marks a person as deleted without removing data
            int personId = _view.GetIntInput("Enter Person ID to soft delete: ");
            bool confirmed = _view.GetConfirmation("Are you sure you want to soft delete this record?");

            if (!confirmed)
            {
                _view.ShowMessage("Soft delete cancelled.");
                return;
            }

            string result = _service.SoftDeletePerson(personId);
            _view.ShowMessage(result);
        }

        private void RestorePerson()
        {   
            // RestorePerson - Restores a soft-deleted record
            int personId = _view.GetIntInput("Enter Person ID to restore: ");
            string result = _service.RestorePerson(personId);
            _view.ShowMessage(result);
        }

        private void ExportToCsv()
        {   
            // ExportToCsv - Exports all records to a CSV file
            string filePath = _view.GetInput("Enter CSV file path (example: festival_people.csv): ");

            try
            {
                _service.ExportPeopleToCsv(filePath);
                _view.ShowMessage("CSV export completed successfully.");
            }
            catch (Exception ex)
            {
                _view.ShowError($"CSV export failed: {ex.Message}");
            }
        }

        private void GenerateReports()
        {   
            // GenerateReports() - Displays available reports and executes selected report
            _view.ShowMessage("\nReports");
            _view.ShowMessage("1. Role report");
            _view.ShowMessage("2. Cost report");

            int reportChoice = _view.GetIntInput("Choose report type: ");

            switch (reportChoice)
            {
                case 1:
                    _view.DisplayReport(_service.GenerateRoleReport());
                    break;
                case 2:
                    _view.DisplayReport(_service.GenerateCostReport());
                    break;
                default:
                    _view.ShowError("Invalid report choice.");
                    break;
            }
        }

        private List<int> ParseIdList(string input)
        {   
            // ParseIdList() - Converts comma-separated input into a list of integers
            if (string.IsNullOrWhiteSpace(input))
                return new List<int>();

            return input
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim())
                .Where(value => int.TryParse(value, out _))
                .Select(int.Parse)
                .Distinct()
                .ToList();
        }

        private Dictionary<int, string> GetGenreMap()
        {   
            // GetGenreMap() - Builds a dictionary of genre IDs to names
            return _service.GetAllGenres()
                .ToDictionary(g => g.GenreId, g => g.GenreName);
        }

        private Dictionary<int, string> GetCategoryMap()
        {
            // GetCategoryMap() - Builds a dictionary of category IDs to names
            return _service.GetAllCategories()
                .ToDictionary(c => c.CategoryId, c => c.CategoryName);
        }
    }
}