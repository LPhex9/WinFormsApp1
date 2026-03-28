using System;
using System.Text.Json.Serialization;

namespace MyPasswordManager.Models
{
    public enum PasswordStrength { Unknown, VeryWeak, Weak, Fair, Good, Strong, VeryStrong }

    public class Credential
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Url { get; set; } = "";
        public string Notes { get; set; } = "";
        public string Category { get; set; } = "General";
        public bool IsFavorite { get; set; } = false;
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateModified { get; set; } = DateTime.Now;
        public DateTime? ExpirationDate { get; set; } = null;
        public DateTime? LastPasswordChange { get; set; } = DateTime.Now;
        public PasswordStrength Strength { get; set; } = PasswordStrength.Unknown;
        public int TimesUsed { get; set; } = 0;

        [JsonIgnore] public bool IsWeak => Strength == PasswordStrength.VeryWeak || Strength == PasswordStrength.Weak;
        [JsonIgnore] public string PasswordPreview => string.IsNullOrEmpty(Password) ? "" : new string('•', Math.Min(Password.Length, 12));
    }
}