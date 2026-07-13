using Microsoft.Data.SqlClient;
using Student_Management_System.Data;
using Student_Management_System.Models;

namespace Student_Management_System.Repositories
{
    public class SubjectRepository
    {
        private readonly DbHelper _db;
        public SubjectRepository(DbHelper db) => _db = db;

        public List<Subject> GetAll(string? search = null)
        {
            var list = new List<Subject>();
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT s.SubjectId, s.SubjectName, s.SubjectCode, s.ClassId,
                       c.ClassName + ' - ' + ISNULL(c.Section,'') AS ClassName,
                       s.TeacherId, t.FullName AS TeacherName
                FROM Subjects s
                LEFT JOIN Classes c ON s.ClassId = c.ClassId
                LEFT JOIN Teachers t ON s.TeacherId = t.TeacherId";
            if (!string.IsNullOrWhiteSpace(search))
                sql += " WHERE s.SubjectName LIKE @Search OR s.SubjectCode LIKE @Search";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(search))
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        // Used when a Teacher logs in: "give me only the subjects I teach"
        public List<Subject> GetByTeacherId(int teacherId)
        {
            var list = new List<Subject>();
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT s.SubjectId, s.SubjectName, s.SubjectCode, s.ClassId,
                       c.ClassName + ' - ' + ISNULL(c.Section,'') AS ClassName,
                       s.TeacherId, t.FullName AS TeacherName
                FROM Subjects s
                LEFT JOIN Classes c ON s.ClassId = c.ClassId
                LEFT JOIN Teachers t ON s.TeacherId = t.TeacherId
                WHERE s.TeacherId = @TeacherId";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TeacherId", teacherId);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        public Subject? GetById(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT s.SubjectId, s.SubjectName, s.SubjectCode, s.ClassId,
                       c.ClassName + ' - ' + ISNULL(c.Section,'') AS ClassName,
                       s.TeacherId, t.FullName AS TeacherName
                FROM Subjects s
                LEFT JOIN Classes c ON s.ClassId = c.ClassId
                LEFT JOIN Teachers t ON s.TeacherId = t.TeacherId
                WHERE s.SubjectId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            return r.Read() ? Map(r) : null;
        }

        public int Create(Subject s)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                INSERT INTO Subjects (SubjectName, SubjectCode, ClassId, TeacherId)
                VALUES (@SubjectName, @SubjectCode, @ClassId, @TeacherId);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            AddParams(cmd, s);

            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public bool Update(int id, Subject s)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                UPDATE Subjects SET SubjectName=@SubjectName, SubjectCode=@SubjectCode, ClassId=@ClassId, TeacherId=@TeacherId
                WHERE SubjectId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            AddParams(cmd, s);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = "DELETE FROM Subjects WHERE SubjectId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        private static void AddParams(SqlCommand cmd, Subject s)
        {
            cmd.Parameters.AddWithValue("@SubjectName", s.SubjectName);
            cmd.Parameters.AddWithValue("@SubjectCode", s.SubjectCode);
            cmd.Parameters.AddWithValue("@ClassId", s.ClassId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@TeacherId", s.TeacherId ?? (object)DBNull.Value);
        }

        private static Subject Map(SqlDataReader r) => new Subject
        {
            SubjectId = r.GetInt32(r.GetOrdinal("SubjectId")),
            SubjectName = r.GetString(r.GetOrdinal("SubjectName")),
            SubjectCode = r.GetString(r.GetOrdinal("SubjectCode")),
            ClassId = r.GetIntOrNull("ClassId"),
            ClassName = r.GetStringOrNull("ClassName"),
            TeacherId = r.GetIntOrNull("TeacherId"),
            TeacherName = r.GetStringOrNull("TeacherName"),
        };
    }
}
