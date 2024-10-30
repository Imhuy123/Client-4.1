using System;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

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

                return Convert.ToBase64String(encryptedBytes, 0, length); // Trả về chuỗi mã hóa dạng Base64
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
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText); // Chuyển chuỗi mã hóa từ Base64 thành mảng byte

                var engine = new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine())); // AES với chế độ CBC và padding PKCS7
                var keyParam = new KeyParameter(Key);
                var parameters = new ParametersWithIV(keyParam, IV);
                engine.Init(false, parameters); // `false` để giải mã

                byte[] decryptedBytes = new byte[engine.GetOutputSize(encryptedBytes.Length)];
                int length = engine.ProcessBytes(encryptedBytes, 0, encryptedBytes.Length, decryptedBytes, 0);
                length += engine.DoFinal(decryptedBytes, length);

                return Encoding.UTF8.GetString(decryptedBytes, 0, length); // Trả về chuỗi giải mã
            }
            catch (FormatException)
            {
                Console.WriteLine("DecryptMessage: Invalid Base64 format");
                throw new FormatException("The encrypted text is not in a valid Base-64 format.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decryption error: {ex.Message}");
                return null;
            }
        }
    }
}
