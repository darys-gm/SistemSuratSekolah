using System;

namespace SistemSuratSekolah.Models
{
    public class SuratMasuk
    {
        public int IdSuratMasuk { get; set; }
        public string NomorSurat { get; set; }
        public DateTime TanggalSurat { get; set; }
        public DateTime TanggalDiterima { get; set; }
        public string AsalSurat { get; set; }
        public string Perihal { get; set; }
        public string Tujuan { get; set; }
        public string JenisSurat { get; set; }
        public string Keterangan { get; set; }
        public string FileSurat { get; set; }
        public string Status { get; set; }
    }
}