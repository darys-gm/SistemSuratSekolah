using System;
using System.Configuration;
using System.IO;

namespace SistemSuratSekolah.Helpers
{
    public static class FileHelper
    {
        public static string FolderDokumen
        {
            get
            {
                string baseFolder = ConfigurationManager.AppSettings["FolderDokumen"] ?? "DokumenSurat";
                string full = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, baseFolder);
                if (!Directory.Exists(full)) Directory.CreateDirectory(full);
                return full;
            }
        }

        public static string CopyFile(string sourceFile)
        {
            if (string.IsNullOrEmpty(sourceFile) || !File.Exists(sourceFile))
                return null;

            string fileName = Path.GetFileName(sourceFile);
            string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + fileName;
            string dest = Path.Combine(FolderDokumen, uniqueName);
            File.Copy(sourceFile, dest, true);
            return uniqueName;
        }

        public static string GetFullPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            return Path.Combine(FolderDokumen, fileName);
        }
    }
}