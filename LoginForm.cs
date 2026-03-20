using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyPasswordManager
{
    public class LoginForm : Form
    {
        // These tell the rest of the app if the user got in
        public string MasterPassword { get; private set; } = "";
        public bool IsAuthenticated { get; private set; } = false;

        private TextBox _txtPassword = null!;
        private Button _btnUnlock = null!;

        public LoginForm()
        {
            // --- Window Styling ---
            this.Text = "Nexus Vault - Security Gate";
            this.Size = new Size(350, 220);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            // --- UI Elements ---
            Label lblPrompt = new Label
            {
                Text = "Enter Master Password:",
                Location = new Point(20, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            _txtPassword = new TextBox
            {
                Location = new Point(20, 60),
                Width = 290,
                UseSystemPasswordChar = true, // Masks the password with dots
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _btnUnlock = new Button
            {
                Text = "UNLOCK VAULT",
                Location = new Point(20, 110),
                Width = 290,
                Height = 45,
                BackColor = Color.FromArgb(0, 122, 204),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            // Event: What happens when you click the button
            _btnUnlock.Click += Unlock_Click;

            this.Controls.Add(lblPrompt);
            this.Controls.Add(_txtPassword);
            this.Controls.Add(_btnUnlock);

            // Allow pressing "Enter" on the keyboard to login
            this.AcceptButton = _btnUnlock;
        }

        private void Unlock_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtPassword.Text))
            {
                MessageBox.Show("Password cannot be empty.", "Security", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Store the password and set authenticated to true
            MasterPassword = _txtPassword.Text;
            IsAuthenticated = true;

            // Close this window so Program.cs can open Form1
            this.Close();
        }
    }
}