using Microsoft.Data.SqlClient;
using Student_Management_System.Data;
using Student_Management_System.Models;

namespace Student_Management_System.Repositories
{
    public class FeeRepository
    {
        private readonly DbHelper _db;
        public FeeRepository(DbHelper db) => _db = db;

        public List<Fee> GetAll(string? statusFilter = null)
        {
            var list = new List<Fee>();
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT f.FeeId, f.StudentId, s.FullName AS StudentName, f.FeeType,
                       f.AmountDue, f.AmountPaid, f.DueDate, f.PaidDate, f.Status
                FROM Fees f LEFT JOIN Students s ON f.StudentId = s.StudentId";
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
                sql += " WHERE f.Status = @Status";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
                cmd.Parameters.AddWithValue("@Status", statusFilter);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        // Used on the Student dashboard: "show only MY fees"
        public List<Fee> GetByStudentId(int studentId)
        {
            var list = new List<Fee>();
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT f.FeeId, f.StudentId, s.FullName AS StudentName, f.FeeType,
                       f.AmountDue, f.AmountPaid, f.DueDate, f.PaidDate, f.Status
                FROM Fees f LEFT JOIN Students s ON f.StudentId = s.StudentId
                WHERE f.StudentId = @StudentId";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        public Fee? GetById(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT f.FeeId, f.StudentId, s.FullName AS StudentName, f.FeeType,
                       f.AmountDue, f.AmountPaid, f.DueDate, f.PaidDate, f.Status
                FROM Fees f LEFT JOIN Students s ON f.StudentId = s.StudentId
                WHERE f.FeeId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();
            return r.Read() ? Map(r) : null;
        }

        public int Create(Fee f)
        {
            using SqlConnection conn = _db.GetConnection();

            // #3 FIX: Auto-calculate status — never trust what the client sends
            f.Status = f.AmountPaid <= 0 ? "Unpaid"
                     : f.AmountPaid >= f.AmountDue ? "Paid"
                     : "Partial";

            // Duplicate check: same student + same fee type in the same calendar month
            string checkSql = @"
                SELECT COUNT(1) FROM Fees
                WHERE StudentId = @StudentId
                  AND FeeType   = @FeeType
                  AND YEAR(DueDate)  = YEAR(@DueDate)
                  AND MONTH(DueDate) = MONTH(@DueDate)";
            using SqlCommand checkCmd = new SqlCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@StudentId", f.StudentId);
            checkCmd.Parameters.AddWithValue("@FeeType", f.FeeType);
            checkCmd.Parameters.Add("@DueDate", System.Data.SqlDbType.Date).Value =
                f.DueDate == default ? DateTime.UtcNow.Date : f.DueDate;
            conn.Open();
            if ((int)checkCmd.ExecuteScalar() > 0)
                throw new InvalidOperationException(
                    $"A {f.FeeType} fee for this student already exists for " +
                    $"{(f.DueDate == default ? DateTime.UtcNow : f.DueDate):MMMM yyyy}. " +
                    "Edit the existing record instead.");

            string sql = @"
                INSERT INTO Fees (StudentId, FeeType, AmountDue, AmountPaid, DueDate, PaidDate, Status)
                VALUES (@StudentId, @FeeType, @AmountDue, @AmountPaid, @DueDate, @PaidDate, @Status);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            AddParams(cmd, f);
            return (int)cmd.ExecuteScalar();
        }

        public bool Update(int id, Fee f)
        {
            // Auto-recalculate status on every update too
            f.Status = f.AmountPaid <= 0 ? "Unpaid"
                     : f.AmountPaid >= f.AmountDue ? "Paid"
                     : "Partial";
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                UPDATE Fees SET StudentId=@StudentId, FeeType=@FeeType, AmountDue=@AmountDue,
                    AmountPaid=@AmountPaid, DueDate=@DueDate, PaidDate=@PaidDate, Status=@Status
                WHERE FeeId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            AddParams(cmd, f);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = "DELETE FROM Fees WHERE FeeId=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        private static void AddParams(SqlCommand cmd, Fee f)
        {
            cmd.Parameters.AddWithValue("@StudentId", f.StudentId);
            cmd.Parameters.AddWithValue("@FeeType", f.FeeType);
            cmd.Parameters.AddWithValue("@AmountDue", f.AmountDue);
            cmd.Parameters.AddWithValue("@AmountPaid", f.AmountPaid);

            var due = f.DueDate == default ? DateTime.UtcNow.Date : f.DueDate;
            cmd.Parameters.Add("@DueDate", System.Data.SqlDbType.Date).Value = due;
            cmd.Parameters.Add("@PaidDate", System.Data.SqlDbType.Date).Value = (object?)f.PaidDate ?? DBNull.Value;
            cmd.Parameters.AddWithValue("@Status", f.Status);
        }

        private static Fee Map(SqlDataReader r) => new Fee
        {
            FeeId = r.GetInt32(r.GetOrdinal("FeeId")),
            StudentId = r.GetInt32(r.GetOrdinal("StudentId")),
            StudentName = r.GetStringOrNull("StudentName"),
            FeeType = r.GetString(r.GetOrdinal("FeeType")),
            AmountDue = r.GetDecimal(r.GetOrdinal("AmountDue")),
            AmountPaid = r.GetDecimal(r.GetOrdinal("AmountPaid")),
            DueDate = r.GetDateTime(r.GetOrdinal("DueDate")),
            PaidDate = r.GetDateTimeOrNull("PaidDate"),
            Status = r.GetString(r.GetOrdinal("Status")),
        };
    }
}
