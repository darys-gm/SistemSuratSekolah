using SistemSuratSekolah.Models;

namespace SistemSuratSekolah.Helpers
{
    public static class Session
    {
        public static User CurrentUser { get; set; }

        public static bool IsAdmin => CurrentUser?.Role == "admin";
        public static bool IsKepsek => CurrentUser?.Role == "kepala_sekolah";
        public static bool IsGuru => CurrentUser?.Role == "guru";
    }
}