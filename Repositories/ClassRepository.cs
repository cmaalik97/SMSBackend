using Microsoft.Data.SqlClient;
using Student_Management_System.Data;
using Student_Management_System.Models;

namespace Student_Management_System.Repositories
{
    public class ClassRepository
    {
        private readonly DbHelper _db;
        public ClassRepository(DbHelper db) => _db = db;

        public List<ClassRoom> GetAll(string? search = null)
        {
            var list = new List<ClassRoom>();
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT c.ClassId, c.ClassName, c.Section, c.RoomNo, c.ClassTeacherId, t.FullName AS ClassTeacherName
                FROM Classes c LEFT JOIN Teachers t ON c.ClassTeacherId = t.TeacherId";
            if (!string.IsNullOrWhiteSpace(search))
                sql += " WHERE c.ClassName LIKE @Search OR c.Section LIKE @Search";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(search))
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        public ClassRoom? GetById(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT c.ClassId, c.ClassName, c.Section, c.RoomNo, c.ClassTeacherId, t.FullName AS ClassTeacherName
                FROM Classes c LEFT JOIN Teachers t ON c.ClassTeacherId = t.TeacherId
                WHERE c.ClassId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            return r.Read() ? Map(r) : null;
        }

        public int Create(ClassRoom c)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                INSERT INTO Classes (ClassName, Section, RoomNo, ClassTeacherId)
                VALUES (@ClassName, @Section, @RoomNo, @ClassTeacherId);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            AddParams(cmd, c);

            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public bool Update(int id, ClassRoom c)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                UPDATE Classes SET ClassName=@ClassName, Section=@Section, RoomNo=@RoomNo, ClassTeacherId=@ClassTeacherId
                WHERE ClassId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            AddParams(cmd, c);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = "DELETE FROM Classes WHERE ClassId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        private static void AddParams(SqlCommand cmd, ClassRoom c)
        {
            cmd.Parameters.AddWithValue("@ClassName", c.ClassName);
            cmd.Parameters.AddWithValue("@Section", c.Section ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@RoomNo", c.RoomNo ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ClassTeacherId", c.ClassTeacherId ?? (object)DBNull.Value);
        }

        private static ClassRoom Map(SqlDataReader r) => new ClassRoom
        {
            ClassId = r.GetInt32(r.GetOrdinal("ClassId")),
            ClassName = r.GetString(r.GetOrdinal("ClassName")),
            Section = r.GetStringOrNull("Section"),
            RoomNo = r.GetStringOrNull("RoomNo"),
            ClassTeacherId = r.GetIntOrNull("ClassTeacherId"),
            ClassTeacherName = r.GetStringOrNull("ClassTeacherName"),
        };
    }
}
