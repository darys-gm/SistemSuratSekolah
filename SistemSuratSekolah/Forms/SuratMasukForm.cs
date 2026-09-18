using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SistemSuratSekolah.Data;
using SistemSuratSekolah.Helpers;

namespace SistemSuratSekolah.Forms
{
    public class SuratMasukForm : Form
    {
        private DataGridView grid;
        private TextBox txtCari;
        private DateTimePicker dtFrom, dtTo;
        private int selectedId = 0;

        public SuratMasukForm()
        {
            Text = "Surat Masuk";
            BackColor = Color.WhiteSmoke;

            var top = new Panel { Dock = DockStyle.Top, Height = 90, Padding = new Padding(10) };

            var lbl = new Label { Text = "Cari:", Location = new Point(10, 15), AutoSize = true };
            txtCari = new TextBox { Location = new Point(55, 12), Width = 200 };
            txtCari.TextChanged += (s, e) => LoadData();

            var lblFrom = new Label { Text = "Dari:", Location = new Point(270, 15), AutoSize = true };
            dtFrom = new DateTimePicker { Location = new Point(310, 12), Width = 130, Format = DateTimePickerFormat.Short };
            dtFrom.Value = new DateTime(DateTime.Now.Year, 1, 1);
            dtFrom.ValueChanged += (s, e) => LoadData();

            var lblTo = new Label { Text = "Sampai:", Location = new Point(450, 15), AutoSize = true };
            dtTo = new DateTimePicker { Location = new Point(510, 12), Width = 130, Format = DateTimePickerFormat.Short };
            dtTo.ValueChanged += (s, e) => LoadData();

            var btnTambah = MakeBtn("Tambah", 660, 10, Color.SeaGreen);
            btnTambah.Click += (s, e) => OpenForm(0);

            var btnEdit = MakeBtn("Edit", 740, 10, Color.SteelBlue);
            btnEdit.Click += (s, e) => { if (selectedId > 0) OpenForm(selectedId); };

            var btnHapus = MakeBtn("Hapus", 820, 10, Color.IndianRed);
            btnHapus.Click += BtnHapus_Click;

            var btnDetail = MakeBtn("Detail", 900, 10, Color.DarkSlateBlue);
            btnDetail.Click += BtnDetail_Click;

            var btnUpload = MakeBtn("Upload", 980, 10, Color.DarkOrange);
            btnUpload.Click += BtnUpload_Click;

            var btnRefresh = MakeBtn("Refresh", 1060, 10, Color.Gray);
            btnRefresh.Click += (s, e) => LoadData();

            top.Controls.AddRange(new Control[] {
                lbl, txtCari, lblFrom, dtFrom, lblTo, dtTo,
                btnTambah, btnEdit, btnHapus, btnDetail, btnUpload, btnRefresh
            });

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
                    selectedId = Convert.ToInt32(grid.SelectedRows[0].Cells["id_surat_masuk"].Value);
            };

            Controls.Add(grid);
            Controls.Add(top);
            LoadData();
        }

        private Button MakeBtn(string text, int x, int y, Color c)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(75, 30),
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
                    string sql = @"SELECT id_surat_masuk, nomor_surat, tanggal_surat, tanggal_diterima,
                                          asal_surat, perihal, tujuan, jenis_surat, status, file_surat
                                   FROM surat_masuk
                                   WHERE (nomor_surat LIKE @q OR perihal LIKE @q OR asal_surat LIKE @q)
                                     AND tanggal_diterima BETWEEN @f AND @t
                                   ORDER BY tanggal_diterima DESC";
                    using (var da = new MySqlDataAdapter(sql, conn))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@q", "%" + txtCari.Text + "%");
                        da.SelectCommand.Parameters.AddWithValue("@f", dtFrom.Value.Date);
                        da.SelectCommand.Parameters.AddWithValue("@t", dtTo.Value.Date);
                        var dt = new DataTable();
                        da.Fill(dt);
                        grid.DataSource = dt;
                        if (grid.Columns.Contains("id_surat_masuk"))
                            grid.Columns["id_surat_masuk"].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void OpenForm(int id)
        {
            var f = new SuratMasukEditForm(id);
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnHapus_Click(object sender, EventArgs e)
        {
            if (selectedId <= 0) { MessageBox.Show("Pilih data terlebih dahulu."); return; }
            if (MessageBox.Show("Yakin ingin menghapus surat ini?", "Konfirmasi",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("DELETE FROM surat_masuk WHERE id_surat_masuk=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", selectedId);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadData();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnDetail_Click(object sender, EventArgs e)
        {
            if (selectedId <= 0) { MessageBox.Show("Pilih data terlebih dahulu."); return; }
            var f = new SuratMasukEditForm(selectedId, true);
            f.ShowDialog();
        }

        private void BtnUpload_Click(object sender, EventArgs e)
        {
            if (selectedId <= 0) { MessageBox.Show("Pilih data terlebih dahulu."); return; }
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Dokumen|*.pdf;*.doc;*.docx;*.jpg;*.png|Semua File|*.*";
                if (ofd.ShowDialog() != DialogResult.OK) return;
                string saved = FileHelper.CopyFile(ofd.FileName);
                if (saved == null) return;
                try
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        using (var cmd = new MySqlCommand("UPDATE surat_masuk SET file_surat=@f WHERE id_surat_masuk=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@f", saved);
                            cmd.Parameters.AddWithValue("@id", selectedId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Dokumen berhasil diunggah.");
                    LoadData();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }
    }

    // Form edit / tambah
    public class SuratMasukEditForm : Form
    {
        private int _id;
        private bool _readOnly;
        private TextBox txtNomor, txtAsal, txtPerihal, txtTujuan, txtJenis, txtKet, txtFile;
        private DateTimePicker dtSurat, dtTerima;
        private ComboBox cbStatus;

        public SuratMasukEditForm(int id, bool readOnly = false)
        {
            _id = id;
            _readOnly = readOnly;
            Text = id == 0 ? "Tambah Surat Masuk" : (readOnly ? "Detail Surat Masuk" : "Edit Surat Masuk");
            Size = new Size(560, 480);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            int y = 20;
            AddLabel("Nomor Surat", 20, y);
            txtNomor = AddText(180, y, 330);

            y += 35; AddLabel("Tanggal Surat", 20, y);
            dtSurat = AddDate(180, y, 150);

            y += 35; AddLabel("Tanggal Diterima", 20, y);
            dtTerima = AddDate(180, y, 150);

            y += 35; AddLabel("Asal/Pengirim", 20, y);
            txtAsal = AddText(180, y, 330);

            y += 35; AddLabel("Perihal", 20, y);
            txtPerihal = AddText(180, y, 330);

            y += 35; AddLabel("Tujuan", 20, y);
            txtTujuan = AddText(180, y, 330);

            y += 35; AddLabel("Jenis Surat", 20, y);
            txtJenis = AddText(180, y, 330);

            y += 35; AddLabel("Status", 20, y);
            cbStatus = new ComboBox
            {
                Location = new Point(180, y),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbStatus.Items.AddRange(new[] { "Baru", "Diproses", "Selesai", "Diarsipkan" });
            cbStatus.SelectedIndex = 0;
            Controls.Add(cbStatus);

            y += 35; AddLabel("Keterangan", 20, y);
            txtKet = new TextBox { Location = new Point(180, y), Width = 330, Height = 50, Multiline = true };
            Controls.Add(txtKet);

            y += 60; AddLabel("File", 20, y);
            txtFile = AddText(180, y, 250);
            txtFile.ReadOnly = true;
            var btnPilih = new Button { Text = "...", Location = new Point(435, y), Width = 40, Height = 23 };
            btnPilih.Click += (s, e) =>
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Dokumen|*.pdf;*.doc;*.docx;*.jpg;*.png|Semua File|*.*";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        txtFile.Text = ofd.FileName;
                    }
                }
            };
            var btnBuka = new Button { Text = "Buka", Location = new Point(480, y), Width = 50, Height = 23 };
            btnBuka.Click += (s, e) =>
            {
                string p = FileHelper.GetFullPath(txtFile.Text);
                if (p != null && System.IO.File.Exists(p))
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(p) { UseShellExecute = true });
                else if (System.IO.File.Exists(txtFile.Text))
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(txtFile.Text) { UseShellExecute = true });
                else
                    MessageBox.Show("File tidak ditemukan.");
            };
            Controls.Add(btnPilih);
            Controls.Add(btnBuka);

            y += 40;
            var btnSimpan = new Button
            {
                Text = "Simpan",
                Location = new Point(180, y),
                Width = 100,
                Height = 32,
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSimpan.Click += BtnSimpan_Click;
            var btnBatal = new Button
            {
                Text = "Batal",
                Location = new Point(290, y),
                Width = 100,
                Height = 32,
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBatal.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnSimpan);
            Controls.Add(btnBatal);

            if (_readOnly)
            {
                foreach (Control c in Controls)
                {
                    if (c is TextBox tb) tb.ReadOnly = true;
                    if (c is DateTimePicker dp) dp.Enabled = false;
                    if (c is ComboBox cb) cb.Enabled = false;
                }
                btnSimpan.Visible = false;
                btnPilih.Visible = false;
            }

            if (_id > 0) LoadData();
        }

        private void AddLabel(string text, int x, int y)
        {
            Controls.Add(new Label { Text = text, Location = new Point(x, y + 3), AutoSize = true });
        }
        private TextBox AddText(int x, int y, int w)
        {
            var t = new TextBox { Location = new Point(x, y), Width = w };
            Controls.Add(t);
            return t;
        }
        private DateTimePicker AddDate(int x, int y, int w)
        {
            var d = new DateTimePicker { Location = new Point(x, y), Width = w, Format = DateTimePickerFormat.Short };
            Controls.Add(d);
            return d;
        }

        private void LoadData()
        {
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("SELECT * FROM surat_masuk WHERE id_surat_masuk=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", _id);
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                txtNomor.Text = rd["nomor_surat"].ToString();
                                dtSurat.Value = Convert.ToDateTime(rd["tanggal_surat"]);
                                dtTerima.Value = Convert.ToDateTime(rd["tanggal_diterima"]);
                                txtAsal.Text = rd["asal_surat"].ToString();
                                txtPerihal.Text = rd["perihal"].ToString();
                                txtTujuan.Text = rd["tujuan"].ToString();
                                txtJenis.Text = rd["jenis_surat"].ToString();
                                txtKet.Text = rd["keterangan"].ToString();
                                txtFile.Text = rd["file_surat"].ToString();
                                cbStatus.SelectedItem = rd["status"].ToString();
                                if (cbStatus.SelectedIndex < 0) cbStatus.SelectedIndex = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnSimpan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNomor.Text) || string.IsNullOrWhiteSpace(txtPerihal.Text))
            {
                MessageBox.Show("Nomor Surat dan Perihal wajib diisi.");
                return;
            }

            string savedFile = txtFile.Text;
            // Jika ada file baru dari drive
            if (!string.IsNullOrEmpty(savedFile) && System.IO.File.Exists(savedFile))
            {
                savedFile = FileHelper.CopyFile(savedFile);
            }

            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql;
                    if (_id == 0)
                    {
                        sql = @"INSERT INTO surat_masuk
                               (nomor_surat, tanggal_surat, tanggal_diterima, asal_surat, perihal,
                                tujuan, jenis_surat, keterangan, file_surat, status)
                               VALUES (@n, @ts, @td, @a, @p, @t, @j, @k, @f, @s)";
                    }
                    else
                    {
                        sql = @"UPDATE surat_masuk SET
                               nomor_surat=@n, tanggal_surat=@ts, tanggal_diterima=@td,
                               asal_surat=@a, perihal=@p, tujuan=@t, jenis_surat=@j,
                               keterangan=@k, file_surat=@f, status=@s
                               WHERE id_surat_masuk=@id";
                    }
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@n", txtNomor.Text.Trim());
                        cmd.Parameters.AddWithValue("@ts", dtSurat.Value.Date);
                        cmd.Parameters.AddWithValue("@td", dtTerima.Value.Date);
                        cmd.Parameters.AddWithValue("@a", txtAsal.Text.Trim());
                        cmd.Parameters.AddWithValue("@p", txtPerihal.Text.Trim());
                        cmd.Parameters.AddWithValue("@t", txtTujuan.Text.Trim());
                        cmd.Parameters.AddWithValue("@j", txtJenis.Text.Trim());
                        cmd.Parameters.AddWithValue("@k", txtKet.Text.Trim());
                        cmd.Parameters.AddWithValue("@f", savedFile ?? "");
                        cmd.Parameters.AddWithValue("@s", cbStatus.SelectedItem?.ToString() ?? "Baru");
                        if (_id > 0) cmd.Parameters.AddWithValue("@id", _id);
                        cmd.ExecuteNonQuery();
                    }
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
    }
}