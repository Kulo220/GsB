namespace GsbVille
{
    partial class Admin
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
            this.cbCommandes = new System.Windows.Forms.ComboBox();
            this.lvListCom = new System.Windows.Forms.ListBox();
            this.btnAccPanier = new System.Windows.Forms.Button();
            this.btnRefPanier = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbCommandes
            // 
            this.cbCommandes.FormattingEnabled = true;
            this.cbCommandes.Location = new System.Drawing.Point(12, 12);
            this.cbCommandes.Name = "cbCommandes";
            this.cbCommandes.Size = new System.Drawing.Size(776, 21);
            this.cbCommandes.TabIndex = 0;
            this.cbCommandes.SelectedIndexChanged += new System.EventHandler(this.cbCommandes_SelectedIndexChanged);
            // 
            // lvListCom
            // 
            this.lvListCom.FormattingEnabled = true;
            this.lvListCom.Location = new System.Drawing.Point(12, 39);
            this.lvListCom.Name = "lvListCom";
            this.lvListCom.Size = new System.Drawing.Size(776, 329);
            this.lvListCom.TabIndex = 1;
            this.lvListCom.SelectedIndexChanged += new System.EventHandler(this.lvListCom_SelectedIndexChanged);
            // 
            // btnAccPanier
            // 
            this.btnAccPanier.Location = new System.Drawing.Point(12, 374);
            this.btnAccPanier.Name = "btnAccPanier";
            this.btnAccPanier.Size = new System.Drawing.Size(385, 64);
            this.btnAccPanier.TabIndex = 2;
            this.btnAccPanier.Text = "Accepter Panier";
            this.btnAccPanier.UseVisualStyleBackColor = true;
            this.btnAccPanier.Click += new System.EventHandler(this.btnAccPanier_Click);
            // 
            // btnRefPanier
            // 
            this.btnRefPanier.Location = new System.Drawing.Point(403, 374);
            this.btnRefPanier.Name = "btnRefPanier";
            this.btnRefPanier.Size = new System.Drawing.Size(385, 64);
            this.btnRefPanier.TabIndex = 3;
            this.btnRefPanier.Text = "Refuser Panier";
            this.btnRefPanier.UseVisualStyleBackColor = true;
            this.btnRefPanier.Click += new System.EventHandler(this.btnRefPanier_Click);
            // 
            // Admin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnRefPanier);
            this.Controls.Add(this.btnAccPanier);
            this.Controls.Add(this.lvListCom);
            this.Controls.Add(this.cbCommandes);
            this.Name = "Admin";
            this.Text = "Admin";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cbCommandes;
        private System.Windows.Forms.ListBox lvListCom;
        private System.Windows.Forms.Button btnAccPanier;
        private System.Windows.Forms.Button btnRefPanier;
    }
}