using System.Net.Sockets;
using System.Net;
using System.Text;

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
        private bool isInCall = false;

        public ChatForm(Socket clientSocket, string currentUser, string chatWithUser, Socket videoSocket)
        {
            InitializeComponent();
            _clientSocket = clientSocket;
            _currentUser = currentUser;
            _chatWithUser = chatWithUser;
            _videoSocket = videoSocket;

            this.Text = $"Chat with {_chatWithUser}";
            txtMessage.KeyDown += TxtMessage_KeyDown;
            this.FormClosed += ChatForm_FormClosed;

            // Start listener on UDP port 8083 for incoming messages
            StartUdpListener(8083);
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
            _videoSocket.Send(messageBytes);
            Console.WriteLine($"Sent UDP message: {message}");
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

        private void StartUdpListener(int udpPort = 8083) // Listen on port 8083 for incoming UDP messages
        {
            udpListener = new UdpClient(udpPort); // Bind directly to port 8083
            udpListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpListener.ExclusiveAddressUse = false;

            listenerThread = new Thread(() =>
            {
                try
                {
                    while (isUdpListenerRunning)
                    {
                        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, udpPort);
                        byte[] receivedBytes = udpListener.Receive(ref remoteEndPoint);
                        string receivedMessage = Encoding.UTF8.GetString(receivedBytes);

                        Console.WriteLine($"Received UDP message: {receivedMessage} from {remoteEndPoint}");

                        if (receivedMessage.StartsWith("RING:"))
                        {
                            Console.WriteLine("Processing RING message...");
                            string[] parts = receivedMessage.Split(new[] { ':', '-', '>' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 2)
                            {
                                string sender = parts[1].Trim();

                                // Send ACK to server to acknowledge RING message
                                SendUdpMessage($"ACK:{_currentUser} has received the call from {sender}<EOF>");

                                // Show incoming call notification
                                Invoke(new Action(() =>
                                {
                                    var result = MessageBox.Show($"You have an incoming call from {sender}. Accept?", "Incoming Call", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                    if (result == DialogResult.Yes)
                                    {
                                        StartAudioStreaming();
                                        SendUdpMessage($"CALL_ACCEPT:{_currentUser}->{sender}<EOF>");
                                    }
                                    else
                                    {
                                        SendUdpMessage($"CALL_REJECT:{_currentUser}->{sender}<EOF>");
                                    }
                                }));
                            }
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
                        else if (receivedMessage.StartsWith("ACK:"))
                        {
                            Invoke(new Action(() =>
                            {
                                MessageBox.Show(receivedMessage.Replace("ACK:", ""), "Acknowledgement", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }));
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
        }

        private void StartAudioStreaming()
        {
            MessageBox.Show("Starting audio streaming between both parties!", "Audio Streaming", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Insert code to initiate audio streaming here
        }

        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            isUdpListenerRunning = false;
            udpListener?.Close();
        }

        private void viewCall_Click(object sender, EventArgs e)
        {
            try
            {
                string ringMessage = $"RING:{_currentUser}->{_chatWithUser}<EOF>";
                SendUdpMessage(ringMessage);

                MessageBox.Show("Calling " + _chatWithUser, "Calling...", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to send ring request: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}