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
        private Socket _videoSocket; // Socket riêng dành cho video call
        private const int VideoPort = 8082; // Cổng riêng cho kết nối video
        private int _udpSourcePort; // Thuộc tính lưu cổng nguồn UDP

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
            int port;

            if (!int.TryParse(txtPort.Text.Trim(), out port))
            {
                AppendStatusMessage("Invalid port number.");
                return;
            }

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

                // Khởi tạo kết nối UDP sau khi kết nối TCP thành công
                ConnectToUdpServer(serverAddress);

                new Thread(ReceiveMessages).Start();
                RequestUserList();
            }
            catch (SocketException ex)
            {
                AppendStatusMessage($"Connection error: {ex.Message}");
                _isConnected = false;
            }
            catch (Exception ex)
            {
                AppendStatusMessage($"Error: {ex.Message}");
                _isConnected = false;
            }
        }

        private void ConnectToUdpServer(string serverAddress)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(serverAddress);
                IPAddress ipAddress = hostEntry.AddressList[0];
                IPEndPoint udpEndPoint = new IPEndPoint(ipAddress, 8082);

                _videoSocket = new Socket(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                _videoSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
                _videoSocket.Connect(udpEndPoint);

                _udpSourcePort = ((IPEndPoint)_videoSocket.LocalEndPoint).Port;
                AppendStatusMessage($"Connected to UDP server on port {_udpSourcePort}");

                SendUdpRegisterMessage();

                SendStunRequest(); // Gửi yêu cầu STUN sau khi kết nối UDP thành công
            }
            catch (SocketException ex)
            {
                AppendStatusMessage($"UDP connection error: {ex.Message}");
            }
            catch (Exception ex)
            {
                AppendStatusMessage($"Error: {ex.Message}");
            }
        }


        private void SendUdpRegisterMessage()
        {
            try
            {
                string registerMessage = $"REGISTER_UDP:{_userName}<EOF>";
                byte[] registerBytes = Encoding.UTF8.GetBytes(registerMessage);
                _videoSocket.Send(registerBytes); // Gửi thông điệp đăng ký đến server qua UDP

                // Nhận phản hồi từ server sau khi đăng ký thành công
                byte[] responseBuffer = new byte[1024];
                int receivedBytes = _videoSocket.Receive(responseBuffer);
                string responseMessage = Encoding.UTF8.GetString(responseBuffer, 0, receivedBytes);

                if (responseMessage.StartsWith("REGISTER_SUCCESS"))
                {
                    AppendStatusMessage("UDP registration successful with server.");
                }
                else
                {
                    AppendStatusMessage("UDP registration failed.");
                }
            }
            catch (SocketException ex)
            {
                AppendStatusMessage($"Failed to send UDP registration message: {ex.Message}");
            }
            catch (Exception ex)
            {
                AppendStatusMessage($"Error: {ex.Message}");
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
                    else if (message.StartsWith("CALL_ACCEPT:"))
                    {
                        string[] parts = message.Split(':');
                        if (parts.Length >= 2)
                        {
                            string callee = parts[1];
                            AppendStatusMessage($"{callee} đã chấp nhận cuộc gọi video.");

                            Invoke(new Action(() =>
                            {
                                VideoForm videoForm = new VideoForm(_clientSocket.RemoteEndPoint.ToString().Split(':')[0], VideoPort);
                                videoForm.Show();
                            }));
                        }
                    }
                    else if (message.StartsWith("CALL_REJECT:"))
                    {
                        string[] parts = message.Split(':');
                        if (parts.Length >= 2)
                        {
                            string callee = parts[1];
                            AppendStatusMessage($"{callee} đã từ chối cuộc gọi video.");
                        }
                    }
                    else
                    {
                        HandleIncomingMessage(message);
                    }
                }
            }
            catch (SocketException ex)
            {
                AppendStatusMessage($"Server disconnected: {ex.Message}");
                _isConnected = false;
            }
            catch (Exception ex)
            {
                AppendStatusMessage($"Error receiving message: {ex.Message}");
                _isConnected = false;
            }
        }

        private void HandleIncomingMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                AppendStatusMessage("Received an empty message.");
                return;
            }

            Console.WriteLine($"[Received Raw Message]: {message}");

            string[] splitMessage = message.Split(new[] { "->", ":" }, StringSplitOptions.None);
            if (splitMessage.Length == 3)
            {
                string fromUser = splitMessage[0].Trim();
                string toUser = splitMessage[1].Trim();
                string content = splitMessage[2].Trim();

                if (toUser == _userName || fromUser == _userName)
                {
                    if (fromUser != _userName)
                    {
                        if (_openChats.ContainsKey(fromUser))
                        {
                            _openChats[fromUser].ReceiveMessage($"{fromUser}: {content}");
                        }
                        else
                        {
                            OpenOrSendToChat(fromUser, content);
                        }

                        if (!_chatHistories.ContainsKey(fromUser))
                        {
                            _chatHistories[fromUser] = new List<string>();
                        }
                        _chatHistories[fromUser].Add($"{fromUser}: {content}");
                    }
                }
            }
            else
            {
                string errorMsg = $"Received message in unexpected format: {message}";
                AppendStatusMessage(errorMsg);
                MessageBox.Show(errorMsg, "Format Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenOrSendToChat(string user, string messageContent)
        {
            if (!_openChats.ContainsKey(user))
            {
                Invoke(new Action(() =>
                {
                    ChatForm chatForm = new ChatForm(_clientSocket, _userName, user, _videoSocket, _udpSourcePort + 1, _publicIP, _publicPort);

                    _openChats[user] = chatForm;

                    chatForm.FormClosed += (s, e) => _openChats.Remove(user);
                    chatForm.Show();
                }));
            }

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
                if (!_openChats.ContainsKey(selectedUser))
                {
                    OpenOrSendToChat(selectedUser, $"{selectedUser}");
                }
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

        private void lstUsers_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // Hàm này hiện tại không sử dụng
        }

        private void RequestChatHistory(string withUser)
        {
            string request = $"GetChatHistory:{withUser}<EOF>";
            _clientSocket.Send(Encoding.UTF8.GetBytes(request));
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }
        private string _publicIP;
        private int _publicPort;


        private void SendStunRequest()
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry("huynas123.synology.me"); // Địa chỉ STUN server của bạn
                IPAddress ipAddress = hostEntry.AddressList[0];
                IPEndPoint stunServerEndPoint = new IPEndPoint(ipAddress, 8082);

                using (var udpClient = new UdpClient(0))
                {
                    byte[] stunBindingRequest = Encoding.UTF8.GetBytes("STUN_BINDING_REQUEST<EOF>");
                    udpClient.Send(stunBindingRequest, stunBindingRequest.Length, stunServerEndPoint);
                    MessageBox.Show("Sent STUN Binding Request to server.", "STUN Process", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Nhận phản hồi từ server STUN
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] responseBytes = udpClient.Receive(ref remoteEndPoint);

                    // Xử lý phản hồi từ STUN
                    ParseStunResponse(responseBytes);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending STUN request: " + ex.Message, "STUN Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ParseStunResponse(byte[] response)
        {
            string responseMessage = Encoding.UTF8.GetString(response);

            if (responseMessage.StartsWith("STUN_BINDING_RESPONSE"))
            {
                // Parse IP và Port từ thông điệp phản hồi
                string[] parts = responseMessage.Split(new[] { ':', '<' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    _publicIP = parts[1].Trim();
                    _publicPort = int.Parse(parts[2].Trim());

                    // Hiển thị thông tin IP và Port công khai trong MessageBox
                    MessageBox.Show($"Public IP: {_publicIP}\nPublic Port: {_publicPort}", "STUN Response", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Invalid STUN response format.", "STUN Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



    }
}
