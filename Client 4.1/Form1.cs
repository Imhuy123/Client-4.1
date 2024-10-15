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

                if (toUser == _userName)
                {
                    OpenOrSendToChat(fromUser, content);
                }

                AppendStatusMessage($"Message from {fromUser} to {toUser}");
            }
        }

        private void OpenOrSendToChat(string fromUser, string messageContent)
        {
            if (!_openChats.ContainsKey(fromUser))
            {
                Invoke(new Action(() =>
                {
                    ChatForm chatForm = new ChatForm(_clientSocket, _userName, fromUser);
                    _openChats[fromUser] = chatForm;

                    if (_chatHistories.ContainsKey(fromUser))
                    {
                        foreach (string msg in _chatHistories[fromUser])
                        {
                            chatForm.ReceiveMessage(msg);
                        }
                    }

                    chatForm.FormClosed += (s, e) => _openChats.Remove(fromUser);
                    chatForm.Show();
                }));
            }

            if (!_chatHistories.ContainsKey(fromUser))
            {
                _chatHistories[fromUser] = new List<string>();
            }
            _chatHistories[fromUser].Add($"{fromUser}: {messageContent}");

            if (_openChats.ContainsKey(fromUser))
            {
                _openChats[fromUser].ReceiveMessage($"{fromUser}: {messageContent}");
            }

            // Thêm người dùng vào danh sách đã nhắn tin nếu chưa có
            if (!_messagedUsers.Contains(fromUser))
            {
                _messagedUsers.Add(fromUser);
                UpdateMessagedUsersList();
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
            string chatWithUser = lstUsers.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(chatWithUser))
            {
                OpenOrSendToChat(chatWithUser, $"Started chat with {chatWithUser}");
            }
        }

        private void lstMessagedUsers_DoubleClick(object sender, EventArgs e)
        {
            string chatWithUser = lstMessagedUsers.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(chatWithUser))
            {
                OpenOrSendToChat(chatWithUser, $"Resuming chat with {chatWithUser}");
            }
        }

        private void lstMessagedUsers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
