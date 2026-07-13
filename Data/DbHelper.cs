using Microsoft.Data.SqlClient;

namespace Student_Management_System.Data
{
    /// <summary>
    /// Same role as your "conn" field in Form1 — but instead of one
    /// connection living for the whole Form, every repository method
    /// asks this helper for a BRAND NEW connection, uses it, and closes
    /// it immediately. That's the correct pattern in a web API, because
    /// many users hit the API at the same time and can't share one
    /// connection like a single desktop Form can.
    /// </summary>

    public class DbHelper
    {
        private readonly string _connectionString;

        public DbHelper(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("Default")!;
        }

        public SqlConnection GetConnection() => new SqlConnection(_connectionString);

    }
}
