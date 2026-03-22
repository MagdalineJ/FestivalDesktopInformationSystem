using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FestivalDesktopInformationSystem.Models;

namespace FestivalDesktopInformationSystem.Services
{
    // Generates text-based reports from the current list of people.
    // These reports can be displayed in the console view.
    public class ReportService
    {
        public string GeneratePeopleCountByRole(List<Person> people)
        {
            if (people == null || people.Count == 0)
                return "No data available for role count report.";

            int performerCount = people.Count(p => p.GetRole() == "Performer" && !p.GetIsDeleted());
            int crewCount = people.Count(p => p.GetRole() == "Crew" && !p.GetIsDeleted());
            int vendorCount = people.Count(p => p.GetRole() == "Vendor" && !p.GetIsDeleted());

            var report = new StringBuilder();
            report.AppendLine("=== People Count by Role ===");
            report.AppendLine($"Performers: {performerCount}");
            report.AppendLine($"Crew: {crewCount}");
            report.AppendLine($"Vendors: {vendorCount}");

            return report.ToString();
        }

        public string GeneratePerformerFeeReport(List<Person> people, List<Genre> genres)
        {
            if (people == null || people.Count == 0)
                return "No data available for performer fee report.";

            var performers = people
                .OfType<Performer>()
                .Where(p => !p.GetIsDeleted())
                .ToList();

            var genreMap = genres.ToDictionary(g => g.GenreId, g => g.GenreName);

            decimal totalFees = performers.Sum(p => p.GetFee());
            decimal averageFee = performers.Count > 0 ? performers.Average(p => p.GetFee()) : 0;

            var report = new StringBuilder();
            report.AppendLine("=== Performer Fee Report ===");
            report.AppendLine($"Total Performers: {performers.Count}");
            report.AppendLine($"Total Fees: {totalFees:C}");
            report.AppendLine($"Average Fee: {averageFee:C}");
            report.AppendLine();

            foreach (var performer in performers)
            {
                var genreNames = performer.GetGenres()
                    .Where(id => genreMap.ContainsKey(id))
                    .Select(id => genreMap[id])
                    .ToList();

                report.AppendLine(
                    $"Performer: {performer.GetName()} | Fee: {performer.GetFee():C} | Genres: {string.Join(", ", genreNames)}"
                );
            }

            return report.ToString();
        }

        public string GenerateCrewCostReport(List<Person> people)
        {
            if (people == null || people.Count == 0)
                return "No data available for crew cost report.";

            var crewMembers = people
                .OfType<Crew>()
                .Where(c => !c.GetIsDeleted())
                .ToList();

            decimal totalWeeklyCost = crewMembers.Sum(c => c.CalculateWeeklyPay());

            var report = new StringBuilder();
            report.AppendLine("=== Crew Weekly Cost Report ===");
            report.AppendLine($"Total Crew Members: {crewMembers.Count}");
            report.AppendLine($"Total Weekly Crew Cost: {totalWeeklyCost:C}");
            report.AppendLine();

            foreach (var crew in crewMembers)
            {
                report.AppendLine(
                    $"Crew: {crew.GetName()} | Employment Type: {crew.GetEmploymentType()} | Weekly Hours: {crew.GetWeeklyHours()} | Weekly Pay: {crew.CalculateWeeklyPay():C}"
                );
            }

            return report.ToString();
        }

        public string GenerateVendorCategoryReport(List<Person> people, List<Category> categories)
        {
            if (people == null || people.Count == 0)
                return "No data available for vendor category report.";

            var vendors = people
                .OfType<Vendor>()
                .Where(v => !v.GetIsDeleted())
                .ToList();

            var categoryMap = categories.ToDictionary(c => c.CategoryId, c => c.CategoryName);

            int totalVendors = vendors.Count;
            int totalCategoriesLinked = vendors.Sum(v => v.GetCategories().Count);

            var report = new StringBuilder();
            report.AppendLine("=== Vendor Category Report ===");
            report.AppendLine($"Total Vendors: {totalVendors}");
            report.AppendLine($"Total Linked Categories: {totalCategoriesLinked}");
            report.AppendLine();

            foreach (var vendor in vendors)
            {
                var categoryNames = vendor.GetCategories()
                    .Where(id => categoryMap.ContainsKey(id))
                    .Select(id => categoryMap[id])
                    .ToList();

                report.AppendLine(
                    $"Vendor: {vendor.GetName()} | Stall Name: {vendor.GetStallName()} | Categories: {string.Join(", ", categoryNames)}"
                );
            }

            return report.ToString();
        }
    }
}