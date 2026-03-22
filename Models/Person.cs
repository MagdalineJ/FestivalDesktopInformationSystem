using System;

namespace FestivalDesktopInformationSystem.Models
{
    // Abstract base class shared by all people in the system.
    // Demonstrates inheritance, encapsulation and polymorphism.
    public abstract class Person
    {
        private int _personId;
        private string _name;
        private string _telephone;
        private string _email;
        private string _role;
        private bool _isDeleted;

        protected Person(int personId, string name, string telephone, string email, string role, bool isDeleted = false)
        {
            _personId = personId;
            _name = name;
            _telephone = telephone;
            _email = email;
            _role = role;
            _isDeleted = isDeleted;
        }

        public int GetPersonId()
        {
            return _personId;
        }

        public string GetName()
        {
            return _name;
        }

        public string GetTelephone()
        {
            return _telephone;
        }

        public string GetEmail()
        {
            return _email;
        }

        public string GetRole()
        {
            return _role;
        }

        public bool GetIsDeleted()
        {
            return _isDeleted;
        }

        public void SetPersonId(int personId)
        {
            if (personId < 0)
                throw new ArgumentException("Person ID cannot be negative.");

            _personId = personId;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.");

            _name = name.Trim();
        }

        public void SetTelephone(string telephone)
        {
            if (string.IsNullOrWhiteSpace(telephone))
                throw new ArgumentException("Telephone cannot be empty.");

            _telephone = telephone.Trim();
        }

        public void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.");

            _email = email.Trim();
        }

        protected void SetRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role cannot be empty.");

            _role = role.Trim();
        }

        public void SetIsDeleted(bool isDeleted)
        {
            _isDeleted = isDeleted;
        }

        public virtual bool Validate()
        {
            return !string.IsNullOrWhiteSpace(_name)
                && !string.IsNullOrWhiteSpace(_telephone)
                && !string.IsNullOrWhiteSpace(_email)
                && !string.IsNullOrWhiteSpace(_role);
        }

        public virtual string DisplayInfo()
        {
            return $"ID: {_personId} | Name: {_name} | Telephone: {_telephone} | Email: {_email} | Role: {_role} | Deleted: {_isDeleted}";
        }

        public abstract string GetRoleDetails();
    }
}