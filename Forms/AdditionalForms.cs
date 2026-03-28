using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MyPasswordManager.Models;

namespace MyPasswordManager
{
    // ===== PASSWORD HISTORY FORM =====
    
    public class PasswordHistoryForm : Form
    {
        private Credential _credential;
        private List<PasswordHistory> _history;
        private ListView _listView = null!;
        
        public PasswordHistoryForm(Credential credential, List<PasswordHistory> history)
        {
            _credential = credential;
            _history = history;
            
            InitializeUI();
            LoadHistory();
        }
        
        private void InitializeUI()
        {
            Text = $"Password History - {_credential.Title}";
            Size = new Size(700, 500);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(45, 45, 48);
            ForeColor = Color.White;
            
            var label = new Label
            {
                Text = $"Password change history for: {_credential.Title}",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            this.Controls.Add(label);
            
            _listView = new ListView
            {
                Location = new Point(20, 60),
                Size = new Size(640, 350),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            
            _listView.Columns.Add("Date Changed", 150);
            _listView.Columns.Add("Old Password", 200);
            _listView.Columns.Add("Strength", 100);
            _listView.Columns.Add("Reason", 150);
            
            this.Controls.Add(_listView);
            
            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(580, 425),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            this.Controls.Add(btnClose);
        }
        
        private void LoadHistory()
        {
            foreach (var entry in _history)
            {
                var item = new ListViewItem(entry.ChangedDate.ToString("dd/MM/yyyy HH:mm"));
                item.SubItems.Add(entry.OldPassword);
                item.SubItems.Add(entry.Strength.ToString());
                item.SubItems.Add(entry.ChangeReason);
                
                _listView.Items.Add(item);
            }
        }
    }
    
    // ===== SETTINGS FORM =====
    
    public class SettingsForm : Form
    {
        public AppSettings Settings { get; private set; }
        
        private NumericUpDown _numAutoLockMinutes = null!;
        private CheckBox _cbAutoLockEnabled = null!;
        private NumericUpDown _numPasswordLength = null!;
        private CheckBox _cbIncludeUppercase = null!;
        private CheckBox _cbIncludeLowercase = null!;
        private CheckBox _cbIncludeNumbers = null!;
        private CheckBox _cbIncludeSymbols = null!;
        private CheckBox _cbExcludeAmbiguous = null!;
        private NumericUpDown _numExpirationDays = null!;
        private CheckBox _cbEnableExpirationWarnings = null!;
        private NumericUpDown _numWarningDays = null!;
        private CheckBox _cbEnableClipboardClear = null!;
        private NumericUpDown _numClipboardSeconds = null!;
        
        public SettingsForm(AppSettings settings)
        {
            Settings = new AppSettings
            {
                // Copy all settings
                AutoLockMinutes = settings.AutoLockMinutes,
                AutoLockEnabled = settings.AutoLockEnabled,
                DefaultPasswordLength = settings.DefaultPasswordLength,
                DefaultIncludeUppercase = settings.DefaultIncludeUppercase,
                DefaultIncludeLowercase = settings.DefaultIncludeLowercase,
                DefaultIncludeNumbers = settings.DefaultIncludeNumbers,
                DefaultIncludeSymbols = settings.DefaultIncludeSymbols,
                ExcludeAmbiguousCharacters = settings.ExcludeAmbiguousCharacters,
                DefaultExpirationDays = settings.DefaultExpirationDays,
                EnableExpirationWarnings = settings.EnableExpirationWarnings,
                ExpirationWarningDays = settings.ExpirationWarningDays,
                EnableClipboardClear = settings.EnableClipboardClear,
                ClipboardClearSeconds = settings.ClipboardClearSeconds
            };
            
            InitializeUI();
            LoadSettings();
        }
        
        private void InitializeUI()
        {
            Text = "Settings";
            Size = new Size(500, 650);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(45, 45, 48);
            ForeColor = Color.White;
            
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };
            
            tabControl.TabPages.Add(CreateSecurityTab());
            tabControl.TabPages.Add(CreatePasswordTab());
            tabControl.TabPages.Add(CreateExpirationTab());
            
            this.Controls.Add(tabControl);
            
            // Buttons
            var btnSave = new Button
            {
                Text = "💾 Save",
                Location = new Point(300, 570),
                Size = new Size(80, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnSave.Click += (s, e) => SaveSettings();
            
            var btnCancel = new Button
            {
                Text = "❌ Cancel",
                Location = new Point(390, 570),
                Size = new Size(80, 35),
                BackColor = Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }
        
        private TabPage CreateSecurityTab()
        {
            var tab = new TabPage("Security");
            tab.BackColor = Color.FromArgb(45, 45, 48);
            
            int y = 20;
            
            // Auto-lock
            var groupAutoLock = new GroupBox
            {
                Text = "Auto-Lock",
                Location = new Point(20, y),
                Size = new Size(420, 120),
                ForeColor = Color.White
            };
            
            _cbAutoLockEnabled = new CheckBox
            {
                Text = "Enable auto-lock after inactivity",
                Location = new Point(15, 30),
                AutoSize = true
            };
            
            var lblMinutes = new Label
            {
                Text = "Minutes of inactivity:",
                Location = new Point(15, 60),
                AutoSize = true
            };
            
            _numAutoLockMinutes = new NumericUpDown
            {
                Location = new Point(180, 58),
                Width = 60,
                Minimum = 1,
                Maximum = 120,
                Value = 5
            };
            
            groupAutoLock.Controls.AddRange(new Control[] 
            { 
                _cbAutoLockEnabled, lblMinutes, _numAutoLockMinutes 
            });
            
            tab.Controls.Add(groupAutoLock);
            y += 140;
            
            // Clipboard
            var groupClipboard = new GroupBox
            {
                Text = "Clipboard",
                Location = new Point(20, y),
                Size = new Size(420, 120),
                ForeColor = Color.White
            };
            
            _cbEnableClipboardClear = new CheckBox
            {
                Text = "Auto-clear clipboard after copying password",
                Location = new Point(15, 30),
                AutoSize = true
            };
            
            var lblSeconds = new Label
            {
                Text = "Clear after (seconds):",
                Location = new Point(15, 60),
                AutoSize = true
            };
            
            _numClipboardSeconds = new NumericUpDown
            {
                Location = new Point(180, 58),
                Width = 60,
                Minimum = 5,
                Maximum = 300,
                Value = 30
            };
            
            groupClipboard.Controls.AddRange(new Control[] 
            { 
                _cbEnableClipboardClear, lblSeconds, _numClipboardSeconds 
            });
            
            tab.Controls.Add(groupClipboard);
            
            return tab;
        }
        
        private TabPage CreatePasswordTab()
        {
            var tab = new TabPage("Password Defaults");
            tab.BackColor = Color.FromArgb(45, 45, 48);
            
            var group = new GroupBox
            {
                Text = "Default Password Generator Settings",
                Location = new Point(20, 20),
                Size = new Size(420, 300),
                ForeColor = Color.White
            };
            
            int y = 30;
            
            var lblLength = new Label
            {
                Text = "Default length:",
                Location = new Point(15, y),
                AutoSize = true
            };
            
            _numPasswordLength = new NumericUpDown
            {
                Location = new Point(150, y - 2),
                Width = 60,
                Minimum = 8,
                Maximum = 64,
                Value = 16
            };
            
            group.Controls.AddRange(new Control[] { lblLength, _numPasswordLength });
            y += 40;
            
            _cbIncludeUppercase = new CheckBox
            {
                Text = "Include Uppercase (A-Z)",
                Location = new Point(15, y),
                AutoSize = true
            };
            y += 30;
            
            _cbIncludeLowercase = new CheckBox
            {
                Text = "Include Lowercase (a-z)",
                Location = new Point(15, y),
                AutoSize = true
            };
            y += 30;
            
            _cbIncludeNumbers = new CheckBox
            {
                Text = "Include Numbers (0-9)",
                Location = new Point(15, y),
                AutoSize = true
            };
            y += 30;
            
            _cbIncludeSymbols = new CheckBox
            {
                Text = "Include Symbols (!@#$%)",
                Location = new Point(15, y),
                AutoSize = true
            };
            y += 40;
            
            _cbExcludeAmbiguous = new CheckBox
            {
                Text = "Exclude ambiguous characters (0/O, l/1/I)",
                Location = new Point(15, y),
                AutoSize = true
            };
            
            group.Controls.AddRange(new Control[] 
            { 
                _cbIncludeUppercase, _cbIncludeLowercase, _cbIncludeNumbers, 
                _cbIncludeSymbols, _cbExcludeAmbiguous 
            });
            
            tab.Controls.Add(group);
            
            return tab;
        }
        
        private TabPage CreateExpirationTab()
        {
            var tab = new TabPage("Password Expiration");
            tab.BackColor = Color.FromArgb(45, 45, 48);
            
            var group = new GroupBox
            {
                Text = "Password Expiration Settings",
                Location = new Point(20, 20),
                Size = new Size(420, 200),
                ForeColor = Color.White
            };
            
            int y = 30;
            
            var lblExpiration = new Label
            {
                Text = "Default expiration (days):",
                Location = new Point(15, y),
                AutoSize = true
            };
            
            _numExpirationDays = new NumericUpDown
            {
                Location = new Point(200, y - 2),
                Width = 60,
                Minimum = 30,
                Maximum = 365,
                Value = 90
            };
            
            group.Controls.AddRange(new Control[] { lblExpiration, _numExpirationDays });
            y += 40;
            
            _cbEnableExpirationWarnings = new CheckBox
            {
                Text = "Enable expiration warnings",
                Location = new Point(15, y),
                AutoSize = true
            };
            y += 30;
            
            var lblWarning = new Label
            {
                Text = "Warn (days before expiration):",
                Location = new Point(15, y),
                AutoSize = true
            };
            
            _numWarningDays = new NumericUpDown
            {
                Location = new Point(220, y - 2),
                Width = 60,
                Minimum = 1,
                Maximum = 30,
                Value = 7
            };
            
            group.Controls.AddRange(new Control[] 
            { 
                _cbEnableExpirationWarnings, lblWarning, _numWarningDays 
            });
            
            tab.Controls.Add(group);
            
            return tab;
        }
        
        private void LoadSettings()
        {
            _cbAutoLockEnabled.Checked = Settings.AutoLockEnabled;
            _numAutoLockMinutes.Value = Settings.AutoLockMinutes;
            _numPasswordLength.Value = Settings.DefaultPasswordLength;
            _cbIncludeUppercase.Checked = Settings.DefaultIncludeUppercase;
            _cbIncludeLowercase.Checked = Settings.DefaultIncludeLowercase;
            _cbIncludeNumbers.Checked = Settings.DefaultIncludeNumbers;
            _cbIncludeSymbols.Checked = Settings.DefaultIncludeSymbols;
            _cbExcludeAmbiguous.Checked = Settings.ExcludeAmbiguousCharacters;
            _numExpirationDays.Value = Settings.DefaultExpirationDays;
            _cbEnableExpirationWarnings.Checked = Settings.EnableExpirationWarnings;
            _numWarningDays.Value = Settings.ExpirationWarningDays;
            _cbEnableClipboardClear.Checked = Settings.EnableClipboardClear;
            _numClipboardSeconds.Value = Settings.ClipboardClearSeconds;
        }
        
        private void SaveSettings()
        {
            Settings.AutoLockEnabled = _cbAutoLockEnabled.Checked;
            Settings.AutoLockMinutes = (int)_numAutoLockMinutes.Value;
            Settings.DefaultPasswordLength = (int)_numPasswordLength.Value;
            Settings.DefaultIncludeUppercase = _cbIncludeUppercase.Checked;
            Settings.DefaultIncludeLowercase = _cbIncludeLowercase.Checked;
            Settings.DefaultIncludeNumbers = _cbIncludeNumbers.Checked;
            Settings.DefaultIncludeSymbols = _cbIncludeSymbols.Checked;
            Settings.ExcludeAmbiguousCharacters = _cbExcludeAmbiguous.Checked;
            Settings.DefaultExpirationDays = (int)_numExpirationDays.Value;
            Settings.EnableExpirationWarnings = _cbEnableExpirationWarnings.Checked;
            Settings.ExpirationWarningDays = (int)_numWarningDays.Value;
            Settings.EnableClipboardClear = _cbEnableClipboardClear.Checked;
            Settings.ClipboardClearSeconds = (int)_numClipboardSeconds.Value;
        }
    }
}
