using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MyPasswordManager.Models;

namespace MyPasswordManager.Services
{
    public class VaultStatistics
    {
        public int TotalCredentials { get; set; }
        public int WeakPasswords { get; set; }
        public int DuplicatePasswordCount { get; set; }
        public int SecurityScore => TotalCredentials == 0 ? 100 : Math.Max(0, 100 - (WeakPasswords * 10));
    }

    public class VaultManager
    {
        private string _path; private string _pass;
        public List<Credential> Credentials { get; set; } = new List<Credential>();
        public AppSettings Settings { get; set; } = new AppSettings();
        public List<PasswordHistory> History { get; set; } = new List<PasswordHistory>();

        public VaultManager(string path, string pass) { _path = path; _pass = pass; }

        public void Load()
        {
            if (!File.Exists(_path)) return;
            try
            {
                string dec = CryptoHelper.Decrypt(File.ReadAllText(_path), _pass);
                var data = JsonSerializer.Deserialize<VaultData>(dec);
                if (data != null) { Credentials = data.Credentials; Settings = data.Settings; History = data.History; }
            }
            catch { throw new UnauthorizedAccessException(); }
        }

        public void Save()
        {
            var data = new VaultData { Credentials = Credentials, Settings = Settings, History = History };
            File.WriteAllText(_path, CryptoHelper.Encrypt(JsonSerializer.Serialize(data), _pass));
        }

        public void AddCredential(Credential c) { Credentials.Add(c); Save(); }
        public void UpdateCredential(Credential c, bool saveToHistory = true) { Save(); }
        public void DeleteCredential(Guid id) { Credentials.RemoveAll(x => x.Id == id); Save(); }
        public List<Credential> GetWeakPasswords() => Credentials.Where(c => c.IsWeak).ToList();
        public Dictionary<string, List<Credential>> GetDuplicatePasswords() => new Dictionary<string, List<Credential>>();
        public VaultStatistics GetStatistics() => new VaultStatistics { TotalCredentials = Credentials.Count, WeakPasswords = GetWeakPasswords().Count };
        public List<PasswordHistory> GetPasswordHistory(Guid id) => History.Where(h => h.CredentialId == id).ToList();
    }

    public class VaultData
    {
        public List<Credential> Credentials { get; set; } = new List<Credential>();
        public AppSettings Settings { get; set; } = new AppSettings();
        public List<PasswordHistory> History { get; set; } = new List<PasswordHistory>();
    }
}