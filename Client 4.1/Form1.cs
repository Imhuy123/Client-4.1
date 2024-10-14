using System;
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

        public Form1()
        {
            InitializeComponent();

            // Thiết lập DNS và Port mặc định
            txtServerDNS.Text = "huynas123.synology.me";
            txtPort.Text = "8081";
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            _userName = txtUserName.Text.Trim();
            string serverAddress = txtServerDNS.Text.Trim();
            int port = int.Parse(txtPort.Text.Trim());

            ConnectToServer(serverAddress, port);
            if (_isConnected)
            {
                // Chạy luồng lấy danh sách người dùng
                Thread userListThread = new Thread(GetUserList);
                userListThread.Start();

                // Chạy luồng nhận tin nhắn
                Thread receiveThread = new Thread(ReceiveMessages);
                receiveThread.Start();
            }
        }

        private void ConnectToServer(string serverAddress, int port)
        {
            try
            {
                // Lấy địa chỉ IP từ DNS hoặc tên server
                IPHostEntry hostEntry = Dns.GetHostEntry(serverAddress);
                IPAddress ipAddress = hostEntry.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Tạo socket và kết nối đến server
                _clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.Connect(remoteEP);

                // Gửi tên người dùng đến server
                byte[] userNameBytes = Encoding.ASCII.GetBytes(_userName + "<EOF>");
                _clientSocket.Send(userNameBytes);

                _isConnected = true;
                UpdateStatus("Connected to the server!");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Connection error: {ex.Message}");
            }
        }

        private void GetUserList()
        {
            while (_isConnected)
            {
                try
                {
                    byte[] requestBytes = Encoding.ASCII.GetBytes("RequestUserList<EOF>");
                    _clientSocket.Send(requestBytes);

                    byte[] buffer = new byte[1024];
                    int receivedBytes = _clientSocket.Receive(buffer);
                    string response = Encoding.ASCII.GetString(buffer, 0, receivedBytes);

                    if (response.StartsWith("UserList:"))
                    {
                        string userList = response.Replace("UserList:", "").Replace("<EOF>", "");
                        Invoke(new Action(() =>
                        {
                            lstUsers.Items.Clear();
                            lstUsers.Items.AddRange(userList.Split(','));
                        }));
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Error retrieving user list: {ex.Message}");
                }

                Thread.Sleep(5000);
            }
        }

        private void ReceiveMessages()
        {
            while (_isConnected)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int receivedBytes = _clientSocket.Receive(buffer);
                    string message = Encoding.UTF8.GetString(buffer, 0, receivedBytes).Replace("<EOF>", "");

                    Invoke(new Action(() =>
                    {
                        rtbMessages.AppendText(message + Environment.NewLine);
                    }));
                }
                catch (Exception)
                {
                    UpdateStatus("Server disconnected.");
                    _isConnected = false;
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string toUser = lstUsers.SelectedItem?.ToString();
            string messageContent = txtMessage.Text.Trim();

            if (string.IsNullOrEmpty(toUser) || string.IsNullOrEmpty(messageContent))
            {
                MessageBox.Show("Please select a user and enter a message.");
                return;
            }

            string message = $"{toUser}:{messageContent}<EOF>";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            _clientSocket.Send(messageBytes);

            txtMessage.Clear();
        }

        private void UpdateStatus(string status)
        {
            Invoke(new Action(() =>
            {
                lblStatus.Text = status;
            }));
        }
    }
}
