namespace GsbVille
{
    partial class Praticien
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
            this.cblistpanier = new System.Windows.Forms.ComboBox();
            this.BtnannulPanier = new System.Windows.Forms.Button();
            this.BtncreerPanier = new System.Windows.Forms.Button();
            this.Lvlistepanier = new System.Windows.Forms.ListView();
            this.designation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.concentration = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.marque = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.prix = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // cblistpanier
            // 
            this.cblistpanier.FormattingEnabled = true;
            this.cblistpanier.Location = new System.Drawing.Point(12, 44);
            this.cblistpanier.Name = "cblistpanier";
            this.cblistpanier.Size = new System.Drawing.Size(776, 21);
            this.cblistpanier.TabIndex = 0;
            this.cblistpanier.SelectedIndexChanged += new System.EventHandler(this.cblistpanier_SelectedIndexChanged);
            // 
            // BtnannulPanier
            // 
            this.BtnannulPanier.Location = new System.Drawing.Point(416, 381);
            this.BtnannulPanier.Name = "BtnannulPanier";
            this.BtnannulPanier.Size = new System.Drawing.Size(372, 45);
            this.BtnannulPanier.TabIndex = 1;
            this.BtnannulPanier.Text = "Annuler un Panier";
            this.BtnannulPanier.UseVisualStyleBackColor = true;
            this.BtnannulPanier.Click += new System.EventHandler(this.BtnannulPanier_Click);
            // 
            // BtncreerPanier
            // 
            this.BtncreerPanier.Location = new System.Drawing.Point(12, 381);
            this.BtncreerPanier.Name = "BtncreerPanier";
            this.BtncreerPanier.Size = new System.Drawing.Size(398, 45);
            this.BtncreerPanier.TabIndex = 2;
            this.BtncreerPanier.Text = "Créer un panier";
            this.BtncreerPanier.UseVisualStyleBackColor = true;
            this.BtncreerPanier.Click += new System.EventHandler(this.BtncreerPanier_Click);
            // 
            // Lvlistepanier
            // 
            this.Lvlistepanier.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.designation,
            this.marque,
            this.concentration,
            this.type,
            this.prix});
            this.Lvlistepanier.HideSelection = false;
            this.Lvlistepanier.Location = new System.Drawing.Point(12, 71);
            this.Lvlistepanier.Name = "Lvlistepanier";
            this.Lvlistepanier.Size = new System.Drawing.Size(776, 304);
            this.Lvlistepanier.TabIndex = 3;
            this.Lvlistepanier.UseCompatibleStateImageBehavior = false;
            this.Lvlistepanier.SelectedIndexChanged += new System.EventHandler(this.Lvlistepanier_SelectedIndexChanged);
            // 
            // type
            // 
            this.type.DisplayIndex = 1;
            // 
            // marque
            // 
            this.marque.DisplayIndex = 3;
            // 
            // Praticien
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.Lvlistepanier);
            this.Controls.Add(this.BtncreerPanier);
            this.Controls.Add(this.BtnannulPanier);
            this.Controls.Add(this.cblistpanier);
            this.Name = "Praticien";
            this.Text = "Praticien";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cblistpanier;
        private System.Windows.Forms.Button BtnannulPanier;
        private System.Windows.Forms.Button BtncreerPanier;
        private System.Windows.Forms.ListView Lvlistepanier;
        private System.Windows.Forms.ColumnHeader designation;
        private System.Windows.Forms.ColumnHeader marque;
        private System.Windows.Forms.ColumnHeader concentration;
        private System.Windows.Forms.ColumnHeader type;
        private System.Windows.Forms.ColumnHeader prix;
    }
}