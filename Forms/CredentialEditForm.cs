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
        private VaultManager _vaultManager;
        
        private TextBox _txtTitle = null!;
        private TextBox _txtUsername = null!;
        private TextBox _txtPassword = null!;
        private TextBox _txtUrl = null!;
        private ComboBox _cbCategory = null!;
        private TextBox _txtNotes = null!;
        private DateTimePicker _dtpExpiration = null!;
        private CheckBox _cbSetExpiration = null!;
        private CheckBox _cbFavorite = null!;
        
        private Label _lblStrength = null!;
        private Button _btnGenerate = null!;
        private Button _btnShowPassword = null!;
        
        public CredentialEditForm(Credential credential, VaultManager vaultManager)
        {
            Credential = credential;
            _vaultManager = vaultManager;
            
            InitializeUI();
            LoadCredentialData();
        }
        
        private void InitializeUI()
        {
            Text = Credential.Id == Guid.Empty ? "Add New Credential" : "Edit Credential";
            Size = new Size(500, 650);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(45, 45, 48);
            ForeColor = Color.White;
            
            int y = 20;
            
            // Title
            AddLabel("Title:", 20, y);
            _txtTitle = AddTextBox(20, y + 25, 440);
            y += 70;
            
            // Username
            AddLabel("Username/Email:", 20, y);
            _txtUsername = AddTextBox(20, y + 25, 440);
            y += 70;
            
            // Password
            AddLabel("Password:", 20, y);
            _txtPassword = AddTextBox(20, y + 25, 320);
            _txtPassword.UseSystemPasswordChar = true;
            _txtPassword.TextChanged += (s, e) => UpdatePasswordStrength();
            
            _btnShowPassword = new Button
            {
                Text = "👁️",
                Location = new Point(350, y + 25),
                Size = new Size(40, 23),
                BackColor = Color.FromArgb(60, 60, 60),
                FlatStyle = FlatStyle.Flat
            };
            _btnShowPassword.Click += (s, e) =>
            {
                _txtPassword.UseSystemPasswordChar = !_txtPassword.UseSystemPasswordChar;
                _btnShowPassword.Text = _txtPassword.UseSystemPasswordChar ? "👁️" : "🔒";
            };
            
            _btnGenerate = new Button
            {
                Text = "🎲 Generate",
                Location = new Point(400, y + 25),
                Size = new Size(60, 23),
                BackColor = Color.FromArgb(0, 122, 204),
                FlatStyle = FlatStyle.Flat
            };
            _btnGenerate.Click += (s, e) => GeneratePassword();
            
            this.Controls.Add(_btnShowPassword);
            this.Controls.Add(_btnGenerate);
            
            _lblStrength = new Label
            {
                Location = new Point(20, y + 55),
                AutoSize = true,
                ForeColor = Color.Gray
            };
            this.Controls.Add(_lblStrength);
            
            y += 85;
            
            // URL
            AddLabel("Website URL:", 20, y);
            _txtUrl = AddTextBox(20, y + 25, 440);
            y += 70;
            
            // Category
            AddLabel("Category:", 20, y);
            _cbCategory = new ComboBox
            {
                Location = new Point(20, y + 25),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            _cbCategory.Items.AddRange(new object[] 
            { 
                "General", "Social Media", "Banking", "Email", "Work", "Shopping", "Other" 
            });
            _cbCategory.SelectedIndex = 0;
            this.Controls.Add(_cbCategory);
            
            _cbFavorite = new CheckBox
            {
                Text = "⭐ Favorite",
                Location = new Point(240, y + 27),
                AutoSize = true,
                ForeColor = Color.White
            };
            this.Controls.Add(_cbFavorite);
            
            y += 55;
            
            // Expiration
            _cbSetExpiration = new CheckBox
            {
                Text = "Set expiration date:",
                Location = new Point(20, y),
                AutoSize = true,
                ForeColor = Color.White
            };
            _cbSetExpiration.CheckedChanged += (s, e) => _dtpExpiration.Enabled = _cbSetExpiration.Checked;
            this.Controls.Add(_cbSetExpiration);
            
            _dtpExpiration = new DateTimePicker
            {
                Location = new Point(20, y + 25),
                Width = 200,
                Format = DateTimePickerFormat.Short,
                Enabled = false,
                Value = DateTime.Now.AddDays(90)
            };
            this.Controls.Add(_dtpExpiration);
            
            y += 70;
            
            // Notes
            AddLabel("Notes:", 20, y);
            _txtNotes = new TextBox
            {
                Location = new Point(20, y + 25),
                Size = new Size(440, 80),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            this.Controls.Add(_txtNotes);
            
            y += 120;
            
            // Buttons
            var btnSave = new Button
            {
                Text = "💾 Save",
                Location = new Point(270, y),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnSave.Click += (s, e) => SaveCredential();
            
            var btnCancel = new Button
            {
                Text = "❌ Cancel",
                Location = new Point(370, y),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }
        
        private Label AddLabel(string text, int x, int y)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Color.White
            };
            this.Controls.Add(label);
            return label;
        }
        
        private TextBox AddTextBox(int x, int y, int width)
        {
            var textBox = new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            this.Controls.Add(textBox);
            return textBox;
        }
        
        private void LoadCredentialData()
        {
            _txtTitle.Text = Credential.Title;
            _txtUsername.Text = Credential.Username;
            _txtPassword.Text = Credential.Password;
            _txtUrl.Text = Credential.Url;
            _txtNotes.Text = Credential.Notes;
            _cbFavorite.Checked = Credential.IsFavorite;
            
            if (!string.IsNullOrEmpty(Credential.Category))
            {
                int index = _cbCategory.Items.IndexOf(Credential.Category);
                if (index >= 0)
                    _cbCategory.SelectedIndex = index;
            }
            
            if (Credential.ExpirationDate.HasValue)
            {
                _cbSetExpiration.Checked = true;
                _dtpExpiration.Value = Credential.ExpirationDate.Value;
            }
            
            UpdatePasswordStrength();
        }
        
        private void GeneratePassword()
        {
            string password = PasswordService.GeneratePassword(16, true, true, true, true);
            _txtPassword.Text = password;
            _txtPassword.UseSystemPasswordChar = false;
            _btnShowPassword.Text = "🔒";
            UpdatePasswordStrength();
        }
        
        private void UpdatePasswordStrength()
        {
            if (string.IsNullOrEmpty(_txtPassword.Text))
            {
                _lblStrength.Text = "";
                return;
            }
            
            var strength = PasswordService.CalculateStrength(_txtPassword.Text);
            double entropy = PasswordService.CalculateEntropy(_txtPassword.Text);
            
            Color strengthColor = strength switch
            {
                PasswordStrength.VeryWeak => Color.DarkRed,
                PasswordStrength.Weak => Color.OrangeRed,
                PasswordStrength.Fair => Color.Orange,
                PasswordStrength.Good => Color.YellowGreen,
                PasswordStrength.Strong => Color.LimeGreen,
                PasswordStrength.VeryStrong => Color.Lime,
                _ => Color.Gray
            };
            
            _lblStrength.Text = $"Strength: {strength} ({entropy:F1} bits entropy)";
            _lblStrength.ForeColor = strengthColor;
            
            // Check for duplicates
            if (PasswordService.IsPasswordReused(_txtPassword.Text, _vaultManager.Credentials, Credential.Id))
            {
                _lblStrength.Text += " ⚠️ REUSED PASSWORD!";
            }
        }
        
        private void SaveCredential()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(_txtTitle.Text))
            {
                MessageBox.Show("Please enter a title.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }
            
            if (string.IsNullOrWhiteSpace(_txtPassword.Text))
            {
                MessageBox.Show("Please enter a password.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }
            
            // Update credential
            Credential.Title = _txtTitle.Text;
            Credential.Username = _txtUsername.Text;
            Credential.Password = _txtPassword.Text;
            Credential.Url = _txtUrl.Text;
            Credential.Category = _cbCategory.SelectedItem?.ToString() ?? "General";
            Credential.Notes = _txtNotes.Text;
            Credential.IsFavorite = _cbFavorite.Checked;
            Credential.Strength = PasswordService.CalculateStrength(_txtPassword.Text);
            Credential.ExpirationDate = _cbSetExpiration.Checked ? _dtpExpiration.Value : null;
            Credential.DateModified = DateTime.Now;
        }
    }
}
