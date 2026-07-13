using Microsoft.Data.SqlClient;
using Student_Management_System.Data;
using Student_Management_System.Models;

namespace Student_Management_System.Repositories
{
    public class ResultRepository
    {
        private readonly DbHelper _db;
        public ResultRepository(DbHelper db) => _db = db;

        public List<Result> GetAll(string? examTypeFilter = null)
        {
            var list = new List<Result>();
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT r.ResultId, r.StudentId, s.FullName AS StudentName, r.SubjectId, sub.SubjectName,
                       r.ExamType, r.Marks, r.MaxMarks, r.Grade, r.ExamDate
                FROM Results r
                LEFT JOIN Students s ON r.StudentId = s.StudentId
                LEFT JOIN Subjects sub ON r.SubjectId = sub.SubjectId";
            if (!string.IsNullOrWhiteSpace(examTypeFilter) && examTypeFilter != "All")
                sql += " WHERE r.ExamType = @ExamType";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(examTypeFilter) && examTypeFilter != "All")
                cmd.Parameters.AddWithValue("@ExamType", examTypeFilter);

            conn.Open();
            using SqlDataReader r2 = cmd.ExecuteReader();
            while (r2.Read()) list.Add(Map(r2));
            return list;
        }

        // Used on the Student dashboard: "show only MY results"
        public List<Result> GetByStudentId(int studentId)
        {
            var list = new List<Result>();
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT r.ResultId, r.StudentId, s.FullName AS StudentName, r.SubjectId, sub.SubjectName,
                       r.ExamType, r.Marks, r.MaxMarks, r.Grade, r.ExamDate
                FROM Results r
                LEFT JOIN Students s ON r.StudentId = s.StudentId
                LEFT JOIN Subjects sub ON r.SubjectId = sub.SubjectId
                WHERE r.StudentId = @StudentId
                ORDER BY r.ExamDate DESC";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId);

            conn.Open();
            using SqlDataReader r2 = cmd.ExecuteReader();
            while (r2.Read()) list.Add(Map(r2));
            return list;
        }

        public Result? GetById(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT r.ResultId, r.StudentId, s.FullName AS StudentName, r.SubjectId, sub.SubjectName,
                       r.ExamType, r.Marks, r.MaxMarks, r.Grade, r.ExamDate
                FROM Results r
                LEFT JOIN Students s ON r.StudentId = s.StudentId
                LEFT JOIN Subjects sub ON r.SubjectId = sub.SubjectId
                WHERE r.ResultId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            using SqlDataReader r2 = cmd.ExecuteReader();
            return r2.Read() ? Map(r2) : null;
        }

        public int Create(Result res)
        {
            using SqlConnection conn = _db.GetConnection();

            // #8 FIX: Prevent duplicate results for the same student + subject +
            // exam type in the same calendar year (academic year).
            string checkSql = @"
                SELECT COUNT(1) FROM Results
                WHERE StudentId = @StudentId
                  AND SubjectId = @SubjectId
                  AND ExamType  = @ExamType
                  AND YEAR(ExamDate) = YEAR(@ExamDate)";
            using SqlCommand checkCmd = new SqlCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@StudentId", res.StudentId);
            checkCmd.Parameters.AddWithValue("@SubjectId", res.SubjectId);
            checkCmd.Parameters.AddWithValue("@ExamType", res.ExamType);
            checkCmd.Parameters.Add("@ExamDate", System.Data.SqlDbType.Date).Value =
                res.ExamDate == default ? DateTime.UtcNow.Date : res.ExamDate;

            conn.Open();
            int existing = (int)checkCmd.ExecuteScalar();
            if (existing > 0)
                throw new InvalidOperationException(
                    $"A {res.ExamType} result for this student in this subject already exists " +
                    $"for the {(res.ExamDate == default ? DateTime.UtcNow.Year : res.ExamDate.Year)} academic year. " +
                    "Edit the existing record instead.");

            string sql = @"
                INSERT INTO Results (StudentId, SubjectId, ExamType, Marks, MaxMarks, Grade, ExamDate)
                VALUES (@StudentId, @SubjectId, @ExamType, @Marks, @MaxMarks, @Grade, @ExamDate);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            AddParams(cmd, res);

            return (int)cmd.ExecuteScalar();
        }

        public bool Update(int id, Result res)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                UPDATE Results SET StudentId=@StudentId, SubjectId=@SubjectId, ExamType=@ExamType,
                    Marks=@Marks, MaxMarks=@MaxMarks, Grade=@Grade, ExamDate=@ExamDate
                WHERE ResultId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            AddParams(cmd, res);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = "DELETE FROM Results WHERE ResultId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        private static void AddParams(SqlCommand cmd, Result res)
        {
            cmd.Parameters.AddWithValue("@StudentId", res.StudentId);
            cmd.Parameters.AddWithValue("@SubjectId", res.SubjectId);
            cmd.Parameters.AddWithValue("@ExamType", res.ExamType);
            cmd.Parameters.AddWithValue("@Marks", res.Marks);
            cmd.Parameters.AddWithValue("@MaxMarks", res.MaxMarks);
            cmd.Parameters.AddWithValue("@Grade", res.Grade ?? (object)DBNull.Value);
            var examDate = res.ExamDate == default ? DateTime.UtcNow.Date : res.ExamDate;
            cmd.Parameters.Add("@ExamDate", System.Data.SqlDbType.Date).Value = examDate;
        }

        private static Result Map(SqlDataReader r) => new Result
        {
            ResultId = r.GetInt32(r.GetOrdinal("ResultId")),
            StudentId = r.GetInt32(r.GetOrdinal("StudentId")),
            StudentName = r.GetStringOrNull("StudentName"),
            SubjectId = r.GetInt32(r.GetOrdinal("SubjectId")),
            SubjectName = r.GetStringOrNull("SubjectName"),
            ExamType = r.GetString(r.GetOrdinal("ExamType")),
            Marks = r.GetDecimal(r.GetOrdinal("Marks")),
            MaxMarks = r.GetDecimal(r.GetOrdinal("MaxMarks")),
            Grade = r.GetStringOrNull("Grade"),
            ExamDate = r.GetDateTime(r.GetOrdinal("ExamDate")),
        };
    }
}
