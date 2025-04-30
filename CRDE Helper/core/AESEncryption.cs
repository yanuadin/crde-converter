using CRDEConverterJsonExcel.config;
using CRDEConverterJsonExcel.controller;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CRDEConverterJsonExcel.core
{
    class AESEncryption
    {
        public static string Encrypt(string plainText)
        {
            AdminController adminController = new AdminController();
            byte[] Salt = Encoding.UTF8.GetBytes(adminController.getSIV());
            string key = adminController.getSKEY();

            try
            {
                if (string.IsNullOrEmpty(plainText))
                    return string.Empty;

                using (Aes aes = Aes.Create())
                {
                    // Derive key using PBKDF2
                    var keyDerivation = new Rfc2898DeriveBytes(
                        key,
                        Salt,
                        100000,
                        HashAlgorithmName.SHA512);

                    aes.Key = keyDerivation.GetBytes(32); // 256-bit key
                    aes.IV = keyDerivation.GetBytes(16);  // 128-bit IV

                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(
                            memoryStream,
                            aes.CreateEncryptor(),
                            CryptoStreamMode.Write))
                        {
                            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                            cryptoStream.FlushFinalBlock();
                        }
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Encryption failed: {ex.Message}");
                return string.Empty;
            }
        }

        public static string Decrypt(string cipherText)
        {
            AdminController adminController = new AdminController();
            byte[] Salt = Encoding.UTF8.GetBytes(adminController.getSIV());
            string key = adminController.getSKEY();

            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            try
            {
                // Verify Base64 format
                byte[] cipherBytes;
                try
                {
                    cipherBytes = Convert.FromBase64String(cipherText);
                }
                catch (FormatException)
                {
                    MessageBox.Show("The input is not a valid Base64 string.");
                    return string.Empty;
                }

                using (Aes aes = Aes.Create())
                {
                    var keyDerivation = new Rfc2898DeriveBytes(
                        key,
                        Salt,
                        100000,
                        HashAlgorithmName.SHA512);

                    aes.Key = keyDerivation.GetBytes(32);
                    aes.IV = keyDerivation.GetBytes(16);
                    aes.Padding = PaddingMode.PKCS7; // Explicitly set padding
                    aes.Mode = CipherMode.CBC; // Explicitly set mode

                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(
                            memoryStream,
                            aes.CreateDecryptor(),
                            CryptoStreamMode.Write))
                        {
                            try
                            {
                                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);
                                cryptoStream.FlushFinalBlock();
                            }
                            catch (CryptographicException ex)
                            {
                                MessageBox.Show($"Decryption failed: {ex.Message}\n" +
                                             "Possible causes:\n" +
                                             "- Wrong password\n" +
                                             "- Corrupted data\n" +
                                             "- Incorrect encryption parameters");
                                return string.Empty;
                            }
                        }
                        return Encoding.UTF8.GetString(memoryStream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical error during decryption: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
