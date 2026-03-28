using System;

namespace MyPasswordManager.Models
{
    /// <summary>
    /// Stores historical versions of passwords for a credential
    /// </summary>
    public class PasswordHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CredentialId { get; set; }
        public string OldPassword { get; set; } = "";
        public DateTime ChangedDate { get; set; } = DateTime.Now;
        public string ChangeReason { get; set; } = "Manual update";
        public PasswordStrength Strength { get; set; } = PasswordStrength.Unknown;
    }
    
    /// <summary>
    /// Secure note storage
    /// </summary>
    public class SecureNote
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string Category { get; set; } = "General";
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateModified { get; set; } = DateTime.Now;
        public bool IsFavorite { get; set; } = false;
    }
    
    /// <summary>
    /// Credit card storage
    /// </summary>
    public class CreditCard
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CardholderName { get; set; } = "";
        public string CardNumber { get; set; } = "";
        public string ExpiryMonth { get; set; } = "";
        public string ExpiryYear { get; set; } = "";
        public string CVV { get; set; } = "";
        public string PIN { get; set; } = "";
        public string BankName { get; set; } = "";
        public CardType Type { get; set; } = CardType.Credit;
        public string Notes { get; set; } = "";
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
    
    public enum CardType
    {
        Credit,
        Debit,
        Prepaid
    }
    
    /// <summary>
    /// App settings and configuration
    /// </summary>
    public class AppSettings
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        // Security Settings
        public int AutoLockMinutes { get; set; } = 5;
        public bool AutoLockEnabled { get; set; } = true;
        public bool TwoFactorEnabled { get; set; } = false;
        public string TwoFactorSecret { get; set; } = "";
        
        // Password Settings
        public int DefaultPasswordLength { get; set; } = 16;
        public bool DefaultIncludeUppercase { get; set; } = true;
        public bool DefaultIncludeLowercase { get; set; } = true;
        public bool DefaultIncludeNumbers { get; set; } = true;
        public bool DefaultIncludeSymbols { get; set; } = true;
        public bool ExcludeAmbiguousCharacters { get; set; } = false;
        
        // Expiration Settings
        public int DefaultExpirationDays { get; set; } = 90;
        public bool EnableExpirationWarnings { get; set; } = true;
        public int ExpirationWarningDays { get; set; } = 7;
        
        // UI Settings
        public bool ShowPasswordPreview { get; set; } = false;
        public bool EnableClipboardClear { get; set; } = true;
        public int ClipboardClearSeconds { get; set; } = 30;
        
        // Backup Settings
        public bool AutoBackupEnabled { get; set; } = true;
        public int AutoBackupDays { get; set; } = 7;
        public string BackupPath { get; set; } = "";
    }
}
