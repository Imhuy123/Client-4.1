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

            string message = $"{_currentUser}->{_chatWithUser}:{messageContent}<EOF>";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            try
            {
                _clientSocket.Send(messageBytes);
                txtMessage.Clear();
                ReceiveMessage($"Me: {messageContent}");
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"Failed to send message: {ex.Message}");
            }
        }

        public void ReceiveMessage(string message)
        {
            // Kiểm tra xem form đã có handle chưa, nếu chưa thì chờ đến khi form sẵn sàng
            if (this.IsHandleCreated)
            {
                Invoke(new Action(() =>
                {
                    rtbChatHistory.AppendText($"{message}{Environment.NewLine}");
                }));
            }
            else
            {
                // Đảm bảo rằng hàm này sẽ chạy lại sau khi form đã được khởi tạo
                this.HandleCreated += (s, e) =>
                {
                    Invoke(new Action(() =>
                    {
                        rtbChatHistory.AppendText($"{message}{Environment.NewLine}");
                    }));
                };
            }
        }

        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Không cần làm gì đặc biệt, cửa sổ chat sẽ được loại bỏ khỏi danh sách trong Form1
        }

        private void rtbChatHistory_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
