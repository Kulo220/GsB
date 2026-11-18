using Mission1AP3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GsbVille
{
    public partial class Panier : Form
    {
        private BaseDeDonnees maBase;
        private int idPraticien;

        // Prévient le formulaire Praticien à chaque ajout / suppression,
        // pour qu'il mette à jour Lvlistepanier et le stock affiché dans cblistpanier.
        public event EventHandler PanierModifie;

        public Panier(BaseDeDonnees uneBase, int unIdPraticien)
        {
            InitializeComponent();
            maBase = uneBase;
            idPraticien = unIdPraticien;

            // Liste déroulante : on peut taper le début du nom pour chercher parmi les médicaments
            cbProduit.DropDownStyle = ComboBoxStyle.DropDown;
            cbProduit.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cbProduit.AutoCompleteSource = AutoCompleteSource.ListItems;

            // Quantité à commander : le NumericUpDown du concepteur (on peut taper 450 directement)
            numericUpDown1.Minimum = 1;
            numericUpDown1.Maximum = 999;
            numericUpDown1.Value = 1;

            // ListView affichée comme un tableau
            maBase.configurerListePanier(lvPanier);

            // Touche Suppr pour retirer la ligne sélectionnée
            lvPanier.KeyDown += lvPanier_KeyDown;

            maBase.chargerProduits(cbProduit);
            RafraichirPanier();
        }

        // Recharge le panier depuis la base
        private void RafraichirPanier()
        {
            maBase.chargerPanier(lvPanier, idPraticien);
            btValider.Enabled = lvPanier.Items.Count > 0;
        }

        // Recharge la liste des médicaments (le stock a changé) en gardant le même sélectionné
        private void RechargerProduits(int idASelectionner)
        {
            maBase.chargerProduits(cbProduit);
            cbProduit.SelectedValue = idASelectionner;
        }

        private void lvPanier_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Cliquer sur une ligne remet ce médicament dans la liste déroulante :
            // un clic sur le bouton d'ajout en rajoute.
            if (lvPanier.SelectedItems.Count == 0) return;

            ListViewItem ligne = lvPanier.SelectedItems[0];
            if (ligne.Tag == null) return; // ligne TOTAL

            cbProduit.SelectedValue = (int)ligne.Tag;
        }

        private void lvPanier_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete || lvPanier.SelectedItems.Count == 0) return;

            ListViewItem ligne = lvPanier.SelectedItems[0];
            if (ligne.Tag == null) return; // on ne supprime pas la ligne TOTAL

            int idMedicament = (int)ligne.Tag;
            DialogResult reponse = MessageBox.Show("Retirer " + ligne.Text + " du panier ?\n"
                + "La quantité sera remise en stock.", "Panier",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (reponse != DialogResult.Yes) return;

            if (maBase.RetirerDuPanier(idPraticien, idMedicament))
            {
                RafraichirPanier();
                RechargerProduits(idMedicament);   // le stock affiché a changé
                PanierModifie?.Invoke(this, EventArgs.Empty);
            }
        }

        private void btProd_Click(object sender, EventArgs e)
        {
            // Le médicament choisi dans la liste (null si rien n'est sélectionné)
            MedicamentChoix produit = cbProduit.SelectedItem as MedicamentChoix;

            // Si on a tapé un texte qui ne correspond à aucun médicament, on refuse
            if (produit == null || cbProduit.Text != produit.Libelle)
            {
                MessageBox.Show("Choisis un médicament dans la liste.", "Panier",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int quantite = (int)numericUpDown1.Value;

            // Retire la quantité du stock et l'ajoute au panier (refusé si stock insuffisant)
            if (maBase.AjouterAuPanier(idPraticien, produit.Id, quantite))
            {
                RafraichirPanier();
                RechargerProduits(produit.Id);     // le stock affiché a changé
                numericUpDown1.Value = 1;
                PanierModifie?.Invoke(this, EventArgs.Empty);
            }
        }

        private void cbProduit_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Rien à faire ici : l'ajout se fait au clic sur le bouton.
        }

        private void btValider_Click(object sender, EventArgs e)
        {
            if (lvPanier.Items.Count == 0)
            {
                MessageBox.Show("Le panier est vide.", "Panier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult reponse = MessageBox.Show("Envoyer la commande ?\n"
                + "Elle devra ensuite être acceptée par un administrateur.", "Panier",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (reponse != DialogResult.Yes) return;

            decimal? montant = maBase.ValiderPanier(idPraticien);
            if (montant.HasValue)
            {
                PanierModifie?.Invoke(this, EventArgs.Empty);
                MessageBox.Show("Commande envoyée (montant : " + montant.Value.ToString("N2") + " €).\n"
                    + "Elle est en attente de validation par un administrateur.",
                    "Commande", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}