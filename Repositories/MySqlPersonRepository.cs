using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using FestivalDesktopInformationSystem.Interfaces;
using FestivalDesktopInformationSystem.Models;

namespace FestivalDesktopInformationSystem.Repositories
{
    // Concrete MySQL repository for all person-related persistence.
    // All SQL for the festival system is kept here to preserve
    // the required separation between controller, business logic,
    // and data access.
    public class MySqlPersonRepository : IPersonRepository
    {
        private readonly string _connectionString;
        private MySqlConnection? _connection;

        public MySqlPersonRepository(DatabaseManager databaseManager)
        {
            _connectionString = DbConfig.ConnectionString;
        }

        // Opens the database connection when needed.
        private void OpenConnection()
        {
            if (_connection == null)
                _connection = new MySqlConnection(_connectionString);

            if (_connection.State != System.Data.ConnectionState.Open)
                _connection.Open();
        }

        // Closes the database connection after an operation.
        private void CloseConnection()
        {
            if (_connection != null && _connection.State != System.Data.ConnectionState.Closed)
                _connection.Close();
        }

        // Adds a new person to the database.
        // The base person row is inserted first, then the role-specific table,
        // then any linked genres or categories are saved.
        public void AddPerson(Person person)
        {
            try
            {
                OpenConnection();

                using var transaction = _connection!.BeginTransaction();

                int personId;

                string insertPersonSql = @"
                    INSERT INTO people (name, telephone, email, role, isDeleted)
                    VALUES (@name, @telephone, @email, @role, @isDeleted);
                    SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(insertPersonSql, _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@name", person.GetName());
                    cmd.Parameters.AddWithValue("@telephone", person.GetTelephone());
                    cmd.Parameters.AddWithValue("@email", person.GetEmail());
                    cmd.Parameters.AddWithValue("@role", person.GetRole());
                    cmd.Parameters.AddWithValue("@isDeleted", person.GetIsDeleted());

                    personId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                person.SetPersonId(personId);

                switch (person)
                {
                    case Performer performer:
                        AddPerformer(performer, transaction);
                        SavePerformerGenres(personId, performer.GetGenres(), transaction);
                        break;

                    case Crew crew:
                        AddCrew(crew, transaction);
                        break;

                    case Vendor vendor:
                        AddVendor(vendor, transaction);
                        SaveVendorCategories(personId, vendor.GetCategories(), transaction);
                        break;

                    default:
                        throw new InvalidOperationException("Unsupported person type.");
                }

                transaction.Commit();
            }
            catch
            {
                throw;
            }
            finally
            {
                CloseConnection();
            }
        }

        // Updates an existing person and their role-specific data.
        public void UpdatePerson(Person person)
        {
            try
            {
                OpenConnection();

                using var transaction = _connection!.BeginTransaction();

                string updatePersonSql = @"
                    UPDATE people
                    SET name = @name,
                        telephone = @telephone,
                        email = @email,
                        role = @role,
                        isDeleted = @isDeleted
                    WHERE personId = @personId;";

                using (var cmd = new MySqlCommand(updatePersonSql, _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@name", person.GetName());
                    cmd.Parameters.AddWithValue("@telephone", person.GetTelephone());
                    cmd.Parameters.AddWithValue("@email", person.GetEmail());
                    cmd.Parameters.AddWithValue("@role", person.GetRole());
                    cmd.Parameters.AddWithValue("@isDeleted", person.GetIsDeleted());
                    cmd.Parameters.AddWithValue("@personId", person.GetPersonId());

                    cmd.ExecuteNonQuery();
                }

                switch (person)
                {
                    case Performer performer:
                        UpdatePerformer(performer, transaction);
                        DeletePerformerGenres(performer.GetPersonId(), transaction);
                        SavePerformerGenres(performer.GetPersonId(), performer.GetGenres(), transaction);
                        break;

                    case Crew crew:
                        UpdateCrew(crew, transaction);
                        break;

                    case Vendor vendor:
                        UpdateVendor(vendor, transaction);
                        DeleteVendorCategories(vendor.GetPersonId(), transaction);
                        SaveVendorCategories(vendor.GetPersonId(), vendor.GetCategories(), transaction);
                        break;

                    default:
                        throw new InvalidOperationException("Unsupported person type.");
                }

                transaction.Commit();
            }
            catch
            {
                throw;
            }
            finally
            {
                CloseConnection();
            }
        }

        // Permanently deletes a person record from the database.
        // Related child rows are expected to be removed by foreign key cascade.
        public bool DeletePerson(int personId)
        {
            try
            {
                OpenConnection();

                string sql = "DELETE FROM people WHERE personId = @personId;";
                using var cmd = new MySqlCommand(sql, _connection);
                cmd.Parameters.AddWithValue("@personId", personId);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            finally
            {
                CloseConnection();
            }
        }

        // Marks a person as deleted without physically removing the row.
        public bool SoftDeletePerson(int personId)
        {
            try
            {
                OpenConnection();

                string sql = "UPDATE people SET isDeleted = 1 WHERE personId = @personId;";
                using var cmd = new MySqlCommand(sql, _connection);
                cmd.Parameters.AddWithValue("@personId", personId);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            finally
            {
                CloseConnection();
            }
        }

        // Restores a previously soft-deleted person.
        public bool RestorePerson(int personId)
        {
            try
            {
                OpenConnection();

                string sql = "UPDATE people SET isDeleted = 0 WHERE personId = @personId;";
                using var cmd = new MySqlCommand(sql, _connection);
                cmd.Parameters.AddWithValue("@personId", personId);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            finally
            {
                CloseConnection();
            }
        }

        // Returns every person in the system with role-specific data loaded.
        public List<Person> GetAllPeople()
        {
            var people = new List<Person>();

            try
            {
                OpenConnection();

                string sql = @"
                    SELECT personId, name, telephone, email, role, isDeleted
                    FROM people
                    ORDER BY personId;";

                using var cmd = new MySqlCommand(sql, _connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    people.Add(MapPerson(reader));
                }
            }
            finally
            {
                CloseConnection();
            }

            return people;
        }

        // Returns people filtered by a specific role.
        public List<Person> GetPeopleByRole(string role)
        {
            var people = new List<Person>();

            try
            {
                OpenConnection();

                string sql = @"
                    SELECT personId, name, telephone, email, role, isDeleted
                    FROM people
                    WHERE role = @role
                    ORDER BY name;";

                using var cmd = new MySqlCommand(sql, _connection);
                cmd.Parameters.AddWithValue("@role", role);

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    people.Add(MapPerson(reader));
                }
            }
            finally
            {
                CloseConnection();
            }

            return people;
        }

        // Returns a single person by ID, or null if not found.
        public Person? GetPersonById(int personId)
        {
            try
            {
                OpenConnection();

                string sql = @"
                    SELECT personId, name, telephone, email, role, isDeleted
                    FROM people
                    WHERE personId = @personId;";

                using var cmd = new MySqlCommand(sql, _connection);
                cmd.Parameters.AddWithValue("@personId", personId);

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                    return MapPerson(reader);

                return null;
            }
            finally
            {
                CloseConnection();
            }
        }

        // Performs a simple partial match search against name, email, telephone and role.
        public List<Person> SearchPeople(string keyword)
        {
            var people = new List<Person>();

            try
            {
                OpenConnection();

                string sql = @"
                    SELECT personId, name, telephone, email, role, isDeleted
                    FROM people
                    WHERE name LIKE @keyword
                       OR email LIKE @keyword
                       OR telephone LIKE @keyword
                       OR role LIKE @keyword
                    ORDER BY name;";

                using var cmd = new MySqlCommand(sql, _connection);
                cmd.Parameters.AddWithValue("@keyword", $"%{keyword}%");

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    people.Add(MapPerson(reader));
                }
            }
            finally
            {
                CloseConnection();
            }

            return people;
        }

        // Sorts people by a supported field.
        // To keep SQL safe, only a fixed list of sort fields is allowed.
        public List<Person> SortPeople(string sortField)
        {
            var people = new List<Person>();

            string orderBy = sortField.ToLower() switch
            {
                "name" => "name",
                "email" => "email",
                "role" => "role",
                "personid" => "personId",
                _ => "name"
            };

            try
            {
                OpenConnection();

                string sql = $@"
                    SELECT personId, name, telephone, email, role, isDeleted
                    FROM people
                    ORDER BY {orderBy};";

                using var cmd = new MySqlCommand(sql, _connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    people.Add(MapPerson(reader));
                }
            }
            finally
            {
                CloseConnection();
            }

            return people;
        }

        // Checks whether an email already exists in the people table.
        public bool EmailExists(string email)
        {
            try
            {
                OpenConnection();

                string sql = "SELECT COUNT(*) FROM people WHERE email = @email;";
                using var cmd = new MySqlCommand(sql, _connection);
                cmd.Parameters.AddWithValue("@email", email);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
            finally
            {
                CloseConnection();
            }
        }

        // Saves performer genre links using the junction table.
        public void SavePerformerGenres(int personId, List<int> genres)
        {
            try
            {
                OpenConnection();
                SavePerformerGenres(personId, genres, null);
            }
            finally
            {
                CloseConnection();
            }
        }

        // Saves vendor category links using the junction table.
        public void SaveVendorCategories(int personId, List<int> categories)
        {
            try
            {
                OpenConnection();
                SaveVendorCategories(personId, categories, null);
            }
            finally
            {
                CloseConnection();
            }
        }

        // Returns all genre IDs linked to a performer.
        public List<int> GetGenresByPerformerId(int personId)
        {
            var genres = new List<int>();

            try
            {
                OpenConnection();

                string sql = @"
                    SELECT genreId
                    FROM performer_genres
                    WHERE personId = @personId;";

                using var cmd = new MySqlCommand(sql, _connection);
                cmd.Parameters.AddWithValue("@personId", personId);

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    genres.Add(Convert.ToInt32(reader["genreId"]));
                }
            }
            finally
            {
                CloseConnection();
            }

            return genres;
        }

        // Returns all category IDs linked to a vendor.
        public List<int> GetCategoriesByVendorId(int personId)
        {
            var categories = new List<int>();

            try
            {
                OpenConnection();

                string sql = @"
                    SELECT categoryId
                    FROM vendor_categories
                    WHERE personId = @personId;";

                using var cmd = new MySqlCommand(sql, _connection);
                cmd.Parameters.AddWithValue("@personId", personId);

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    categories.Add(Convert.ToInt32(reader["categoryId"]));
                }
            }
            finally
            {
                CloseConnection();
            }

            return categories;
        }

        // Maps a base people row to the correct subclass.
        // This is where polymorphic loading happens.
        private Person MapPerson(MySqlDataReader reader)
        {
            int personId = Convert.ToInt32(reader["personId"]);
            string name = reader["name"].ToString() ?? string.Empty;
            string telephone = reader["telephone"].ToString() ?? string.Empty;
            string email = reader["email"].ToString() ?? string.Empty;
            string role = reader["role"].ToString() ?? string.Empty;
            bool isDeleted = Convert.ToBoolean(reader["isDeleted"]);

            return role switch
            {
                "Performer" => BuildPerformer(personId, name, telephone, email, isDeleted),
                "Crew" => BuildCrew(personId, name, telephone, email, isDeleted),
                "Vendor" => BuildVendor(personId, name, telephone, email, isDeleted),
                _ => throw new InvalidOperationException("Unknown role found in database.")
            };
        }

        // Builds a Performer object by loading performer-specific data.
        private Performer BuildPerformer(int personId, string name, string telephone, string email, bool isDeleted)
        {
            decimal fee = 0;
            var genres = new List<int>();

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string performerSql = "SELECT fee FROM performers WHERE personId = @personId;";
            using (var cmd = new MySqlCommand(performerSql, connection))
            {
                cmd.Parameters.AddWithValue("@personId", personId);
                object? result = cmd.ExecuteScalar();

                if (result != null)
                    fee = Convert.ToDecimal(result);
            }

            string genreSql = "SELECT genreId FROM performer_genres WHERE personId = @personId;";
            using (var cmd = new MySqlCommand(genreSql, connection))
            {
                cmd.Parameters.AddWithValue("@personId", personId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    genres.Add(Convert.ToInt32(reader["genreId"]));
                }
            }

            return new Performer(personId, name, telephone, email, fee, genres, isDeleted);
        }

        // Builds a Crew object by loading crew-specific data.
        private Crew BuildCrew(int personId, string name, string telephone, string email, bool isDeleted)
        {
            decimal hourlyRate = 0;
            string employmentType = string.Empty;
            int weeklyHours = 0;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                SELECT hourlyRate, employmentType, weeklyHours
                FROM crew
                WHERE personId = @personId;";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@personId", personId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                hourlyRate = Convert.ToDecimal(reader["hourlyRate"]);
                employmentType = reader["employmentType"].ToString() ?? string.Empty;
                weeklyHours = Convert.ToInt32(reader["weeklyHours"]);
            }

            return new Crew(personId, name, telephone, email, hourlyRate, employmentType, weeklyHours, isDeleted);
        }

        // Builds a Vendor object by loading vendor-specific data.
        private Vendor BuildVendor(int personId, string name, string telephone, string email, bool isDeleted)
        {
            string stallName = string.Empty;
            var categories = new List<int>();

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string vendorSql = "SELECT stallName FROM vendors WHERE personId = @personId;";
            using (var cmd = new MySqlCommand(vendorSql, connection))
            {
                cmd.Parameters.AddWithValue("@personId", personId);
                object? result = cmd.ExecuteScalar();

                if (result != null)
                    stallName = result.ToString() ?? string.Empty;
            }

            string categorySql = "SELECT categoryId FROM vendor_categories WHERE personId = @personId;";
            using (var cmd = new MySqlCommand(categorySql, connection))
            {
                cmd.Parameters.AddWithValue("@personId", personId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    categories.Add(Convert.ToInt32(reader["categoryId"]));
                }
            }

            return new Vendor(personId, name, telephone, email, stallName, categories, isDeleted);
        }

        // Inserts a performer row into the role-specific table.
        private void AddPerformer(Performer performer, MySqlTransaction transaction)
        {
            string sql = @"
                INSERT INTO performers (personId, fee)
                VALUES (@personId, @fee);";

            using var cmd = new MySqlCommand(sql, _connection, transaction);
            cmd.Parameters.AddWithValue("@personId", performer.GetPersonId());
            cmd.Parameters.AddWithValue("@fee", performer.GetFee());

            cmd.ExecuteNonQuery();
        }

        // Inserts a crew row into the role-specific table.
        private void AddCrew(Crew crew, MySqlTransaction transaction)
        {
            string sql = @"
                INSERT INTO crew (personId, hourlyRate, employmentType, weeklyHours)
                VALUES (@personId, @hourlyRate, @employmentType, @weeklyHours);";

            using var cmd = new MySqlCommand(sql, _connection, transaction);
            cmd.Parameters.AddWithValue("@personId", crew.GetPersonId());
            cmd.Parameters.AddWithValue("@hourlyRate", crew.GetHourlyRate());
            cmd.Parameters.AddWithValue("@employmentType", crew.GetEmploymentType());
            cmd.Parameters.AddWithValue("@weeklyHours", crew.GetWeeklyHours());

            cmd.ExecuteNonQuery();
        }

        // Inserts a vendor row into the role-specific table.
        private void AddVendor(Vendor vendor, MySqlTransaction transaction)
        {
            string sql = @"
                INSERT INTO vendors (personId, stallName)
                VALUES (@personId, @stallName);";

            using var cmd = new MySqlCommand(sql, _connection, transaction);
            cmd.Parameters.AddWithValue("@personId", vendor.GetPersonId());
            cmd.Parameters.AddWithValue("@stallName", vendor.GetStallName());

            cmd.ExecuteNonQuery();
        }

        // Updates an existing performer row.
        private void UpdatePerformer(Performer performer, MySqlTransaction transaction)
        {
            string sql = @"
                UPDATE performers
                SET fee = @fee
                WHERE personId = @personId;";

            using var cmd = new MySqlCommand(sql, _connection, transaction);
            cmd.Parameters.AddWithValue("@fee", performer.GetFee());
            cmd.Parameters.AddWithValue("@personId", performer.GetPersonId());

            cmd.ExecuteNonQuery();
        }

        // Updates an existing crew row.
        private void UpdateCrew(Crew crew, MySqlTransaction transaction)
        {
            string sql = @"
                UPDATE crew
                SET hourlyRate = @hourlyRate,
                    employmentType = @employmentType,
                    weeklyHours = @weeklyHours
                WHERE personId = @personId;";

            using var cmd = new MySqlCommand(sql, _connection, transaction);
            cmd.Parameters.AddWithValue("@hourlyRate", crew.GetHourlyRate());
            cmd.Parameters.AddWithValue("@employmentType", crew.GetEmploymentType());
            cmd.Parameters.AddWithValue("@weeklyHours", crew.GetWeeklyHours());
            cmd.Parameters.AddWithValue("@personId", crew.GetPersonId());

            cmd.ExecuteNonQuery();
        }

        // Updates an existing vendor row.
        private void UpdateVendor(Vendor vendor, MySqlTransaction transaction)
        {
            string sql = @"
                UPDATE vendors
                SET stallName = @stallName
                WHERE personId = @personId;";

            using var cmd = new MySqlCommand(sql, _connection, transaction);
            cmd.Parameters.AddWithValue("@stallName", vendor.GetStallName());
            cmd.Parameters.AddWithValue("@personId", vendor.GetPersonId());

            cmd.ExecuteNonQuery();
        }

        // Deletes all existing performer-genre links before re-saving them.
        private void DeletePerformerGenres(int personId, MySqlTransaction transaction)
        {
            string sql = "DELETE FROM performer_genres WHERE personId = @personId;";
            using var cmd = new MySqlCommand(sql, _connection, transaction);
            cmd.Parameters.AddWithValue("@personId", personId);
            cmd.ExecuteNonQuery();
        }

        // Deletes all existing vendor-category links before re-saving them.
        private void DeleteVendorCategories(int personId, MySqlTransaction transaction)
        {
            string sql = "DELETE FROM vendor_categories WHERE personId = @personId;";
            using var cmd = new MySqlCommand(sql, _connection, transaction);
            cmd.Parameters.AddWithValue("@personId", personId);
            cmd.ExecuteNonQuery();
        }

        // Saves performer genre junction rows.
        private void SavePerformerGenres(int personId, List<int> genres, MySqlTransaction? transaction)
        {
            foreach (int genreId in genres)
            {
                string sql = @"
                    INSERT INTO performer_genres (personId, genreId)
                    VALUES (@personId, @genreId);";

                using var cmd = transaction == null
                    ? new MySqlCommand(sql, _connection)
                    : new MySqlCommand(sql, _connection, transaction);

                cmd.Parameters.AddWithValue("@personId", personId);
                cmd.Parameters.AddWithValue("@genreId", genreId);
                cmd.ExecuteNonQuery();
            }
        }

        // Saves vendor category junction rows.
        private void SaveVendorCategories(int personId, List<int> categories, MySqlTransaction? transaction)
        {
            foreach (int categoryId in categories)
            {
                string sql = @"
                    INSERT INTO vendor_categories (personId, categoryId)
                    VALUES (@personId, @categoryId);";

                using var cmd = transaction == null
                    ? new MySqlCommand(sql, _connection)
                    : new MySqlCommand(sql, _connection, transaction);

                cmd.Parameters.AddWithValue("@personId", personId);
                cmd.Parameters.AddWithValue("@categoryId", categoryId);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Genre> GetAllGenres()
        {
            var genres = new List<Genre>();

            try
            {
                OpenConnection();

                string sql = "SELECT genreId, genreName FROM genres ORDER BY genreName;";
                using var cmd = new MySqlCommand(sql, _connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    genres.Add(new Genre(
                        Convert.ToInt32(reader["genreId"]),
                        reader["genreName"].ToString() ?? string.Empty
                    ));
                }
            }
            finally
            {
                CloseConnection();
            }

            return genres;
        }

        public List<Category> GetAllCategories()
        {
            var categories = new List<Category>();

            try
            {
                OpenConnection();

                string sql = "SELECT categoryId, categoryName FROM categories ORDER BY categoryName;";
                using var cmd = new MySqlCommand(sql, _connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    categories.Add(new Category(
                        Convert.ToInt32(reader["categoryId"]),
                        reader["categoryName"].ToString() ?? string.Empty
                    ));
                }
            }
            finally
            {
                CloseConnection();
            }

            return categories;
        }
    }
}