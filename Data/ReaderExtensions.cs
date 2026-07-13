using Microsoft.Data.SqlClient;

namespace Student_Management_System.Data
{
    /// <summary>
    /// In your example you read columns with dr[1].ToString(). That breaks
    /// the moment a column is NULL. These small helper methods do the
    /// IsDBNull check for you, so every repository's "map row -> object"
    /// code stays short and doesn't repeat the same NULL-check 8 times.
    /// </summary>

    public static class ReaderExtensions
    {
        public static string? GetStringOrNull(this SqlDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? null : r.GetString(i);
        }

        public static int? GetIntOrNull(this SqlDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? null : r.GetInt32(i);
        }

        public static decimal? GetDecimalOrNull(this SqlDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? null : r.GetDecimal(i);
        }

        public static DateTime? GetDateTimeOrNull(this SqlDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? null : r.GetDateTime(i);
        }



    }
}
