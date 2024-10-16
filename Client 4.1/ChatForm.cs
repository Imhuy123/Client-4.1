﻿using System;
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

            string message = $"{_currentUser}->{_chatWithUser}:{messageContent}<EOF>";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            try
            {
                if (_clientSocket.Connected) // Kiểm tra kết nối trước khi gửi
                {
                    _clientSocket.Send(messageBytes); // Gửi tin nhắn qua socket
                    txtMessage.Clear();
                    ReceiveMessage($"Me: {messageContent}"); // Hiển thị tin nhắn của chính người dùng
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

        public void ReceiveMessage(string message)
        {
            if (this.IsHandleCreated)
            {
                Invoke(new Action(() =>
                {
                    rtbChatHistory.AppendText($"{message}{Environment.NewLine}"); // Hiển thị tin nhắn
                    rtbChatHistory.SelectionStart = rtbChatHistory.Text.Length;
                    rtbChatHistory.ScrollToCaret(); // Tự động cuộn xuống cuối khi có tin nhắn mới
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

        private void rtbChatHistory_TextChanged(object sender, EventArgs e)
        {
            // Tự động cuộn xuống cuối khi có tin nhắn mới
            rtbChatHistory.SelectionStart = rtbChatHistory.Text.Length;
            rtbChatHistory.ScrollToCaret();
        }
    }
}
