using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SistemSuratSekolah.Data;
using SistemSuratSekolah.Helpers;

namespace SistemSuratSekolah.Forms
{
    public class SuratKeluarForm : Form
    {
        private DataGridView grid;
        private TextBox txtCari;
        private int selectedId = 0;

        public SuratKeluarForm()
        {
            Text = "Surat Keluar";
            BackColor = Color.WhiteSmoke;

            var top = new Panel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(10) };

            var lbl = new Label { Text = "Cari:", Location = new Point(10, 15), AutoSize = true };
            txtCari = new TextBox { Location = new Point(55, 12), Width = 220 };
            txtCari.TextChanged += (s, e) => LoadData();

            var btnTambah = MakeBtn("Tambah", 300, 10, Color.SeaGreen);
            btnTambah.Click += (s, e) => OpenForm(0);

            var btnEdit = MakeBtn("Edit", 380, 10, Color.SteelBlue);
            btnEdit.Click += (s, e) => { if (selectedId > 0) OpenForm(selectedId); };

            var btnHapus = MakeBtn("Hapus", 460, 10, Color.IndianRed);
            btnHapus.Click += BtnHapus_Click;

            var btnAjukan = MakeBtn("Ajukan", 540, 10, Color.DarkOrange);
            btnAjukan.Click += BtnAjukan_Click;

            var btnRefresh = MakeBtn("Refresh", 620, 10, Color.Gray);
            btnRefresh.Click += (s, e) => LoadData();

            top.Controls.AddRange(new Control[] { lbl, txtCari, btnTambah, btnEdit, btnHapus, btnAjukan, btnRefresh });

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

        private Button MakeBtn(string t, int x, int y, Color c)
        {
            return new Button
            {
                Text = t,
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
                    string sql = @"SELECT id_surat_keluar, nomor_surat, tanggal_surat, tujuan_surat,
                                          perihal, jenis_surat, penandatangan, status, file_surat
                                   FROM surat_keluar
                                   WHERE nomor_surat LIKE @q OR perihal LIKE @q OR tujuan_surat LIKE @q
                                   ORDER BY tanggal_surat DESC";
                    using (var da = new MySqlDataAdapter(sql, conn))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@q", "%" + txtCari.Text + "%");
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

        private void OpenForm(int id)
        {
            var f = new SuratKeluarEditForm(id);
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnHapus_Click(object sender, EventArgs e)
        {
            if (selectedId <= 0) { MessageBox.Show("Pilih data dulu."); return; }
            if (MessageBox.Show("Hapus surat ini?", "Konfirmasi", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("DELETE FROM surat_keluar WHERE id_surat_keluar=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", selectedId);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadData();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnAjukan_Click(object sender, EventArgs e)
        {
            if (selectedId <= 0) { MessageBox.Show("Pilih data dulu."); return; }
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("UPDATE surat_keluar SET status='Diajukan' WHERE id_surat_keluar=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", selectedId);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Surat diajukan untuk verifikasi.");
                LoadData();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
    }

    public class SuratKeluarEditForm : Form
    {
        private int _id;
        private TextBox txtNomor, txtTujuan, txtPerihal, txtJenis, txtTTD, txtKet, txtFile;
        private DateTimePicker dtSurat;
        private ComboBox cbStatus;

        public SuratKeluarEditForm(int id)
        {
            _id = id;
            Text = id == 0 ? "Tambah Surat Keluar" : "Edit Surat Keluar";
            Size = new Size(560, 460);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            int y = 20;
            AddLabel("Nomor Surat", 20, y);
            txtNomor = AddText(180, y, 330);

            y += 35; AddLabel("Tanggal Surat", 20, y);
            dtSurat = AddDate(180, y, 150);

            y += 35; AddLabel("Tujuan Surat", 20, y);
            txtTujuan = AddText(180, y, 330);

            y += 35; AddLabel("Perihal", 20, y);
            txtPerihal = AddText(180, y, 330);

            y += 35; AddLabel("Jenis Surat", 20, y);
            txtJenis = AddText(180, y, 330);

            y += 35; AddLabel("Penandatangan", 20, y);
            txtTTD = AddText(180, y, 330);

            y += 35; AddLabel("Status", 20, y);
            cbStatus = new ComboBox { Location = new Point(180, y), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cbStatus.Items.AddRange(new[] { "Draft", "Diajukan", "Disetujui", "Ditolak", "Dikirim", "Diarsipkan" });
            cbStatus.SelectedIndex = 0;
            Controls.Add(cbStatus);

            y += 35; AddLabel("Keterangan", 20, y);
            txtKet = new TextBox { Location = new Point(180, y), Width = 330, Height = 40, Multiline = true };
            Controls.Add(txtKet);

            y += 55; AddLabel("File", 20, y);
            txtFile = AddText(180, y, 250);
            txtFile.ReadOnly = true;
            var btnPilih = new Button { Text = "...", Location = new Point(435, y), Width = 40, Height = 23 };
            btnPilih.Click += (s, e) =>
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Dokumen|*.pdf;*.doc;*.docx;*.jpg;*.png|Semua File|*.*";
                    if (ofd.ShowDialog() == DialogResult.OK) txtFile.Text = ofd.FileName;
                }
            };
            Controls.Add(btnPilih);

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

            if (_id > 0) LoadData();
        }

        private void AddLabel(string t, int x, int y) => Controls.Add(new Label { Text = t, Location = new Point(x, y + 3), AutoSize = true });
        private TextBox AddText(int x, int y, int w) { var t = new TextBox { Location = new Point(x, y), Width = w }; Controls.Add(t); return t; }
        private DateTimePicker AddDate(int x, int y, int w) { var d = new DateTimePicker { Location = new Point(x, y), Width = w, Format = DateTimePickerFormat.Short }; Controls.Add(d); return d; }

        private void LoadData()
        {
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("SELECT * FROM surat_keluar WHERE id_surat_keluar=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", _id);
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                txtNomor.Text = rd["nomor_surat"].ToString();
                                dtSurat.Value = Convert.ToDateTime(rd["tanggal_surat"]);
                                txtTujuan.Text = rd["tujuan_surat"].ToString();
                                txtPerihal.Text = rd["perihal"].ToString();
                                txtJenis.Text = rd["jenis_surat"].ToString();
                                txtTTD.Text = rd["penandatangan"].ToString();
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
            if (!string.IsNullOrEmpty(savedFile) && System.IO.File.Exists(savedFile))
                savedFile = FileHelper.CopyFile(savedFile);

            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql;
                    if (_id == 0)
                    {
                        sql = @"INSERT INTO surat_keluar
                               (nomor_surat, tanggal_surat, tujuan_surat, perihal, jenis_surat,
                                penandatangan, keterangan, file_surat, status)
                               VALUES (@n, @t, @tu, @p, @j, @ttd, @k, @f, @s)";
                    }
                    else
                    {
                        sql = @"UPDATE surat_keluar SET
                               nomor_surat=@n, tanggal_surat=@t, tujuan_surat=@tu, perihal=@p,
                               jenis_surat=@j, penandatangan=@ttd, keterangan=@k,
                               file_surat=@f, status=@s
                               WHERE id_surat_keluar=@id";
                    }
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@n", txtNomor.Text.Trim());
                        cmd.Parameters.AddWithValue("@t", dtSurat.Value.Date);
                        cmd.Parameters.AddWithValue("@tu", txtTujuan.Text.Trim());
                        cmd.Parameters.AddWithValue("@p", txtPerihal.Text.Trim());
                        cmd.Parameters.AddWithValue("@j", txtJenis.Text.Trim());
                        cmd.Parameters.AddWithValue("@ttd", txtTTD.Text.Trim());
                        cmd.Parameters.AddWithValue("@k", txtKet.Text.Trim());
                        cmd.Parameters.AddWithValue("@f", savedFile ?? "");
                        cmd.Parameters.AddWithValue("@s", cbStatus.SelectedItem?.ToString() ?? "Draft");
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