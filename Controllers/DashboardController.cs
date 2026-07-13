using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Student_Management_System.Data;

namespace Student_Management_System.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Roles = "Admin")]

    public class DashboardController : ControllerBase
    {
        private readonly DbHelper _db;
        public DashboardController(DbHelper db) => _db = db;

        // One single query that returns every dashboard number at once -
        // cheaper than calling 6 separate endpoints from React.
        [HttpGet("summary")]
        public IActionResult Summary()
        {
            using SqlConnection conn = _db.GetConnection();
            string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM Students)  AS TotalStudents,
                    (SELECT COUNT(*) FROM Teachers)  AS TotalTeachers,
                    (SELECT COUNT(*) FROM Classes)   AS TotalClasses,
                    (SELECT COUNT(*) FROM Subjects)  AS TotalSubjects,
                    (SELECT COUNT(*) FROM Results)   AS TotalResults,
                    (SELECT COUNT(*) FROM Users)     AS TotalUsers,
                    (SELECT ISNULL(SUM(AmountPaid),0)           FROM Fees) AS FeesCollected,
                    (SELECT ISNULL(SUM(AmountDue - AmountPaid),0) FROM Fees) AS FeesDue,
                    (SELECT COUNT(*) FROM Attendance WHERE AttendanceDate = CAST(GETDATE() AS DATE) AND Status='Present') AS PresentToday,
                    (SELECT COUNT(*) FROM Attendance WHERE AttendanceDate = CAST(GETDATE() AS DATE)) AS MarkedToday";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            conn.Open();
            using SqlDataReader r = cmd.ExecuteReader();

            if (!r.Read()) return Ok(new { });

            int presentToday = r.GetInt32(r.GetOrdinal("PresentToday"));
            int markedToday = r.GetInt32(r.GetOrdinal("MarkedToday"));

            return Ok(new
            {
                totalStudents = r.GetInt32(r.GetOrdinal("TotalStudents")),
                totalTeachers = r.GetInt32(r.GetOrdinal("TotalTeachers")),
                totalClasses = r.GetInt32(r.GetOrdinal("TotalClasses")),
                totalSubjects = r.GetInt32(r.GetOrdinal("TotalSubjects")),
                totalResults = r.GetInt32(r.GetOrdinal("TotalResults")),
                totalUsers = r.GetInt32(r.GetOrdinal("TotalUsers")),
                feesCollected = r.GetDecimal(r.GetOrdinal("FeesCollected")),
                feesDue = r.GetDecimal(r.GetOrdinal("FeesDue")),
                attendanceRateToday = markedToday == 0 ? 0 : Math.Round((double)presentToday / markedToday * 100, 1)
            });
        }

    }
}
