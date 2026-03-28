using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MyPasswordManager.Models;

namespace MyPasswordManager.Services
{
    /// <summary>
    /// Service for password management operations
    /// </summary>
    public class PasswordService
    {
        /// <summary>
        /// Calculates password strength based on multiple criteria
        /// </summary>
        public static PasswordStrength CalculateStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
                return PasswordStrength.Unknown;

            int score = 0;
            
            // Length scoring
            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            if (password.Length >= 16) score++;
            if (password.Length >= 20) score++;
            
            // Character variety scoring
            if (Regex.IsMatch(password, @"[a-z]")) score++; // Has lowercase
            if (Regex.IsMatch(password, @"[A-Z]")) score++; // Has uppercase
            if (Regex.IsMatch(password, @"\d")) score++;     // Has numbers
            if (Regex.IsMatch(password, @"[^a-zA-Z0-9]")) score++; // Has symbols
            
            // Pattern penalties
            if (Regex.IsMatch(password, @"(.)\1{2,}")) score--; // Repeated characters
            if (Regex.IsMatch(password, @"(012|123|234|345|456|567|678|789|890)")) score--; // Sequential numbers
            if (Regex.IsMatch(password, @"(abc|bcd|cde|def|efg|fgh|ghi|hij|ijk|jkl|klm|lmn|mno|nop|opq|pqr|qrs|rst|stu|tuv|uvw|vwx|wxy|xyz)")) score--; // Sequential letters
            
            // Common password check (basic)
            string[] commonPasswords = { "password", "123456", "qwerty", "admin", "welcome", "letmein" };
            if (commonPasswords.Any(p => password.ToLower().Contains(p)))
                score -= 3;
            
            // Map score to strength
            if (score <= 2) return PasswordStrength.VeryWeak;
            if (score <= 4) return PasswordStrength.Weak;
            if (score <= 6) return PasswordStrength.Fair;
            if (score <= 8) return PasswordStrength.Good;
            if (score <= 10) return PasswordStrength.Strong;
            return PasswordStrength.VeryStrong;
        }
        
        /// <summary>
        /// Calculates password entropy in bits
        /// </summary>
        public static double CalculateEntropy(string password)
        {
            if (string.IsNullOrEmpty(password))
                return 0;

            int charset = 0;
            
            if (Regex.IsMatch(password, @"[a-z]")) charset += 26;
            if (Regex.IsMatch(password, @"[A-Z]")) charset += 26;
            if (Regex.IsMatch(password, @"\d")) charset += 10;
            if (Regex.IsMatch(password, @"[^a-zA-Z0-9]")) charset += 32;
            
            if (charset == 0) return 0;
            
            return Math.Log2(Math.Pow(charset, password.Length));
        }
        
        /// <summary>
        /// Estimates time to crack password
        /// </summary>
        public static string EstimateCrackTime(string password)
        {
            double entropy = CalculateEntropy(password);
            const double guessesPerSecond = 1e9; // 1 billion guesses/sec (conservative estimate)
            
            double totalGuesses = Math.Pow(2, entropy);
            double seconds = totalGuesses / guessesPerSecond / 2; // Average case
            
            const int minute = 60;
            const int hour = minute * 60;
            const int day = hour * 24;
            const int year = day * 365;
            const int century = year * 100;
            
            if (seconds < 1) return "Instant";
            if (seconds < minute) return $"{Math.Round(seconds)} secunde";
            if (seconds < hour) return $"{Math.Round(seconds / minute)} minute";
            if (seconds < day) return $"{Math.Round(seconds / hour)} ore";
            if (seconds < year) return $"{Math.Round(seconds / day)} zile";
            if (seconds < century) return $"{Math.Round(seconds / year)} ani";
            return "Secole";
        }
        
        /// <summary>
        /// Finds duplicate passwords in a list
        /// </summary>
        public static Dictionary<string, List<Credential>> FindDuplicates(IEnumerable<Credential> credentials)
        {
            var passwordGroups = credentials
                .Where(c => !string.IsNullOrEmpty(c.Password))
                .GroupBy(c => ComputePasswordHash(c.Password))
                .Where(g => g.Count() > 1)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            return passwordGroups;
        }
        
        /// <summary>
        /// Checks if a password is reused
        /// </summary>
        public static bool IsPasswordReused(string password, IEnumerable<Credential> allCredentials, Guid? excludeId = null)
        {
            string passwordHash = ComputePasswordHash(password);
            
            return allCredentials
                .Where(c => !excludeId.HasValue || c.Id != excludeId.Value)
                .Any(c => ComputePasswordHash(c.Password) == passwordHash);
        }
        
        /// <summary>
        /// Generates a secure random password
        /// </summary>
        public static string GeneratePassword(
            int length = 16,
            bool includeUppercase = true,
            bool includeLowercase = true,
            bool includeNumbers = true,
            bool includeSymbols = true,
            bool excludeAmbiguous = false)
        {
            string charset = "";
            
            if (includeLowercase)
                charset += excludeAmbiguous ? "abcdefghijkmnpqrstuvwxyz" : "abcdefghijklmnopqrstuvwxyz";
            
            if (includeUppercase)
                charset += excludeAmbiguous ? "ABCDEFGHJKLMNPQRSTUVWXYZ" : "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            
            if (includeNumbers)
                charset += excludeAmbiguous ? "23456789" : "0123456789";
            
            if (includeSymbols)
                charset += "!@#$%^&*()_+-=[]{}|;:,.<>?";
            
            if (string.IsNullOrEmpty(charset))
                throw new ArgumentException("At least one character type must be selected");
            
            var password = new StringBuilder(length);
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);
                
                foreach (byte b in randomBytes)
                {
                    password.Append(charset[b % charset.Length]);
                }
            }
            
            return password.ToString();
        }
        
        /// <summary>
        /// Generates a passphrase from random words
        /// </summary>
        public static string GeneratePassphrase(int wordCount = 4, string separator = "-", bool capitalize = false)
        {
            // Word list (in production, load from file)
            string[] wordList = {
                "albastru", "carte", "castel", "deal", "elefant", "floare", "ginere",
                "hotel", "insula", "jucarie", "luna", "masina", "nor", "ocean",
                "peste", "roata", "soare", "tigru", "vapor", "zebra", "abandon",
                "binary", "cascade", "diamond", "eclipse", "falcon", "glacier",
                "horizon", "impulse", "jungle", "knight", "legacy", "matrix",
                "nebula", "orbit", "phoenix", "quantum", "raven", "summit",
                "thunder", "universe", "vector", "whisper", "zenith"
            };
            
            var words = new List<string>();
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[wordCount];
                rng.GetBytes(randomBytes);
                
                foreach (byte b in randomBytes)
                {
                    string word = wordList[b % wordList.Length];
                    if (capitalize)
                        word = char.ToUpper(word[0]) + word.Substring(1);
                    words.Add(word);
                }
            }
            
            return string.Join(separator, words);
        }
        
        /// <summary>
        /// Computes a hash of the password for comparison (not for storage!)
        /// </summary>
        private static string ComputePasswordHash(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;
            
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
