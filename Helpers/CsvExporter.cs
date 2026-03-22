using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FestivalDesktopInformationSystem.Models;

namespace FestivalDesktopInformationSystem.Helpers
{
    // Exports person data into a CSV file.
    public class CsvExporter
    {
        public void ExportPeople(
            string filePath,
            List<Person> people,
            Dictionary<int, string> genreMap,
            Dictionary<int, string> categoryMap)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty.");

            if (people == null)
                throw new ArgumentNullException(nameof(people));

            var lines = new List<string>
            {
                "PersonID,Name,Telephone,Email,Role,Deleted,RoleDetails"
            };

            foreach (var person in people)
            {
                string roleDetails = "";

                if (person is Performer performer)
                {
                    var genreNames = performer.GetGenres()
                        .Where(id => genreMap.ContainsKey(id))
                        .Select(id => genreMap[id]);

                    roleDetails =
                        $"Fee: {performer.GetFee():C} | Genres: {string.Join(" ", genreNames)}";
                }
                else if (person is Crew crew)
                {
                    roleDetails =
                        $"Hourly Rate: {crew.GetHourlyRate():C} | " +
                        $"Employment Type: {crew.GetEmploymentType()} | " +
                        $"Weekly Hours: {crew.GetWeeklyHours()} | " +
                        $"Weekly Pay: {crew.CalculateWeeklyPay():C}";
                }
                else if (person is Vendor vendor)
                {
                    var categoryNames = vendor.GetCategories()
                        .Where(id => categoryMap.ContainsKey(id))
                        .Select(id => categoryMap[id]);

                    roleDetails =
                        $"Stall Name: {vendor.GetStallName()} | Categories: {string.Join(" ", categoryNames)}";
                }
                else
                {
                    roleDetails = person.GetRoleDetails();
                }

                string line = string.Join(",",
                    EscapeCsv(person.GetPersonId().ToString()),
                    EscapeCsv(person.GetName()),
                    EscapeCsv(person.GetTelephone()),
                    EscapeCsv(person.GetEmail()),
                    EscapeCsv(person.GetRole()),
                    EscapeCsv(person.GetIsDeleted().ToString()),
                    EscapeCsv(roleDetails)
                );

                lines.Add(line);
            }

            File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }

        private string EscapeCsv(string value)
        {
            if (value == null)
                return "\"\"";

            string escaped = value.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }
    }
}