using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SistemSuratSekolah.Data;
using SistemSuratSekolah.Helpers;

namespace SistemSuratSekolah.Forms
{
    public class DashboardForm : Form
    {
        private Label lblTotalMasuk, lblTotalKeluar, lblDisposisi, lblVerifikasi;
        private Panel contentPanel;

        public DashboardForm()
        {
            Text = "Dashboard - Sistem Informasi Surat Sekolah";
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.WhiteSmoke;

            // Sidebar
            var sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(45, 55, 90)
            };

            var lblBrand = new Label
            {
                Text = "SURAT SEKOLAH",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(30, 40, 70)
            };
            sidebar.Controls.Add(lblBrand);

            var userInfo = new Label
            {
                Text = "Login: " + Session.CurrentUser?.Nama + "\nRole: " + Session.CurrentUser?.Role,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter
            };
            sidebar.Controls.Add(userInfo);
            userInfo.BringToFront();

            // Menu buttons
            AddMenuButton(sidebar, "🏠  Dashboard", (s, e) => ShowDashboard());
            AddMenuButton(sidebar, "📥  Surat Masuk", (s, e) => OpenChild(new SuratMasukForm()));
            AddMenuButton(sidebar, "📤  Surat Keluar", (s, e) => OpenChild(new SuratKeluarForm()));
            AddMenuButton(sidebar, "📋  Disposisi", (s, e) => OpenChild(new DisposisiForm()));
            AddMenuButton(sidebar, "✅  Verifikasi", (s, e) => OpenChild(new VerifikasiForm()));
            AddMenuButton(sidebar, "🗂️  Arsip", (s, e) => OpenChild(new ArsipForm()));
            AddMenuButton(sidebar, "📊  Laporan", (s, e) => OpenChild(new LaporanForm()));
            AddMenuButton(sidebar, "🚪  Logout", (s, e) =>
            {
                Session.CurrentUser = null;
                Hide();
                var login = new LoginForm();
                login.FormClosed += (s2, e2) => Close();
                login.Show();
            });

            // Header
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(70, 90, 150)
            };
            var lblHeader = new Label
            {
                Text = "  Dashboard",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(lblHeader);

            contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = Color.WhiteSmoke };

            Controls.Add(contentPanel);
            Controls.Add(header);
            Controls.Add(sidebar);

            ShowDashboard();
        }

        private void AddMenuButton(Panel parent, string text, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 55, 90),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Font = new Font("Segoe UI", 10)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            parent.Controls.Add(btn);
            btn.BringToFront();
        }

        private void OpenChild(Form child)
        {
            contentPanel.Controls.Clear();
            child.TopLevel = false;
            child.FormBorderStyle = FormBorderStyle.None;
            child.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(child);
            child.Show();
        }

        private void ShowDashboard()
        {
            contentPanel.Controls.Clear();

            var title = new Label
            {
                Text = "Selamat Datang, " + Session.CurrentUser?.Nama,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(45, 55, 90)
            };
            contentPanel.Controls.Add(title);

            lblTotalMasuk = MakeCard("Surat Masuk", "0", Color.SteelBlue, new Point(20, 80));
            lblTotalKeluar = MakeCard("Surat Keluar", "0", Color.SeaGreen, new Point(260, 80));
            lblDisposisi = MakeCard("Disposisi Aktif", "0", Color.Orange, new Point(500, 80));
            lblVerifikasi = MakeCard("Menunggu Verifikasi", "0", Color.IndianRed, new Point(740, 80));

            contentPanel.Controls.Add(lblTotalMasuk.Parent);
            contentPanel.Controls.Add(lblTotalKeluar.Parent);
            contentPanel.Controls.Add(lblDisposisi.Parent);
            contentPanel.Controls.Add(lblVerifikasi.Parent);

            LoadStatistics();
        }

        private Label MakeCard(string title, string value, Color color, Point loc)
        {
            var card = new Panel
            {
                Location = loc,
                Size = new Size(220, 110),
                BackColor = color
            };
            var lblT = new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Location = new Point(15, 15),
                AutoSize = true
            };
            var lblV = new Label
            {
                Text = value,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                Location = new Point(15, 45),
                AutoSize = true
            };
            card.Controls.Add(lblT);
            card.Controls.Add(lblV);
            return lblV;
        }

        private int Scalar(string sql)
        {
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(sql, conn))
                        return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch { return 0; }
        }

        private void LoadStatistics()
        {
            lblTotalMasuk.Text = Scalar("SELECT COUNT(*) FROM surat_masuk").ToString();
            lblTotalKeluar.Text = Scalar("SELECT COUNT(*) FROM surat_keluar").ToString();
            lblDisposisi.Text = Scalar("SELECT COUNT(*) FROM disposisi WHERE status <> 'Selesai'").ToString();
            lblVerifikasi.Text = Scalar("SELECT COUNT(*) FROM surat_keluar WHERE status='Diajukan'").ToString();
        }
    }
}