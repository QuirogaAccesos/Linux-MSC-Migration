namespace AA.Pango.App
{
    partial class FormV4
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
            this.panelHeader = new System.Windows.Forms.Panel();
            this.tableHeader = new System.Windows.Forms.TableLayoutPanel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.tableHeader2 = new System.Windows.Forms.TableLayoutPanel();
            this.lblHeader2 = new System.Windows.Forms.Label();
            this.lblChargingHours = new System.Windows.Forms.Label();
            this.lblInstructions = new System.Windows.Forms.Label();
            this.lblInstructions2 = new System.Windows.Forms.Label();
            this.lblInstructions3 = new System.Windows.Forms.Label();
            this.lblCurrentDate = new System.Windows.Forms.Label();
            this.tableLayoutFoot = new System.Windows.Forms.TableLayoutPanel();
            this.imgFootDownload = new System.Windows.Forms.PictureBox();
            this.imgFootLogoPango = new System.Windows.Forms.PictureBox();
            this.lblFootDownload = new System.Windows.Forms.Label();
            this.imgFootYellowBtn = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnShowQrCode = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnPayByPlate = new System.Windows.Forms.Button();
            this.panelInit = new System.Windows.Forms.Panel();
            this.txtInputCode = new System.Windows.Forms.TextBox();
            this.lblInitMsg = new System.Windows.Forms.Label();
            this.panelKO = new System.Windows.Forms.Panel();
            this.lblStatusKO = new System.Windows.Forms.Label();
            this.plateOrPhone1 = new AA.Pango.App.Exit.V4.PlateOrPhone();
            this.btnGoBack = new System.Windows.Forms.Button();
            this.panelFoot = new System.Windows.Forms.Panel();
            this.tableLayoutFoot2 = new System.Windows.Forms.TableLayoutPanel();
            this.imgFootLogo2 = new System.Windows.Forms.PictureBox();
            this.imgFootLogoPango2 = new System.Windows.Forms.PictureBox();
            this.imgFootBtn24h = new System.Windows.Forms.PictureBox();
            this.lblFootDate2 = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.tableLayoutFoot.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgFootDownload)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgFootLogoPango)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgFootYellowBtn)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panelInit.SuspendLayout();
            this.panelHeader.SuspendLayout();
            this.tableHeader.SuspendLayout();
            this.tableHeader2.SuspendLayout();
            this.panelKO.SuspendLayout();
            this.panelFoot.SuspendLayout();
            this.tableLayoutFoot2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgFootLogo2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgFootLogoPango2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgFootBtn24h)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.statusStrip1.ForeColor = System.Drawing.Color.Black;
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tlblVersion,
            this.tlblTerminalId,
            this.tlblDate,
            this.tlblStatusTerminal});
            this.statusStrip1.Location = new System.Drawing.Point(0, 878);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 9, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1600, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tlblVersion
            // 
            this.tlblVersion.Name = "tlblVersion";
            this.tlblVersion.Size = new System.Drawing.Size(0, 17);
            // 
            // tlblTerminalId
            // 
            this.tlblTerminalId.Name = "tlblTerminalId";
            this.tlblTerminalId.Size = new System.Drawing.Size(0, 17);
            // 
            // tlblDate
            // 
            this.tlblDate.Name = "tlblDate";
            this.tlblDate.Size = new System.Drawing.Size(0, 17);
            // 
            // tlblStatusTerminal
            // 
            this.tlblStatusTerminal.Name = "tlblStatusTerminal";
            this.tlblStatusTerminal.Size = new System.Drawing.Size(0, 17);
            // 
            // lblHeader
            // 
            this.lblHeader.AutoEllipsis = true;
            this.lblHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHeader.Font = new System.Drawing.Font("Calibri", 44F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Margin = new System.Windows.Forms.Padding(0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(880, 70);
            this.lblHeader.TabIndex = 4;
            this.lblHeader.Text = "FormV4_Welcome";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCurrentDate
            // 
            this.lblCurrentDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrentDate.Font = new System.Drawing.Font("Calibri", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentDate.Location = new System.Drawing.Point(1065, 2);
            this.lblCurrentDate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCurrentDate.Name = "lblCurrentDate";
            this.lblCurrentDate.Size = new System.Drawing.Size(533, 98);
            this.lblCurrentDate.TabIndex = 29;
            this.lblCurrentDate.Text = "lblCurrentDate";
            this.lblCurrentDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutFoot
            // 
            this.tableLayoutFoot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutFoot.ColumnCount = 5;
            this.tableLayoutFoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9.038344F));
            this.tableLayoutFoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.096571F));
            this.tableLayoutFoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.2534F));
            this.tableLayoutFoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.13633F));
            this.tableLayoutFoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.47534F));
            this.tableLayoutFoot.Controls.Add(this.imgFootDownload, 1, 0);
            this.tableLayoutFoot.Controls.Add(this.lblCurrentDate, 4, 0);
            this.tableLayoutFoot.Controls.Add(this.imgFootLogoPango, 2, 0);
            this.tableLayoutFoot.Controls.Add(this.lblFootDownload, 0, 0);
            this.tableLayoutFoot.Controls.Add(this.imgFootYellowBtn, 3, 0);
            this.tableLayoutFoot.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutFoot.Name = "tableLayoutFoot";
            this.tableLayoutFoot.RowCount = 1;
            this.tableLayoutFoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutFoot.Size = new System.Drawing.Size(1600, 100);
            this.tableLayoutFoot.TabIndex = 31;
            // 
            // imgFootDownload
            // 
            this.imgFootDownload.Image = global::AA.Pango.App.Exit.Properties.Resources.downloadApp;
            this.imgFootDownload.Location = new System.Drawing.Point(147, 3);
            this.imgFootDownload.Name = "imgFootDownload";
            this.imgFootDownload.Size = new System.Drawing.Size(91, 94);
            this.imgFootDownload.TabIndex = 32;
            this.imgFootDownload.TabStop = false;
            // 
            // imgFootLogoPango
            // 
            this.imgFootLogoPango.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imgFootLogoPango.BackgroundImage = global::AA.Pango.App.Exit.Properties.Resources.LogoPangoMSC;
            this.imgFootLogoPango.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.imgFootLogoPango.Location = new System.Drawing.Point(243, 2);
            this.imgFootLogoPango.Margin = new System.Windows.Forms.Padding(2);
            this.imgFootLogoPango.Name = "imgFootLogoPango";
            this.imgFootLogoPango.Size = new System.Drawing.Size(304, 96);
            this.imgFootLogoPango.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgFootLogoPango.TabIndex = 0;
            this.imgFootLogoPango.TabStop = false;
            // 
            // lblFootDownload
            // 
            this.lblFootDownload.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblFootDownload.Font = new System.Drawing.Font("Calibri", 21F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFootDownload.Location = new System.Drawing.Point(3, 4);
            this.lblFootDownload.Name = "lblFootDownload";
            this.lblFootDownload.Size = new System.Drawing.Size(138, 91);
            this.lblFootDownload.TabIndex = 32;
            this.lblFootDownload.Text = "Download App";
            this.lblFootDownload.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // imgFootYellowBtn
            // 
            this.imgFootYellowBtn.BackgroundImage = global::AA.Pango.App.Exit.Properties.Resources.btn24h;
            this.imgFootYellowBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.imgFootYellowBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgFootYellowBtn.Location = new System.Drawing.Point(549, 0);
            this.imgFootYellowBtn.Margin = new System.Windows.Forms.Padding(0);
            this.imgFootYellowBtn.Name = "imgFootYellowBtn";
            this.imgFootYellowBtn.Size = new System.Drawing.Size(514, 100);
            this.imgFootYellowBtn.TabIndex = 33;
            this.imgFootYellowBtn.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanel1.Controls.Add(this.btnShowQrCode, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnPayByPlate, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 247);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1600, 530);
            this.tableLayoutPanel1.TabIndex = 32;
            // 
            // btnShowQrCode
            // 
            this.btnShowQrCode.BackColor = System.Drawing.Color.Transparent;
            this.btnShowQrCode.BackgroundImage = global::AA.Pango.App.Exit.Properties.Resources.btnScanQR_Hamilton;
            this.btnShowQrCode.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnShowQrCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnShowQrCode.FlatAppearance.BorderSize = 0;
            this.btnShowQrCode.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnShowQrCode.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnShowQrCode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShowQrCode.Font = new System.Drawing.Font("Calibri", 50F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnShowQrCode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(85)))), ((int)(((byte)(151)))));
            this.btnShowQrCode.Location = new System.Drawing.Point(960, 50);
            this.btnShowQrCode.Margin = new System.Windows.Forms.Padding(80, 50, 80, 50);
            this.btnShowQrCode.Name = "btnShowQrCode";
            this.btnShowQrCode.Size = new System.Drawing.Size(560, 430);
            this.btnShowQrCode.TabIndex = 30;
            this.btnShowQrCode.Text = "FormV4_BtnShowQR";
            this.btnShowQrCode.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnShowQrCode.UseVisualStyleBackColor = false;
            this.btnShowQrCode.Click += new System.EventHandler(this.btnShowQrCode_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::AA.Pango.App.Exit.Properties.Resources.orImage;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(723, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(154, 524);
            this.pictureBox1.TabIndex = 31;
            this.pictureBox1.TabStop = false;
            // 
            // btnPayByPlate
            // 
            this.btnPayByPlate.BackColor = System.Drawing.Color.Transparent;
            this.btnPayByPlate.BackgroundImage = global::AA.Pango.App.Exit.Properties.Resources.btnPayPlate;
            this.btnPayByPlate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnPayByPlate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPayByPlate.FlatAppearance.BorderSize = 0;
            this.btnPayByPlate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnPayByPlate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnPayByPlate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPayByPlate.Font = new System.Drawing.Font("Calibri", 50F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPayByPlate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(85)))), ((int)(((byte)(151)))));
            this.btnPayByPlate.Image = global::AA.Pango.App.Exit.Properties.Resources.payCardContactless;
            this.btnPayByPlate.Location = new System.Drawing.Point(80, 50);
            this.btnPayByPlate.Margin = new System.Windows.Forms.Padding(80, 50, 80, 50);
            this.btnPayByPlate.Name = "btnPayByPlate";
            this.btnPayByPlate.Size = new System.Drawing.Size(560, 430);
            this.btnPayByPlate.TabIndex = 30;
            this.btnPayByPlate.Text = "FormV4_BtnPayPlate";
            this.btnPayByPlate.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnPayByPlate.UseVisualStyleBackColor = false;
            this.btnPayByPlate.Click += new System.EventHandler(this.btnPayByPlate_Click);
            // 
            // panelInit
            // 
            this.panelInit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelInit.Controls.Add(this.txtInputCode);
            this.panelInit.Controls.Add(this.tableLayoutPanel1);
            this.panelInit.Controls.Add(this.lblInitMsg);
            this.panelInit.Controls.Add(this.panelHeader);
            this.panelInit.Location = new System.Drawing.Point(0, 0);
            this.panelInit.Name = "panelInit";
            this.panelInit.Size = new System.Drawing.Size(1600, 777);
            this.panelInit.TabIndex = 36;
            // 
            // txtInputCode
            // 
            this.txtInputCode.Location = new System.Drawing.Point(8, 8);
            this.txtInputCode.Margin = new System.Windows.Forms.Padding(2);
            this.txtInputCode.Name = "txtInputCode";
            this.txtInputCode.Size = new System.Drawing.Size(679, 21);
            this.txtInputCode.TabIndex = 3;
            this.txtInputCode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            this.txtInputCode.Leave += new System.EventHandler(this.txtInputCode_Leave);
            // 
            // lblInitMsg
            // 
            this.lblInitMsg.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblInitMsg.Font = new System.Drawing.Font("Calibri", 44F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInitMsg.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(85)))), ((int)(((byte)(151)))));
            this.lblInitMsg.Location = new System.Drawing.Point(0, 140);
            this.lblInitMsg.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblInitMsg.Name = "lblInitMsg";
            this.lblInitMsg.Size = new System.Drawing.Size(1600, 104);
            this.lblInitMsg.TabIndex = 1;
            this.lblInitMsg.Text = "FormV4_InitMsg";
            this.lblInitMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.White;
            this.panelHeader.Controls.Add(this.tableHeader);
            this.panelHeader.Controls.Add(this.tableHeader2);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(1600, 140);
            this.panelHeader.TabIndex = 36;
            // 
            // tableHeader
            // 
            this.tableHeader.ColumnCount = 1;
            this.tableHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableHeader.Controls.Add(this.lblHeader, 0, 0);
            this.tableHeader.Controls.Add(this.lblInstructions, 0, 1);
            this.tableHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableHeader.Location = new System.Drawing.Point(0, 0);
            this.tableHeader.Margin = new System.Windows.Forms.Padding(0);
            this.tableHeader.Name = "tableHeader";
            this.tableHeader.RowCount = 2;
            this.tableHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableHeader.Size = new System.Drawing.Size(1600, 140);
            this.tableHeader.TabIndex = 30;
            // 
            // lblHeader
            // 
            this.lblHeader.AutoEllipsis = true;
            this.lblHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font = new System.Drawing.Font("Calibri", 44F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Margin = new System.Windows.Forms.Padding(0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(1600, 70);
            this.lblHeader.TabIndex = 4;
            this.lblHeader.Text = "FormV4_Welcome";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblInstructions
            // 
            this.lblInstructions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.lblInstructions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblInstructions.Font = new System.Drawing.Font("Calibri", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstructions.ForeColor = System.Drawing.Color.White;
            this.lblInstructions.Location = new System.Drawing.Point(0, 70);
            this.lblInstructions.Margin = new System.Windows.Forms.Padding(0);
            this.lblInstructions.Name = "lblInstructions";
            this.lblInstructions.Size = new System.Drawing.Size(1600, 70);
            this.lblInstructions.TabIndex = 30;
            this.lblInstructions.Text = "FormV4_Instructions";
            this.lblInstructions.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableHeader2
            // 
            this.tableHeader2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableHeader2.ColumnCount = 2;
            this.tableHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tableHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableHeader2.Controls.Add(this.lblHeader2, 0, 0);
            this.tableHeader2.Controls.Add(this.lblChargingHours, 1, 0);
            this.tableHeader2.Controls.Add(this.lblInstructions2, 0, 1);
            this.tableHeader2.Controls.Add(this.lblInstructions3, 1, 1);
            this.tableHeader2.Location = new System.Drawing.Point(0, 0);
            this.tableHeader2.Margin = new System.Windows.Forms.Padding(0);
            this.tableHeader2.Name = "tableHeader2";
            this.tableHeader2.RowCount = 2;
            this.tableHeader2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableHeader2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableHeader2.Size = new System.Drawing.Size(1600, 140);
            this.tableHeader2.TabIndex = 31;
            // 
            // lblHeader2
            // 
            this.lblHeader2.AutoEllipsis = true;
            this.lblHeader2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.lblHeader2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader2.Font = new System.Drawing.Font("Calibri", 44F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader2.ForeColor = System.Drawing.Color.White;
            this.lblHeader2.Location = new System.Drawing.Point(0, 0);
            this.lblHeader2.Margin = new System.Windows.Forms.Padding(0);
            this.lblHeader2.Name = "lblHeader2";
            this.lblHeader2.Size = new System.Drawing.Size(715, 70);
            this.lblHeader2.TabIndex = 4;
            this.lblHeader2.Text = "FormV4_Welcome";
            this.lblHeader2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblChargingHours
            // 
            this.lblChargingHours.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.lblChargingHours.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblChargingHours.Font = new System.Drawing.Font("Calibri", 33F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChargingHours.ForeColor = System.Drawing.Color.White;
            this.lblChargingHours.Location = new System.Drawing.Point(715, 0);
            this.lblChargingHours.Margin = new System.Windows.Forms.Padding(0);
            this.lblChargingHours.Name = "lblChargingHours";
            this.lblChargingHours.Size = new System.Drawing.Size(585, 70);
            this.lblChargingHours.TabIndex = 4;
            this.lblChargingHours.Text = "FormV4_ChargingHours";
            this.lblChargingHours.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblInstructions2
            // 
            this.lblInstructions2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.lblInstructions2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblInstructions2.Font = new System.Drawing.Font("Calibri", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstructions2.ForeColor = System.Drawing.Color.White;
            this.lblInstructions2.Location = new System.Drawing.Point(0, 70);
            this.lblInstructions2.Margin = new System.Windows.Forms.Padding(0);
            this.lblInstructions2.Name = "lblInstructions";
            this.lblInstructions2.Size = new System.Drawing.Size(715, 70);
            this.lblInstructions2.TabIndex = 30;
            this.lblInstructions2.Text = "FormV4_Instructions";
            this.lblInstructions2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblInstructions3
            // 
            this.lblInstructions3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.lblInstructions3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblInstructions3.Font = new System.Drawing.Font("Calibri", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstructions3.ForeColor = System.Drawing.Color.White;
            this.lblInstructions3.Location = new System.Drawing.Point(715, 70);
            this.lblInstructions3.Margin = new System.Windows.Forms.Padding(0);
            this.lblInstructions3.Name = "lblInstructions3";
            this.lblInstructions3.Size = new System.Drawing.Size(585, 70);
            this.lblInstructions3.TabIndex = 30;
            this.lblInstructions3.Text = "FormV4_Instructions2";
            this.lblInstructions3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelKO
            // 
            this.panelKO.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelKO.Controls.Add(this.lblStatusKO);
            this.panelKO.Controls.Add(this.plateOrPhone1);
            this.panelKO.Controls.Add(this.btnGoBack);
            this.panelKO.Location = new System.Drawing.Point(0, 0);
            this.panelKO.Margin = new System.Windows.Forms.Padding(0);
            this.panelKO.Name = "panelKO";
            this.panelKO.Size = new System.Drawing.Size(1600, 775);
            this.panelKO.TabIndex = 36;
            this.panelKO.Visible = false;
            // 
            // lblStatusKO
            // 
            this.lblStatusKO.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.lblStatusKO.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatusKO.Font = new System.Drawing.Font("Calibri", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatusKO.ForeColor = System.Drawing.Color.White;
            this.lblStatusKO.Location = new System.Drawing.Point(0, 0);
            this.lblStatusKO.Name = "lblStatusKO";
            this.lblStatusKO.Size = new System.Drawing.Size(1600, 140);
            this.lblStatusKO.TabIndex = 7;
            this.lblStatusKO.Text = "FormV4_EnterData";
            this.lblStatusKO.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // plateOrPhone1
            // 
            this.plateOrPhone1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plateOrPhone1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(227)))), ((int)(((byte)(243)))));
            this.plateOrPhone1.Location = new System.Drawing.Point(0, 180);
            this.plateOrPhone1.Margin = new System.Windows.Forms.Padding(4);
            this.plateOrPhone1.Name = "plateOrPhone1";
            this.plateOrPhone1.Size = new System.Drawing.Size(1600, 460);
            this.plateOrPhone1.TabIndex = 32;
            // 
            // btnGoBack
            // 
            this.btnGoBack.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGoBack.BackColor = System.Drawing.Color.White;
            this.btnGoBack.FlatAppearance.BorderSize = 3;
            this.btnGoBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGoBack.Font = new System.Drawing.Font("Calibri", 32F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGoBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(85)))), ((int)(((byte)(151)))));
            this.btnGoBack.Location = new System.Drawing.Point(450, 698);
            this.btnGoBack.Margin = new System.Windows.Forms.Padding(2);
            this.btnGoBack.Name = "btnGoBack";
            this.btnGoBack.Size = new System.Drawing.Size(700, 65);
            this.btnGoBack.TabIndex = 28;
            this.btnGoBack.Text = "FormV4_btnBack";
            this.btnGoBack.UseVisualStyleBackColor = false;
            this.btnGoBack.Click += new System.EventHandler(this.btnGoBack_Click);
            // 
            // panelFoot
            // 
            this.panelFoot.BackColor = System.Drawing.Color.White;
            this.panelFoot.Controls.Add(this.tableLayoutFoot);
            this.panelFoot.Controls.Add(this.tableLayoutFoot2);
            this.panelFoot.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFoot.Location = new System.Drawing.Point(0, 778);
            this.panelFoot.Name = "panelFoot";
            this.panelFoot.Size = new System.Drawing.Size(1600, 100);
            this.panelFoot.TabIndex = 36;
            // 
            // tableLayoutFoot2
            // 
            this.tableLayoutFoot2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutFoot2.ColumnCount = 4;
            this.tableLayoutFoot2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutFoot2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutFoot2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutFoot2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutFoot2.Controls.Add(this.imgFootLogo2, 0, 0);
            this.tableLayoutFoot2.Controls.Add(this.imgFootLogoPango2, 1, 0);
            this.tableLayoutFoot2.Controls.Add(this.imgFootBtn24h, 2, 0);
            this.tableLayoutFoot2.Controls.Add(this.lblFootDate2, 3, 0);
            this.tableLayoutFoot2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutFoot2.Name = "tableLayoutFoot2";
            this.tableLayoutFoot2.RowCount = 1;
            this.tableLayoutFoot2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutFoot2.Size = new System.Drawing.Size(1600, 100);
            this.tableLayoutFoot2.TabIndex = 31;
            // 
            // imgFootLogo2
            // 
            this.imgFootLogo2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imgFootLogo2.BackgroundImage = global::AA.Pango.App.Exit.Properties.Resources.LogoHamilton;
            this.imgFootLogo2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.imgFootLogo2.Location = new System.Drawing.Point(2, 2);
            this.imgFootLogo2.Margin = new System.Windows.Forms.Padding(2);
            this.imgFootLogo2.Name = "imgFootLogo2";
            this.imgFootLogo2.Size = new System.Drawing.Size(396, 96);
            this.imgFootLogo2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgFootLogo2.TabIndex = 0;
            this.imgFootLogo2.TabStop = false;
            // 
            // imgFootLogoPango2
            // 
            this.imgFootLogoPango2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imgFootLogoPango2.BackgroundImage = global::AA.Pango.App.Exit.Properties.Resources.LogoPangoMSC;
            this.imgFootLogoPango2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.imgFootLogoPango2.Location = new System.Drawing.Point(402, 2);
            this.imgFootLogoPango2.Margin = new System.Windows.Forms.Padding(2);
            this.imgFootLogoPango2.Name = "imgFootLogoPango2";
            this.imgFootLogoPango2.Size = new System.Drawing.Size(396, 96);
            this.imgFootLogoPango2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgFootLogoPango2.TabIndex = 0;
            this.imgFootLogoPango2.TabStop = false;
            // 
            // imgFootBtn24h
            // 
            this.imgFootBtn24h.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imgFootBtn24h.BackgroundImage = global::AA.Pango.App.Exit.Properties.Resources.btn24h;
            this.imgFootBtn24h.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.imgFootBtn24h.Location = new System.Drawing.Point(802, 2);
            this.imgFootBtn24h.Margin = new System.Windows.Forms.Padding(2);
            this.imgFootBtn24h.Name = "imgFootBtn24h";
            this.imgFootBtn24h.Size = new System.Drawing.Size(396, 96);
            this.imgFootBtn24h.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgFootBtn24h.TabIndex = 0;
            this.imgFootBtn24h.TabStop = false;
            // 
            // lblFootDate2
            // 
            this.lblFootDate2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFootDate2.Font = new System.Drawing.Font("Calibri", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFootDate2.Location = new System.Drawing.Point(1202, 2);
            this.lblFootDate2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblFootDate2.Name = "lblFootDate2";
            this.lblFootDate2.Size = new System.Drawing.Size(396, 98);
            this.lblFootDate2.TabIndex = 29;
            this.lblFootDate2.Text = "lblCurrentDate";
            this.lblFootDate2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormV4
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(227)))), ((int)(((byte)(243)))));
            this.ClientSize = new System.Drawing.Size(1600, 900);
            this.ControlBox = false;
            this.Controls.Add(this.panelInit);
            this.Controls.Add(this.panelKO);
            this.Controls.Add(this.panelFoot);
            this.Controls.Add(this.statusStrip1);
            this.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FormV4";
            this.Text = "Pango Terminal - Exit";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing_1);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tableLayoutFoot.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgFootDownload)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgFootLogoPango)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgFootYellowBtn)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panelInit.ResumeLayout(false);
            this.panelInit.PerformLayout();
            this.panelHeader.ResumeLayout(false);
            this.tableHeader.ResumeLayout(false);
            this.tableHeader2.ResumeLayout(false);
            this.panelKO.ResumeLayout(false);
            this.panelFoot.ResumeLayout(false);
            this.tableLayoutFoot2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgFootLogo2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgFootLogoPango2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgFootBtn24h)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox imgFootLogoPango;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tlblVersion;
        private System.Windows.Forms.ToolStripStatusLabel tlblTerminalId;
        private System.Windows.Forms.ToolStripStatusLabel tlblDate;
        private System.Windows.Forms.ToolStripStatusLabel tlblStatusTerminal;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.TableLayoutPanel tableHeader;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Label lblInstructions;
        private System.Windows.Forms.TableLayoutPanel tableHeader2;
        private System.Windows.Forms.Label lblHeader2;
        private System.Windows.Forms.Label lblChargingHours;
        private System.Windows.Forms.Label lblInstructions2;
        private System.Windows.Forms.Label lblInstructions3;
        private System.Windows.Forms.Button btnPayByPlate;
        private System.Windows.Forms.Label lblCurrentDate;
        private System.Windows.Forms.Button btnShowQrCode;
        private System.Windows.Forms.TableLayoutPanel tableLayoutFoot;
        private System.Windows.Forms.Label lblFootDownload;
        private System.Windows.Forms.PictureBox imgFootDownload;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox imgFootYellowBtn;
        private System.Windows.Forms.Panel panelInit;
        private System.Windows.Forms.Panel panelKO;
        private System.Windows.Forms.Label lblStatusKO;
        private Exit.V4.PlateOrPhone plateOrPhone1;
        private System.Windows.Forms.Button btnGoBack;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblInitMsg;
        private System.Windows.Forms.TextBox txtInputCode;
        private System.Windows.Forms.Panel panelFoot;
        private System.Windows.Forms.TableLayoutPanel tableLayoutFoot2;
        private System.Windows.Forms.PictureBox imgFootLogo2;
        private System.Windows.Forms.PictureBox imgFootLogoPango2;
        private System.Windows.Forms.Label lblFootDate2;
        private System.Windows.Forms.PictureBox imgFootBtn24h;
    }
}

