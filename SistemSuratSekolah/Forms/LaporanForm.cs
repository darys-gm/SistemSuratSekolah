using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SistemSuratSekolah.Data;

namespace SistemSuratSekolah.Forms
{
    public class LaporanForm : Form
    {
        private ComboBox cbJenis, cbBulan;
        private NumericUpDown numTahun;
        private DataGridView grid;
        private DataTable currentData;

        public LaporanForm()
        {
            Text = "Laporan Surat";
            BackColor = Color.WhiteSmoke;

            var top = new Panel { Dock = DockStyle.Top, Height = 110, Padding = new Padding(10) };

            var lblJenis = new Label { Text = "Jenis Laporan:", Location = new Point(20, 20), AutoSize = true };
            cbJenis = new ComboBox
            {
                Location = new Point(130, 17),
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbJenis.Items.AddRange(new object[] { "Surat Masuk", "Surat Keluar" });
            cbJenis.SelectedIndex = 0;

            var lblBulan = new Label { Text = "Bulan:", Location = new Point(310, 20), AutoSize = true };
            cbBulan = new ComboBox
            {
                Location = new Point(360, 17),
                Width = 130,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbBulan.Items.AddRange(new object[] {
                "Semua","Januari","Februari","Maret","April","Mei","Juni",
                "Juli","Agustus","September","Oktober","November","Desember"
            });
            cbBulan.SelectedIndex = 0;

            var lblTahun = new Label { Text = "Tahun:", Location = new Point(510, 20), AutoSize = true };
            numTahun = new NumericUpDown
            {
                Location = new Point(560, 17),
                Width = 80,
                Minimum = 2000,
                Maximum = 2100,
                Value = DateTime.Now.Year
            };

            var btnTampilkan = MakeBtn("Tampilkan", 660, 15, 100, Color.SteelBlue);
            btnTampilkan.Click += (s, e) => LoadData();

            var btnCetak = MakeBtn("Cetak", 770, 15, 80, Color.SeaGreen);
            btnCetak.Click += BtnCetak_Click;

            var btnExcel = MakeBtn("Export CSV", 860, 15, 100, Color.DarkOrange);
            btnExcel.Click += BtnExcel_Click;

            top.Controls.AddRange(new Control[] {
                lblJenis, cbJenis, lblBulan, cbBulan, lblTahun, numTahun,
                btnTampilkan, btnCetak, btnExcel
            });

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };

            Controls.Add(grid);
            Controls.Add(top);
        }

        private Button MakeBtn(string t, int x, int y, int w, Color c)
        {
            return new Button
            {
                Text = t,
                Location = new Point(x, y),
                Size = new Size(w, 32),
                BackColor = c,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
        }

        private void LoadData()
        {
            try
            {
                string jenis = cbJenis.SelectedItem.ToString();
                int bulan = cbBulan.SelectedIndex;
                int tahun = (int)numTahun.Value;

                string sql;
                if (jenis == "Surat Masuk")
                {
                    sql = @"SELECT nomor_surat, tanggal_surat, asal_surat AS pengirim,
                                   perihal, jenis_surat, status
                            FROM surat_masuk
                            WHERE YEAR(tanggal_diterima)=@y
                              AND (@m=0 OR MONTH(tanggal_diterima)=@m)
                            ORDER BY tanggal_diterima";
                }
                else
                {
                    sql = @"SELECT nomor_surat, tanggal_surat, tujuan_surat AS tujuan,
                                   perihal, jenis_surat, status
                            FROM surat_keluar
                            WHERE YEAR(tanggal_surat)=@y
                              AND (@m=0 OR MONTH(tanggal_surat)=@m)
                            ORDER BY tanggal_surat";
                }

                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var da = new MySqlDataAdapter(sql, conn))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@y", tahun);
                        da.SelectCommand.Parameters.AddWithValue("@m", bulan);
                        currentData = new DataTable();
                        da.Fill(currentData);
                        grid.DataSource = currentData;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnCetak_Click(object sender, EventArgs e)
        {
            if (currentData == null || currentData.Rows.Count == 0)
            {
                MessageBox.Show("Tidak ada data untuk dicetak.");
                return;
            }

            var pd = new PrintDocument();
            pd.PrintPage += (s, ev) =>
            {
                var g = ev.Graphics;
                var fontHeader = new Font("Arial", 14, FontStyle.Bold);
                var fontSub = new Font("Arial", 10);
                var fontBody = new Font("Consolas", 9);

                int y = 40;
                string jenis = cbJenis.SelectedItem.ToString();
                string periode = cbBulan.SelectedItem + " " + numTahun.Value;

                g.DrawString("LAPORAN " + jenis.ToUpper(), fontHeader, Brushes.Black, 180, y); y += 25;
                g.DrawString("Periode: " + periode, fontSub, Brushes.Black, 40, y); y += 30;

                // Header kolom
                int[] colX = { 40, 90, 220, 340, 480, 600 };
                g.DrawString("No", fontBody, Brushes.Black, colX[0], y);
                g.DrawString("Nomor", fontBody, Brushes.Black, colX[1], y);
                g.DrawString("Tanggal", fontBody, Brushes.Black, colX[2], y);
                g.DrawString("Pengirim/Tujuan", fontBody, Brushes.Black, colX[3], y);
                g.DrawString("Perihal", fontBody, Brushes.Black, colX[4], y);
                g.DrawString("Status", fontBody, Brushes.Black, colX[5], y);
                y += 18;
                g.DrawLine(Pens.Black, 40, y, 780, y);
                y += 5;

                int no = 1;
                foreach (DataRow row in currentData.Rows)
                {
                    if (y > ev.MarginBounds.Bottom - 40)
                    {
                        ev.HasMorePages = true;
                        return;
                    }
                    g.DrawString(no.ToString(), fontBody, Brushes.Black, colX[0], y);
                    g.DrawString(Truncate(row[0].ToString(), 25), fontBody, Brushes.Black, colX[1], y);
                    g.DrawString(Convert.ToDateTime(row[1]).ToString("dd/MM/yyyy"), fontBody, Brushes.Black, colX[2], y);
                    g.DrawString(Truncate(row[2].ToString(), 22), fontBody, Brushes.Black, colX[3], y);
                    g.DrawString(Truncate(row[3].ToString(), 20), fontBody, Brushes.Black, colX[4], y);
                    g.DrawString(Truncate(row[5].ToString(), 12), fontBody, Brushes.Black, colX[5], y);
                    y += 16;
                    no++;
                }
            };

            using (var ppd = new PrintPreviewDialog())
            {
                ppd.Document = pd;
                ppd.WindowState = FormWindowState.Maximized;
                ppd.ShowDialog();
            }
        }

        private string Truncate(string s, int len)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length > len ? s.Substring(0, len) + "…" : s;
        }

        private void BtnExcel_Click(object sender, EventArgs e)
        {
            if (currentData == null || currentData.Rows.Count == 0)
            {
                MessageBox.Show("Tidak ada data.");
                return;
            }
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV File|*.csv";
                sfd.FileName = "Laporan_" + cbJenis.SelectedItem + "_" + cbBulan.SelectedItem + "_" + numTahun.Value + ".csv";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();
                // Header
                for (int i = 0; i < currentData.Columns.Count; i++)
                {
                    sb.Append(currentData.Columns[i].ColumnName);
                    if (i < currentData.Columns.Count - 1) sb.Append(",");
                }
                sb.AppendLine();
                foreach (DataRow row in currentData.Rows)
                {
                    for (int i = 0; i < currentData.Columns.Count; i++)
                    {
                        string v = row[i].ToString().Replace(",", ";");
                        sb.Append(v);
                        if (i < currentData.Columns.Count - 1) sb.Append(",");
                    }
                    sb.AppendLine();
                }
                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("File berhasil disimpan.");
            }
        }
    }
}