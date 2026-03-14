using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace MyPasswordManager
{
    // Credential model
    public class Credential
    {
        public string Title { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Category { get; set; } = "General";
    }

    public class Form1 : Form
    {
        private BindingList<Credential> _vault = new BindingList<Credential>();

        // Controls
        private TabControl _tabs;
        private TextBox _txtOutput, _txtTitle, _txtUser;
        private NumericUpDown _numLen;
        private CheckBox _cUpper, _cLower, _cNum, _cSym;
        private DataGridView _grid;

        public Form1()
        {
            // This is the "Actual Code" approach - no designer file needed
            InitializeCustomComponents();
            this.Text = "Nexus Password Vault v1.0";
            this.Size = new Size(800, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(45, 45, 48); // Dark Theme feel
        }

        private void InitializeCustomComponents()
        {
            _tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(12, 12) };

            // --- TAB 1: GENERATOR ---
            var pageGen = new TabPage("Generator");
            pageGen.BackColor = Color.White;

            var lblHeader = new Label { Text = "Password Configuration", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };

            _numLen = new NumericUpDown { Location = new Point(150, 65), Value = 20, Minimum = 4, Maximum = 128 };
            var lblLen = new Label { Text = "Password Length:", Location = new Point(25, 67), AutoSize = true };

            _cUpper = new CheckBox { Text = "Uppercase (A-Z)", Location = new Point(30, 110), Checked = true, AutoSize = true };
            _cLower = new CheckBox { Text = "Lowercase (a-z)", Location = new Point(30, 140), Checked = true, AutoSize = true };
            _cNum = new CheckBox { Text = "Numbers (0-9)", Location = new Point(200, 110), Checked = true, AutoSize = true };
            _cSym = new CheckBox { Text = "Symbols (!@#$)", Location = new Point(200, 140), Checked = true, AutoSize = true };

            var btnGen = new Button { Text = "GENERATE", Location = new Point(30, 190), Size = new Size(120, 40), BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnGen.Click += (s, e) => Generate();

            _txtOutput = new TextBox { Location = new Point(30, 250), Width = 500, Font = new Font("Consolas", 14), ReadOnly = true, BackColor = Color.FromArgb(240, 240, 240) };

            var btnSave = new Button { Text = "SAVE TO VAULT", Location = new Point(30, 300), Size = new Size(150, 35), FlatStyle = FlatStyle.Flat };
            btnSave.Click += (s, e) => SaveGenerated();

            pageGen.Controls.AddRange(new Control[] { lblHeader, lblLen, _numLen, _cUpper, _cLower, _cNum, _cSym, btnGen, _txtOutput, btnSave });

            // --- TAB 2: VAULT ---
            var pageVault = new TabPage("Secure Vault");

            _grid = new DataGridView
            {
                Dock = DockStyle.Bottom,
                Height = 350,
                DataSource = _vault,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            var lblVaultHeader = new Label { Text = "Stored Credentials", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 10) };
            _txtTitle = new TextBox { Location = new Point(10, 40), PlaceholderText = "Website/App Name", Width = 200 };
            _txtUser = new TextBox { Location = new Point(220, 40), PlaceholderText = "Username/Email", Width = 200 };

            pageVault.Controls.AddRange(new Control[] { lblVaultHeader, _txtTitle, _txtUser, _grid });

            _tabs.TabPages.Add(pageGen);
            _tabs.TabPages.Add(pageVault);
            this.Controls.Add(_tabs);
        }

        private void Generate()
        {
            string pool = "";
            if (_cUpper.Checked) pool += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (_cLower.Checked) pool += "abcdefghijklmnopqrstuvwxyz";
            if (_cNum.Checked) pool += "0123456789";
            if (_cSym.Checked) pool += "!@#$%^&*()_+";

            if (pool == "") return;

            var res = new StringBuilder();
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[(int)_numLen.Value];
                rng.GetBytes(data);
                foreach (byte b in data) res.Append(pool[b % pool.Length]);
            }
            _txtOutput.Text = res.ToString();
        }

        private void SaveGenerated()
        {
            if (string.IsNullOrEmpty(_txtOutput.Text)) return;
            _vault.Add(new Credential { Title = "Generated Entry", Password = _txtOutput.Text });
            _tabs.SelectedIndex = 1; // Switch to Vault
        }
    }
}