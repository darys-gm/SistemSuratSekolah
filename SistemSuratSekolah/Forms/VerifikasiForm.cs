using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SistemSuratSekolah.Data;
using SistemSuratSekolah.Helpers;

namespace SistemSuratSekolah.Forms
{
    public class VerifikasiForm : Form
    {
        private DataGridView grid;
        private int selectedId = 0;

        public VerifikasiForm()
        {
            Text = "Verifikasi Surat Keluar";
            BackColor = Color.WhiteSmoke;

            var top = new Panel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(10) };
            var btnSetuju = MakeBtn("Setujui", 10, 10, 100, Color.SeaGreen);
            btnSetuju.Click += (s, e) => ProcessVerifikasi("Disetujui");

            var btnTolak = MakeBtn("Tolak", 120, 10, 100, Color.IndianRed);
            btnTolak.Click += (s, e) => ProcessVerifikasi("Ditolak");

            var btnKirim = MakeBtn("Tandai Dikirim", 230, 10, 130, Color.DarkOrange);
            btnKirim.Click += (s, e) => ProcessVerifikasi("Dikirim");

            var btnArsip = MakeBtn("Arsipkan", 370, 10, 100, Color.DarkSlateBlue);
            btnArsip.Click += (s, e) => ProcessVerifikasi("Diarsipkan");

            var btnRefresh = MakeBtn("Refresh", 480, 10, 80, Color.Gray);
            btnRefresh.Click += (s, e) => LoadData();

            top.Controls.AddRange(new Control[] { btnSetuju, btnTolak, btnKirim, btnArsip, btnRefresh });

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };
            grid.SelectionChanged += (s, e) =>
            {
                if (grid.SelectedRows.Count > 0)
                    selectedId = Convert.ToInt32(grid.SelectedRows[0].Cells["id_surat_keluar"].Value);
            };

            Controls.Add(grid);
            Controls.Add(top);
            LoadData();
        }

        private Button MakeBtn(string t, int x, int y, int w, Color c)
        {
            return new Button
            {
                Text = t,
                Location = new Point(x, y),
                Size = new Size(w, 30),
                BackColor = c,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
        }

        private void LoadData()
        {
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT id_surat_keluar, nomor_surat, tanggal_surat, tujuan_surat,
                                          perihal, jenis_surat, penandatangan, status
                                   FROM surat_keluar
                                   WHERE status IN ('Draft','Diajukan','Disetujui','Ditolak','Dikirim')
                                   ORDER BY tanggal_surat DESC";
                    using (var da = new MySqlDataAdapter(sql, conn))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        grid.DataSource = dt;
                        if (grid.Columns.Contains("id_surat_keluar"))
                            grid.Columns["id_surat_keluar"].Visible = false;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void ProcessVerifikasi(string status)
        {
            if (selectedId <= 0) { MessageBox.Show("Pilih surat dulu."); return; }

            string catatan = "";
            if (status == "Ditolak")
            {
                catatan = InputBoxForm.Show("Catatan Penolakan",
                                "Masukkan catatan penolakan:", "");
                if (string.IsNullOrWhiteSpace(catatan)) return;
            }

            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("UPDATE surat_keluar SET status=@st WHERE id_surat_keluar=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@st", status);
                        cmd.Parameters.AddWithValue("@id", selectedId);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new MySqlCommand(@"INSERT INTO verifikasi
                        (id_surat_keluar, id_user, tanggal_verifikasi, status, catatan)
                        VALUES (@s, @u, @t, @st, @c)", conn))
                    {
                        cmd.Parameters.AddWithValue("@s", selectedId);
                        cmd.Parameters.AddWithValue("@u", Session.CurrentUser?.IdUser ?? 1);
                        cmd.Parameters.AddWithValue("@t", DateTime.Now);
                        cmd.Parameters.AddWithValue("@st", status);
                        cmd.Parameters.AddWithValue("@c", catatan);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Status surat berhasil diubah menjadi: " + status);
                LoadData();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
    }
}