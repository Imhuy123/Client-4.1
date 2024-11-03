using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Client_4._1
{
    public class VideoCallManager
    {
        private readonly Socket _clientSocket;
        private readonly string _currentUser;
        private readonly string _chatWithUser;

        public VideoCallManager(Socket clientSocket, string currentUser, string chatWithUser)
        {
            _clientSocket = clientSocket;
            _currentUser = currentUser;
            _chatWithUser = chatWithUser;
        }

        public void StartCallRequest()
        {
            string callRequestMessage = $"CALL_REQUEST:{_currentUser}:{_chatWithUser}<EOF>";
            byte[] messageBytes = Encoding.UTF8.GetBytes(callRequestMessage);
            _clientSocket.Send(messageBytes);
            OpenVideoForm();
        }

        public void AcceptCall(string caller)
        {
            string acceptMessage = $"CALL_ACCEPT:{_currentUser}:{caller}<EOF>";
            byte[] messageBytes = Encoding.UTF8.GetBytes(acceptMessage);
            _clientSocket.Send(messageBytes);
            OpenVideoForm();
        }

        public void RejectCall(string caller)
        {
            string rejectMessage = $"CALL_REJECT:{_currentUser}:{caller}<EOF>";
            byte[] messageBytes = Encoding.UTF8.GetBytes(rejectMessage);
            _clientSocket.Send(messageBytes);
        }

        private void OpenVideoForm()
        {
            VideoForm videoForm = new VideoForm("127.0.0.1", 8082); // Adjust IP and port as needed
            videoForm.Show();
        }
    }
}
