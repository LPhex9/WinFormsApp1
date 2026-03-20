using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace MyPasswordManager
{
    public class Credential
    {
        public string Title { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Category { get; set; } = "General";
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }

    public class Form1 : Form
    {
        // 1. Added ' = null!' to satisfy the Nullable reference check
        private TabControl _tabs = null!;
        private TextBox _txtOutput = null!;
        private NumericUpDown _numLen = null!;
        private CheckBox _cUpper = null!, _cLower = null!, _cNumbers = null!, _cSymbols = null!;
        private DataGridView _grid = null!;
        private BindingList<Credential> _vault = new BindingList<Credential>();

        private string _vaultPath = "vault.nexus";
        private string _currentMasterPassword;

        public Form1(string masterKey)
        {
            _currentMasterPassword = masterKey;

            Text = "Nexus Password Vault v1.0";
            Size = new Size(800, 500);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;

            InitializeCustomComponents();
            LoadVault(_currentMasterPassword);
        }

        private void InitializeCustomComponents()
        {
            _tabs = new TabControl { Dock = DockStyle.Fill };
            TabPage pageGen = new TabPage("Generator");
            TabPage pageVault = new TabPage("Vault");

            pageGen.BackColor = pageVault.BackColor = Color.FromArgb(45, 45, 48);
            _tabs.TabPages.Add(pageGen);
            _tabs.TabPages.Add(pageVault);

            // --- GENERATOR TAB ---
            Label lblLen = new Label { Text = "Length:", Location = new Point(20, 20), AutoSize = true };
            _numLen = new NumericUpDown { Location = new Point(100, 18), Value = 16, Minimum = 4, Maximum = 128 };

            _cUpper = new CheckBox { Text = "Uppercase (A-Z)", Location = new Point(20, 50), Checked = true, AutoSize = true };
            _cLower = new CheckBox { Text = "Lowercase (a-z)", Location = new Point(20, 80), Checked = true, AutoSize = true };
            _cNumbers = new CheckBox { Text = "Numbers (0-9)", Location = new Point(20, 110), Checked = true, AutoSize = true };
            _cSymbols = new CheckBox { Text = "Symbols (!@#$)", Location = new Point(20, 140), Checked = true, AutoSize = true };

            _txtOutput = new TextBox { Location = new Point(20, 180), Width = 350, Font = new Font("Consolas", 12), ReadOnly = true };

            Button btnGen = new Button { Text = "Generate", Location = new Point(20, 220), Width = 150, Height = 40, BackColor = Color.FromArgb(0, 122, 204), FlatStyle = FlatStyle.Flat };
            btnGen.Click += (s, e) => Generate();

            Button btnSave = new Button { Text = "Save to Vault", Location = new Point(180, 220), Width = 150, Height = 40, BackColor = Color.FromArgb(0, 122, 204), FlatStyle = FlatStyle.Flat };
            btnSave.Click += (s, e) => SaveGeneratedToVault();

            pageGen.Controls.AddRange(new Control[] { lblLen, _numLen, _cUpper, _cLower, _cNumbers, _cSymbols, _txtOutput, btnGen, btnSave });

            // --- VAULT TAB ---
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = _vault,
                BackgroundColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.Black,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None
            };
            pageVault.Controls.Add(_grid);

            this.Controls.Add(_tabs);
        }

        private void Generate()
        {
            string pool = "";
            if (_cUpper.Checked) pool += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (_cLower.Checked) pool += "abcdefghijklmnopqrstuvwxyz";
            if (_cNumbers.Checked) pool += "0123456789";
            if (_cSymbols.Checked) pool += "!@#$%^&*()_+-=[]{}|;:,.<>?";

            if (pool == "") return;

            StringBuilder res = new StringBuilder();
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[(int)_numLen.Value];
                rng.GetBytes(data);
                foreach (byte b in data) res.Append(pool[b % pool.Length]);
            }
            _txtOutput.Text = res.ToString();
        }

        private void SaveGeneratedToVault()
        {
            if (string.IsNullOrEmpty(_txtOutput.Text)) return;

            _vault.Add(new Credential { Title = "New Entry", Password = _txtOutput.Text });
            SaveVault(_currentMasterPassword);
            _tabs.SelectedIndex = 1;
        }

        private void SaveVault(string masterPassword)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(_vault);
                string encryptedData = CryptoHelper.Encrypt(jsonString, masterPassword);
                File.WriteAllText(_vaultPath, encryptedData);
            }
            catch (Exception ex) { MessageBox.Show($"Save error: {ex.Message}"); }
        }

        private void LoadVault(string masterPassword)
        {
            if (!File.Exists(_vaultPath)) return;
            try
            {
                string encryptedData = File.ReadAllText(_vaultPath);
                string jsonString = CryptoHelper.Decrypt(encryptedData, masterPassword);
                var decryptedList = JsonSerializer.Deserialize<List<Credential>>(jsonString);
                _vault.Clear();
                if (decryptedList != null) foreach (var item in decryptedList) _vault.Add(item);
            }
            catch (CryptographicException)
            {
                MessageBox.Show("Wrong Master Password!", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Application.Exit();
            }
            catch (Exception ex) { MessageBox.Show($"Load error: {ex.Message}"); }
        }
    }
}