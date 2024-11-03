﻿using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using NAudio.Wave;

namespace Client_4._1
{
    public partial class VideoForm : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;
        private WaveInEvent waveIn;
        private Label audioStatusLabel;
        private bool isCameraOn = false;
        private bool isVoiceInputOn = true;
        private Thread receiveThread;
        private bool isReceiving = true;

        public VideoForm(string serverIp, int udpPort)
        {
            InitializeComponent();

            udpClient = new UdpClient();
            serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), udpPort);

            this.Load += VideoForm_Load;
            this.FormClosing += VideoForm_FormClosing;

            audioStatusLabel = new Label
            {
                Text = "",
                AutoSize = true,
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.Red,
                Location = new Point(10, 10)
            };
            this.Controls.Add(audioStatusLabel);

            btnTurnoff.Text = "BẬT CAMERA";
            btnTurnoff.Click += BtnTurnoff_Click;

            audio.Click += Audio_Click;
            audio.Text = "TẮT VOICE INPUT";

            // Start listening for incoming UDP packets from server
            receiveThread = new Thread(ReceiveFramesFromServer);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        private void VideoForm_Load(object sender, EventArgs e)
        {
            InitializeAudio();

            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("Không có camera nào được kết nối.");
                return;
            }

            foreach (FilterInfo device in videoDevices)
            {
                cmbCameras.Items.Add(device.Name);
            }

            cmbCameras.SelectedIndex = 0;
        }

        private void StartVideoStream()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                StopVideoStream();
            }

            videoSource = new VideoCaptureDevice(videoDevices[cmbCameras.SelectedIndex].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();
            isCameraOn = true;
            btnTurnoff.Text = "TẮT CAMERA";
        }

        private void StopVideoStream()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
                videoSource.NewFrame -= VideoSource_NewFrame;
                videoSource = null;
            }

            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
                pictureBox.Image = null;
            }

            isCameraOn = false;
            btnTurnoff.Text = "BẬT CAMERA";
        }

        private void BtnTurnoff_Click(object sender, EventArgs e)
        {
            if (isCameraOn)
            {
                StopVideoStream();
            }
            else
            {
                StartVideoStream();
            }
        }

        private void Audio_Click(object sender, EventArgs e)
        {
            if (isVoiceInputOn)
            {
                if (waveIn != null)
                {
                    waveIn.StopRecording();
                }
                audio.Text = "BẬT VOICE INPUT";
                isVoiceInputOn = false;
            }
            else
            {
                if (waveIn != null)
                {
                    waveIn.StartRecording();
                }
                audio.Text = "TẮT VOICE INPUT";
                isVoiceInputOn = true;
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            using (Bitmap frame = (Bitmap)eventArgs.Frame.Clone())
            {
                Bitmap resizedFrame = ResizeImageToFitPictureBox(frame, pictureBox.Width, pictureBox.Height);

                if (pictureBox.InvokeRequired)
                {
                    pictureBox.Invoke(new MethodInvoker(() =>
                    {
                        pictureBox.Image?.Dispose();
                        pictureBox.Image = resizedFrame;
                    }));
                }
                else
                {
                    pictureBox.Image?.Dispose();
                    pictureBox.Image = resizedFrame;
                }

                // Send video frame to server over UDP
                SendFrameToServer(resizedFrame);
            }
        }

        private void SendFrameToServer(Bitmap frame)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                frame.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] frameData = ms.ToArray();
                udpClient.Send(frameData, frameData.Length, serverEndPoint);
            }
        }

        private Bitmap ResizeImageToFitPictureBox(Bitmap image, int width, int height)
        {
            float ratioX = (float)width / image.Width;
            float ratioY = (float)height / image.Height;
            float ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            Bitmap resizedImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }

        private void VideoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopVideoStream();

            if (waveIn != null)
            {
                waveIn.StopRecording();
                waveIn.Dispose();
                waveIn = null;
            }

            udpClient.Close(); // Close UDP client when form closes
            isReceiving = false; // Set the flag to stop receiving
        }

        private void InitializeAudio()
        {
            waveIn = new WaveInEvent();
            waveIn.WaveFormat = new WaveFormat(8000, 1);
            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.StartRecording();
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (!isVoiceInputOn) return;

            bool hasSound = false;

            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index]);
                float sample32 = sample / 32768f;

                if (Math.Abs(sample32) > 0.02)
                {
                    hasSound = true;
                    break;
                }
            }

            if (hasSound)
            {
                UpdateAudioStatus("Có âm thanh");
            }
            else
            {
                UpdateAudioStatus("");
            }
        }

        private void UpdateAudioStatus(string message)
        {
            if (audioStatusLabel.InvokeRequired)
            {
                audioStatusLabel.Invoke(new MethodInvoker(() =>
                {
                    audioStatusLabel.Text = message;
                }));
            }
            else
            {
                audioStatusLabel.Text = message;
            }
        }

        private void ReceiveFramesFromServer()
        {
            while (isReceiving)
            {
                try
                {
                    byte[] receivedData = udpClient.Receive(ref serverEndPoint);
                    using (MemoryStream ms = new MemoryStream(receivedData))
                    {
                        Bitmap receivedFrame = new Bitmap(ms);
                        if (pictureBox1.InvokeRequired)
                        {
                            pictureBox1.Invoke(new MethodInvoker(() =>
                            {
                                pictureBox1.Image?.Dispose();
                                pictureBox1.Image = receivedFrame;
                            }));
                        }
                        else
                        {
                            pictureBox1.Image?.Dispose();
                            pictureBox1.Image = receivedFrame;
                        }
                    }
                }
                catch (SocketException)
                {
                    break; // Break on socket exceptions (e.g., form closing)
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error receiving video frame: " + ex.Message);
                    break;
                }
            }
        }
    }
}
