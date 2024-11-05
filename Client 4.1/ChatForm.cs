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

        public ChatForm(Socket clientSocket, string currentUser, string chatWithUser, Socket videoSocket, int localUdpPort)
        {
            InitializeComponent();
            _clientSocket = clientSocket;
            _currentUser = currentUser;
            _chatWithUser = chatWithUser;
            _videoSocket = videoSocket;
            _localUdpPort = localUdpPort;

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

        private void SendUdpMessage(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // Lấy địa chỉ IP của server từ DNS
            IPAddress[] addresses = Dns.GetHostAddresses("huynas123.synology.me");
            if (addresses.Length == 0)
            {
                Console.WriteLine("Không tìm thấy địa chỉ IP cho tên miền huynas123.synology.me.");
                return;
            }

            IPEndPoint serverEndPoint = new IPEndPoint(addresses[0], 8082); // Server lắng nghe tại cổng 8082
            udpListener.Send(messageBytes, messageBytes.Length, serverEndPoint); // Sử dụng udpListener đã khởi tạo ở cổng động để gửi
            Console.WriteLine($"Sent UDP message: {message} from client to server on port 8082");
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
                // Lắng nghe trên tất cả các địa chỉ IP và tất cả các cổng
                udpListener = new UdpClient(new IPEndPoint(IPAddress.Any, 0));

                listenerThread = new Thread(() =>
                {
                    try
                    {
                        while (isUdpListenerRunning)
                        {
                            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                            byte[] receivedBytes = udpListener.Receive(ref remoteEndPoint);
                            string receivedMessage = Encoding.UTF8.GetString(receivedBytes);

                            Console.WriteLine($"Received UDP message from {remoteEndPoint.Address}:{remoteEndPoint.Port}");

                            // Xử lý các thông điệp UDP như trước
                            if (receivedMessage.StartsWith("RING:"))
                            {
                                HandleRingMessage(receivedMessage, remoteEndPoint);
                            }
                            else if (receivedMessage.StartsWith("CALL_ACCEPT:") && !isInCall)
                            {
                                isInCall = true;
                                Invoke(new Action(StartAudioStreaming));
                            }
                            else if (receivedMessage.StartsWith("CALL_REJECT:"))
                            {
                                MessageBox.Show($"{_chatWithUser} has rejected the call.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    catch (SocketException ex)
                    {
                        if (isUdpListenerRunning)
                        {
                            MessageBox.Show("SocketException: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                });

                listenerThread.IsBackground = true;
                listenerThread.Start();

                // Thông báo rằng listener đã được khởi tạo
                MessageBox.Show("UDP Listener is running on all ports", "UDP Port Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing UDP listener: " + ex.Message);
            }
        }


        private void HandleRingMessage(string receivedMessage, IPEndPoint remoteEndPoint)
        {
            Console.WriteLine("Processing RING message...");
            string[] parts = receivedMessage.Split(new[] { ':', '-', '>' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                string sender = parts[1].Trim();

                SendUdpMessage($"ACK:{_currentUser} has received the call from {sender}<EOF>");

                Invoke(new Action(() =>
                {
                    var result = MessageBox.Show($"You have an incoming call from {sender}. Accept?", "Incoming Call", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        isCallAccepted = true;
                        StartAudioStreaming();
                        SendUdpMessage($"CALL_ACCEPT:{_currentUser}->{sender}<EOF>");
                    }
                    else
                    {
                        isCallAccepted = false;
                        SendUdpMessage($"CALL_REJECT:{_currentUser}->{sender}<EOF>");
                    }
                }));
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

        private void viewCall_Click(object sender, EventArgs e)
        {
            isCallAccepted = false;
            Thread udpSenderThread = new Thread(() =>
            {
                int elapsed = 0;
                while (!isCallAccepted && elapsed < 10000)
                {
                    string ringMessage = $"RING:{_currentUser}->{_chatWithUser}<EOF>";
                    SendUdpMessage(ringMessage);
                    Console.WriteLine("Sending RING message...");

                    Thread.Sleep(1000);
                    elapsed += 1000;
                }

                if (!isCallAccepted)
                {
                    MessageBox.Show("The call was not accepted.", "Call Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            });

            udpSenderThread.Start();
        }
    }
}
