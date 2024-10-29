using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Client_4._1
{
    public partial class ChatForm : Form
    {
        private readonly Socket _clientSocket;
        private readonly string _currentUser;
        private readonly string _chatWithUser;

        // Khóa 32 bytes cho AES-256
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32 ký tự

        // IV 16 bytes cho AES
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890123456"); // 16 ký tự

        public ChatForm(Socket clientSocket, string currentUser, string chatWithUser)
        {
            InitializeComponent();
            _clientSocket = clientSocket;
            _currentUser = currentUser;
            _chatWithUser = chatWithUser;

            this.Text = $"Chat with {_chatWithUser}";
            txtMessage.KeyDown += TxtMessage_KeyDown;
            this.FormClosed += ChatForm_FormClosed;
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SendMessage();
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            string messageContent = txtMessage.Text.Trim();

            if (string.IsNullOrEmpty(messageContent))
            {
                MessageBox.Show("Please enter a message.");
                return;
            }

            try
            {
                string encryptedMessage = EncryptMessage(messageContent);
                string message = $"{_currentUser}->{_chatWithUser}:{encryptedMessage}<EOF>";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                if (_clientSocket.Connected)
                {
                    _clientSocket.Send(messageBytes);
                    txtMessage.Clear();
                    ReceiveMessage($"Me: {messageContent}");
                }
                else
                {
                    MessageBox.Show("You are not connected to the server.");
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"Failed to send message: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        public void ReceiveMessage(string encryptedMessage)
        {
            // Loại bỏ phần "<EOF>"
            string messageContent = encryptedMessage.Replace("<EOF>", string.Empty);

            // In ra chuỗi đã xử lý
            Console.WriteLine($"Message content for decryption: {messageContent}");

            try
            {
                // Gọi hàm giải mã
                string decryptedMessage = DecryptMessage(messageContent);
                if (this.IsHandleCreated && decryptedMessage != null)
                {
                    Invoke(new Action(() =>
                    {
                        rtbChatHistory.AppendText($"{decryptedMessage}{Environment.NewLine}");
                        rtbChatHistory.SelectionStart = rtbChatHistory.Text.Length;
                        rtbChatHistory.ScrollToCaret();
                    }));
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("The encrypted text is not in a valid Base-64 format.");
            }
        }





        public void LoadChatHistory(List<string> chatHistory)
        {
            if (this.IsHandleCreated)
            {
                Invoke(new Action(() =>
                {
                    foreach (var msg in chatHistory)
                    {
                        string decryptedMsg = DecryptMessage(msg);
                        rtbChatHistory.AppendText($"{decryptedMsg}{Environment.NewLine}");
                    }
                    rtbChatHistory.SelectionStart = rtbChatHistory.Text.Length;
                    rtbChatHistory.ScrollToCaret();
                }));
            }
        }

        private string EncryptMessage(string plainText)
        {
            var engine = new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine())); // Sử dụng chế độ CBC
            var keyParam = new KeyParameter(Key);
            var parameters = new ParametersWithIV(keyParam, IV);
            engine.Init(true, parameters); // true cho mã hóa

            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = new byte[engine.GetOutputSize(inputBytes.Length)];
            int length = engine.ProcessBytes(inputBytes, 0, inputBytes.Length, encryptedBytes, 0);
            length += engine.DoFinal(encryptedBytes, length);

            return Convert.ToBase64String(encryptedBytes, 0, length);
        }

        private string DecryptMessage(string encryptedText)
        {
            try
            {
                // Kiểm tra nếu chuỗi không phải là Base64 hợp lệ
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

                var engine = new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine())); // Sử dụng chế độ CBC
                var keyParam = new KeyParameter(Key);
                var parameters = new ParametersWithIV(keyParam, IV);
                engine.Init(false, parameters); // false cho giải mã

                byte[] decryptedBytes = new byte[engine.GetOutputSize(encryptedBytes.Length)];
                int length = engine.ProcessBytes(encryptedBytes, 0, encryptedBytes.Length, decryptedBytes, 0);
                length += engine.DoFinal(decryptedBytes, length);

                return Encoding.UTF8.GetString(decryptedBytes, 0, length);
            }
            catch (FormatException)
            {
                MessageBox.Show("The encrypted text is not in a valid Base-64 format.");
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during decryption: {ex.Message}");
                return null;
            }
        }


        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Đóng form chat
        }

        private void rtbChatHistory_TextChanged(object sender, EventArgs e)
        {
            rtbChatHistory.SelectionStart = rtbChatHistory.Text.Length;
            rtbChatHistory.ScrollToCaret();
        }
    }
}
