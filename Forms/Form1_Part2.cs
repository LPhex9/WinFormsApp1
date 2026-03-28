// CONTINUATION OF Form1.cs - PART 2: Functionality Methods

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MyPasswordManager.Models;
using MyPasswordManager.Services;

namespace MyPasswordManager
{
    public partial class Form1
    {
        // ===== AUTO-LOCK FUNCTIONALITY =====
        
        private void InitializeAutoLock()
        {
            _autoLockTimer = new Timer();
            _autoLockTimer.Interval = _vaultManager.Settings.AutoLockMinutes * 60 * 1000;
            _autoLockTimer.Tick += (s, e) =>
            {
                _autoLockTimer.Stop();
                LockVault();
            };
            
            if (_vaultManager.Settings.AutoLockEnabled)
            {
                _autoLockTimer.Start();
                
                // Reset timer on user activity
                this.MouseMove += ResetAutoLockTimer;
                this.KeyPress += ResetAutoLockTimer;
                this.Click += ResetAutoLockTimer;
            }
        }
        
        private void ResetAutoLockTimer(object? sender, EventArgs e)
        {
            if (_vaultManager.Settings.AutoLockEnabled)
            {
                _autoLockTimer.Stop();
                _autoLockTimer.Start();
            }
        }
        
        private void LockVault()
        {
            _autoLockTimer.Stop();
            
            var result = MessageBox.Show(
                "Vault will be locked. You'll need to enter your master password again.",
                "Lock Vault",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information);
            
            if (result == DialogResult.OK)
            {
                this.Hide();
                
                using (var loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() == DialogResult.OK && loginForm.IsAuthenticated)
                    {
                        _currentMasterPassword = loginForm.MasterPassword;
                        
                        // Verify password by trying to load vault
                        try
                        {
                            var tempVault = new VaultManager("vault.nexus", _currentMasterPassword);
                            tempVault.Load();
                            
                            this.Show();
                            _autoLockTimer.Start();
                        }
                        catch
                        {
                            MessageBox.Show("Incorrect password!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Application.Exit();
                        }
                    }
                    else
                    {
                        Application.Exit();
                    }
                }
            }
            else
            {
                _autoLockTimer.Start();
            }
        }
        
        // ===== PASSWORD GENERATOR =====
        
        private void GeneratePassword()
        {
            try
            {
                string password = PasswordService.GeneratePassword(
                    length: (int)_numLength.Value,
                    includeUppercase: _cbUpper.Checked,
                    includeLowercase: _cbLower.Checked,
                    includeNumbers: _cbNumbers.Checked,
                    includeSymbols: _cbSymbols.Checked,
                    excludeAmbiguous: _cbExcludeAmbiguous.Checked
                );
                
                _txtGeneratedPassword.Text = password;
                UpdatePasswordStrengthDisplay(password);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating password: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void UpdatePasswordStrengthDisplay(string password)
        {
            var strength = PasswordService.CalculateStrength(password);
            double entropy = PasswordService.CalculateEntropy(password);
            string crackTime = PasswordService.EstimateCrackTime(password);
            
            // Update labels
            _lblStrength.Text = strength.ToString();
            _lblEntropy.Text = $"Entropy: {entropy:F1} bits";
            _lblCrackTime.Text = $"Time to crack: {crackTime}";
            
            // Update strength color
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
            
            _lblStrength.ForeColor = strengthColor;
            
            // Update strength bars
            int activeBarCount = (int)strength;
            for (int i = 0; i < 4; i++)
            {
                var bar = _pnlStrengthBars.Controls[$"bar{i}"];
                if (bar != null)
                {
                    bar.BackColor = i < activeBarCount ? strengthColor : Color.FromArgb(80, 80, 80);
                }
            }
        }
        
        private void CopyGeneratedPassword()
        {
            if (!string.IsNullOrEmpty(_txtGeneratedPassword.Text))
            {
                Clipboard.SetText(_txtGeneratedPassword.Text);
                MessageBox.Show("Password copied to clipboard!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Optional: Clear clipboard after X seconds
                if (_vaultManager.Settings.EnableClipboardClear)
                {
                    StartClipboardClearTimer();
                }
            }
        }
        
        private void SaveGeneratedToVault()
        {
            if (string.IsNullOrEmpty(_txtGeneratedPassword.Text))
            {
                MessageBox.Show("Please generate a password first!", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var credential = new Credential
            {
                Title = "New Entry",
                Password = _txtGeneratedPassword.Text,
                Strength = PasswordService.CalculateStrength(_txtGeneratedPassword.Text)
            };
            
            // Open edit dialog to fill in details
            using (var editForm = new CredentialEditForm(credential, _vaultManager))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    _vaultManager.AddCredential(editForm.Credential);
                    RefreshVault();
                    _mainTabs.SelectedIndex = 0; // Switch to Vault tab
                    
                    MessageBox.Show("Password saved to vault!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        
        // ===== VAULT OPERATIONS =====
        
        private void LoadVaultData()
        {
            RefreshVault();
            LoadStatistics();
        }
        
        private void RefreshVault()
        {
            _credentialsBinding.Clear();
            
            foreach (var cred in _vaultManager.Credentials.OrderByDescending(c => c.DateModified))
            {
                _credentialsBinding.Add(cred);
            }
            
            // Reset filters
            _txtSearch.Text = "";
            _cbCategoryFilter.SelectedIndex = 0;
        }
        
        private void FilterVault()
        {
            string searchText = _txtSearch.Text.ToLower();
            string selectedCategory = _cbCategoryFilter.SelectedItem?.ToString() ?? "All";
            
            _credentialsBinding.Clear();
            
            var filtered = _vaultManager.Credentials.AsEnumerable();
            
            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(c =>
                    c.Title.ToLower().Contains(searchText) ||
                    c.Username.ToLower().Contains(searchText) ||
                    c.Url.ToLower().Contains(searchText) ||
                    c.Category.ToLower().Contains(searchText)
                );
            }
            
            // Apply category filter
            if (selectedCategory != "All")
            {
                filtered = filtered.Where(c => c.Category == selectedCategory);
            }
            
            foreach (var cred in filtered.OrderByDescending(c => c.DateModified))
            {
                _credentialsBinding.Add(cred);
            }
        }
        
        private void AddCredential()
        {
            var credential = new Credential();
            
            using (var editForm = new CredentialEditForm(credential, _vaultManager))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    _vaultManager.AddCredential(editForm.Credential);
                    RefreshVault();
                    LoadStatistics();
                }
            }
        }
        
        private void EditCredential()
        {
            if (_gridVault.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a credential to edit.", "Info", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            var credential = (Credential)_gridVault.SelectedRows[0].DataBoundItem;
            
            using (var editForm = new CredentialEditForm(credential, _vaultManager))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    _vaultManager.UpdateCredential(editForm.Credential);
                    RefreshVault();
                    LoadStatistics();
                }
            }
        }
        
        private void DeleteCredential()
        {
            if (_gridVault.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a credential to delete.", "Info", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            var credential = (Credential)_gridVault.SelectedRows[0].DataBoundItem;
            
            var result = MessageBox.Show(
                $"Are you sure you want to delete '{credential.Title}'?\nThis action cannot be undone!",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            
            if (result == DialogResult.Yes)
            {
                _vaultManager.DeleteCredential(credential.Id);
                RefreshVault();
                LoadStatistics();
                
                MessageBox.Show("Credential deleted successfully.", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        private void ViewPassword()
        {
            if (_gridVault.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a credential to view.", "Info", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            var credential = (Credential)_gridVault.SelectedRows[0].DataBoundItem;
            
            MessageBox.Show(
                $"Password for '{credential.Title}':\n\n{credential.Password}",
                "Password",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        
        private void CopyPassword()
        {
            if (_gridVault.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a credential to copy.", "Info", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            var credential = (Credential)_gridVault.SelectedRows[0].DataBoundItem;
            
            Clipboard.SetText(credential.Password);
            
            // Update usage stats
            credential.TimesUsed++;
            credential.LastAccessed = DateTime.Now;
            _vaultManager.UpdateCredential(credential, saveToHistory: false);
            
            MessageBox.Show("Password copied to clipboard!", "Success", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // Optional: Clear clipboard after X seconds
            if (_vaultManager.Settings.EnableClipboardClear)
            {
                StartClipboardClearTimer();
            }
        }
        
        private void StartClipboardClearTimer()
        {
            if (_clipboardTimer != null)
            {
                _clipboardTimer.Stop();
                _clipboardTimer.Dispose();
            }
            
            _clipboardTimer = new Timer();
            _clipboardTimer.Interval = _vaultManager.Settings.ClipboardClearSeconds * 1000;
            _clipboardTimer.Tick += (s, e) =>
            {
                Clipboard.Clear();
                _clipboardTimer.Stop();
            };
            _clipboardTimer.Start();
        }
        
        // ===== STATISTICS =====
        
        private void LoadStatistics()
        {
            var stats = _vaultManager.GetStatistics();
            
            _lblSecurityScore.Text = $"{stats.SecurityScore}/100";
            _lblSecurityScore.ForeColor = stats.SecurityScore >= 80 ? Color.Lime :
                                          stats.SecurityScore >= 50 ? Color.Orange : Color.Red;
            
            _lblTotalCreds.Text = $"Total Credentials: {stats.TotalCredentials}";
            _lblWeakPasswords.Text = $"Weak Passwords: {stats.WeakPasswords}";
            _lblDuplicates.Text = $"Duplicate Passwords: {stats.DuplicatePasswordCount}";
            
            // Load weak passwords list
            _lstWeakPasswords.Items.Clear();
            foreach (var weak in _vaultManager.GetWeakPasswords())
            {
                _lstWeakPasswords.Items.Add($"[{weak.Strength}] {weak.Title} - {weak.Username}");
            }
            
            // Load duplicates list
            _lstDuplicates.Items.Clear();
            var duplicates = _vaultManager.GetDuplicatePasswords();
            foreach (var group in duplicates)
            {
                _lstDuplicates.Items.Add($"{group.Value.Count} entries share the same password:");
                foreach (var cred in group.Value)
                {
                    _lstDuplicates.Items.Add($"  - {cred.Title} ({cred.Username})");
                }
                _lstDuplicates.Items.Add(""); // Empty line
            }
        }
        
        private void FixWeakPassword()
        {
            if (_lstWeakPasswords.SelectedIndex < 0) return;
            
            // Find the credential
            var weakCreds = _vaultManager.GetWeakPasswords();
            if (_lstWeakPasswords.SelectedIndex < weakCreds.Count)
            {
                var credential = weakCreds[_lstWeakPasswords.SelectedIndex];
                
                // Generate a strong password
                string newPassword = PasswordService.GeneratePassword(20, true, true, true, true);
                
                var result = MessageBox.Show(
                    $"Replace weak password for '{credential.Title}' with:\n\n{newPassword}\n\nContinue?",
                    "Fix Weak Password",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    credential.Password = newPassword;
                    credential.Strength = PasswordService.CalculateStrength(newPassword);
                    credential.LastPasswordChange = DateTime.Now;
                    
                    _vaultManager.UpdateCredential(credential, saveToHistory: true);
                    
                    LoadStatistics();
                    RefreshVault();
                    
                    MessageBox.Show("Password updated successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        
        private void ViewDuplicateDetails()
        {
            ShowDuplicates();
        }
        
        // ===== MENU OPERATIONS =====
        
        private void ImportFromCSV()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                dialog.Title = "Import Passwords from CSV";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        int imported = _vaultManager.ImportFromCSV(dialog.FileName);
                        
                        RefreshVault();
                        LoadStatistics();
                        
                        MessageBox.Show($"Successfully imported {imported} credentials!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error importing: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        private void ExportToCSV()
        {
            var result = MessageBox.Show(
                "WARNING: Exported file will contain your passwords in plain text!\n\n" +
                "Make sure to store it securely and delete it when done.\n\nContinue?",
                "Security Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            
            if (result == DialogResult.Yes)
            {
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                    dialog.Title = "Export Passwords to CSV";
                    dialog.FileName = $"vault_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            _vaultManager.ExportToCSV(dialog.FileName);
                            
                            MessageBox.Show($"Vault exported successfully to:\n{dialog.FileName}", "Success", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error exporting: {ex.Message}", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
        
        private void ShowDuplicates()
        {
            var duplicates = _vaultManager.GetDuplicatePasswords();
            
            if (duplicates.Count == 0)
            {
                MessageBox.Show("No duplicate passwords found! 🎉", "Great!", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            var message = "Found duplicate passwords:\n\n";
            
            foreach (var group in duplicates)
            {
                message += $"• {group.Value.Count} credentials share the same password:\n";
                foreach (var cred in group.Value)
                {
                    message += $"  - {cred.Title} ({cred.Username})\n";
                }
                message += "\n";
            }
            
            MessageBox.Show(message, "Duplicate Passwords", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        
        private void ShowWeakPasswords()
        {
            var weakPasswords = _vaultManager.GetWeakPasswords();
            
            if (weakPasswords.Count == 0)
            {
                MessageBox.Show("No weak passwords found! 🎉", "Excellent!", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            _mainTabs.SelectedIndex = 2; // Switch to Statistics tab
            MessageBox.Show($"Found {weakPasswords.Count} weak passwords.\nCheck the Statistics tab for details.", 
                "Weak Passwords", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        
        private void ShowPasswordHistory()
        {
            if (_gridVault.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a credential to view history.", "Info", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            var credential = (Credential)_gridVault.SelectedRows[0].DataBoundItem;
            var history = _vaultManager.GetPasswordHistory(credential.Id);
            
            if (history.Count == 0)
            {
                MessageBox.Show("No password history available for this credential.", "Info", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            using (var historyForm = new PasswordHistoryForm(credential, history))
            {
                historyForm.ShowDialog();
            }
        }
        
        private void ShowSettings()
        {
            using (var settingsForm = new SettingsForm(_vaultManager.Settings))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    _vaultManager.Save();
                    
                    // Restart auto-lock with new settings
                    _autoLockTimer.Stop();
                    InitializeAutoLock();
                    
                    MessageBox.Show("Settings saved successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        
        private void ShowAbout()
        {
            MessageBox.Show(
                "Nexus Password Vault Pro v2.0\n\n" +
                "A secure password manager with:\n" +
                "• AES-256 encryption\n" +
                "• Password strength analysis\n" +
                "• Duplicate detection\n" +
                "• Password history\n" +
                "• Auto-lock protection\n" +
                "• Import/Export functionality\n\n" +
                "Built with security and usability in mind.",
                "About Nexus Password Vault",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
