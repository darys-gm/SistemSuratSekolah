using System.Drawing;
using System.Windows.Forms;

namespace SistemSuratSekolah.Forms
{
    public class InputBoxForm : Form
    {
        private TextBox txtInput;

        public string InputText => txtInput.Text;

        public InputBoxForm(string title, string prompt, string defaultValue = "")
        {
            Text = title;
            Size = new Size(420, 230);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.WhiteSmoke;

            var lblPrompt = new Label
            {
                Text = prompt,
                Location = new Point(20, 20),
                Size = new Size(370, 40),
                Font = new Font("Segoe UI", 10)
            };
            Controls.Add(lblPrompt);

            txtInput = new TextBox
            {
                Location = new Point(20, 70),
                Width = 370,
                Height = 80,
                Multiline = true,
                Font = new Font("Segoe UI", 10),
                Text = defaultValue
            };
            Controls.Add(txtInput);

            var btnOK = new Button
            {
                Text = "OK",
                Location = new Point(200, 160),
                Width = 90,
                Height = 32,
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            Controls.Add(btnOK);

            var btnCancel = new Button
            {
                Text = "Batal",
                Location = new Point(300, 160),
                Width = 90,
                Height = 32,
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(btnCancel);

            AcceptButton = btnOK;
            CancelButton = btnCancel;
        }

        public static string Show(string title, string prompt, string defaultValue = "")
        {
            using (var f = new InputBoxForm(title, prompt, defaultValue))
            {
                return f.ShowDialog() == DialogResult.OK ? f.InputText : null;
            }
        }
    }
}