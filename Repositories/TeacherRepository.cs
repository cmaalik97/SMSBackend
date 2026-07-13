using Microsoft.Data.SqlClient;
using Student_Management_System.Data;
using Student_Management_System.Models;

namespace Student_Management_System.Repositories
{
    public class TeacherRepository
    {
        private readonly DbHelper _db;
        public TeacherRepository(DbHelper db) => _db = db;

        public List<Teacher> GetAll(string? search = null)
        {
            var list = new List<Teacher>();
            using SqlConnection conn = _db.GetConnection();
            string sql = "SELECT TeacherId, UserId, FullName, Email, Phone, Qualification, Salary, JoiningDate, Address FROM Teachers";
            if (!string.IsNullOrWhiteSpace(search))
                sql += " WHERE FullName LIKE @Search OR Email LIKE @Search OR Qualification LIKE @Search";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(search))
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        public Teacher? GetById(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = "SELECT TeacherId, UserId, FullName, Email, Phone, Qualification, Salary, JoiningDate, Address FROM Teachers WHERE TeacherId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            return r.Read() ? Map(r) : null;
        }

        // Used when a Teacher logs in: "give me only MY row" (their salary, profile, etc.)
        public Teacher? GetByUserId(int userId)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = "SELECT TeacherId, UserId, FullName, Email, Phone, Qualification, Salary, JoiningDate, Address FROM Teachers WHERE UserId=@UserId";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            return r.Read() ? Map(r) : null;
        }

        public int Create(Teacher t)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                INSERT INTO Teachers (UserId, FullName, Email, Phone, Qualification, Salary, JoiningDate, Address)
                VALUES (@UserId, @FullName, @Email, @Phone, @Qualification, @Salary, @JoiningDate, @Address);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            AddParams(cmd, t);

            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public bool Update(int id, Teacher t)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                UPDATE Teachers SET FullName=@FullName, Email=@Email, Phone=@Phone,
                    Qualification=@Qualification, Salary=@Salary, JoiningDate=@JoiningDate, Address=@Address
                WHERE TeacherId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            AddParams(cmd, t);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = "DELETE FROM Teachers WHERE TeacherId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        private static void AddParams(SqlCommand cmd, Teacher t)
        {
            cmd.Parameters.AddWithValue("@UserId", t.UserId);
            cmd.Parameters.AddWithValue("@FullName", t.FullName);
            cmd.Parameters.AddWithValue("@Email", t.Email);
            cmd.Parameters.AddWithValue("@Phone", t.Phone ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Qualification", t.Qualification ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Salary", t.Salary);

            var joining = t.JoiningDate == default ? DateTime.UtcNow.Date : t.JoiningDate;
            cmd.Parameters.Add("@JoiningDate", System.Data.SqlDbType.Date).Value = joining;
            cmd.Parameters.AddWithValue("@Address", t.Address ?? (object)DBNull.Value);
        }

        private static Teacher Map(SqlDataReader r) => new Teacher
        {
            TeacherId = r.GetInt32(r.GetOrdinal("TeacherId")),
            UserId = r.GetInt32(r.GetOrdinal("UserId")),
            FullName = r.GetString(r.GetOrdinal("FullName")),
            Email = r.GetString(r.GetOrdinal("Email")),
            Phone = r.GetStringOrNull("Phone"),
            Qualification = r.GetStringOrNull("Qualification"),
            Salary = r.GetDecimal(r.GetOrdinal("Salary")),
            JoiningDate = r.GetDateTime(r.GetOrdinal("JoiningDate")),
            Address = r.GetStringOrNull("Address"),
        };
    }
}
