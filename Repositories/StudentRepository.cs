using Microsoft.Data.SqlClient;
using Student_Management_System.Data;
using Student_Management_System.Models;
namespace Student_Management_System.Repositories
{
    public class StudentRepository
    {
        private readonly DbHelper _db;
        public StudentRepository(DbHelper db) => _db = db;

        // GET ALL — with an optional search box.
        // search=null  -> "select * from stdregister" (your dataload())
        // search="ali" -> only rows where name/email/phone contain "ali"
        public List<Student> GetAll(string? search = null)
        {
            var list = new List<Student>();
            using SqlConnection conn = _db.GetConnection();

            string sql = @"
                SELECT s.StudentId, s.UserId, s.FullName, s.Email, s.Phone, s.DOB, s.Gender,
                       s.ClassId, c.ClassName + ' - ' + ISNULL(c.Section,'') AS ClassName,
                       s.ParentName, s.ParentPhone, s.Address, s.AdmissionDate
                FROM Students s
                LEFT JOIN Classes c ON s.ClassId = c.ClassId";

            if (!string.IsNullOrWhiteSpace(search))
                sql += " WHERE s.FullName LIKE @Search OR s.Email LIKE @Search OR s.Phone LIKE @Search";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(search))
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        // GET ALL students in one class (used by the "filter by class" dropdown)
        public List<Student> GetByClass(int classId)
        {
            var list = new List<Student>();
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT s.StudentId, s.UserId, s.FullName, s.Email, s.Phone, s.DOB, s.Gender,
                       s.ClassId, c.ClassName + ' - ' + ISNULL(c.Section,'') AS ClassName,
                       s.ParentName, s.ParentPhone, s.Address, s.AdmissionDate
                FROM Students s LEFT JOIN Classes c ON s.ClassId = c.ClassId
                WHERE s.ClassId = @ClassId";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ClassId", classId);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        // GET ONE — like your button4_Click "find by StdID"
        public Student? GetById(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT s.StudentId, s.UserId, s.FullName, s.Email, s.Phone, s.DOB, s.Gender,
                       s.ClassId, c.ClassName + ' - ' + ISNULL(c.Section,'') AS ClassName,
                       s.ParentName, s.ParentPhone, s.Address, s.AdmissionDate
                FROM Students s LEFT JOIN Classes c ON s.ClassId = c.ClassId
                WHERE s.StudentId = @Id";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            return r.Read() ? Map(r) : null;
        }

        // Used when a Student logs in: "give me only MY row"
        public Student? GetByUserId(int userId)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT s.StudentId, s.UserId, s.FullName, s.Email, s.Phone, s.DOB, s.Gender,
                       s.ClassId, c.ClassName + ' - ' + ISNULL(c.Section,'') AS ClassName,
                       s.ParentName, s.ParentPhone, s.Address, s.AdmissionDate
                FROM Students s LEFT JOIN Classes c ON s.ClassId = c.ClassId
                WHERE s.UserId = @UserId";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            return r.Read() ? Map(r) : null;
        }

        // CREATE — like your button1_Click, but with parameters instead of
        // gluing textBox text into the SQL string (which is unsafe).
        public int Create(Student s)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                INSERT INTO Students (UserId, FullName, Email, Phone, DOB, Gender, ClassId, ParentName, ParentPhone, Address, AdmissionDate)
                VALUES (@UserId, @FullName, @Email, @Phone, @DOB, @Gender, @ClassId, @ParentName, @ParentPhone, @Address, @AdmissionDate);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            AddParams(cmd, s);

            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        // UPDATE — like your button2_Click
        public bool Update(int id, Student s)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                UPDATE Students SET
                    FullName=@FullName, Email=@Email, Phone=@Phone, DOB=@DOB, Gender=@Gender,
                    ClassId=@ClassId, ParentName=@ParentName, ParentPhone=@ParentPhone,
                    Address=@Address, AdmissionDate=@AdmissionDate
                WHERE StudentId=@Id";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            AddParams(cmd, s);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        // DELETE — like your button3_Click
        public bool Delete(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = "DELETE FROM Students WHERE StudentId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        // ---------- helpers ----------
        private static void AddParams(SqlCommand cmd, Student s)
        {
            cmd.Parameters.AddWithValue("@UserId", s.UserId);
            cmd.Parameters.AddWithValue("@FullName", s.FullName);
            cmd.Parameters.AddWithValue("@Email", s.Email);
            cmd.Parameters.AddWithValue("@Phone", s.Phone ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@DOB", s.DOB ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Gender", s.Gender ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ClassId", s.ClassId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ParentName", s.ParentName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ParentPhone", s.ParentPhone ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", s.Address ?? (object)DBNull.Value);

            // Use explicit SqlDbType.Date (not the inferred old SqlDbType.DateTime,
            // which only accepts dates from 1753 onward and overflows on the
            // default DateTime value 0001-01-01 if the form didn't send one)
            var admission = s.AdmissionDate == default ? DateTime.UtcNow.Date : s.AdmissionDate;
            cmd.Parameters.Add("@AdmissionDate", System.Data.SqlDbType.Date).Value = admission;
        }

        private static Student Map(SqlDataReader r) => new Student
        {
            StudentId = r.GetInt32(r.GetOrdinal("StudentId")),
            UserId = r.GetInt32(r.GetOrdinal("UserId")),
            FullName = r.GetString(r.GetOrdinal("FullName")),
            Email = r.GetString(r.GetOrdinal("Email")),
            Phone = r.GetStringOrNull("Phone"),
            DOB = r.GetDateTimeOrNull("DOB"),
            Gender = r.GetStringOrNull("Gender"),
            ClassId = r.GetIntOrNull("ClassId"),
            ClassName = r.GetStringOrNull("ClassName"),
            ParentName = r.GetStringOrNull("ParentName"),
            ParentPhone = r.GetStringOrNull("ParentPhone"),
            Address = r.GetStringOrNull("Address"),
            AdmissionDate = r.GetDateTime(r.GetOrdinal("AdmissionDate")),
        };

    }
}
