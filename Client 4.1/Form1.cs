using System;
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

        private Dictionary<string, ChatForm> _openChats = new Dictionary<string, ChatForm>();
        private Dictionary<string, List<string>> _chatHistories = new Dictionary<string, List<string>>();
        private List<string> _messagedUsers = new List<string>(); // Danh sách đã nhắn tin

        public Form1()
        {
            InitializeComponent();
            txtServerDNS.Text = "huynas123.synology.me";
            txtPort.Text = "8081";

            lstUsers.DoubleClick += lstUsers_DoubleClick;
            lstMessagedUsers.DoubleClick += lstMessagedUsers_DoubleClick;
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

                // Kiểm tra xem tin nhắn có liên quan đến người dùng hiện tại không
                if (toUser == _userName || fromUser == _userName)
                {
                    OpenOrSendToChat(fromUser, content);

                    // Lưu tin nhắn vào lịch sử
                    if (!_chatHistories.ContainsKey(fromUser))
                    {
                        _chatHistories[fromUser] = new List<string>();
                    }
                    _chatHistories[fromUser].Add($"{fromUser}: {content}");
                }
            }
        }

        private void OpenOrSendToChat(string user, string messageContent)
        {
            if (!_openChats.ContainsKey(user))
            {
                Invoke(new Action(() =>
                {
                    ChatForm chatForm = new ChatForm(_clientSocket, _userName, user);
                    _openChats[user] = chatForm;

                    // Hiển thị lịch sử tin nhắn nếu có
                    if (_chatHistories.ContainsKey(user))
                    {
                        foreach (string msg in _chatHistories[user])
                        {
                            chatForm.ReceiveMessage(msg);
                        }
                    }

                    chatForm.FormClosed += (s, e) => _openChats.Remove(user);
                    chatForm.Show();
                }));
            }

            // Gửi tin nhắn tới cửa sổ chat
            if (_openChats.ContainsKey(user))
            {
                _openChats[user].ReceiveMessage($"{user}: {messageContent}");
            }
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

        private void UpdateMessagedUsersList()
        {
            lstMessagedUsers.Items.Clear();
            lstMessagedUsers.Items.AddRange(_messagedUsers.ToArray());
        }

        private void RequestUserList()
        {
            byte[] requestBytes = Encoding.UTF8.GetBytes("GetUserList<EOF>");
            _clientSocket.Send(requestBytes);
        }

        private void lstUsers_DoubleClick(object sender, EventArgs e)
        {
            string selectedUser = lstUsers.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(selectedUser))
            {
                if (!_messagedUsers.Contains(selectedUser))
                {
                    _messagedUsers.Add(selectedUser);
                    UpdateMessagedUsersList();
                }

                AppendStatusMessage($"User {selectedUser} added to Messaged Users.");
            }
        }

        private void lstMessagedUsers_DoubleClick(object sender, EventArgs e)
        {
            string selectedUser = lstMessagedUsers.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(selectedUser))
            {
                OpenOrSendToChat(selectedUser, $"Resuming chat with {selectedUser}");
            }
        }

        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedUser = lstUsers.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedUser))
            {
                AppendStatusMessage($"Selected user: {selectedUser}");
            }
        }

        private void lstMessagedUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedUser = lstMessagedUsers.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedUser))
            {
                AppendStatusMessage($"Selected user from Messaged Users: {selectedUser}");
            }
        }
    }
}
