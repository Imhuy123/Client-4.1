using System;
using System.Collections.Generic;
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
        private Dictionary<string, ChatForm> _openChats = new Dictionary<string, ChatForm>();

        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            _userName = txtUserName.Text.Trim();
            string serverIp = txtServerIp.Text.Trim();
            int port = int.Parse(txtPort.Text.Trim());

            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clientSocket.Connect(serverIp, port);

            byte[] userNameBytes = Encoding.UTF8.GetBytes(_userName + "<EOF>");
            _clientSocket.Send(userNameBytes);

            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            MessageBox.Show("Connected to the server!");
        }

        private void ReceiveMessages()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int receivedBytes = _clientSocket.Receive(buffer);
                    string message = Encoding.UTF8.GetString(buffer, 0, receivedBytes).Replace("<EOF>", "");

                    string[] splitMessage = message.Split(new char[] { ':' }, 2);
                    if (splitMessage.Length == 2)
                    {
                        string fromUser = splitMessage[0].Trim();
                        string content = splitMessage[1].Trim();

                        // Kiểm tra xem đã mở ChatForm với người dùng này chưa
                        if (_openChats.ContainsKey(fromUser))
                        {
                            _openChats[fromUser].ReceiveMessage(content);
                        }
                        else
                        {
                            Invoke(new Action(() =>
                            {
                                // Tạo cửa sổ Chat mới nếu chưa có
                                ChatForm chatForm = new ChatForm(_clientSocket, _userName, fromUser);
                                chatForm.ReceiveMessage(content);
                                _openChats.Add(fromUser, chatForm);
                                chatForm.Show();
                            }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error receiving message: {ex.Message}");
                    break;
                }
            }
        }

        private void lstUsers_DoubleClick(object sender, EventArgs e)
        {
            string selectedUser = lstUsers.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedUser)) return;

            if (!_openChats.ContainsKey(selectedUser))
            {
                // Mở cửa sổ chat mới nếu chưa mở
                ChatForm chatForm = new ChatForm(_clientSocket, _userName, selectedUser);
                _openChats.Add(selectedUser, chatForm);
                chatForm.Show();
            }
        }
    }
}
