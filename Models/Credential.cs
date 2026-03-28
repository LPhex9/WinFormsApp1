using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyPasswordManager.Models
{
    /// <summary>
    /// Enhanced Credential model with all necessary fields for a complete password manager
    /// </summary>
    public class Credential
    {
        // ===== Basic Information =====
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Url { get; set; } = "";
        public string Notes { get; set; } = "";
        
        // ===== Organization =====
        public string Category { get; set; } = "General";
        public List<string> Tags { get; set; } = new List<string>();
        public bool IsFavorite { get; set; } = false;
        
        // ===== Dates =====
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateModified { get; set; } = DateTime.Now;
        public DateTime? ExpirationDate { get; set; } = null;
        public DateTime? LastPasswordChange { get; set; } = DateTime.Now;
        
        // ===== Security Metrics =====
        public PasswordStrength Strength { get; set; } = PasswordStrength.Unknown;
        public int TimesUsed { get; set; } = 0;
        public DateTime? LastAccessed { get; set; } = null;
        public bool IsCompromised { get; set; } = false;
        
        // ===== Custom Fields =====
        public List<CustomField> CustomFields { get; set; } = new List<CustomField>();
        
        // ===== Computed Properties (not serialized) =====
        [JsonIgnore]
        public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.Now;
        
        [JsonIgnore]
        public bool IsExpiringSoon => ExpirationDate.HasValue && 
                                       ExpirationDate.Value < DateTime.Now.AddDays(7) &&
                                       ExpirationDate.Value > DateTime.Now;
        
        [JsonIgnore]
        public bool IsWeak => Strength == PasswordStrength.VeryWeak || 
                              Strength == PasswordStrength.Weak;
        
        [JsonIgnore]
        public int DaysSinceLastChange => LastPasswordChange.HasValue 
            ? (DateTime.Now - LastPasswordChange.Value).Days 
            : 0;
        
        [JsonIgnore]
        public string PasswordPreview => string.IsNullOrEmpty(Password) 
            ? "" 
            : new string('•', Math.Min(Password.Length, 12));
    }
    
    /// <summary>
    /// Custom field for storing additional user-defined data
    /// </summary>
    public class CustomField
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
        public CustomFieldType Type { get; set; } = CustomFieldType.Text;
        public bool IsProtected { get; set; } = false; // Should this field be encrypted/hidden?
    }
    
    /// <summary>
    /// Password strength levels
    /// </summary>
    public enum PasswordStrength
    {
        Unknown,
        VeryWeak,
        Weak,
        Fair,
        Good,
        Strong,
        VeryStrong
    }
    
    /// <summary>
    /// Types of custom fields
    /// </summary>
    public enum CustomFieldType
    {
        Text,
        Password,
        Email,
        Url,
        Phone,
        Date,
        Number,
        MultilineText
    }
}
