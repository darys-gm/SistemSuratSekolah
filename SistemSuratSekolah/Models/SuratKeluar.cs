using System;

namespace SistemSuratSekolah.Models
{
    public class SuratKeluar
    {
        public int IdSuratKeluar { get; set; }
        public string NomorSurat { get; set; }
        public DateTime TanggalSurat { get; set; }
        public string TujuanSurat { get; set; }
        public string Perihal { get; set; }
        public string JenisSurat { get; set; }
        public string Penandatangan { get; set; }
        public string Keterangan { get; set; }
        public string FileSurat { get; set; }
        public string Status { get; set; }
    }
}