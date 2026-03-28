using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MyPasswordManager.Models;
using MyPasswordManager.Services;

namespace MyPasswordManager
{
    public class Form1 : Form
    {
        // Core components
        private VaultManager _vaultManager = null!;
        private string _currentMasterPassword;
        private Timer _autoLockTimer = null!;
        private Timer _clipboardTimer = null!;
        
        // UI Components
        private TabControl _mainTabs = null!;
        private BindingList<Credential> _credentialsBinding = new BindingList<Credential>();
        
        // Generator Tab
        private NumericUpDown _numLength = null!;
        private CheckBox _cbUpper = null!, _cbLower = null!, _cbNumbers = null!, _cbSymbols = null!;
        private CheckBox _cbExcludeAmbiguous = null!;
        private TextBox _txtGeneratedPassword = null!;
        private Button _btnGenerate = null!, _btnCopyGenerated = null!;
        private Label _lblStrength = null!, _lblEntropy = null!, _lblCrackTime = null!;
        private Panel _pnlStrengthBars = null!;
        
        // Vault Tab
        private DataGridView _gridVault = null!;
        private TextBox _txtSearch = null!;
        private ComboBox _cbCategoryFilter = null!;
        private Button _btnAdd = null!, _btnEdit = null!, _btnDelete = null!;
        private Button _btnViewPassword = null!, _btnCopyPassword = null!;
        private ToolStrip _toolStrip = null!;
        
        // Statistics Tab
        private Label _lblTotalCreds = null!, _lblWeakPasswords = null!, _lblDuplicates = null!;
        private Label _lblSecurityScore = null!;
        private ListBox _lstWeakPasswords = null!, _lstDuplicates = null!;
        
        public Form1(string masterPassword)
        {
            _currentMasterPassword = masterPassword;
            
            InitializeVault();
            InitializeUI();
            InitializeAutoLock();
            
            LoadVaultData();
        }
        
        private void InitializeVault()
        {
            _vaultManager = new VaultManager("vault.nexus", _currentMasterPassword);
            
            try
            {
                _vaultManager.Load();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Incorrect master password!", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
        
        private void InitializeUI()
        {
            // Main Form Settings
            Text = "Nexus Password Vault Pro v2.0";
            Size = new Size(1200, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
            
            // Create Menu Strip
            CreateMenuStrip();
            
            // Create Tool Strip
            CreateToolStrip();
            
            // Create Main Tabs
            CreateMainTabs();
            
            // Create Status Bar
            CreateStatusBar();
        }
        
        private void CreateMenuStrip()
        {
            var menuStrip = new MenuStrip();
            
            // File Menu
            var fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.Add("&Import from CSV...", null, (s, e) => ImportFromCSV());
            fileMenu.DropDownItems.Add("&Export to CSV...", null, (s, e) => ExportToCSV());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("&Lock Vault", null, (s, e) => LockVault());
            fileMenu.DropDownItems.Add("E&xit", null, (s, e) => Application.Exit());
            
            // Tools Menu
            var toolsMenu = new ToolStripMenuItem("&Tools");
            toolsMenu.DropDownItems.Add("Check for &Duplicates", null, (s, e) => ShowDuplicates());
            toolsMenu.DropDownItems.Add("Check &Weak Passwords", null, (s, e) => ShowWeakPasswords());
            toolsMenu.DropDownItems.Add("View Password &History...", null, (s, e) => ShowPasswordHistory());
            toolsMenu.DropDownItems.Add(new ToolStripSeparator());
            toolsMenu.DropDownItems.Add("&Settings...", null, (s, e) => ShowSettings());
            
            // Help Menu
            var helpMenu = new ToolStripMenuItem("&Help");
            helpMenu.DropDownItems.Add("&About", null, (s, e) => ShowAbout());
            
            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, toolsMenu, helpMenu });
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }
        
        private void CreateToolStrip()
        {
            _toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            
            var btnAddNew = new ToolStripButton("➕ Add", null, (s, e) => AddCredential());
            var btnRefresh = new ToolStripButton("🔄 Refresh", null, (s, e) => RefreshVault());
            var btnLock = new ToolStripButton("🔒 Lock", null, (s, e) => LockVault());
            
            _toolStrip.Items.AddRange(new ToolStripItem[] { btnAddNew, btnRefresh, btnLock });
            this.Controls.Add(_toolStrip);
        }
        
        private void CreateMainTabs()
        {
            _mainTabs = new TabControl 
            { 
                Dock = DockStyle.Fill,
                Padding = new Point(10, 10)
            };
            
            var tabGenerator = CreateGeneratorTab();
            var tabVault = CreateVaultTab();
            var tabStatistics = CreateStatisticsTab();
            var tabNotes = CreateNotesTab();
            
            _mainTabs.TabPages.AddRange(new TabPage[] { tabVault, tabGenerator, tabStatistics, tabNotes });
            this.Controls.Add(_mainTabs);
        }
        
        private TabPage CreateGeneratorTab()
        {
            var tab = new TabPage("🔐 Generator");
            tab.BackColor = Color.FromArgb(45, 45, 48);
            
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            // Password Display
            var pnlDisplay = new Panel { Location = new Point(20, 20), Size = new Size(600, 120) };
            pnlDisplay.BorderStyle = BorderStyle.FixedSingle;
            
            _txtGeneratedPassword = new TextBox
            {
                Location = new Point(10, 10),
                Size = new Size(580, 30),
                Font = new Font("Consolas", 14, FontStyle.Bold),
                ReadOnly = true,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.Lime,
                TextAlign = HorizontalAlignment.Center
            };
            
            // Strength Indicators
            var lblStrengthLabel = new Label 
            { 
                Text = "Strength:", 
                Location = new Point(10, 50), 
                AutoSize = true 
            };
            
            _lblStrength = new Label
            {
                Location = new Point(80, 50),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            
            _pnlStrengthBars = new Panel { Location = new Point(10, 75), Size = new Size(580, 10) };
            CreateStrengthBars();
            
            _lblEntropy = new Label 
            { 
                Text = "Entropy: 0 bits", 
                Location = new Point(250, 50), 
                AutoSize = true 
            };
            
            _lblCrackTime = new Label 
            { 
                Text = "Time to crack: N/A", 
                Location = new Point(400, 50), 
                AutoSize = true 
            };
            
            pnlDisplay.Controls.AddRange(new Control[] 
            { 
                _txtGeneratedPassword, lblStrengthLabel, _lblStrength, 
                _pnlStrengthBars, _lblEntropy, _lblCrackTime 
            });
            
            // Options Panel
            var pnlOptions = new Panel { Location = new Point(20, 160), Size = new Size(600, 200) };
            
            var lblLength = new Label { Text = "Length:", Location = new Point(10, 10), AutoSize = true };
            _numLength = new NumericUpDown 
            { 
                Location = new Point(80, 8), 
                Value = 16, 
                Minimum = 8, 
                Maximum = 64, 
                Width = 60 
            };
            _numLength.ValueChanged += (s, e) => GeneratePassword();
            
            _cbUpper = new CheckBox 
            { 
                Text = "Uppercase (A-Z)", 
                Location = new Point(10, 40), 
                Checked = true, 
                AutoSize = true 
            };
            _cbUpper.CheckedChanged += (s, e) => GeneratePassword();
            
            _cbLower = new CheckBox 
            { 
                Text = "Lowercase (a-z)", 
                Location = new Point(10, 70), 
                Checked = true, 
                AutoSize = true 
            };
            _cbLower.CheckedChanged += (s, e) => GeneratePassword();
            
            _cbNumbers = new CheckBox 
            { 
                Text = "Numbers (0-9)", 
                Location = new Point(10, 100), 
                Checked = true, 
                AutoSize = true 
            };
            _cbNumbers.CheckedChanged += (s, e) => GeneratePassword();
            
            _cbSymbols = new CheckBox 
            { 
                Text = "Symbols (!@#$%)", 
                Location = new Point(10, 130), 
                Checked = true, 
                AutoSize = true 
            };
            _cbSymbols.CheckedChanged += (s, e) => GeneratePassword();
            
            _cbExcludeAmbiguous = new CheckBox 
            { 
                Text = "Exclude ambiguous (0/O, l/1/I)", 
                Location = new Point(10, 160), 
                AutoSize = true 
            };
            _cbExcludeAmbiguous.CheckedChanged += (s, e) => GeneratePassword();
            
            pnlOptions.Controls.AddRange(new Control[] 
            { 
                lblLength, _numLength, _cbUpper, _cbLower, 
                _cbNumbers, _cbSymbols, _cbExcludeAmbiguous 
            });
            
            // Action Buttons
            _btnGenerate = new Button
            {
                Text = "🎲 Generate New Password",
                Location = new Point(20, 380),
                Size = new Size(250, 45),
                BackColor = Color.FromArgb(0, 122, 204),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnGenerate.Click += (s, e) => GeneratePassword();
            
            _btnCopyGenerated = new Button
            {
                Text = "📋 Copy to Clipboard",
                Location = new Point(290, 380),
                Size = new Size(200, 45),
                BackColor = Color.FromArgb(0, 150, 136),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnCopyGenerated.Click += (s, e) => CopyGeneratedPassword();
            
            var btnSaveToVault = new Button
            {
                Text = "💾 Save to Vault",
                Location = new Point(510, 380),
                Size = new Size(150, 45),
                BackColor = Color.FromArgb(76, 175, 80),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSaveToVault.Click += (s, e) => SaveGeneratedToVault();
            
            mainPanel.Controls.AddRange(new Control[] 
            { 
                pnlDisplay, pnlOptions, _btnGenerate, _btnCopyGenerated, btnSaveToVault 
            });
            
            tab.Controls.Add(mainPanel);
            
            // Generate initial password
            GeneratePassword();
            
            return tab;
        }
        
        private void CreateStrengthBars()
        {
            for (int i = 0; i < 4; i++)
            {
                var bar = new Panel
                {
                    Name = $"bar{i}",
                    Location = new Point(i * 147, 0),
                    Size = new Size(140, 10),
                    BackColor = Color.FromArgb(80, 80, 80)
                };
                _pnlStrengthBars.Controls.Add(bar);
            }
        }
        
        private TabPage CreateVaultTab()
        {
            var tab = new TabPage("🗝️ Vault");
            tab.BackColor = Color.FromArgb(45, 45, 48);
            
            // Search and Filter Panel
            var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
            
            var lblSearch = new Label { Text = "Search:", Location = new Point(10, 15), AutoSize = true };
            
            _txtSearch = new TextBox 
            { 
                Location = new Point(70, 12), 
                Width = 300,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            _txtSearch.TextChanged += (s, e) => FilterVault();
            
            var lblCategory = new Label 
            { 
                Text = "Category:", 
                Location = new Point(400, 15), 
                AutoSize = true 
            };
            
            _cbCategoryFilter = new ComboBox 
            { 
                Location = new Point(480, 12), 
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cbCategoryFilter.Items.AddRange(new object[] 
            { 
                "All", "General", "Social Media", "Banking", "Email", "Work", "Shopping", "Other" 
            });
            _cbCategoryFilter.SelectedIndex = 0;
            _cbCategoryFilter.SelectedIndexChanged += (s, e) => FilterVault();
            
            pnlSearch.Controls.AddRange(new Control[] 
            { 
                lblSearch, _txtSearch, lblCategory, _cbCategoryFilter 
            });
            
            // Grid
            _gridVault = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = _credentialsBinding,
                BackgroundColor = Color.FromArgb(30, 30, 30),
                GridColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    BackColor = Color.FromArgb(45, 45, 48),
                    ForeColor = Color.White,
                    SelectionBackColor = Color.FromArgb(0, 122, 204),
                    SelectionForeColor = Color.White
                },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None
            };
            
            // Configure columns
            _gridVault.Columns.Clear();
            _gridVault.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "Title", 
                HeaderText = "Title", 
                FillWeight = 20 
            });
            _gridVault.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "Username", 
                HeaderText = "Username", 
                FillWeight = 20 
            });
            _gridVault.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "PasswordPreview", 
                HeaderText = "Password", 
                FillWeight = 15 
            });
            _gridVault.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "Category", 
                HeaderText = "Category", 
                FillWeight = 15 
            });
            _gridVault.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "Strength", 
                HeaderText = "Strength", 
                FillWeight = 10 
            });
            _gridVault.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "DateModified", 
                HeaderText = "Last Modified", 
                FillWeight = 20,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });
            
            // Buttons Panel
            var pnlButtons = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(10) };
            
            _btnAdd = new Button 
            { 
                Text = "➕ Add", 
                Location = new Point(10, 10), 
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(0, 122, 204),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnAdd.Click += (s, e) => AddCredential();
            
            _btnEdit = new Button 
            { 
                Text = "✏️ Edit", 
                Location = new Point(140, 10), 
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(255, 152, 0),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnEdit.Click += (s, e) => EditCredential();
            
            _btnDelete = new Button 
            { 
                Text = "🗑️ Delete", 
                Location = new Point(270, 10), 
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(244, 67, 54),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnDelete.Click += (s, e) => DeleteCredential();
            
            _btnViewPassword = new Button 
            { 
                Text = "👁️ View Password", 
                Location = new Point(410, 10), 
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(103, 58, 183),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnViewPassword.Click += (s, e) => ViewPassword();
            
            _btnCopyPassword = new Button 
            { 
                Text = "📋 Copy Password", 
                Location = new Point(570, 10), 
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(0, 150, 136),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnCopyPassword.Click += (s, e) => CopyPassword();
            
            pnlButtons.Controls.AddRange(new Control[] 
            { 
                _btnAdd, _btnEdit, _btnDelete, _btnViewPassword, _btnCopyPassword 
            });
            
            tab.Controls.Add(_gridVault);
            tab.Controls.Add(pnlSearch);
            tab.Controls.Add(pnlButtons);
            
            return tab;
        }
        
        private TabPage CreateStatisticsTab()
        {
            var tab = new TabPage("📊 Statistics");
            tab.BackColor = Color.FromArgb(45, 45, 48);
            
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };
            
            // Security Score
            var pnlScore = new GroupBox 
            { 
                Text = "Security Score", 
                Location = new Point(20, 20), 
                Size = new Size(300, 120),
                ForeColor = Color.White
            };
            
            _lblSecurityScore = new Label
            {
                Location = new Point(20, 40),
                Size = new Size(260, 60),
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            pnlScore.Controls.Add(_lblSecurityScore);
            
            // Statistics Labels
            var pnlStats = new GroupBox 
            { 
                Text = "Vault Statistics", 
                Location = new Point(340, 20), 
                Size = new Size(300, 120),
                ForeColor = Color.White
            };
            
            _lblTotalCreds = new Label 
            { 
                Text = "Total Credentials: 0", 
                Location = new Point(20, 30), 
                AutoSize = true 
            };
            
            _lblWeakPasswords = new Label 
            { 
                Text = "Weak Passwords: 0", 
                Location = new Point(20, 55), 
                AutoSize = true,
                ForeColor = Color.Orange
            };
            
            _lblDuplicates = new Label 
            { 
                Text = "Duplicate Passwords: 0", 
                Location = new Point(20, 80), 
                AutoSize = true,
                ForeColor = Color.Red
            };
            
            pnlStats.Controls.AddRange(new Control[] 
            { 
                _lblTotalCreds, _lblWeakPasswords, _lblDuplicates 
            });
            
            // Weak Passwords List
            var pnlWeak = new GroupBox 
            { 
                Text = "Weak Passwords (Click to Fix)", 
                Location = new Point(20, 160), 
                Size = new Size(400, 300),
                ForeColor = Color.White
            };
            
            _lstWeakPasswords = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            _lstWeakPasswords.DoubleClick += (s, e) => FixWeakPassword();
            
            pnlWeak.Controls.Add(_lstWeakPasswords);
            
            // Duplicates List
            var pnlDup = new GroupBox 
            { 
                Text = "Duplicate Passwords (Click to View)", 
                Location = new Point(440, 160), 
                Size = new Size(400, 300),
                ForeColor = Color.White
            };
            
            _lstDuplicates = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            _lstDuplicates.DoubleClick += (s, e) => ViewDuplicateDetails();
            
            pnlDup.Controls.Add(_lstDuplicates);
            
            // Refresh Button
            var btnRefreshStats = new Button
            {
                Text = "🔄 Refresh Statistics",
                Location = new Point(20, 480),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(0, 122, 204),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefreshStats.Click += (s, e) => LoadStatistics();
            
            mainPanel.Controls.AddRange(new Control[] 
            { 
                pnlScore, pnlStats, pnlWeak, pnlDup, btnRefreshStats 
            });
            
            tab.Controls.Add(mainPanel);
            
            return tab;
        }
        
        private TabPage CreateNotesTab()
        {
            var tab = new TabPage("📝 Secure Notes");
            tab.BackColor = Color.FromArgb(45, 45, 48);
            
            var label = new Label
            {
                Text = "Secure Notes feature coming soon!",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14)
            };
            
            tab.Controls.Add(label);
            
            return tab;
        }
        
        private void CreateStatusBar()
        {
            var statusStrip = new StatusStrip();
            var lblStatus = new ToolStripStatusLabel("Ready");
            var lblTime = new ToolStripStatusLabel 
            { 
                Spring = true, 
                TextAlign = ContentAlignment.MiddleRight 
            };
            
            var timer = new Timer { Interval = 1000 };
            timer.Tick += (s, e) => lblTime.Text = DateTime.Now.ToString("HH:mm:ss");
            timer.Start();
            
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblTime });
            this.Controls.Add(statusStrip);
        }

        // ... TO BE CONTINUED IN NEXT FILE ...
    }
}
