namespace AA.Pango.App
{
    partial class FormV2PayByPlate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormV2PayByPlate));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tlblVersion = new System.Windows.Forms.ToolStripStatusLabel();
            this.tlblTerminalId = new System.Windows.Forms.ToolStripStatusLabel();
            this.tlblDate = new System.Windows.Forms.ToolStripStatusLabel();
            this.tlblStatusTerminal = new System.Windows.Forms.ToolStripStatusLabel();
            this.txtInputCode = new System.Windows.Forms.TextBox();
            this.btnGoBack = new System.Windows.Forms.Button();
            this.lblPlateNotRead = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblCurrentDate = new System.Windows.Forms.Label();
            this.lblPlateRead = new System.Windows.Forms.Label();
            this.Fondo = new System.Windows.Forms.PictureBox();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Fondo)).BeginInit();
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
            this.statusStrip1.Location = new System.Drawing.Point(0, 1022);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 14, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1626, 28);
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
            // txtInputCode
            // 
            this.txtInputCode.Location = new System.Drawing.Point(12, 12);
            this.txtInputCode.Name = "txtInputCode";
            this.txtInputCode.Size = new System.Drawing.Size(1016, 26);
            this.txtInputCode.TabIndex = 3;
            this.txtInputCode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // btnGoBack
            // 
            this.btnGoBack.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGoBack.BackColor = System.Drawing.Color.White;
            this.btnGoBack.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGoBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(85)))), ((int)(((byte)(151)))));
            this.btnGoBack.Location = new System.Drawing.Point(11, 775);
            this.btnGoBack.Name = "btnGoBack";
            this.btnGoBack.Size = new System.Drawing.Size(1603, 145);
            this.btnGoBack.TabIndex = 28;
            this.btnGoBack.Text = "BACK";
            this.btnGoBack.UseVisualStyleBackColor = false;
            this.btnGoBack.Click += new System.EventHandler(this.btnGoBack_Click);
            // 
            // lblPlateNotRead
            // 
            this.lblPlateNotRead.AllowDrop = true;
            this.lblPlateNotRead.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPlateNotRead.AutoEllipsis = true;
            this.lblPlateNotRead.BackColor = System.Drawing.Color.Transparent;
            this.lblPlateNotRead.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlateNotRead.ForeColor = System.Drawing.Color.White;
            this.lblPlateNotRead.Location = new System.Drawing.Point(2, 284);
            this.lblPlateNotRead.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPlateNotRead.Name = "lblPlateNotRead";
            this.lblPlateNotRead.Size = new System.Drawing.Size(1626, 900);
            this.lblPlateNotRead.TabIndex = 8;
            this.lblPlateNotRead.Text = resources.GetString("lblPlateNotRead.Text");
            this.lblPlateNotRead.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AllowDrop = true;
            this.label2.AutoEllipsis = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(85)))), ((int)(((byte)(151)))));
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(1626, 22);
            this.label2.TabIndex = 5;
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AllowDrop = true;
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoEllipsis = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1626, 235);
            this.label1.TabIndex = 4;
            this.label1.Text = "Welcome to The \r\nCentennial Garage";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCurrentDate
            // 
            this.lblCurrentDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrentDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentDate.ForeColor = System.Drawing.Color.White;
            this.lblCurrentDate.Location = new System.Drawing.Point(13, 934);
            this.lblCurrentDate.Name = "lblCurrentDate";
            this.lblCurrentDate.Size = new System.Drawing.Size(1602, 75);
            this.lblCurrentDate.TabIndex = 29;
            this.lblCurrentDate.Text = "label3";
            this.lblCurrentDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPlateRead
            // 
            this.lblPlateRead.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPlateRead.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlateRead.ForeColor = System.Drawing.Color.White;
            this.lblPlateRead.Image = global::AA.Pango.App.Properties.Resources.lpr;
            this.lblPlateRead.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.lblPlateRead.Location = new System.Drawing.Point(-1, 364);
            this.lblPlateRead.Name = "lblPlateRead";
            this.lblPlateRead.Size = new System.Drawing.Size(1626, 682);
            this.lblPlateRead.TabIndex = 30;
            this.lblPlateRead.Text = "You will Pay-at-the-Exit with your Credit Card. Wait for the gate to open";
            this.lblPlateRead.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblPlateRead.Visible = false;
            // 
            // Fondo
            // 
            this.Fondo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Fondo.Location = new System.Drawing.Point(0, 29);
            this.Fondo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Fondo.Name = "Fondo";
            this.Fondo.Size = new System.Drawing.Size(1626, 1065);
            this.Fondo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Fondo.TabIndex = 13;
            this.Fondo.TabStop = false;
            // 
            // FormV2PayByPlate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(85)))), ((int)(((byte)(151)))));
            this.ClientSize = new System.Drawing.Size(1626, 1050);
            this.ControlBox = false;
            this.Controls.Add(this.lblPlateRead);
            this.Controls.Add(this.lblCurrentDate);
            this.Controls.Add(this.btnGoBack);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.txtInputCode);
            this.Controls.Add(this.Fondo);
            this.Controls.Add(this.lblPlateNotRead);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormV2PayByPlate";
            this.Text = "Pango Terminal - Entry";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Fondo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tlblVersion;
        private System.Windows.Forms.ToolStripStatusLabel tlblTerminalId;
        private System.Windows.Forms.ToolStripStatusLabel tlblDate;
        private System.Windows.Forms.ToolStripStatusLabel tlblStatusTerminal;
        private System.Windows.Forms.TextBox txtInputCode;
        private System.Windows.Forms.PictureBox Fondo;
        private System.Windows.Forms.Button btnGoBack;
        private System.Windows.Forms.Label lblPlateNotRead;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblCurrentDate;
        private System.Windows.Forms.Label lblPlateRead;
    }
}

