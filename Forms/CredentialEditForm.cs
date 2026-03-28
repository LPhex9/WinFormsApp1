using System;
using System.Drawing;
using System.Windows.Forms;
using MyPasswordManager.Models;
using MyPasswordManager.Services;

namespace MyPasswordManager
{
    public class CredentialEditForm : Form
    {
        public Credential Credential { get; private set; }
        private TextBox _txtTitle = null!;
        private TextBox _txtUser = null!;
        private TextBox _txtPass = null!;
        private TextBox _txtUrl = null!;

        public CredentialEditForm(Credential credential, VaultManager vault)
        {
            this.Credential = credential;
            this.Text = "Edit Credential";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Label lblTitle = new Label { Text = "Title:", Location = new Point(20, 20), AutoSize = true };
            _txtTitle = new TextBox { Text = Credential.Title, Location = new Point(120, 20), Width = 200 };

            Label lblUser = new Label { Text = "Username:", Location = new Point(20, 60), AutoSize = true };
            _txtUser = new TextBox { Text = Credential.Username, Location = new Point(120, 60), Width = 200 };

            Label lblPass = new Label { Text = "Password:", Location = new Point(20, 100), AutoSize = true };
            _txtPass = new TextBox { Text = Credential.Password, Location = new Point(120, 100), Width = 200 };

            Label lblUrl = new Label { Text = "URL:", Location = new Point(20, 140), AutoSize = true };
            _txtUrl = new TextBox { Text = Credential.Url, Location = new Point(120, 140), Width = 200 };

            Button btnSave = new Button
            {
                Text = "Save",
                Location = new Point(120, 250),
                Width = 100,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204)
            };

            btnSave.Click += (s, e) => {
                Credential.Title = _txtTitle.Text;
                Credential.Username = _txtUser.Text;
                Credential.Password = _txtPass.Text;
                Credential.Url = _txtUrl.Text;
                Credential.DateModified = DateTime.Now;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            this.Controls.AddRange(new Control[] { lblTitle, _txtTitle, lblUser, _txtUser, lblPass, _txtPass, lblUrl, _txtUrl, btnSave });
        }
    }
}