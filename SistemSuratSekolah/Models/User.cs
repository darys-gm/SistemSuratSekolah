namespace SistemSuratSekolah.Models
{
    public class User
    {
        public int IdUser { get; set; }
        public string Nama { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
    }
}