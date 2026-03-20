using System;
using System.IO;
using System.Security.Cryptography;

namespace MyPasswordManager
{
    public static class CryptoHelper
    {
        // Security Constants
        private const int SaltSize = 16; // 128-bit salt
        private const int KeySize = 32;  // 256-bit key for AES-256
        private const int IvSize = 16;   // AES IV is always 16 bytes
        private const int Iterations = 600000; // OWASP recommended standard

        /// <summary>
        /// Encrypts plain text into a secure Base64 string containing the Salt, IV, and CipherText.
        /// </summary>
        public static string Encrypt(string plainText, string masterPassword)
        {
            // 1. Generate a random Salt
            byte[] salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            // 2. Derive the Key using PBKDF2 (The "Stretching" phase)
            using var pbkdf2 = new Rfc2898DeriveBytes(masterPassword, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] key = pbkdf2.GetBytes(KeySize);

            // 3. Setup AES-256
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV(); // Creates a random IV
            byte[] iv = aes.IV;

            // 4. Encrypt the actual data
            using MemoryStream msEncrypt = new MemoryStream();
            using CryptoStream csEncrypt = new CryptoStream(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using StreamWriter swEncrypt = new StreamWriter(csEncrypt);

            swEncrypt.Write(plainText);
            swEncrypt.Close();

            byte[] cipherText = msEncrypt.ToArray();

            // 5. The "Package Deal" - Combine Salt + IV + CipherText into one array
            byte[] result = new byte[SaltSize + IvSize + cipherText.Length];
            Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
            Buffer.BlockCopy(iv, 0, result, SaltSize, IvSize);
            Buffer.BlockCopy(cipherText, 0, result, SaltSize + IvSize, cipherText.Length);

            // Return it as a safe string that can be easily saved to a text file
            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Decrypts the packaged Base64 string back into plain text.
        /// </summary>
        public static string Decrypt(string encryptedPackage, string masterPassword)
        {
            // Convert the string back into raw bytes
            byte[] fullCipher = Convert.FromBase64String(encryptedPackage);

            // 1. Slice the package back into its 3 parts (Salt, IV, CipherText)
            byte[] salt = new byte[SaltSize];
            byte[] iv = new byte[IvSize];
            byte[] cipherText = new byte[fullCipher.Length - SaltSize - IvSize];

            Buffer.BlockCopy(fullCipher, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(fullCipher, SaltSize, iv, 0, IvSize);
            Buffer.BlockCopy(fullCipher, SaltSize + IvSize, cipherText, 0, cipherText.Length);

            // 2. Derive the exact same Key using the extracted Salt
            using var pbkdf2 = new Rfc2898DeriveBytes(masterPassword, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] key = pbkdf2.GetBytes(KeySize);

            // 3. Decrypt the data using the Key and extracted IV
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using MemoryStream msDecrypt = new MemoryStream(cipherText);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd(); // Returns the original plain text!
        }
    }
}