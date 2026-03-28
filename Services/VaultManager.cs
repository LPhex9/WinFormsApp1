using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MyPasswordManager.Models;

namespace MyPasswordManager.Services
{
    /// <summary>
    /// Central manager for all vault operations
    /// </summary>
    public class VaultManager
    {
        private readonly string _vaultPath;
        private readonly string _masterPassword;
        
        public List<Credential> Credentials { get; private set; } = new List<Credential>();
        public List<PasswordHistory> History { get; private set; } = new List<PasswordHistory>();
        public List<SecureNote> SecureNotes { get; private set; } = new List<SecureNote>();
        public List<CreditCard> CreditCards { get; private set; } = new List<CreditCard>();
        public AppSettings Settings { get; private set; } = new AppSettings();
        
        public VaultManager(string vaultPath, string masterPassword)
        {
            _vaultPath = vaultPath;
            _masterPassword = masterPassword;
        }
        
        /// <summary>
        /// Loads the entire vault from encrypted file
        /// </summary>
        public void Load()
        {
            if (!File.Exists(_vaultPath))
            {
                // First time - create empty vault
                Save();
                return;
            }
            
            try
            {
                string encryptedData = File.ReadAllText(_vaultPath);
                string jsonString = CryptoHelper.Decrypt(encryptedData, _masterPassword);
                
                var vaultData = JsonSerializer.Deserialize<VaultData>(jsonString);
                
                if (vaultData != null)
                {
                    Credentials = vaultData.Credentials ?? new List<Credential>();
                    History = vaultData.History ?? new List<PasswordHistory>();
                    SecureNotes = vaultData.SecureNotes ?? new List<SecureNote>();
                    CreditCards = vaultData.CreditCards ?? new List<CreditCard>();
                    Settings = vaultData.Settings ?? new AppSettings();
                }
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                throw new UnauthorizedAccessException("Incorrect master password");
            }
        }
        
        /// <summary>
        /// Saves the entire vault to encrypted file
        /// </summary>
        public void Save()
        {
            var vaultData = new VaultData
            {
                Credentials = Credentials,
                History = History,
                SecureNotes = SecureNotes,
                CreditCards = CreditCards,
                Settings = Settings,
                LastModified = DateTime.Now
            };
            
            string jsonString = JsonSerializer.Serialize(vaultData, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            string encryptedData = CryptoHelper.Encrypt(jsonString, _masterPassword);
            File.WriteAllText(_vaultPath, encryptedData);
        }
        
        // ===== CREDENTIAL OPERATIONS =====
        
        public void AddCredential(Credential credential)
        {
            credential.DateCreated = DateTime.Now;
            credential.DateModified = DateTime.Now;
            credential.Strength = PasswordService.CalculateStrength(credential.Password);
            
            Credentials.Add(credential);
            Save();
        }
        
        public void UpdateCredential(Credential credential, bool saveToHistory = true)
        {
            var existing = Credentials.FirstOrDefault(c => c.Id == credential.Id);
            if (existing == null)
                return;
            
            // Save old password to history
            if (saveToHistory && existing.Password != credential.Password)
            {
                var historyEntry = new PasswordHistory
                {
                    CredentialId = credential.Id,
                    OldPassword = existing.Password,
                    ChangedDate = DateTime.Now,
                    ChangeReason = "Manual update",
                    Strength = existing.Strength
                };
                
                History.Add(historyEntry);
                credential.LastPasswordChange = DateTime.Now;
            }
            
            credential.DateModified = DateTime.Now;
            credential.Strength = PasswordService.CalculateStrength(credential.Password);
            
            int index = Credentials.IndexOf(existing);
            Credentials[index] = credential;
            
            Save();
        }
        
        public void DeleteCredential(Guid credentialId)
        {
            var credential = Credentials.FirstOrDefault(c => c.Id == credentialId);
            if (credential != null)
            {
                Credentials.Remove(credential);
                
                // Also remove history
                History.RemoveAll(h => h.CredentialId == credentialId);
                
                Save();
            }
        }
        
        public Credential? GetCredential(Guid credentialId)
        {
            return Credentials.FirstOrDefault(c => c.Id == credentialId);
        }
        
        public List<Credential> SearchCredentials(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Credentials;
            
            query = query.ToLower();
            
            return Credentials.Where(c =>
                c.Title.ToLower().Contains(query) ||
                c.Username.ToLower().Contains(query) ||
                c.Url.ToLower().Contains(query) ||
                c.Category.ToLower().Contains(query) ||
                c.Notes.ToLower().Contains(query)
            ).ToList();
        }
        
        public List<Credential> GetCredentialsByCategory(string category)
        {
            return Credentials.Where(c => c.Category == category).ToList();
        }
        
        public List<Credential> GetFavorites()
        {
            return Credentials.Where(c => c.IsFavorite).ToList();
        }
        
        public List<Credential> GetExpiredPasswords()
        {
            return Credentials.Where(c => c.IsExpired).ToList();
        }
        
        public List<Credential> GetExpiringSoonPasswords()
        {
            return Credentials.Where(c => c.IsExpiringSoon).ToList();
        }
        
        public List<Credential> GetWeakPasswords()
        {
            return Credentials.Where(c => c.IsWeak).ToList();
        }
        
        public Dictionary<string, List<Credential>> GetDuplicatePasswords()
        {
            return PasswordService.FindDuplicates(Credentials);
        }
        
        // ===== HISTORY OPERATIONS =====
        
        public List<PasswordHistory> GetPasswordHistory(Guid credentialId)
        {
            return History
                .Where(h => h.CredentialId == credentialId)
                .OrderByDescending(h => h.ChangedDate)
                .ToList();
        }
        
        public void ClearOldHistory(int keepLastN = 10)
        {
            var grouped = History.GroupBy(h => h.CredentialId);
            
            var toRemove = new List<PasswordHistory>();
            
            foreach (var group in grouped)
            {
                var sorted = group.OrderByDescending(h => h.ChangedDate).Skip(keepLastN);
                toRemove.AddRange(sorted);
            }
            
            foreach (var item in toRemove)
            {
                History.Remove(item);
            }
            
            if (toRemove.Any())
                Save();
        }
        
        // ===== SECURE NOTES =====
        
        public void AddSecureNote(SecureNote note)
        {
            SecureNotes.Add(note);
            Save();
        }
        
        public void UpdateSecureNote(SecureNote note)
        {
            var existing = SecureNotes.FirstOrDefault(n => n.Id == note.Id);
            if (existing != null)
            {
                note.DateModified = DateTime.Now;
                int index = SecureNotes.IndexOf(existing);
                SecureNotes[index] = note;
                Save();
            }
        }
        
        public void DeleteSecureNote(Guid noteId)
        {
            var note = SecureNotes.FirstOrDefault(n => n.Id == noteId);
            if (note != null)
            {
                SecureNotes.Remove(note);
                Save();
            }
        }
        
        // ===== CREDIT CARDS =====
        
        public void AddCreditCard(CreditCard card)
        {
            CreditCards.Add(card);
            Save();
        }
        
        public void UpdateCreditCard(CreditCard card)
        {
            var existing = CreditCards.FirstOrDefault(c => c.Id == card.Id);
            if (existing != null)
            {
                int index = CreditCards.IndexOf(existing);
                CreditCards[index] = card;
                Save();
            }
        }
        
        public void DeleteCreditCard(Guid cardId)
        {
            var card = CreditCards.FirstOrDefault(c => c.Id == cardId);
            if (card != null)
            {
                CreditCards.Remove(card);
                Save();
            }
        }
        
        // ===== STATISTICS =====
        
        public VaultStatistics GetStatistics()
        {
            return new VaultStatistics
            {
                TotalCredentials = Credentials.Count,
                TotalSecureNotes = SecureNotes.Count,
                TotalCreditCards = CreditCards.Count,
                WeakPasswords = Credentials.Count(c => c.IsWeak),
                ExpiredPasswords = Credentials.Count(c => c.IsExpired),
                ExpiringSoonPasswords = Credentials.Count(c => c.IsExpiringSoon),
                DuplicatePasswordCount = GetDuplicatePasswords().Sum(kvp => kvp.Value.Count),
                AveragePasswordStrength = Credentials.Any() 
                    ? Credentials.Average(c => (int)c.Strength) 
                    : 0,
                OldestPasswordDays = Credentials.Any() && Credentials.Any(c => c.LastPasswordChange.HasValue)
                    ? Credentials.Where(c => c.LastPasswordChange.HasValue)
                                .Max(c => c.DaysSinceLastChange)
                    : 0
            };
        }
        
        // ===== IMPORT/EXPORT =====
        
        public void ExportToCSV(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Title,Username,Password,URL,Category,Notes,DateCreated,ExpirationDate");
                
                foreach (var cred in Credentials)
                {
                    writer.WriteLine($"{EscapeCSV(cred.Title)}," +
                                   $"{EscapeCSV(cred.Username)}," +
                                   $"{EscapeCSV(cred.Password)}," +
                                   $"{EscapeCSV(cred.Url)}," +
                                   $"{EscapeCSV(cred.Category)}," +
                                   $"{EscapeCSV(cred.Notes)}," +
                                   $"{cred.DateCreated:yyyy-MM-dd HH:mm:ss}," +
                                   $"{(cred.ExpirationDate?.ToString("yyyy-MM-dd") ?? "")}");
                }
            }
        }
        
        public int ImportFromCSV(string filePath)
        {
            int imported = 0;
            
            using (var reader = new StreamReader(filePath))
            {
                reader.ReadLine(); // Skip header
                
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    var values = ParseCSVLine(line);
                    if (values.Length < 3) continue;
                    
                    var credential = new Credential
                    {
                        Title = values.Length > 0 ? values[0] : "",
                        Username = values.Length > 1 ? values[1] : "",
                        Password = values.Length > 2 ? values[2] : "",
                        Url = values.Length > 3 ? values[3] : "",
                        Category = values.Length > 4 ? values[4] : "Imported",
                        Notes = values.Length > 5 ? values[5] : ""
                    };
                    
                    AddCredential(credential);
                    imported++;
                }
            }
            
            return imported;
        }
        
        private string EscapeCSV(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
        
        private string[] ParseCSVLine(string line)
        {
            var values = new List<string>();
            bool inQuotes = false;
            var currentValue = new System.Text.StringBuilder();
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentValue.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }
            
            values.Add(currentValue.ToString());
            return values.ToArray();
        }
    }
    
    /// <summary>
    /// Container for all vault data
    /// </summary>
    public class VaultData
    {
        public List<Credential> Credentials { get; set; } = new List<Credential>();
        public List<PasswordHistory> History { get; set; } = new List<PasswordHistory>();
        public List<SecureNote> SecureNotes { get; set; } = new List<SecureNote>();
        public List<CreditCard> CreditCards { get; set; } = new List<CreditCard>();
        public AppSettings Settings { get; set; } = new AppSettings();
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
    
    /// <summary>
    /// Statistics about the vault
    /// </summary>
    public class VaultStatistics
    {
        public int TotalCredentials { get; set; }
        public int TotalSecureNotes { get; set; }
        public int TotalCreditCards { get; set; }
        public int WeakPasswords { get; set; }
        public int ExpiredPasswords { get; set; }
        public int ExpiringSoonPasswords { get; set; }
        public int DuplicatePasswordCount { get; set; }
        public double AveragePasswordStrength { get; set; }
        public int OldestPasswordDays { get; set; }
        
        public int SecurityScore
        {
            get
            {
                if (TotalCredentials == 0) return 100;
                
                int score = 100;
                
                // Penalties
                score -= (WeakPasswords * 30) / TotalCredentials;
                score -= (ExpiredPasswords * 20) / TotalCredentials;
                score -= (DuplicatePasswordCount * 15) / TotalCredentials;
                
                if (OldestPasswordDays > 365)
                    score -= 10;
                
                return Math.Max(0, score);
            }
        }
    }
}
