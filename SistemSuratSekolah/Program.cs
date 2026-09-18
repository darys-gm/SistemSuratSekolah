using System;
using System.Windows.Forms;
using SistemSuratSekolah.Forms;

namespace SistemSuratSekolah
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}