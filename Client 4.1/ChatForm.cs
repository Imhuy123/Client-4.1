using System;
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
            this.FormClosed += ChatForm_FormClosed; // Đảm bảo form được xóa khỏi danh sách khi đóng
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Ngăn không cho tiếng bíp khi nhấn Enter
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

            // Mã hóa tin nhắn
            string encryptedMessage = BouncyCastleEncryptionHelper.Encrypt(messageContent);
            if (encryptedMessage == null)
            {
                MessageBox.Show("Encryption failed.");
                return;
            }

            // Đóng gói tin nhắn với định dạng mã hóa và gửi lên server
            string message = $"{_currentUser}->{_chatWithUser}:{encryptedMessage}<EOF>";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            try
            {
                if (_clientSocket.Connected)
                {
                    _clientSocket.Send(messageBytes); // Gửi tin nhắn mã hóa qua socket
                    txtMessage.Clear();
                    ReceiveMessage($"Me : {encryptedMessage}"); // Hiển thị tin nhắn mã hóa của người gửi
                }
                else
                {
                    MessageBox.Show("You are not connected to the server.");
                }
            }
            catch (SocketException ex)
            {

            }
            catch (Exception ex)
            {

            }
        }

        public void ReceiveMessage(string message)
        {
            // Tách tin nhắn với dấu ":" để lấy từ người gửi và nội dung
            var splitMessage = message.Split(new[] { ":" }, 2, StringSplitOptions.None);
            if (splitMessage.Length == 2)
            {
                string fromUser = splitMessage[0].Trim();
                string content = splitMessage[1].Trim().Replace("<EOF>", "");

                try
                {
                    // Thử giải mã nội dung
                    string decryptedMessage = BouncyCastleEncryptionHelper.Decrypt(content);
                    if (decryptedMessage != null)
                    {
                        // Nếu giải mã thành công, hiển thị tin nhắn đã giải mã
                        if (this.IsHandleCreated)
                        {
                            Invoke(new Action(() =>
                            {
                                rtbChatHistory.AppendText($"{fromUser}: {decryptedMessage}{Environment.NewLine}");
                                rtbChatHistory.SelectionStart = rtbChatHistory.Text.Length;
                                rtbChatHistory.ScrollToCaret();
                            }));
                        }
                    }
                }
                catch
                {
                    // Bỏ qua mọi lỗi giải mã mà không hiển thị bất kỳ thông báo nào
                }
            }
        }








        // Hàm để append thông báo lỗi định dạng vào RichTextBox
        private void AppendFormatErrorMessage(string message)
        {
            if (this.IsHandleCreated)
            {
                Invoke(new Action(() =>
                {
                    rtbChatHistory.AppendText($"[Format Error]: Received message in unexpected format. Content: {message}{Environment.NewLine}");
                    rtbChatHistory.SelectionStart = rtbChatHistory.Text.Length;
                    rtbChatHistory.ScrollToCaret();
                }));
            }
        }



        // Phương thức để tải lịch sử chat
        public void LoadChatHistory(List<string> chatHistory)
        {
            if (this.IsHandleCreated)
            {
                Invoke(new Action(() =>
                {
                    foreach (var msg in chatHistory)
                    {
                        rtbChatHistory.AppendText($"{msg}{Environment.NewLine}");
                    }
                    rtbChatHistory.SelectionStart = rtbChatHistory.Text.Length;
                    rtbChatHistory.ScrollToCaret(); // Tự động cuộn xuống cuối khi có tin nhắn mới
                }));
            }
        }

        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Cửa sổ chat được loại bỏ khỏi danh sách trong Form1 khi đóng
        }

        private void viewCall_Click(object sender, EventArgs e)
        {

        }
    }
}