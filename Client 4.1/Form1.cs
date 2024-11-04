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
        private bool isUdpListenerRunning = true;

        private Dictionary<string, ChatForm> _openChats = new Dictionary<string, ChatForm>();
        private List<string> _messagedUsers = new List<string>();
        private Socket _videoSocket;
        private const int VideoPort = 8082;

        public Form1()
        {
            InitializeComponent();
            txtServerDNS.Text = "huynas123.synology.me";
            txtPort.Text = "8081";

            lstUsers.DoubleClick += lstUsers_DoubleClick;
           
            this.FormClosed += Form1_FormClosed;
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
                IPAddress ipAddress = Dns.GetHostEntry(serverAddress).AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                _clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.Connect(remoteEP);

                byte[] userNameBytes = Encoding.UTF8.GetBytes(_userName + "<EOF>");
                _clientSocket.Send(userNameBytes);

                _isConnected = true;
                AppendStatusMessage("Connected to the server!");

                ConnectToUdpServer(serverAddress);
                new Thread(ReceiveMessages).Start();
                RequestUserList();
            }
            catch (SocketException ex)
            {
                AppendStatusMessage($"Connection error: {ex.Message}");
                _isConnected = false;
            }
        }

        private void ConnectToUdpServer(string serverAddress)
        {
            try
            {
                IPAddress ipAddress = Dns.GetHostEntry(serverAddress).AddressList[0];
                IPEndPoint udpEndPoint = new IPEndPoint(ipAddress, VideoPort);

                _videoSocket = new Socket(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                _videoSocket.Connect(udpEndPoint);

                AppendStatusMessage("Connected to the UDP server on port " + VideoPort);

                string registerMessage = $"REGISTER_UDP:{_userName}<EOF>";
                byte[] registerBytes = Encoding.UTF8.GetBytes(registerMessage);
                _videoSocket.Send(registerBytes);

                byte[] responseBuffer = new byte[1024];
                int receivedBytes = _videoSocket.Receive(responseBuffer);
                string responseMessage = Encoding.UTF8.GetString(responseBuffer, 0, receivedBytes);

                if (responseMessage.StartsWith("REGISTER_SUCCESS"))
                {
                    AppendStatusMessage("UDP registration successful with server.");
                    SendInitialUdpMessage();
                    StartUdpListener();
                }
                else
                {
                    AppendStatusMessage("UDP registration failed.");
                }
            }
            catch (SocketException ex)
            {
                AppendStatusMessage($"UDP connection error: {ex.Message}");
            }
        }

        private void StartUdpListener()
        {
            Thread udpListenerThread = new Thread(() =>
            {
                UdpClient udpClient = new UdpClient(VideoPort);
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                while (isUdpListenerRunning)
                {
                    try
                    {
                        byte[] receivedBytes = udpClient.Receive(ref remoteEndPoint);
                        string receivedMessage = Encoding.UTF8.GetString(receivedBytes);

                        Console.WriteLine($"Received UDP message: {receivedMessage} from {remoteEndPoint}");
                        AppendStatusMessage($"Received UDP message from {remoteEndPoint}: {receivedMessage}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("UDP listening error: " + ex.Message);
                    }
                }
            });
            udpListenerThread.IsBackground = true;
            udpListenerThread.Start();
        }

        private void SendInitialUdpMessage()
        {
            string message = $"INITIAL_UDP:{_userName}<EOF>";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            _videoSocket.Send(messageBytes);
            AppendStatusMessage("Sent initial UDP message to server.");
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
            catch (SocketException ex)
            {
                AppendStatusMessage($"Server disconnected: {ex.Message}");
                _isConnected = false;
            }
        }

        private void HandleIncomingMessage(string message)
        {
            AppendStatusMessage($"Received message: {message}");
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
            Invoke(new Action(() =>
            {
                lstUsers.Items.Clear();
                if (!string.IsNullOrEmpty(userList))
                {
                    string[] users = userList.Split(',');
                    lstUsers.Items.AddRange(users);
                }
            }));
        }

        private void RequestUserList()
        {
            byte[] requestBytes = Encoding.UTF8.GetBytes("GetUserList<EOF>");
            _clientSocket.Send(requestBytes);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            isUdpListenerRunning = false;
            _videoSocket?.Close();
            _clientSocket?.Close();
        }

        private void lstUsers_DoubleClick(object sender, EventArgs e)
        {
            string selectedUser = lstUsers.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedUser))
            {
                AppendStatusMessage($"User {selectedUser} selected for chat.");
            }
        }
    }
}
