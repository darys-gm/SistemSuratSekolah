using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SistemSuratSekolah.Data;
using SistemSuratSekolah.Helpers;

namespace SistemSuratSekolah.Forms
{
    public class ArsipForm : Form
    {
        private DataGridView grid;
        private TextBox txtCari;
        private ComboBox cbJenis, cbTahun;
        private string selectedFile = "";

        public ArsipForm()
        {
            Text = "Arsip Surat";
            BackColor = Color.WhiteSmoke;

            var top = new Panel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(10) };
            var lbl = new Label { Text = "Cari:", Location = new Point(10, 15), AutoSize = true };
            txtCari = new TextBox { Location = new Point(55, 12), Width = 200 };
            txtCari.TextChanged += (s, e) => LoadData();

            var lblJ = new Label { Text = "Jenis:", Location = new Point(270, 15), AutoSize = true };
            cbJenis = new ComboBox { Location = new Point(315, 12), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cbJenis.Items.AddRange(new object[] { "Semua", "Surat Masuk", "Surat Keluar" });
            cbJenis.SelectedIndex = 0;
            cbJenis.SelectedIndexChanged += (s, e) => LoadData();

            var lblT = new Label { Text = "Tahun:", Location = new Point(460, 15), AutoSize = true };
            cbTahun = new ComboBox { Location = new Point(510, 12), Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };
            cbTahun.Items.Add("Semua");
            for (int i = DateTime.Now.Year; i >= DateTime.Now.Year - 10; i--) cbTahun.Items.Add(i.ToString());
            cbTahun.SelectedIndex = 0;
            cbTahun.SelectedIndexChanged += (s, e) => LoadData();

            var btnBuka = new Button
            {
                Text = "Buka",
                Location = new Point(620, 10),
                Size = new Size(80, 30),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBuka.Click += BtnBuka_Click;

            var btnDownload = new Button
            {
                Text = "Download",
                Location = new Point(710, 10),
                Size = new Size(90, 30),
                BackColor = Color.DarkOrange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDownload.Click += BtnDownload_Click;

            var btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(810, 10),
                Size = new Size(80, 30),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadData();

            top.Controls.AddRange(new Control[] { lbl, txtCari, lblJ, cbJenis, lblT, cbTahun, btnBuka, btnDownload, btnRefresh });

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
                if (grid.SelectedRows.Count > 0 && grid.Columns.Contains("file_surat"))
                    selectedFile = grid.SelectedRows[0].Cells["file_surat"].Value?.ToString() ?? "";
            };

            Controls.Add(grid);
            Controls.Add(top);
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                string keyword = "%" + txtCari.Text + "%";
                string jenis = cbJenis.SelectedItem?.ToString() ?? "Semua";
                string tahunFilter = cbTahun.SelectedItem?.ToString() ?? "Semua";

                string whereTahun = "";
                if (tahunFilter != "Semua") whereTahun = " AND YEAR(tanggal) = " + tahunFilter;

                string sqlMasuk = $@"SELECT 'Surat Masuk' AS jenis_surat_ket, id_surat_masuk AS id,
                                            nomor_surat, tanggal_surat AS tanggal, perihal,
                                            asal_surat AS asal_tujuan, status, file_surat
                                     FROM surat_masuk
                                     WHERE (nomor_surat LIKE @q OR perihal LIKE @q OR asal_surat LIKE @q)
                                           AND status IN ('Selesai','Diarsipkan') {whereTahun}";

                string sqlKeluar = $@"SELECT 'Surat Keluar' AS jenis_surat_ket, id_surat_keluar AS id,
                                            nomor_surat, tanggal_surat AS tanggal, perihal,
                                            tujuan_surat AS asal_tujuan, status, file_surat
                                      FROM surat_keluar
                                      WHERE (nomor_surat LIKE @q OR perihal LIKE @q OR tujuan_surat LIKE @q)
                                            AND status IN ('Diarsipkan','Dikirim') {whereTahun}";

                string finalSql;
                if (jenis == "Surat Masuk") finalSql = sqlMasuk;
                else if (jenis == "Surat Keluar") finalSql = sqlKeluar;
                else finalSql = sqlMasuk + " UNION ALL " + sqlKeluar + " ORDER BY tanggal DESC";

                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var da = new MySqlDataAdapter(finalSql, conn))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@q", keyword);
                        var dt = new DataTable();
                        da.Fill(dt);
                        grid.DataSource = dt;
                        if (grid.Columns.Contains("id")) grid.Columns["id"].Visible = false;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnBuka_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFile)) { MessageBox.Show("Tidak ada dokumen."); return; }
            string p = FileHelper.GetFullPath(selectedFile);
            if (p != null && System.IO.File.Exists(p))
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(p) { UseShellExecute = true });
            else
                MessageBox.Show("File tidak ditemukan: " + selectedFile);
        }

        private void BtnDownload_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFile)) { MessageBox.Show("Tidak ada dokumen."); return; }
            string p = FileHelper.GetFullPath(selectedFile);
            if (p == null || !System.IO.File.Exists(p)) { MessageBox.Show("File tidak ditemukan."); return; }

            using (var sfd = new SaveFileDialog())
            {
                sfd.FileName = System.IO.Path.GetFileName(p);
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.Copy(p, sfd.FileName, true);
                    MessageBox.Show("File tersimpan di: " + sfd.FileName);
                }
            }
        }
    }
}