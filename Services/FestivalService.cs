using System;
using System.Collections.Generic;
using System.Linq;
using FestivalDesktopInformationSystem.Helpers;
using FestivalDesktopInformationSystem.Interfaces;
using FestivalDesktopInformationSystem.Models;

namespace FestivalDesktopInformationSystem.Services
{
    // Main business layer for the festival system.
    // This class applies validation rules and delegates persistence to the repository.
    public class FestivalService
    {
        private readonly IPersonRepository _repository;
        private readonly ValidationHelper _validator;
        private readonly CsvExporter _csvExporter;
        private readonly ReportService _reportService;

        public FestivalService(
            IPersonRepository repository,
            ValidationHelper validator,
            CsvExporter csvExporter,
            ReportService reportService)
        {
            _repository = repository;
            _validator = validator;
            _csvExporter = csvExporter;
            _reportService = reportService;
        }

        public string AddPerformer(
            string name,
            string telephone,
            string email,
            decimal fee,
            List<int> genres)
        {
            if (!_validator.ValidateName(name))
                return "Error: Invalid name.";

            if (!_validator.ValidateTelephone(telephone))
                return "Error: Invalid telephone.";

            if (!_validator.ValidateEmail(email))
                return "Error: Invalid email format.";

            if (!_validator.ValidateDecimal(fee))
                return "Error: Fee cannot be negative.";

            if (!_validator.ValidateGenres(genres))
                return "Error: Performer must have at least one valid genre.";

            if (_repository.EmailExists(email))
                return "Error: Email already exists. A new person cannot be added with a duplicate email.";

            var performer = new Performer(0, name, telephone, email, fee, genres);

            if (!performer.Validate())
                return "Error: Performer validation failed.";

            _repository.AddPerson(performer);
            return "Performer added successfully.";
        }

        public string AddCrew(
            string name,
            string telephone,
            string email,
            decimal hourlyRate,
            string employmentType,
            int weeklyHours)
        {
            if (!_validator.ValidateName(name))
                return "Error: Invalid name.";

            if (!_validator.ValidateTelephone(telephone))
                return "Error: Invalid telephone.";

            if (!_validator.ValidateEmail(email))
                return "Error: Invalid email format.";

            if (!_validator.ValidateDecimal(hourlyRate))
                return "Error: Hourly rate cannot be negative.";

            if (!_validator.ValidateEmploymentType(employmentType))
                return "Error: Employment type must be FullTime or PartTime.";

            if (!_validator.ValidateWeeklyHours(employmentType, weeklyHours))
                return "Error: Weekly hours do not match the employment type rules.";

            if (_repository.EmailExists(email))
                return "Error: Email already exists. A new person cannot be added with a duplicate email.";

            var crew = new Crew(0, name, telephone, email, hourlyRate, employmentType, weeklyHours);

            if (!crew.Validate())
                return "Error: Crew validation failed.";

            _repository.AddPerson(crew);
            return "Crew member added successfully.";
        }

        public string AddVendor(
            string name,
            string telephone,
            string email,
            string stallName,
            List<int> categories)
        {
            if (!_validator.ValidateName(name))
                return "Error: Invalid name.";

            if (!_validator.ValidateTelephone(telephone))
                return "Error: Invalid telephone.";

            if (!_validator.ValidateEmail(email))
                return "Error: Invalid email format.";

            if (string.IsNullOrWhiteSpace(stallName))
                return "Error: Stall name cannot be empty.";

            if (!_validator.ValidateCategories(categories))
                return "Error: Vendor must have at least one valid category.";

            if (_repository.EmailExists(email))
                return "Error: Email already exists. A new person cannot be added with a duplicate email.";

            var vendor = new Vendor(0, name, telephone, email, stallName, categories);

            if (!vendor.Validate())
                return "Error: Vendor validation failed.";

            _repository.AddPerson(vendor);
            return "Vendor added successfully.";
        }

        public List<Person> GetAllPeople()
        {
            return _repository.GetAllPeople();
        }

        public List<Person> GetPeopleByRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return new List<Person>();

            return _repository.GetPeopleByRole(role);
        }

        public List<Person> SearchPeople(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<Person>();

            return _repository.SearchPeople(keyword.Trim());
        }

        public List<Person> filterPeople(string filterField)
        {
            var validFields = new List<string> { "personid", "name", "email", "role" };

            if (!validFields.Contains(filterField.ToLower()))
                throw new ArgumentException("Invalid filter field.");

            return _repository.filterPeople(filterField);
        }

        public string EditPerson(Person updatedPerson)
        {
            if (updatedPerson == null)
                return "Error: Updated person data is missing.";

            var existingPerson = _repository.GetPersonById(updatedPerson.GetPersonId());

            if (existingPerson == null)
                return "Error: Person not found.";

            if (!_validator.ValidateName(updatedPerson.GetName()))
                return "Error: Invalid name.";

            if (!_validator.ValidateTelephone(updatedPerson.GetTelephone()))
                return "Error: Invalid telephone.";

            if (!_validator.ValidateEmail(updatedPerson.GetEmail()))
                return "Error: Invalid email format.";

            var sameEmailOwnedByAnother = _repository
                .GetAllPeople()
                .Any(p => p.GetEmail().Equals(updatedPerson.GetEmail(), StringComparison.OrdinalIgnoreCase)
                       && p.GetPersonId() != updatedPerson.GetPersonId());

            if (sameEmailOwnedByAnother)
                return "Error: Another record already uses that email.";

            switch (updatedPerson)
            {
                case Performer performer:
                    if (!_validator.ValidateDecimal(performer.GetFee()))
                        return "Error: Invalid performer fee.";

                    if (!_validator.ValidateGenres(performer.GetGenres()))
                        return "Error: Invalid performer genres.";
                    break;

                case Crew crew:
                    if (!_validator.ValidateDecimal(crew.GetHourlyRate()))
                        return "Error: Invalid crew hourly rate.";

                    if (!_validator.ValidateEmploymentType(crew.GetEmploymentType()))
                        return "Error: Invalid employment type.";

                    if (!_validator.ValidateWeeklyHours(crew.GetEmploymentType(), crew.GetWeeklyHours()))
                        return "Error: Invalid weekly hours for employment type.";
                    break;

                case Vendor vendor:
                    if (string.IsNullOrWhiteSpace(vendor.GetStallName()))
                        return "Error: Invalid stall name.";

                    if (!_validator.ValidateCategories(vendor.GetCategories()))
                        return "Error: Invalid vendor categories.";
                    break;

                default:
                    return "Error: Unsupported person type.";
            }

            _repository.UpdatePerson(updatedPerson);
            return "Person updated successfully.";
        }

        public string DeletePerson(int personId)
        {
            bool deleted = _repository.DeletePerson(personId);
            return deleted ? "Person deleted successfully." : "Person not found.";
        }

        public string SoftDeletePerson(int personId)
        {
            bool deleted = _repository.SoftDeletePerson(personId);
            return deleted ? "Person soft-deleted successfully." : "Person not found.";
        }

        public string RestorePerson(int personId)
        {
            bool restored = _repository.RestorePerson(personId);
            return restored ? "Person restored successfully." : "Person not found.";
        }

        public void ExportPeopleToCsv(string filePath)
        {
            var people = _repository.GetAllPeople();

            var genreMap = _repository.GetAllGenres()
                .ToDictionary(g => g.GenreId, g => g.GenreName);

            var categoryMap = _repository.GetAllCategories()
                .ToDictionary(c => c.CategoryId, c => c.CategoryName);

            _csvExporter.ExportPeople(filePath, people, genreMap, categoryMap);
        }

        public string GenerateRoleReport()
        {
            var people = _repository.GetAllPeople();
            return _reportService.GeneratePeopleCountByRole(people);
        }

        public string GenerateCostReport()
        {
            var people = _repository.GetAllPeople();
            var genres = _repository.GetAllGenres();
            var categories = _repository.GetAllCategories();

            string performerFeeReport = _reportService.GeneratePerformerFeeReport(people, genres);
            string crewCostReport = _reportService.GenerateCrewCostReport(people);
            string vendorCategoryReport = _reportService.GenerateVendorCategoryReport(people, categories);

            return performerFeeReport
                   + Environment.NewLine
                   + crewCostReport
                   + Environment.NewLine
                   + vendorCategoryReport;
        }

        public List<Genre> GetAllGenres()
        {
            return _repository.GetAllGenres();
        }

        public List<Category> GetAllCategories()
        {
            return _repository.GetAllCategories();
        }

        public Person? GetPersonById(int personId)
        {
            return _repository.GetPersonById(personId);
        }
    }
}