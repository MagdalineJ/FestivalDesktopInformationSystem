using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FestivalDesktopInformationSystem.Helpers
{
    // Central validation helper used by the service layer.
    // Keeps validation rules in one place for reuse and clarity.
    public class ValidationHelper
    {
        // Checks that a name is not empty and has a sensible minimum length.
        public bool ValidateName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Trim().Length >= 2;
        }

        // Checks that the telephone is not empty and contains allowed characters.
        // This is basic validation to prevent obvious bad input.
        public bool ValidateTelephone(string telephone)
        {
            if (string.IsNullOrWhiteSpace(telephone))
                return false;

            string pattern = @"^[0-9+\-\s()]{7,20}$";
            return Regex.IsMatch(telephone.Trim(), pattern);
        }

        // Basic email format validation.
        public bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email.Trim(), pattern);
        }

        // Ensures a decimal value is not negative.
        public bool ValidateDecimal(decimal value)
        {
            return value >= 0;
        }

        // Validates the employment type for crew members.
        public bool ValidateEmploymentType(string type)
            {
                if (string.IsNullOrWhiteSpace(type))
                    return false;

                string normalized = type.Replace(" ", "").ToLower();

                return normalized == "fulltime" || normalized == "parttime";
            }

        // Applies the coursework rule for weekly hours based on employment type.
      public bool ValidateWeeklyHours(string type, int hours)
        {
            if (string.IsNullOrWhiteSpace(type))
                return false;

            // Normalize input (same as SetEmploymentType)
            string normalized = type.Replace(" ", "").ToLower();

            if (normalized == "fulltime")
                return hours >= 25 && hours <= 40;

            if (normalized == "parttime")
                return hours >= 1 && hours <= 24;

            return false;
        }

        // Validates performer genre IDs.
        // At least one genre is required and all IDs must be positive.
        public bool ValidateGenres(List<int> genres)
        {
            return genres != null && genres.Count > 0 && genres.All(g => g > 0);
        }

        // Validates vendor category IDs.
        // At least one category is required and all IDs must be positive.
        public bool ValidateCategories(List<int> categories)
        {
            return categories != null && categories.Count > 0 && categories.All(c => c > 0);
        }
    }
}