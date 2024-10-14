namespace Client_4._1
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.TextBox txtServerDNS;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.ListBox lstUsers;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.RichTextBox rtbMessages;
        private System.Windows.Forms.Label lblStatus;

        private void InitializeComponent()
        {
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.txtServerDNS = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lstUsers = new System.Windows.Forms.ListBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.rtbMessages = new System.Windows.Forms.RichTextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // UserName TextBox
            this.txtUserName.Location = new System.Drawing.Point(12, 12);
            this.txtUserName.Size = new System.Drawing.Size(150, 20);
            this.txtUserName.PlaceholderText = "Enter your username";

            // ServerDNS TextBox
            this.txtServerDNS.Location = new System.Drawing.Point(12, 38);
            this.txtServerDNS.Size = new System.Drawing.Size(150, 20);

            // Port TextBox
            this.txtPort.Location = new System.Drawing.Point(12, 64);
            this.txtPort.Size = new System.Drawing.Size(150, 20);

            // Connect Button
            this.btnConnect.Location = new System.Drawing.Point(180, 12);
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.Text = "Connect";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);

            // Users ListBox
            this.lstUsers.Location = new System.Drawing.Point(12, 100);
            this.lstUsers.Size = new System.Drawing.Size(150, 200);

            // Message TextBox
            this.txtMessage.Location = new System.Drawing.Point(180, 100);
            this.txtMessage.Size = new System.Drawing.Size(150, 20);

            // Send Button
            this.btnSend.Location = new System.Drawing.Point(180, 130);
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.Text = "Send";
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);

            // Messages RichTextBox
            this.rtbMessages.Location = new System.Drawing.Point(180, 160);
            this.rtbMessages.Size = new System.Drawing.Size(300, 140);

            // Status Label
            this.lblStatus.Location = new System.Drawing.Point(12, 320);
            this.lblStatus.Size = new System.Drawing.Size(300, 23);

            // Form1
            this.ClientSize = new System.Drawing.Size(500, 350);
            this.Controls.Add(this.txtUserName);
            this.Controls.Add(this.txtServerDNS);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.lstUsers);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.rtbMessages);
            this.Controls.Add(this.lblStatus);
            this.Text = "Socket Client GUI";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
