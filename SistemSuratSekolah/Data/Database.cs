using System.Configuration;
using MySql.Data.MySqlClient;

namespace SistemSuratSekolah.Data
{
    public static class Database
    {
        private static readonly string ConnStr =
            ConfigurationManager.ConnectionStrings["DbSuratSekolah"].ConnectionString;

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnStr);
        }

        public static bool TestConnection(out string message)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    message = "Koneksi ke database berhasil.";
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                message = "Koneksi gagal: " + ex.Message;
                return false;
            }
        }
    }
}