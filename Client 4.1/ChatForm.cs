using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client_4._1
{
    public partial class ChatForm : Form
    {
        private readonly Socket _clientSocket;
        private readonly string _currentUser;
        private readonly string _chatWithUser;
        private UdpClient udpListener;
        private Thread listenerThread;
        private bool isUdpListenerRunning = true;
        private readonly Socket _videoSocket;
        private readonly int _localUdpPort;
        private bool isInCall = false;
        private bool isCallAccepted = false;
        private readonly string _publicIP;
        private readonly int _publicPort;

        public ChatForm(Socket clientSocket, string currentUser, string chatWithUser, Socket videoSocket, int localUdpPort, string publicIP, int publicPort)
        {
            InitializeComponent();
            _clientSocket = clientSocket;
            _currentUser = currentUser;
            _chatWithUser = chatWithUser;
            _videoSocket = videoSocket;
            _localUdpPort = localUdpPort;
            _publicIP = publicIP;
            _publicPort = publicPort;

            this.Text = $"Chat with {_chatWithUser}";
            txtMessage.KeyDown += TxtMessage_KeyDown;
            this.FormClosed += ChatForm_FormClosed;

            StartUdpListener();
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

            string encryptedMessage = BouncyCastleEncryptionHelper.Encrypt(messageContent);
            if (encryptedMessage == null)
            {
                MessageBox.Show("Encryption failed.");
                return;
            }

            string message = $"{_currentUser}->{_chatWithUser}:{encryptedMessage}<EOF>";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            try
            {
                if (_clientSocket.Connected)
                {
                    _clientSocket.Send(messageBytes);
                    txtMessage.Clear();
                    ReceiveMessage($"Me : {encryptedMessage}");
                }
                else
                {
                    MessageBox.Show("You are not connected to the server.");
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Socket exception: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void SendUdpMessage(string message, string destinationIP, int destinationPort)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            IPEndPoint destEndPoint = new IPEndPoint(IPAddress.Parse(destinationIP), destinationPort);
            udpListener.Send(messageBytes, messageBytes.Length, destEndPoint);
        }

        public void ReceiveMessage(string message)
        {
            var splitMessage = message.Split(new[] { ":" }, 2, StringSplitOptions.None);
            if (splitMessage.Length == 2)
            {
                string fromUser = splitMessage[0].Trim();
                string content = splitMessage[1].Trim().Replace("<EOF>", "");

                try
                {
                    string decryptedMessage = BouncyCastleEncryptionHelper.Decrypt(content);
                    if (decryptedMessage != null)
                    {
                        if (this.IsHandleCreated)
                        {
                            Invoke(new Action(() =>
                            {
                                rtbChatHistory.AppendText($"{fromUser}: {decryptedMessage}{Environment.NewLine}");
                                rtbChatHistory.SelectionStart = rtbChatHistory.Text.Length;
                                rtbChatHistory.ScrollToCaret();
                            }));
                        }
                    }
                }
                catch
                {
                    // Ignore decryption errors
                }
            }
        }

        private void StartUdpListener()
        {
            try
            {
                udpListener = new UdpClient(_publicPort);
                listenerThread = new Thread(() =>
                {
                    while (isUdpListenerRunning)
                    {
                        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        byte[] receivedBytes = udpListener.Receive(ref remoteEndPoint);
                        string receivedMessage = Encoding.UTF8.GetString(receivedBytes);

                        if (receivedMessage.StartsWith("RING:"))
                        {
                            HandleRingMessage(receivedMessage, remoteEndPoint);
                        }
                    }
                });

                listenerThread.IsBackground = true;
                listenerThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing UDP listener: " + ex.Message);
            }
        }

        private void HandleRingMessage(string receivedMessage, IPEndPoint remoteEndPoint)
        {
            // Tách các phần tử dựa trên dấu phân cách, đảm bảo định dạng chính xác của chuỗi
            string[] parts = receivedMessage.Split(new[] { ':', '-', '>', '<' }, StringSplitOptions.RemoveEmptyEntries);

            // Kiểm tra độ dài của `parts` để đảm bảo rằng nó chứa đủ các thành phần
            if (parts.Length >= 5) // `parts[0]` là "RING", `parts[1]` là sender, `parts[2]` là receiver, `parts[3]` là IP, `parts[4]` là port
            {
                string sender = parts[1].Trim();
                string receiver = parts[2].Trim();
                string senderPublicIP = parts[3].Trim();

                // Kiểm tra nếu `parts[4]` có thể chuyển thành số nguyên
                if (int.TryParse(parts[4].Trim(), out int senderPublicPort))
                {
                    MessageBox.Show($"Debug: sender = '{sender}', receiver = '{receiver}', IP = '{senderPublicIP}', Port = '{senderPublicPort}'", "Debug");

                    Invoke(new Action(() =>
                    {
                        var result = MessageBox.Show($"You have an incoming call from {sender}. Accept?", "Incoming Call", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            isCallAccepted = true;
                            StartAudioStreaming();

                            // Gửi lại thông tin STUN của máy nhận khi chấp nhận cuộc gọi
                            SendUdpMessage($"CALL_ACCEPT:{_currentUser}->{sender}:{_publicIP}:{_publicPort}<EOF>", senderPublicIP, senderPublicPort);
                        }
                        else
                        {
                            SendUdpMessage($"CALL_REJECT:{_currentUser}->{sender}<EOF>", senderPublicIP, senderPublicPort);
                        }
                    }));
                }
                else
                {
                    MessageBox.Show("Error parsing port: Port is not a valid integer.", "Format Error");
                }
            }
            else
            {
                MessageBox.Show("Received message format is incorrect.", "Format Error");
            }
        }




        private void StartAudioStreaming()
        {
            MessageBox.Show("Starting audio streaming between both parties!", "Audio Streaming", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            isUdpListenerRunning = false;
            udpListener?.Close();
        }

        private void StartVideoCall()
        {
            isCallAccepted = false;
            Thread udpSenderThread = new Thread(() =>
            {
                int elapsed = 0;
                while (!isCallAccepted && elapsed < 10000)
                {
                    string ringMessage = $"RING:{_currentUser}->{_chatWithUser}:{_publicIP}:{_publicPort}<EOF>";
                    SendUdpMessage(ringMessage, _publicIP, _publicPort);
                    Thread.Sleep(1000);
                    elapsed += 1000;
                }

                if (!isCallAccepted)
                {
                    MessageBox.Show("The call was not accepted.", "Call Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Console.WriteLine("The call was accepted.");
                }
            });

            udpSenderThread.Start();
        }



        private void viewCall_Click(object sender, EventArgs e)
        {
            StartVideoCall();
        }
    }
}
