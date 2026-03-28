using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MyPasswordManager.Models;
using MyPasswordManager.Services;
// FIX: Solve ambiguity between System.Windows.Forms.Timer and System.Threading.Timer
using Timer = System.Windows.Forms.Timer;

namespace MyPasswordManager
{
    public partial class Form1 : Form
    {
        // --- Core Variables ---
        private VaultManager _vaultManager = null!;
        private string _currentMasterPassword = string.Empty;
        private Timer _autoLockTimer = null!;
        private BindingList<Credential> _credentialsBinding = new BindingList<Credential>();

        // --- UI Controls ---
        private TextBox _txtGeneratedPassword = null!;

        public Form1(string masterPassword)
        {
            _currentMasterPassword = masterPassword;

            // 1. Setup Data
            InitializeVault();

            // 2. Setup UI
            InitializeCustomUI();

            // 3. Setup Security
            InitializeAutoLock();

            // 4. Load Data into Grid
            RefreshVault();
        }

        private void InitializeVault()
        {
            _vaultManager = new VaultManager("vault.nexus", _currentMasterPassword);
            try
            {
                _vaultManager.Load();
            }
            catch (Exception)
            {
                MessageBox.Show("Error loading vault. Check your password.");
                Application.Exit();
            }
        }

        private void InitializeAutoLock()
        {
            _autoLockTimer = new Timer { Interval = 300000 }; // 5 minutes
            _autoLockTimer.Tick += (s, e) => LockVault();
            _autoLockTimer.Start();
        }

        private void InitializeCustomUI()
        {
            // --- Window Styling ---
            this.Text = "Nexus Password Vault Pro";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            // --- Tab Control ---
            TabControl mainTabs = new TabControl { Dock = DockStyle.Fill };
            TabPage pageVault = new TabPage("🗝️ Vault");
            TabPage pageGen = new TabPage("🔐 Generator");
            mainTabs.TabPages.Add(pageVault);
            mainTabs.TabPages.Add(pageGen);

            // --- VAULT PAGE: DataGrid ---
            DataGridView grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = _credentialsBinding,
                BackgroundColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.None,
                ForeColor = Color.Black, // Text inside grid needs to be dark for visibility
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // --- VAULT PAGE: Buttons ---
            Panel pnlVaultButtons = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.FromArgb(45, 45, 48) };

            Button btnAdd = new Button { Text = "➕ Add New", Location = new Point(10, 10), Width = 120, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White };
            btnAdd.Click += (s, e) => AddCredential();

            Button btnRefresh = new Button { Text = "🔄 Refresh", Location = new Point(140, 10), Width = 100, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.Gray, ForeColor = Color.White };
            btnRefresh.Click += (s, e) => RefreshVault();

            Button btnSettings = new Button { Text = "⚙️ Settings", Location = new Point(250, 10), Width = 100, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.Gray, ForeColor = Color.White };
            btnSettings.Click += (s, e) => ShowSettings();

            pnlVaultButtons.Controls.AddRange(new Control[] { btnAdd, btnRefresh, btnSettings });
            pageVault.Controls.Add(grid);
            pageVault.Controls.Add(pnlVaultButtons);

            // --- GENERATOR PAGE ---
            Label lblGen = new Label { Text = "Secure Password Generator", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            _txtGeneratedPassword = new TextBox { Location = new Point(20, 60), Width = 400, Font = new Font("Consolas", 12), ReadOnly = true };

            Button btnGenAction = new Button { Text = "🎲 Generate", Location = new Point(20, 100), Width = 150, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0, 150, 136) };
            btnGenAction.Click += (s, e) => GeneratePassword();

            pageGen.Controls.AddRange(new Control[] { lblGen, _txtGeneratedPassword, btnGenAction });

            this.Controls.Add(mainTabs);
        }

        // --- Logic Methods ---

        private void RefreshVault()
        {
            _credentialsBinding.Clear();
            foreach (var c in _vaultManager.Credentials.OrderByDescending(x => x.DateModified))
            {
                _credentialsBinding.Add(c);
            }
        }

        private void LockVault()
        {
            _autoLockTimer.Stop();
            this.Hide();
            using (var login = new LoginForm())
            {
                if (login.ShowDialog() == DialogResult.OK && login.IsAuthenticated)
                {
                    _currentMasterPassword = login.MasterPassword;
                    this.Show();
                    _autoLockTimer.Start();
                }
                else { Application.Exit(); }
            }
        }

        private void GeneratePassword()
        {
            _txtGeneratedPassword.Text = PasswordService.GeneratePassword(16, true, true, true, true);
        }

        private void AddCredential()
        {
            var newCred = new Credential();
            using (var editForm = new CredentialEditForm(newCred, _vaultManager))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    _vaultManager.AddCredential(editForm.Credential);
                    RefreshVault();
                }
            }
        }

        private void ShowSettings()
        {
            using (var settingsForm = new SettingsForm(_vaultManager.Settings))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    _vaultManager.Save();
                    MessageBox.Show("Settings saved successfully.");
                }
            }
        }

        // --- Placeholders to satisfy references in other forms/menus ---
        private void EditCredential() { }
        private void DeleteCredential() { }
        private void ViewPassword() { }
        private void CopyPassword() { }
        private void LoadStatistics() { }
        private void FilterVault() { }
        private void ImportFromCSV() { }
        private void ExportToCSV() { }
        private void ShowDuplicates() { }
        private void ShowWeakPasswords() { }
        private void ShowPasswordHistory() { }
        private void ShowAbout() { }
    }
}