namespace AA.Pango.App.Exit.V3
{
    partial class PlateOrPhone
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnPlate = new System.Windows.Forms.Button();
            this.btnPhone = new System.Windows.Forms.Button();
            this.labelError = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.btnPlate, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnPhone, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelError, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1600, 464);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // btnPlate
            // 
            this.btnPlate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(99)))), ((int)(((byte)(41)))));
            this.btnPlate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPlate.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.btnPlate.FlatAppearance.BorderSize = 2;
            this.btnPlate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlate.Font = new System.Drawing.Font("Calibri", 40F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPlate.ForeColor = System.Drawing.Color.White;
            this.btnPlate.Location = new System.Drawing.Point(250, 17);
            this.btnPlate.Margin = new System.Windows.Forms.Padding(250, 17, 150, 17);
            this.btnPlate.Name = "btnPlate";
            this.btnPlate.Size = new System.Drawing.Size(400, 430);
            this.btnPlate.TabIndex = 0;
            this.btnPlate.Text = "PlateOrPhone_Plate";
            this.btnPlate.UseVisualStyleBackColor = false;
            this.btnPlate.Click += new System.EventHandler(this.btnPlate_Click);
            // 
            // btnPhone
            // 
            this.btnPhone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(117)))), ((int)(((byte)(182)))));
            this.btnPhone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPhone.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.btnPhone.FlatAppearance.BorderSize = 2;
            this.btnPhone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPhone.Font = new System.Drawing.Font("Calibri", 40F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPhone.ForeColor = System.Drawing.Color.White;
            this.btnPhone.Location = new System.Drawing.Point(950, 17);
            this.btnPhone.Margin = new System.Windows.Forms.Padding(150, 17, 250, 17);
            this.btnPhone.Name = "btnPhone";
            this.btnPhone.Size = new System.Drawing.Size(400, 430);
            this.btnPhone.TabIndex = 1;
            this.btnPhone.Text = "PlateOrPhone_Phone";
            this.btnPhone.UseVisualStyleBackColor = false;
            this.btnPhone.Click += new System.EventHandler(this.btnPhone_Click);
            // 
            // labelError
            // 
            this.labelError.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelError.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.labelError, 2);
            this.labelError.Font = new System.Drawing.Font("Calibri", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelError.ForeColor = System.Drawing.Color.Red;
            this.labelError.Location = new System.Drawing.Point(3, 417);
            this.labelError.Name = "labelError";
            this.labelError.Size = new System.Drawing.Size(1594, 47);
            this.labelError.TabIndex = 2;
            this.labelError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PlateOrPhone
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(227)))), ((int)(((byte)(243)))));
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "PlateOrPhone";
            this.Size = new System.Drawing.Size(1600, 464);
            this.Load += new System.EventHandler(this.OnLoad);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnPlate;
        private System.Windows.Forms.Button btnPhone;
        private System.Windows.Forms.Label labelError;
    }
}
