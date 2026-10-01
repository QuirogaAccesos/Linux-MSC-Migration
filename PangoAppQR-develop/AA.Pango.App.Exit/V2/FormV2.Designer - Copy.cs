namespace AA.Pango.App
{
    partial class FormV2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tlblVersion = new System.Windows.Forms.ToolStripStatusLabel();
            this.tlblTerminalId = new System.Windows.Forms.ToolStripStatusLabel();
            this.tlblDate = new System.Windows.Forms.ToolStripStatusLabel();
            this.tlblStatusTerminal = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnShowQrCode = new System.Windows.Forms.Button();
            this.btnScanTicket = new System.Windows.Forms.Button();
            this.btnPayByPlate = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblCurrentDate = new System.Windows.Forms.Label();
            this.lblInstructions = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tlblVersion,
            this.tlblTerminalId,
            this.tlblDate,
            this.tlblStatusTerminal});
            this.statusStrip1.Location = new System.Drawing.Point(0, 1028);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 14, 0);
            this.statusStrip1.Size = new System.Drawing.Size(2072, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tlblVersion
            // 
            this.tlblVersion.Name = "tlblVersion";
            this.tlblVersion.Size = new System.Drawing.Size(0, 15);
            // 
            // tlblTerminalId
            // 
            this.tlblTerminalId.Name = "tlblTerminalId";
            this.tlblTerminalId.Size = new System.Drawing.Size(0, 15);
            // 
            // tlblDate
            // 
            this.tlblDate.Name = "tlblDate";
            this.tlblDate.Size = new System.Drawing.Size(0, 15);
            // 
            // tlblStatusTerminal
            // 
            this.tlblStatusTerminal.Name = "tlblStatusTerminal";
            this.tlblStatusTerminal.Size = new System.Drawing.Size(0, 15);
            // 
            // btnShowQrCode
            // 
            this.btnShowQrCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowQrCode.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.btnShowQrCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnShowQrCode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(85)))), ((int)(((byte)(151)))));
            this.btnShowQrCode.Location = new System.Drawing.Point(211, 502);
            this.btnShowQrCode.Name = "btnShowQrCode";
            this.btnShowQrCode.Size = new System.Drawing.Size(1654, 141);
            this.btnShowQrCode.TabIndex = 26;
            this.btnShowQrCode.Text = "1. Show QR Code";
            this.btnShowQrCode.UseVisualStyleBackColor = false;
            this.btnShowQrCode.Click += new System.EventHandler(this.btnOption1_Click);
            // 
            // btnScanTicket
            // 
            this.btnScanTicket.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnScanTicket.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.btnScanTicket.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnScanTicket.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(85)))), ((int)(((byte)(151)))));
            this.btnScanTicket.Location = new System.Drawing.Point(210, 686);
            this.btnScanTicket.Name = "btnScanTicket";
            this.btnScanTicket.Size = new System.Drawing.Size(1655, 143);
            this.btnScanTicket.TabIndex = 27;
            this.btnScanTicket.Text = "2. Scan Ticket";
            this.btnScanTicket.UseVisualStyleBackColor = false;
            this.btnScanTicket.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnPayByPlate
            // 
            this.btnPayByPlate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPayByPlate.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.btnPayByPlate.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPayByPlate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(85)))), ((int)(((byte)(151)))));
            this.btnPayByPlate.Location = new System.Drawing.Point(210, 868);
            this.btnPayByPlate.Name = "btnPayByPlate";
            this.btnPayByPlate.Size = new System.Drawing.Size(1652, 140);
            this.btnPayByPlate.TabIndex = 28;
            this.btnPayByPlate.Text = "3. Pay by Plate. Insert Credit Card for payment";
            this.btnPayByPlate.UseVisualStyleBackColor = false;
            this.btnPayByPlate.Click += new System.EventHandler(this.button3_Click);
            // 
            // label2
            // 
            this.label2.AllowDrop = true;
            this.label2.AutoEllipsis = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(85)))), ((int)(((byte)(151)))));
            this.label2.Location = new System.Drawing.Point(0, 258);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(2072, 75);
            this.label2.TabIndex = 5;
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AllowDrop = true;
            this.label1.AutoEllipsis = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(2072, 258);
            this.label1.TabIndex = 4;
            this.label1.Text = "Welcome to The Centennial Garage";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCurrentDate
            // 
            this.lblCurrentDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrentDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentDate.ForeColor = System.Drawing.Color.White;
            this.lblCurrentDate.Location = new System.Drawing.Point(0, 712);
            this.lblCurrentDate.Name = "lblCurrentDate";
            this.lblCurrentDate.Size = new System.Drawing.Size(2072, 75);
            this.lblCurrentDate.TabIndex = 29;
            this.lblCurrentDate.Text = "label3";
            this.lblCurrentDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblInstructions
            // 
            this.lblInstructions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInstructions.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstructions.ForeColor = System.Drawing.Color.White;
            this.lblInstructions.Location = new System.Drawing.Point(12, 246);
            this.lblInstructions.Name = "lblInstructions";
            this.lblInstructions.Size = new System.Drawing.Size(2048, 202);
            this.lblInstructions.TabIndex = 30;
            this.lblInstructions.Text = "To exit the garage, please choose one of the following options:";
            this.lblInstructions.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.BackgroundImage = global::AA.Pango.App.Exit.Properties.Resources.Logo_Pango_MSC_v2_0_1;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Location = new System.Drawing.Point(0, 812);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(2072, 213);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // FormV2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(85)))), ((int)(((byte)(151)))));
            this.ClientSize = new System.Drawing.Size(2072, 1050);
            this.ControlBox = false;
            this.Controls.Add(this.lblInstructions);
            this.Controls.Add(this.lblCurrentDate);
            this.Controls.Add(this.btnPayByPlate);
            this.Controls.Add(this.btnScanTicket);
            this.Controls.Add(this.btnShowQrCode);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "FormV2";
            this.Text = "Pango Terminal - Exit";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing_1);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tlblVersion;
        private System.Windows.Forms.ToolStripStatusLabel tlblTerminalId;
        private System.Windows.Forms.ToolStripStatusLabel tlblDate;
        private System.Windows.Forms.ToolStripStatusLabel tlblStatusTerminal;
        private System.Windows.Forms.Button btnShowQrCode;
        private System.Windows.Forms.Button btnScanTicket;
        private System.Windows.Forms.Button btnPayByPlate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblCurrentDate;
        private System.Windows.Forms.Label lblInstructions;
    }
}

