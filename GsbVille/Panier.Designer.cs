namespace GsbVille
{
    partial class Panier
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
            this.lvPanier = new System.Windows.Forms.ListView();
            this.cbProduit = new System.Windows.Forms.ComboBox();
            this.btProd = new System.Windows.Forms.Button();
            this.btValider = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // lvPanier
            // 
            this.lvPanier.HideSelection = false;
            this.lvPanier.Location = new System.Drawing.Point(13, 34);
            this.lvPanier.Name = "lvPanier";
            this.lvPanier.Size = new System.Drawing.Size(486, 404);
            this.lvPanier.TabIndex = 0;
            this.lvPanier.UseCompatibleStateImageBehavior = false;
            this.lvPanier.SelectedIndexChanged += new System.EventHandler(this.lvPanier_SelectedIndexChanged);
            // 
            // cbProduit
            // 
            this.cbProduit.FormattingEnabled = true;
            this.cbProduit.Location = new System.Drawing.Point(505, 34);
            this.cbProduit.Name = "cbProduit";
            this.cbProduit.Size = new System.Drawing.Size(283, 21);
            this.cbProduit.TabIndex = 1;
            this.cbProduit.SelectedIndexChanged += new System.EventHandler(this.cbProduit_SelectedIndexChanged);
            // 
            // btProd
            // 
            this.btProd.Location = new System.Drawing.Point(505, 87);
            this.btProd.Name = "btProd";
            this.btProd.Size = new System.Drawing.Size(283, 167);
            this.btProd.TabIndex = 2;
            this.btProd.Text = "Ajouter Produit";
            this.btProd.UseVisualStyleBackColor = true;
            this.btProd.Click += new System.EventHandler(this.btProd_Click);
            // 
            // btValider
            // 
            this.btValider.Location = new System.Drawing.Point(505, 260);
            this.btValider.Name = "btValider";
            this.btValider.Size = new System.Drawing.Size(283, 178);
            this.btValider.TabIndex = 3;
            this.btValider.Text = "Valider Panier";
            this.btValider.UseVisualStyleBackColor = true;
            this.btValider.Click += new System.EventHandler(this.btValider_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(505, 61);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(283, 20);
            this.numericUpDown1.TabIndex = 4;
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // Panier
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.btValider);
            this.Controls.Add(this.btProd);
            this.Controls.Add(this.cbProduit);
            this.Controls.Add(this.lvPanier);
            this.Name = "Panier";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvPanier;
        private System.Windows.Forms.ComboBox cbProduit;
        private System.Windows.Forms.Button btProd;
        private System.Windows.Forms.Button btValider;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
    }
}