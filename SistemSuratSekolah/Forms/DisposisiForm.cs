using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SistemSuratSekolah.Data;
using SistemSuratSekolah.Helpers;

namespace SistemSuratSekolah.Forms
{
    public class DisposisiForm : Form
    {
        private DataGridView grid;
        private int selectedId = 0;

        public DisposisiForm()
        {
            Text = "Disposisi Surat";
            BackColor = Color.WhiteSmoke;

            var top = new Panel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(10) };
            var btnTambah = MakeBtn("Tambah Disposisi", 10, 10, 160, Color.SeaGreen);
            btnTambah.Click += (s, e) => OpenForm(0);

            var btnUpdate = MakeBtn("Update Status", 180, 10, 130, Color.DarkOrange);
            btnUpdate.Click += BtnUpdate_Click;

            var btnHapus = MakeBtn("Hapus", 320, 10, 80, Color.IndianRed);
            btnHapus.Click += BtnHapus_Click;

            var btnRefresh = MakeBtn("Refresh", 410, 10, 80, Color.Gray);
            btnRefresh.Click += (s, e) => LoadData();

            top.Controls.AddRange(new Control[] { btnTambah, btnUpdate, btnHapus, btnRefresh });

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
                    selectedId = Convert.ToInt32(grid.SelectedRows[0].Cells["id_disposisi"].Value);
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
                    string sql = @"SELECT d.id_disposisi, sm.nomor_surat, u.nama AS penerima,
                                          d.tanggal_disposisi, d.instruksi, d.status
                                   FROM disposisi d
                                   JOIN surat_masuk sm ON sm.id_surat_masuk = d.id_surat_masuk
                                   JOIN users u ON u.id_user = d.id_user
                                   ORDER BY d.tanggal_disposisi DESC";
                    using (var da = new MySqlDataAdapter(sql, conn))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        grid.DataSource = dt;
                        if (grid.Columns.Contains("id_disposisi"))
                            grid.Columns["id_disposisi"].Visible = false;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void OpenForm(int id)
        {
            var f = new DisposisiEditForm(id);
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnHapus_Click(object sender, EventArgs e)
        {
            if (selectedId <= 0) { MessageBox.Show("Pilih data dulu."); return; }
            if (MessageBox.Show("Hapus disposisi ini?", "Konfirmasi", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("DELETE FROM disposisi WHERE id_disposisi=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", selectedId);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadData();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedId <= 0) { MessageBox.Show("Pilih data dulu."); return; }
            var f = new UpdateStatusDisposisiForm(selectedId);
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }
    }

    public class DisposisiEditForm : Form
    {
        private int _id;
        private ComboBox cbSurat, cbPenerima, cbStatus;
        private DateTimePicker dtDisposisi;
        private TextBox txtInstruksi;

        public DisposisiEditForm(int id)
        {
            _id = id;
            Text = id == 0 ? "Tambah Disposisi" : "Edit Disposisi";
            Size = new Size(560, 400);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            int y = 20;
            AddLabel("Surat Masuk", 20, y);
            cbSurat = new ComboBox { Location = new Point(180, y), Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };
            Controls.Add(cbSurat);

            y += 35; AddLabel("Penerima", 20, y);
            cbPenerima = new ComboBox { Location = new Point(180, y), Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };
            Controls.Add(cbPenerima);

            y += 35; AddLabel("Tanggal", 20, y);
            dtDisposisi = new DateTimePicker { Location = new Point(180, y), Width = 150, Format = DateTimePickerFormat.Short };
            Controls.Add(dtDisposisi);

            y += 35; AddLabel("Status", 20, y);
            cbStatus = new ComboBox { Location = new Point(180, y), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cbStatus.Items.AddRange(new[] { "Belum Diproses", "Diproses", "Selesai" });
            cbStatus.SelectedIndex = 0;
            Controls.Add(cbStatus);

            y += 35; AddLabel("Instruksi", 20, y);
            txtInstruksi = new TextBox { Location = new Point(180, y), Width = 330, Height = 60, Multiline = true };
            Controls.Add(txtInstruksi);

            y += 80;
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

            LoadSurat();
            LoadPenerima();
            if (_id > 0) LoadData();
        }

        private void AddLabel(string t, int x, int y) => Controls.Add(new Label { Text = t, Location = new Point(x, y + 3), AutoSize = true });

        private void LoadSurat()
        {
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("SELECT id_surat_masuk, CONCAT(nomor_surat,' - ',perihal) AS info FROM surat_masuk ORDER BY tanggal_diterima DESC", conn))
                    using (var rd = cmd.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(rd);
                        cbSurat.DataSource = dt;
                        cbSurat.DisplayMember = "info";
                        cbSurat.ValueMember = "id_surat_masuk";
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void LoadPenerima()
        {
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("SELECT id_user, CONCAT(nama,' (',role,')') AS info FROM users WHERE status='aktif' AND role IN ('guru','kepala_sekolah')", conn))
                    using (var rd = cmd.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(rd);
                        cbPenerima.DataSource = dt;
                        cbPenerima.DisplayMember = "info";
                        cbPenerima.ValueMember = "id_user";
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void LoadData()
        {
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("SELECT * FROM disposisi WHERE id_disposisi=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", _id);
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                cbSurat.SelectedValue = rd["id_surat_masuk"];
                                cbPenerima.SelectedValue = rd["id_user"];
                                dtDisposisi.Value = Convert.ToDateTime(rd["tanggal_disposisi"]);
                                txtInstruksi.Text = rd["instruksi"].ToString();
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
            if (cbSurat.SelectedValue == null || cbPenerima.SelectedValue == null)
            {
                MessageBox.Show("Surat dan Penerima wajib dipilih.");
                return;
            }

            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql;
                    if (_id == 0)
                        sql = @"INSERT INTO disposisi (id_surat_masuk, id_user, tanggal_disposisi, instruksi, status)
                                VALUES (@s, @u, @t, @i, @st)";
                    else
                        sql = @"UPDATE disposisi SET id_surat_masuk=@s, id_user=@u,
                                tanggal_disposisi=@t, instruksi=@i, status=@st
                                WHERE id_disposisi=@id";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@s", cbSurat.SelectedValue);
                        cmd.Parameters.AddWithValue("@u", cbPenerima.SelectedValue);
                        cmd.Parameters.AddWithValue("@t", dtDisposisi.Value.Date);
                        cmd.Parameters.AddWithValue("@i", txtInstruksi.Text.Trim());
                        cmd.Parameters.AddWithValue("@st", cbStatus.SelectedItem?.ToString() ?? "Belum Diproses");
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

    public class UpdateStatusDisposisiForm : Form
    {
        private int _id;
        private ComboBox cbStatus;

        public UpdateStatusDisposisiForm(int id)
        {
            _id = id;
            Text = "Update Status Disposisi";
            Size = new Size(360, 180);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            Controls.Add(new Label { Text = "Status Baru:", Location = new Point(20, 20), AutoSize = true });
            cbStatus = new ComboBox { Location = new Point(120, 17), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cbStatus.Items.AddRange(new[] { "Belum Diproses", "Diproses", "Selesai" });
            cbStatus.SelectedIndex = 0;
            Controls.Add(cbStatus);

            var btn = new Button
            {
                Text = "Simpan",
                Location = new Point(120, 60),
                Width = 100,
                Height = 30,
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btn.Click += (s, e) =>
            {
                try
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        using (var cmd = new MySqlCommand("UPDATE disposisi SET status=@st WHERE id_disposisi=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@st", cbStatus.SelectedItem.ToString());
                            cmd.Parameters.AddWithValue("@id", _id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };
            Controls.Add(btn);
        }
    }
}