using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Client_4._1
{
    public partial class ChatForm : Form
    {
        private readonly Socket _clientSocket;
        private readonly string _currentUser;
        private readonly string _chatWithUser;

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
                // Mã hóa tin nhắn
                string encryptedMessage = BouncyCastleEncryptionHelper.Encrypt(messageContent);
                AppendLog($"[SendMessage] Encrypted Message: {encryptedMessage}");

                string message = $"{_currentUser}->{_chatWithUser}:{encryptedMessage}<EOF>";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                if (_clientSocket.Connected)
                {
                    _clientSocket.Send(messageBytes);
                    txtMessage.Clear();

                    // Hiển thị tin nhắn gửi đi trên cửa sổ chat
                    ReceiveMessage($"Me: {messageContent}");
                    AppendLog($"[SendMessage] Message sent successfully to {_chatWithUser}: {messageContent}");
                }
                else
                {
                    MessageBox.Show("You are not connected to the server.");
                    AppendLog("[ERROR] Failed to send message - not connected to the server.");
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"Failed to send message: {ex.Message}");
                AppendLog($"[ERROR] Failed to send message: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
                AppendLog($"[ERROR] An error occurred: {ex.Message}");
            }
        }



        public void ReceiveMessage(string encryptedMessage)
        {
            // Loại bỏ "<EOF>" nếu có trong tin nhắn
            string messageContent = encryptedMessage.Replace("<EOF>", string.Empty);
            AppendLog($"[ReceiveMessage] Raw Received = {encryptedMessage}");
            AppendLog($"[ReceiveMessage] Without <EOF> = {messageContent}");

            // Kiểm tra nếu tin nhắn là dạng mã hóa (không cần kiểm tra Base64 ở bước này)
            AppendLog("[INFO] Displaying encrypted message directly.");
            DisplayMessage($"Encrypted Message: {messageContent}");
        }



        // Hàm hiển thị tin nhắn ra cửa sổ chat
        private void DisplayMessage(string message)
        {
            if (this.IsHandleCreated)
            {
                Invoke(new Action(() =>
                {
                    rtbChatHistory.AppendText($"{message}{Environment.NewLine}");
                    rtbChatHistory.SelectionStart = rtbChatHistory.Text.Length;
                    rtbChatHistory.ScrollToCaret();
                }));
            }
        }



        private bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }





        public void LoadChatHistory(List<string> chatHistory)
        {
            if (this.IsHandleCreated)
            {
                Invoke(new Action(() =>
                {
                    foreach (var msg in chatHistory)
                    {
                        try
                        {
                            // Giải mã từng tin nhắn trong lịch sử
                            string decryptedMsg = BouncyCastleEncryptionHelper.Decrypt(msg);
                            rtbChatHistory.AppendText($"{decryptedMsg}{Environment.NewLine}");
                            AppendLog($"[LoadChatHistory] Decrypted message from history: {decryptedMsg}");
                        }
                        catch (Exception ex)
                        {
                            AppendLog($"[ERROR] Failed to decrypt a message in chat history: {ex.Message}");
                        }
                    }
                    rtbChatHistory.SelectionStart = rtbChatHistory.Text.Length;
                    rtbChatHistory.ScrollToCaret();
                    AppendLog("[LoadChatHistory] Chat history loaded successfully.");
                }));
            }
        }



        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Đóng form chat
            // Xử lý các tài nguyên khi form bị đóng, nếu cần
        }

        private void rtbChatHistory_TextChanged(object sender, EventArgs e)
        {
            rtbChatHistory.SelectionStart = rtbChatHistory.Text.Length;
            rtbChatHistory.ScrollToCaret();
        }
        private void AppendLog(string logMessage)
        {
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action(() => rtbLog.AppendText($"{logMessage}{Environment.NewLine}")));
            }
            else
            {
                rtbLog.AppendText($"{logMessage}{Environment.NewLine}");
            }
            rtbLog.SelectionStart = rtbLog.Text.Length;
            rtbLog.ScrollToCaret();
        }

    }
}
