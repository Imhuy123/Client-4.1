using System;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System.Windows.Forms;

namespace Client_4._1
{
    public static class BouncyCastleEncryptionHelper
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32 bytes cho AES-256
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890123456"); // 16 bytes cho AES

        // Mã hóa
        public static string Encrypt(string plainText)
        {
            try
            {
                var engine = new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine())); // AES với chế độ CBC và padding PKCS7
                var keyParam = new KeyParameter(Key);
                var parameters = new ParametersWithIV(keyParam, IV);
                engine.Init(true, parameters); // `true` để mã hóa

                byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = new byte[engine.GetOutputSize(inputBytes.Length)];
                int length = engine.ProcessBytes(inputBytes, 0, inputBytes.Length, encryptedBytes, 0);
                length += engine.DoFinal(encryptedBytes, length);

                // Chuyển đổi sang chuỗi Base64 chính xác theo độ dài mã hóa
                return Convert.ToBase64String(encryptedBytes, 0, length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Encryption error: {ex.Message}");
                return null;
            }
        }

        // Giải mã
        public static string Decrypt(string encryptedText)
        {
            try
            {
                // Chuyển đổi chuỗi Base64 thành mảng byte
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

                // Thiết lập AES với chế độ CBC và padding PKCS7
                var engine = new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine()));
                var keyParam = new KeyParameter(Key);
                var parameters = new ParametersWithIV(keyParam, IV);
                engine.Init(false, parameters); // `false` để giải mã

                // Giải mã dữ liệu 
                byte[] decryptedBytes = new byte[engine.GetOutputSize(encryptedBytes.Length)];
                int length = engine.ProcessBytes(encryptedBytes, 0, encryptedBytes.Length, decryptedBytes, 0);
                length += engine.DoFinal(decryptedBytes, length);

                // Xử lý độ dài để loại bỏ dữ liệu dư thừa
                return Encoding.UTF8.GetString(decryptedBytes, 0, length);
            }
            catch (FormatException ex)
            {
                
                return null;
            }
            catch (InvalidCipherTextException ex)
            {
                MessageBox.Show("Decryption failed: Invalid ciphertext, possibly due to incorrect key or IV.", "Decryption Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Decryption failed: {ex.Message}", "Decryption Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
