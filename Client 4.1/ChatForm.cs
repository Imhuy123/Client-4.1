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
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string messageContent = txtMessage.Text.Trim();

            if (string.IsNullOrEmpty(messageContent))
            {
                MessageBox.Show("Please enter a message.");
                return;
            }

            string message = $"{_chatWithUser}:{messageContent}<EOF>";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            _clientSocket.Send(messageBytes);

            txtMessage.Clear();
            rtbChatHistory.AppendText($"Me: {messageContent}{Environment.NewLine}");
        }

        public void ReceiveMessage(string message)
        {
            Invoke(new Action(() =>
            {
                rtbChatHistory.AppendText($"{_chatWithUser}: {message}{Environment.NewLine}");
            }));
        }
    }
}
