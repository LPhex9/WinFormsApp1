using System;

namespace MyPasswordManager.Models
{
    public class PasswordHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CredentialId { get; set; }
        public string OldPassword { get; set; } = "";
        public DateTime ChangedDate { get; set; } = DateTime.Now;
        public string ChangeReason { get; set; } = "Manual update";
        public PasswordStrength Strength { get; set; } = PasswordStrength.Unknown;
    }

    public class AppSettings
    {
        // Core Security
        public int AutoLockMinutes { get; set; } = 5;
        public bool AutoLockEnabled { get; set; } = true;
        public bool EnableClipboardClear { get; set; } = true;
        public int ClipboardClearSeconds { get; set; } = 30;

        // Password Generator Defaults
        public int DefaultPasswordLength { get; set; } = 16;
        public bool DefaultIncludeUppercase { get; set; } = true;
        public bool DefaultIncludeLowercase { get; set; } = true;
        public bool DefaultIncludeNumbers { get; set; } = true;
        public bool DefaultIncludeSymbols { get; set; } = true;
        public bool ExcludeAmbiguousCharacters { get; set; } = false;

        // Expiration
        public int DefaultExpirationDays { get; set; } = 90;
        public bool EnableExpirationWarnings { get; set; } = true;
        public int ExpirationWarningDays { get; set; } = 7;
    }

    public class SecureNote
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
    }
}