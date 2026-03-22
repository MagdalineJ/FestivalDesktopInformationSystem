using System;

namespace FestivalDesktopInformationSystem.Models
{
    // Crew inherits from Person and adds employment details.
    public class Crew : Person
    {
        private decimal _hourlyRate;
        private string _employmentType;
        private int _weeklyHours;

        public Crew(
            int personId,
            string name,
            string telephone,
            string email,
            decimal hourlyRate,
            string employmentType,
            int weeklyHours,
            bool isDeleted = false)
            : base(personId, name, telephone, email, "Crew", isDeleted)
        {
            _employmentType = string.Empty;

            SetHourlyRate(hourlyRate);
            SetEmploymentType(employmentType);
            SetWeeklyHours(weeklyHours);
        }

        public void SetHourlyRate(decimal rate)
        {
            if (rate < 0)
                throw new ArgumentException("Error: Hourly rate cannot be negative.");

            _hourlyRate = rate;
        }

        public decimal GetHourlyRate()
        {
            return _hourlyRate;
        }

        public void SetEmploymentType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Error: Employment type cannot be empty.");

            string normalized = type.Replace(" ", "").ToLower();

            if (normalized == "fulltime")
            {
                _employmentType = "FullTime";
            }
            else if (normalized == "parttime")
            {
                _employmentType = "PartTime";
            }
            else
            {
                throw new ArgumentException("Error: Employment type must be FullTime or PartTime.");
            }
        }

        public string GetEmploymentType()
        {
            return _employmentType;
        }

        public void SetWeeklyHours(int hours)
        {
            if (_employmentType == "FullTime" && (hours < 25 || hours > 40))
                throw new ArgumentException("Error: FullTime weekly hours must be between 25 and 40.");

            if (_employmentType == "PartTime" && (hours < 1 || hours > 24))
                throw new ArgumentException("Error: PartTime weekly hours must be between 1 and 24.");

            _weeklyHours = hours;
        }

        public int GetWeeklyHours()
        {
            return _weeklyHours;
        }

        public decimal CalculateWeeklyPay()
        {
            return _hourlyRate * _weeklyHours;
        }

        public override bool Validate()
        {
            if (!base.Validate())
                return false;

            if (_hourlyRate < 0)
                return false;

            if (_employmentType != "FullTime" && _employmentType != "PartTime")
                return false;

            if (_employmentType == "FullTime" && (_weeklyHours < 25 || _weeklyHours > 40))
                return false;

            if (_employmentType == "PartTime" && (_weeklyHours < 1 || _weeklyHours > 24))
                return false;

            return true;
        }

        public override string GetRoleDetails()
        {
            return $"Hourly Rate: {_hourlyRate:C} | Employment Type: {_employmentType} | Weekly Hours: {_weeklyHours} | Weekly Pay: {CalculateWeeklyPay():C}";
        }

        public override string DisplayInfo()
        {
            return $"{base.DisplayInfo()} | {GetRoleDetails()}";
        }
    }
}