using System;
using System.Collections.Generic;
using System.Linq;

namespace FestivalDesktopInformationSystem.Models
{
    // Vendor inherits from Person and adds stall name and category IDs.
    public class Vendor : Person
    {
        private string _stallName;
        private List<int> _categories;

        public Vendor(
            int personId,
            string name,
            string telephone,
            string email,
            string stallName,
            List<int>? categories = null,
            bool isDeleted = false)
            : base(personId, name, telephone, email, "Vendor", isDeleted)
        {
            _stallName = stallName;
            _categories = categories ?? new List<int>();
        }

        public void SetStallName(string stallName)
        {
            if (string.IsNullOrWhiteSpace(stallName))
                throw new ArgumentException("Stall name cannot be empty.");

            _stallName = stallName.Trim();
        }

        public string GetStallName()
        {
            return _stallName;
        }

        public void SetCategories(List<int> categories)
        {
            _categories = categories ?? new List<int>();
        }

        public List<int> GetCategories()
        {
            return _categories;
        }

        public void AddCategory(int categoryId)
        {
            if (categoryId <= 0)
                throw new ArgumentException("Category ID must be greater than 0.");

            if (!_categories.Contains(categoryId))
                _categories.Add(categoryId);
        }

        public void RemoveCategory(int categoryId)
        {
            _categories.Remove(categoryId);
        }

        public override bool Validate()
        {
            return base.Validate()
                && !string.IsNullOrWhiteSpace(_stallName)
                && _categories != null
                && _categories.Count > 0
                && _categories.All(id => id > 0);
        }

        public override string GetRoleDetails()
        {
            return $"Stall Name: {_stallName} | Category IDs: {string.Join(", ", _categories)}";
        }

        public override string DisplayInfo()
        {
            return $"{base.DisplayInfo()} | {GetRoleDetails()}";
        }
    }
}