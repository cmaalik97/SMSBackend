using Microsoft.Data.SqlClient;
using Student_Management_System.Data;
using Student_Management_System.Models;

namespace Student_Management_System.Repositories
{
    public class UserRepository
    {
        private readonly DbHelper _db;
        public UserRepository(DbHelper db) => _db = db;

        public List<User> GetAll(string? search = null)
        {
            var list = new List<User>();
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT u.UserId, u.FullName, u.Email, u.PasswordHash, u.RoleId, ro.RoleName, u.IsActive, u.CreatedAt
                FROM Users u JOIN Roles ro ON u.RoleId = ro.RoleId";
            if (!string.IsNullOrWhiteSpace(search))
                sql += " WHERE u.FullName LIKE @Search OR u.Email LIKE @Search";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(search))
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        public User? GetById(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT u.UserId, u.FullName, u.Email, u.PasswordHash, u.RoleId, ro.RoleName, u.IsActive, u.CreatedAt
                FROM Users u JOIN Roles ro ON u.RoleId = ro.RoleId WHERE u.UserId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            return r.Read() ? Map(r) : null;
        }

        // THE LOGIN LOOKUP — AuthController calls this with the email
        // the person typed on the Sign In page, then checks the password.
        public User? GetByEmail(string email)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT u.UserId, u.FullName, u.Email, u.PasswordHash, u.RoleId, ro.RoleName, u.IsActive, u.CreatedAt
                FROM Users u JOIN Roles ro ON u.RoleId = ro.RoleId
                WHERE u.Email=@Email AND u.IsActive = 1";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Email", email);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            return r.Read() ? Map(r) : null;
        }

        public bool EmailExists(string email)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = "SELECT COUNT(1) FROM Users WHERE Email=@Email";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Email", email);

            conn.Open();
            return (int)cmd.ExecuteScalar() > 0;
        }

        public int GetRoleId(string roleName)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = "SELECT RoleId FROM Roles WHERE RoleName=@RoleName";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@RoleName", roleName);

            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        // Creates the login row. PasswordHash must already be hashed
        // (never store a plain-text password) - see AuthController.
        public int Create(User u)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                INSERT INTO Users (FullName, Email, PasswordHash, RoleId, IsActive, CreatedAt)
                VALUES (@FullName, @Email, @PasswordHash, @RoleId, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@FullName", u.FullName);
            cmd.Parameters.AddWithValue("@Email", u.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", u.PasswordHash);
            cmd.Parameters.AddWithValue("@RoleId", u.RoleId);
            cmd.Parameters.AddWithValue("@IsActive", u.IsActive);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public bool Update(int id, User u)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                UPDATE Users SET FullName=@FullName, Email=@Email, RoleId=@RoleId, IsActive=@IsActive
                WHERE UserId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@FullName", u.FullName);
            cmd.Parameters.AddWithValue("@Email", u.Email);
            cmd.Parameters.AddWithValue("@RoleId", u.RoleId);
            cmd.Parameters.AddWithValue("@IsActive", u.IsActive);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = "DELETE FROM Users WHERE UserId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            try
            {
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == 547) // FK constraint violation
            {
                throw new InvalidOperationException(
                    "This user still has a Student or Teacher profile linked to them. " +
                    "Delete that profile first (in the Students or Teachers table), then delete the user.");
            }
        }

        private static User Map(SqlDataReader r) => new User
        {
            UserId = r.GetInt32(r.GetOrdinal("UserId")),
            FullName = r.GetString(r.GetOrdinal("FullName")),
            Email = r.GetString(r.GetOrdinal("Email")),
            PasswordHash = r.GetString(r.GetOrdinal("PasswordHash")),
            RoleId = r.GetInt32(r.GetOrdinal("RoleId")),
            RoleName = r.GetStringOrNull("RoleName"),
            IsActive = r.GetBoolean(r.GetOrdinal("IsActive")),
            CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
        };
    }
}
