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
        private System.Windows.Forms.Label lblConnectionStatus;





        private void InitializeComponent()
        {
            txtUserName = new TextBox();
            txtServerDNS = new TextBox();
            txtPort = new TextBox();
            lstUsers = new ListBox();
            btnConnect = new Button();
            rtbMessages = new RichTextBox();
            lstMessagedUsers = new ListBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            SuspendLayout();
            // 
            // txtUserName
            // 
            txtUserName.Location = new Point(17, 50);
            txtUserName.Name = "txtUserName";
            txtUserName.PlaceholderText = "Enter your username";
            txtUserName.Size = new Size(150, 23);
            txtUserName.TabIndex = 0;
            // 
            // txtServerDNS
            // 
            txtServerDNS.Location = new Point(17, 85);
            txtServerDNS.Name = "txtServerDNS";
            txtServerDNS.Size = new Size(150, 23);
            txtServerDNS.TabIndex = 1;
            // 
            // txtPort
            // 
            txtPort.Location = new Point(17, 114);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(150, 23);
            txtPort.TabIndex = 2;
            // 
            // lstUsers
            // 
            lstUsers.ItemHeight = 15;
            lstUsers.Location = new Point(232, 182);
            lstUsers.Name = "lstUsers";
            lstUsers.Size = new Size(155, 199);
            lstUsers.TabIndex = 3;
            lstUsers.DoubleClick += lstUsers_DoubleClick;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(258, 50);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(101, 85);
            btnConnect.TabIndex = 5;
            btnConnect.Text = "Connect";
            btnConnect.Click += btnConnect_Click;
            // 
            // rtbMessages
            // 
            rtbMessages.Location = new Point(58, 457);
            rtbMessages.Name = "rtbMessages";
            rtbMessages.Size = new Size(292, 69);
            rtbMessages.TabIndex = 10;
            rtbMessages.Text = "";
            // 
            // lstMessagedUsers
            // 
            lstMessagedUsers.ItemHeight = 15;
            lstMessagedUsers.Location = new Point(17, 182);
            lstMessagedUsers.Name = "lstMessagedUsers";
            lstMessagedUsers.Size = new Size(155, 199);
            lstMessagedUsers.TabIndex = 9;
        
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(232, 155);
            label1.Name = "label1";
            label1.Size = new Size(138, 15);
            label1.TabIndex = 11;
            label1.Text = "Danh sách người kết nối ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(17, 155);
            label2.Name = "label2";
            label2.Size = new Size(143, 15);
            label2.TabIndex = 12;
            label2.Text = "Danh sách người nhắn tín";
      
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(232, 397);
            label3.Name = "label3";
            label3.Size = new Size(145, 30);
            label3.TabIndex = 13;
            label3.Text = "Ấn double click để vào \r\ndanh sách người nhắn tin ";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(17, 397);
            label4.Name = "label4";
            label4.Size = new Size(152, 15);
            label4.TabIndex = 14;
            label4.Text = "Ấn double click để mở chát";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(17, 18);
            label5.Name = "label5";
            label5.Size = new Size(131, 15);
            label5.TabIndex = 15;
            label5.Text = "Nhập tên và ấn Conect ";
            // 
            // Form1
            // 
            ClientSize = new Size(405, 552);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(lstMessagedUsers);
            Controls.Add(txtUserName);
            Controls.Add(txtServerDNS);
            Controls.Add(txtPort);
            Controls.Add(lstUsers);
            Controls.Add(btnConnect);
            Controls.Add(rtbMessages);
            Name = "Form1";
            Text = "Socket Client GUI";
            ResumeLayout(false);
            PerformLayout();
        }

        private ListBox lstMessagedUsers;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
    }
}
