using System;
using System.Text;
using System.Security.Cryptography;
using MyPasswordManager.Models;

namespace MyPasswordManager.Services
{
    public static class PasswordService
    {
        public static string GeneratePassword(int length, bool useUpper, bool useLower, bool useNum, bool useSym)
        {
            string pool = "";
            if (useUpper) pool += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (useLower) pool += "abcdefghijklmnopqrstuvwxyz";
            if (useNum) pool += "0123456789";
            if (useSym) pool += "!@#$%^&*()";

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(pool[RandomNumberGenerator.GetInt32(pool.Length)]);
            }
            return sb.ToString();
        }

        public static PasswordStrength CalculateStrength(string password)
        {
            if (password.Length < 8) return PasswordStrength.Weak;
            if (password.Length < 12) return PasswordStrength.Fair;
            return PasswordStrength.Strong;
        }
    }
}