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
    public partial class Admin : Form
    {
        private BaseDeDonnees maBase;

        // Rafraîchissement automatique toutes les 10 secondes (nouvelles commandes, commandes annulées)
        private System.Windows.Forms.Timer minuteur;
        private bool rafraichissementEnCours = false;

        // Vrai pendant qu'on recharge cbCommandes : on ignore alors SelectedIndexChanged
        private bool chargementListe = false;

        public Admin(BaseDeDonnees uneBase)
        {
            InitializeComponent();
            maBase = uneBase;

            // cbCommandes : uniquement les commandes en attente (choix uniquement, pas de saisie)
            cbCommandes.DropDownStyle = ComboBoxStyle.DropDownList;

            // lvListCom (une ListBox) : détail de la commande choisie.
            // Police à largeur fixe pour que les colonnes de texte restent alignées.
            lvListCom.Font = new Font("Consolas", 9);

            RafraichirTout();

            // Minuteur : appelle minuteur_Tick toutes les 10 000 ms
            minuteur = new System.Windows.Forms.Timer();
            minuteur.Interval = 10000;
            minuteur.Tick += minuteur_Tick;
            minuteur.Start();

            // On arrête le minuteur quand le formulaire se ferme
            this.FormClosed += Admin_FormClosed;
        }

        // Recharge la liste des commandes en attente en gardant la commande choisie si elle y est encore
        private void RechargerCommandes()
        {
            object selection = cbCommandes.SelectedValue;

            chargementListe = true;
            try
            {
                maBase.chargerCommandesEnAttente(cbCommandes);
                if (selection != null) cbCommandes.SelectedValue = selection;
                // La commande choisie n'est plus en attente (acceptée, refusée ou annulée) : on prend la première
                if (cbCommandes.SelectedIndex < 0 && cbCommandes.Items.Count > 0)
                    cbCommandes.SelectedIndex = 0;
            }
            finally
            {
                chargementListe = false;
            }
        }

        // Affiche le détail de la commande choisie et active / désactive les boutons
        private void AfficherDetail()
        {
            CommandeChoix choix = cbCommandes.SelectedItem as CommandeChoix;

            if (choix == null)
            {
                // Aucune commande en attente : rien à afficher, rien à accepter ni refuser
                lvListCom.Items.Clear();
                btnAccPanier.Enabled = false;
                btnRefPanier.Enabled = false;
                return;
            }

            // null : l'admin peut voir les commandes de tous les praticiens
            maBase.chargerLignesCommande(lvListCom, choix.Id, null);
            btnAccPanier.Enabled = true;
            btnRefPanier.Enabled = true;
        }

        private void RafraichirTout()
        {
            RechargerCommandes();
            AfficherDetail();
        }

        private void minuteur_Tick(object sender, EventArgs e)
        {
            // Évite qu'un rafraîchissement démarre pendant qu'un autre n'est pas fini
            if (rafraichissementEnCours) return;

            // Plus de connexion : on arrête, sinon un message d'erreur s'afficherait toutes les 10 s
            if (!maBase.EstConnectee())
            {
                minuteur.Stop();
                return;
            }

            rafraichissementEnCours = true;
            try
            {
                // Liste seulement si elle n'est pas ouverte (sinon elle se fermerait sous les yeux de l'admin)
                if (!cbCommandes.DroppedDown)
                {
                    RechargerCommandes();
                }
                AfficherDetail();
            }
            finally
            {
                rafraichissementEnCours = false;
            }
        }

        private void Admin_FormClosed(object sender, FormClosedEventArgs e)
        {
            minuteur.Stop();
            minuteur.Dispose();
        }

        private void cbCommandes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (chargementListe) return; // rechargement en cours : on ne fait rien
            AfficherDetail();
        }

        private void lvListCom_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Affichage seulement.
        }

        private void btnAccPanier_Click(object sender, EventArgs e)
        {
            CommandeChoix choix = cbCommandes.SelectedItem as CommandeChoix;
            if (choix == null)
            {
                MessageBox.Show("Aucune commande en attente sélectionnée.", "Commande",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult reponse = MessageBox.Show("Accepter la commande N°" + choix.Id + " ?", "Commande",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (reponse != DialogResult.Yes) return;

            // La base n'accepte que si la commande est ENCORE en attente
            if (maBase.AccepterCommande(choix.Id))
            {
                MessageBox.Show("Commande N°" + choix.Id + " acceptée.", "Commande",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("La commande N°" + choix.Id + " n'est plus en attente : "
                    + "elle a été annulée par le praticien ou déjà traitée.", "Commande",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            RafraichirTout(); // la commande traitée disparaît de la liste
        }

        private void btnRefPanier_Click(object sender, EventArgs e)
        {
            CommandeChoix choix = cbCommandes.SelectedItem as CommandeChoix;
            if (choix == null)
            {
                MessageBox.Show("Aucune commande en attente sélectionnée.", "Commande",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult reponse = MessageBox.Show("Refuser la commande N°" + choix.Id + " ?\n"
                + "Les quantités seront remises en stock.", "Commande",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (reponse != DialogResult.Yes) return;

            // La base ne refuse que si la commande est ENCORE en attente (et rend alors le stock)
            if (maBase.RefuserCommande(choix.Id))
            {
                MessageBox.Show("Commande N°" + choix.Id + " refusée. Les quantités ont été remises en stock.",
                    "Commande", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("La commande N°" + choix.Id + " n'est plus en attente : "
                    + "elle a été annulée par le praticien ou déjà traitée.", "Commande",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            RafraichirTout(); // la commande traitée disparaît de la liste
        }
    }
}