namespace AA.Pango.App.Exit.V4
{
    partial class PaymentOptions
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
            this.panel = new System.Windows.Forms.TableLayoutPanel();
            this.btnPango = new System.Windows.Forms.Button();
            this.btnLG1 = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // 
            // panel
            // 
            this.panel.ColumnCount = 2;
            this.panel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.panel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.panel.Controls.Add(this.btnPango, 0, 0);
            this.panel.Controls.Add(this.btnLG1, 1, 0);
            //this.panel.Controls.Add(this.labelError, 0, 1);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Name = "panel";
            this.panel.RowCount = 2;
            this.panel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.panel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.panel.Size = new System.Drawing.Size(1600, 464);
            this.panel.TabIndex = 0;
            // 
            // btnPango
            // 
            this.btnPango.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(99)))), ((int)(((byte)(41)))));
            this.btnPango.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPango.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.btnPango.FlatAppearance.BorderSize = 2;
            this.btnPango.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPango.Font = new System.Drawing.Font("Calibri", 40F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPango.ForeColor = System.Drawing.Color.White;
            this.btnPango.Location = new System.Drawing.Point(250, 17);
            this.btnPango.Margin = new System.Windows.Forms.Padding(250, 17, 150, 17);
            this.btnPango.Name = "btnPango";
            this.btnPango.Size = new System.Drawing.Size(400, 430);
            this.btnPango.TabIndex = 0;
            this.btnPango.Text = "PaymentOptions_Pango";
            this.btnPango.UseVisualStyleBackColor = false;
            this.btnPango.Click += new System.EventHandler(this.btnPango_Click);
            // 
            // btnLG1
            // 
            this.btnLG1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(117)))), ((int)(((byte)(182)))));
            this.btnLG1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLG1.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.btnLG1.FlatAppearance.BorderSize = 2;
            this.btnLG1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLG1.Font = new System.Drawing.Font("Calibri", 40F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLG1.ForeColor = System.Drawing.Color.White;
            this.btnLG1.Location = new System.Drawing.Point(950, 17);
            this.btnLG1.Margin = new System.Windows.Forms.Padding(150, 17, 250, 17);
            this.btnLG1.Name = "btnLG1";
            this.btnLG1.Size = new System.Drawing.Size(400, 430);
            this.btnLG1.TabIndex = 1;
            this.btnLG1.Text = "PaymentOptions_LG1";
            this.btnLG1.UseVisualStyleBackColor = false;
            this.btnLG1.Click += new System.EventHandler(this.btnLG1_Click);
            // 
            // PaymentOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(227)))), ((int)(((byte)(243)))));
            this.Controls.Add(this.panel);
            this.Name = "PaymentOptions";
            this.Size = new System.Drawing.Size(1600, 464);
            this.Load += new System.EventHandler(this.OnLoad);
            this.panel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel panel;
        private System.Windows.Forms.Button btnPango;
        private System.Windows.Forms.Button btnLG1;
    }
}
