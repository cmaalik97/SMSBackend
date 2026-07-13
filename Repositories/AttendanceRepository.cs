using Microsoft.Data.SqlClient;
using Student_Management_System.Data;
using Student_Management_System.Models;

namespace Student_Management_System.Repositories
{
    public class AttendanceRepository
    {
        private readonly DbHelper _db;
        public AttendanceRepository(DbHelper db) => _db = db;

        public List<Attendance> GetAll(string? statusFilter = null)
        {
            var list = new List<Attendance>();
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT a.AttendanceId, a.StudentId, s.FullName AS StudentName, a.ClassId,
                       c.ClassName + ' - ' + ISNULL(c.Section,'') AS ClassName,
                       a.AttendanceDate, a.Status, a.MarkedByTeacherId
                FROM Attendance a
                LEFT JOIN Students s ON a.StudentId = s.StudentId
                LEFT JOIN Classes c ON a.ClassId = c.ClassId";
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
                sql += " WHERE a.Status = @Status";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
                cmd.Parameters.AddWithValue("@Status", statusFilter);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        // Used on the Student dashboard: "show only MY attendance"
        public List<Attendance> GetByStudentId(int studentId)
        {
            var list = new List<Attendance>();
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT a.AttendanceId, a.StudentId, s.FullName AS StudentName, a.ClassId,
                       c.ClassName + ' - ' + ISNULL(c.Section,'') AS ClassName,
                       a.AttendanceDate, a.Status, a.MarkedByTeacherId
                FROM Attendance a
                LEFT JOIN Students s ON a.StudentId = s.StudentId
                LEFT JOIN Classes c ON a.ClassId = c.ClassId
                WHERE a.StudentId = @StudentId
                ORDER BY a.AttendanceDate DESC";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        public Attendance? GetById(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT a.AttendanceId, a.StudentId, s.FullName AS StudentName, a.ClassId,
                       c.ClassName + ' - ' + ISNULL(c.Section,'') AS ClassName,
                       a.AttendanceDate, a.Status, a.MarkedByTeacherId
                FROM Attendance a
                LEFT JOIN Students s ON a.StudentId = s.StudentId
                LEFT JOIN Classes c ON a.ClassId = c.ClassId
                WHERE a.AttendanceId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            return r.Read() ? Map(r) : null;
        }

        public int Create(Attendance a)
        {
            using SqlConnection conn = _db.GetConnection();

            // #3 FIX: Block duplicate attendance — one record per student per day only.
            var date = a.AttendanceDate == default ? DateTime.UtcNow.Date : a.AttendanceDate;
            string checkSql = @"SELECT COUNT(1) FROM Attendance
                                WHERE StudentId=@StudentId AND AttendanceDate=@AttDate";
            using SqlCommand checkCmd = new SqlCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@StudentId", a.StudentId);
            checkCmd.Parameters.Add("@AttDate", System.Data.SqlDbType.Date).Value = date;
            conn.Open();
            if ((int)checkCmd.ExecuteScalar() > 0)
                throw new InvalidOperationException(
                    "Attendance for this student on this date already exists. " +
                    "Edit the existing record instead of creating a new one.");

            string sql = @"
                INSERT INTO Attendance (StudentId, ClassId, AttendanceDate, Status, MarkedByTeacherId)
                VALUES (@StudentId, @ClassId, @AttendanceDate, @Status, @MarkedByTeacherId);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            AddParams(cmd, a);
            return (int)cmd.ExecuteScalar();
        }

        public bool Update(int id, Attendance a)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                UPDATE Attendance SET StudentId=@StudentId, ClassId=@ClassId,
                    AttendanceDate=@AttendanceDate, Status=@Status, MarkedByTeacherId=@MarkedByTeacherId
                WHERE AttendanceId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            AddParams(cmd, a);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = "DELETE FROM Attendance WHERE AttendanceId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        private static void AddParams(SqlCommand cmd, Attendance a)
        {
            cmd.Parameters.AddWithValue("@StudentId", a.StudentId);
            cmd.Parameters.AddWithValue("@ClassId", a.ClassId);
            var attDate = a.AttendanceDate == default ? DateTime.UtcNow.Date : a.AttendanceDate;
            cmd.Parameters.Add("@AttendanceDate", System.Data.SqlDbType.Date).Value = attDate;
            cmd.Parameters.AddWithValue("@Status", a.Status);
            cmd.Parameters.AddWithValue("@MarkedByTeacherId", a.MarkedByTeacherId ?? (object)DBNull.Value);
        }

        private static Attendance Map(SqlDataReader r) => new Attendance
        {
            AttendanceId = r.GetInt32(r.GetOrdinal("AttendanceId")),
            StudentId = r.GetInt32(r.GetOrdinal("StudentId")),
            StudentName = r.GetStringOrNull("StudentName"),
            ClassId = r.GetInt32(r.GetOrdinal("ClassId")),
            ClassName = r.GetStringOrNull("ClassName"),
            AttendanceDate = r.GetDateTime(r.GetOrdinal("AttendanceDate")),
            Status = r.GetString(r.GetOrdinal("Status")),
            MarkedByTeacherId = r.GetIntOrNull("MarkedByTeacherId"),
        };
    }
}
