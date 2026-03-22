using System;
using System.Collections.Generic;
using System.Linq;

namespace FestivalDesktopInformationSystem.Models
{
    // Performer inherits from Person and adds fee and genre IDs.
    public class Performer : Person
    {
        private decimal _fee;
        private List<int> _genres;

        public Performer(
            int personId,
            string name,
            string telephone,
            string email,
            decimal fee,
            List<int>? genres = null,
            bool isDeleted = false)
            : base(personId, name, telephone, email, "Performer", isDeleted)
        {
            _fee = fee;
            _genres = genres ?? new List<int>();
        }

        public void SetFee(decimal fee)
        {
            if (fee < 0)
                throw new ArgumentException("Fee cannot be negative.");

            _fee = fee;
        }

        public decimal GetFee()
        {
            return _fee;
        }

        public void SetGenres(List<int> genres)
        {
            _genres = genres ?? new List<int>();
        }

        public List<int> GetGenres()
        {
            return _genres;
        }

        public void AddGenre(int genreId)
        {
            if (genreId <= 0)
                throw new ArgumentException("Genre ID must be greater than 0.");

            if (!_genres.Contains(genreId))
                _genres.Add(genreId);
        }

        public void RemoveGenre(int genreId)
        {
            _genres.Remove(genreId);
        }

        public override bool Validate()
        {
            return base.Validate()
                && _fee >= 0
                && _genres != null
                && _genres.Count > 0
                && _genres.All(id => id > 0);
        }

        public override string GetRoleDetails()
        {
            return $"Fee: {_fee:C} | Genre IDs: {string.Join(", ", _genres)}";
        }

        public override string DisplayInfo()
        {
            return $"{base.DisplayInfo()} | {GetRoleDetails()}";
        }
    }
}