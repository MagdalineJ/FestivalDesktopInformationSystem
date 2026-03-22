using System;
using System.Collections.Generic;
using System.Linq;
using FestivalDesktopInformationSystem.Models;

namespace FestivalDesktopInformationSystem.Views
{
    // Concrete console-based implementation of the festival view.
    // This class is responsible only for input and output.
    public class ConsoleView : IFestivalView
    {
        public void DisplayMenu()
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("   FESTIVAL DESKTOP INFORMATION SYSTEM");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Add new record");
            Console.WriteLine("2. View all records");
            Console.WriteLine("3. View records by role");
            Console.WriteLine("4. Search people");
            Console.WriteLine("5. Sort people");
            Console.WriteLine("6. Edit person");
            Console.WriteLine("7. Delete person");
            Console.WriteLine("8. Soft delete person");
            Console.WriteLine("9. Restore person");
            Console.WriteLine("10. Export to CSV");
            Console.WriteLine("11. Generate reports");
            Console.WriteLine("0. Exit");
            Console.WriteLine("========================================");
        }

        public void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void ShowError(string message)
        {
            Console.WriteLine($"Error: {message}");
        }

        public string GetInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }

        public int GetIntInput(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int result))
                    return result;

                Console.WriteLine("Invalid number. Please enter a whole number.");
            }
        }

        public decimal GetDecimalInput(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();

                if (decimal.TryParse(input, out decimal result))
                    return result;

                Console.WriteLine("Invalid decimal value. Please try again.");
            }
        }

        public bool GetConfirmation(string prompt)
        {
            Console.Write($"{prompt} (Y/N): ");
            string response = (Console.ReadLine() ?? string.Empty).Trim().ToUpper();
            return response == "Y" || response == "YES";
        }

        public void DisplayPeople(
            List<Person> people,
            Dictionary<int, string> genreMap,
            Dictionary<int, string> categoryMap)
        {
            Console.WriteLine();

            if (people == null || people.Count == 0)
            {
                Console.WriteLine("No records found.");
                return;
            }

            Console.WriteLine("============== RECORDS ==============");

            foreach (var person in people)
            {
                if (person is Performer performer)
                {
                    var genreNames = performer.GetGenres()
                        .Where(id => genreMap.ContainsKey(id))
                        .Select(id => genreMap[id])
                        .ToList();

                    Console.WriteLine(
                        $"ID: {performer.GetPersonId()} | " +
                        $"Name: {performer.GetName()} | " +
                        $"Telephone: {performer.GetTelephone()} | " +
                        $"Email: {performer.GetEmail()} | " +
                        $"Role: {performer.GetRole()} | " +
                        $"Deleted: {performer.GetIsDeleted()} | " +
                        $"Fee: {performer.GetFee():C} | " +
                        $"Genres: {string.Join(", ", genreNames)}"
                    );
                }
                else if (person is Crew crew)
                {
                    Console.WriteLine(
                        $"ID: {crew.GetPersonId()} | " +
                        $"Name: {crew.GetName()} | " +
                        $"Telephone: {crew.GetTelephone()} | " +
                        $"Email: {crew.GetEmail()} | " +
                        $"Role: {crew.GetRole()} | " +
                        $"Deleted: {crew.GetIsDeleted()} | " +
                        $"Hourly Rate: {crew.GetHourlyRate():C} | " +
                        $"Employment Type: {crew.GetEmploymentType()} | " +
                        $"Weekly Hours: {crew.GetWeeklyHours()} | " +
                        $"Weekly Pay: {crew.CalculateWeeklyPay():C}"
                    );
                }
                else if (person is Vendor vendor)
                {
                    var categoryNames = vendor.GetCategories()
                        .Where(id => categoryMap.ContainsKey(id))
                        .Select(id => categoryMap[id])
                        .ToList();

                    Console.WriteLine(
                        $"ID: {vendor.GetPersonId()} | " +
                        $"Name: {vendor.GetName()} | " +
                        $"Telephone: {vendor.GetTelephone()} | " +
                        $"Email: {vendor.GetEmail()} | " +
                        $"Role: {vendor.GetRole()} | " +
                        $"Deleted: {vendor.GetIsDeleted()} | " +
                        $"Stall Name: {vendor.GetStallName()} | " +
                        $"Categories: {string.Join(", ", categoryNames)}"
                    );
                }
                else
                {
                    Console.WriteLine(person.DisplayInfo());
                }
            }

            Console.WriteLine("=====================================");
        }

        public void DisplayReport(string report)
        {
            Console.WriteLine();
            Console.WriteLine(report);
        }

        public void DisplayGenres(List<Genre> genres)
        {
            Console.WriteLine();
            Console.WriteLine("Available Genres:");

            foreach (var genre in genres)
            {
                Console.WriteLine($"{genre.GenreId} - {genre.GenreName}");
            }
        }

        public void DisplayCategories(List<Category> categories)
        {
            Console.WriteLine();
            Console.WriteLine("Available Categories:");

            foreach (var category in categories)
            {
                Console.WriteLine($"{category.CategoryId} - {category.CategoryName}");
            }
        }
    }
}