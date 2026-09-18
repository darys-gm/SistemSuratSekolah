using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SistemSuratSekolah.Data;
using SistemSuratSekolah.Helpers;
using SistemSuratSekolah.Models;

namespace SistemSuratSekolah.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnKeluar;

        public LoginForm()
        {
            Text = "Login - Sistem Informasi Surat Sekolah";
            Size = new Size(420, 300);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.WhiteSmoke;

            var lblTitle = new Label
            {
                Text = "SISTEM INFORMASI SURAT SEKOLAH",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.DarkSlateBlue,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60
            };

            var lblUser = new Label { Text = "Username", Location = new Point(40, 90), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(40, 110), Width = 320, Font = new Font("Segoe UI", 10) };

            var lblPass = new Label { Text = "Password", Location = new Point(40, 145), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(40, 165), Width = 320, UseSystemPasswordChar = true, Font = new Font("Segoe UI", 10) };

            btnLogin = new Button
            {
                Text = "LOGIN",
                Location = new Point(40, 210),
                Width = 150,
                Height = 35,
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.Click += BtnLogin_Click;

            btnKeluar = new Button
            {
                Text = "KELUAR",
                Location = new Point(210, 210),
                Width = 150,
                Height = 35,
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnKeluar.Click += (s, e) => Application.Exit();

            Controls.Add(lblTitle);
            Controls.Add(lblUser);
            Controls.Add(txtUsername);
            Controls.Add(lblPass);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(btnKeluar);

            AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Username dan Password wajib diisi.", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT id_user, nama, username, role, status FROM users WHERE username=@u AND password=@p";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@p", HashHelper.Sha256(password));
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                string status = rd["status"].ToString();
                                if (status != "aktif")
                                {
                                    MessageBox.Show("Akun Anda tidak aktif.", "Peringatan",
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                Session.CurrentUser = new User
                                {
                                    IdUser = Convert.ToInt32(rd["id_user"]),
                                    Nama = rd["nama"].ToString(),
                                    Username = rd["username"].ToString(),
                                    Role = rd["role"].ToString(),
                                    Status = status
                                };

                                Hide();
                                var dash = new DashboardForm();
                                dash.FormClosed += (s2, e2) => Close();
                                dash.Show();
                            }
                            else
                            {
                                MessageBox.Show("Username atau Password salah.", "Gagal Login",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Kesalahan",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}