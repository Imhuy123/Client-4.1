﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client_4._1
{
    public partial class Form1 : Form
    {
        private Socket _clientSocket;
        private string _userName;
        private bool _isConnected = false;

        // Lưu trữ tất cả các cửa sổ ChatForm đã mở
        private Dictionary<string, ChatForm> _openChats = new Dictionary<string, ChatForm>();

        public Form1()
        {
            InitializeComponent();
            txtServerDNS.Text = "huynas123.synology.me";
            txtPort.Text = "8081";
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            _userName = txtUserName.Text.Trim();
            string serverAddress = txtServerDNS.Text.Trim();
            int port = int.Parse(txtPort.Text.Trim());

            ConnectToServer(serverAddress, port);
        }

        private void ConnectToServer(string serverAddress, int port)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(serverAddress);
                IPAddress ipAddress = hostEntry.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                _clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.Connect(remoteEP);

                byte[] userNameBytes = Encoding.UTF8.GetBytes(_userName + "<EOF>");
                _clientSocket.Send(userNameBytes);

                _isConnected = true;
                AppendStatusMessage("Connected to the server!");

                new Thread(ReceiveMessages).Start();
                RequestUserList();
            }
            catch (Exception ex)
            {
                AppendStatusMessage($"Connection error: {ex.Message}");
            }
        }

        private void ReceiveMessages()
        {
            try
            {
                while (_isConnected)
                {
                    byte[] buffer = new byte[1024];
                    int receivedBytes = _clientSocket.Receive(buffer);
                    string message = Encoding.UTF8.GetString(buffer, 0, receivedBytes).Replace("<EOF>", "");

                    if (message.StartsWith("UserList:"))
                    {
                        UpdateUserList(message.Replace("UserList:", ""));
                    }
                    else
                    {
                        HandleIncomingMessage(message);
                    }
                }
            }
            catch (Exception)
            {
                AppendStatusMessage("Server disconnected.");
                _isConnected = false;
            }
        }

        private void HandleIncomingMessage(string message)
        {
            string[] splitMessage = message.Split(new[] { "->", ":" }, StringSplitOptions.None);
            if (splitMessage.Length == 3)
            {
                string fromUser = splitMessage[0].Trim();
                string toUser = splitMessage[1].Trim();
                string content = splitMessage[2].Trim();

                if (toUser == _userName)  // Tin nhắn dành cho người dùng hiện tại
                {
                    OpenOrSendToChat(fromUser, content);
                }

                // Thông báo trạng thái ai nhắn đến ai trên rtbMessages
                AppendStatusMessage($"Message from {fromUser} to {toUser}");
            }
        }

        private void OpenOrSendToChat(string fromUser, string messageContent)
        {
            if (!_openChats.ContainsKey(fromUser))
            {
                // Mở ChatForm mới nếu chưa tồn tại
                ChatForm chatForm = new ChatForm(_clientSocket, _userName, fromUser);
                _openChats[fromUser] = chatForm;
                chatForm.Show();
            }

            // Gửi tin nhắn vào ChatForm tương ứng
            _openChats[fromUser].ReceiveMessage($"{fromUser}: {messageContent}");
        }

        private void AppendStatusMessage(string status)
        {
            Invoke(new Action(() =>
            {
                rtbMessages.AppendText($"[Status]: {status}{Environment.NewLine}");
            }));
        }

        private void UpdateUserList(string userList)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateUserList(userList)));
                return;
            }

            lstUsers.Items.Clear();
            if (!string.IsNullOrEmpty(userList))
            {
                string[] users = userList.Split(',');
                lstUsers.Items.AddRange(users);
            }
        }

        private void RequestUserList()
        {
            byte[] requestBytes = Encoding.UTF8.GetBytes("GetUserList<EOF>");
            _clientSocket.Send(requestBytes);
        }

        private void lstUsers_DoubleClick(object sender, EventArgs e)
        {
            string chatWithUser = lstUsers.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(chatWithUser))
            {
                OpenOrSendToChat(chatWithUser, $"Started chat with {chatWithUser}");
            }
        }
    }
}
