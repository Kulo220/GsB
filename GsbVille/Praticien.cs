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
    public partial class Praticien : Form
    {
        private BaseDeDonnees maBase;
        private int idPraticien;

        // Rafraîchissement automatique toutes les 10 secondes
        private System.Windows.Forms.Timer minuteur;
        private bool rafraichissementEnCours = false;

        // Vrai pendant qu'on recharge cblistpanier : on ignore alors SelectedIndexChanged
        private bool chargementListe = false;

        public Praticien(BaseDeDonnees uneBase, int unIdPraticien)
        {
            InitializeComponent();
            maBase = uneBase;
            idPraticien = unIdPraticien;

            // cblistpanier : "Panier en cours" + les commandes envoyées par le praticien (choix uniquement, pas de saisie)
            cblistpanier.DropDownStyle = ComboBoxStyle.DropDownList;

            // Lvlistepanier : contenu de ce qui est choisi dans cblistpanier
            maBase.configurerListePanier(Lvlistepanier);

            RafraichirTout();

            // Minuteur : appelle minuteur_Tick toutes les 10 000 ms
            minuteur = new System.Windows.Forms.Timer();
            minuteur.Interval = 10000;
            minuteur.Tick += minuteur_Tick;
            minuteur.Start();

            // On arrête le minuteur quand le formulaire se ferme
            this.FormClosed += Praticien_FormClosed;
        }

        // Recharge la liste des commandes (les statuts ont pu changer) en gardant le choix actuel
        private void RechargerListeCommandes()
        {
            object selection = cblistpanier.SelectedValue;

            chargementListe = true;
            try
            {
                maBase.chargerCommandesPraticien(cblistpanier, idPraticien);
                if (selection != null) cblistpanier.SelectedValue = selection;
                if (cblistpanier.SelectedIndex < 0 && cblistpanier.Items.Count > 0)
                    cblistpanier.SelectedIndex = 0; // la commande choisie n'existe plus : retour au panier
            }
            finally
            {
                chargementListe = false;
            }
        }

        // Affiche dans Lvlistepanier le panier en cours ou la commande choisie
        private void AfficherSelection()
        {
            CommandeChoix choix = cblistpanier.SelectedItem as CommandeChoix;

            if (choix == null || choix.Id == 0)
            {
                // Panier en cours : prix actuels
                maBase.chargerPanier(Lvlistepanier, idPraticien);
            }
            else
            {
                // Commande envoyée : prix figés le jour de l'envoi
                maBase.chargerLignesCommande(Lvlistepanier, choix.Id, idPraticien);
            }
        }

        private void RafraichirTout()
        {
            RechargerListeCommandes();
            AfficherSelection();
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
                // Liste des commandes seulement si elle n'est pas ouverte (sinon elle se fermerait sous les yeux du praticien)
                if (!cblistpanier.DroppedDown)
                {
                    RechargerListeCommandes();
                }
                AfficherSelection();
            }
            finally
            {
                rafraichissementEnCours = false;
            }
        }

        private void Praticien_FormClosed(object sender, FormClosedEventArgs e)
        {
            minuteur.Stop();
            minuteur.Dispose();
        }

        private void BtncreerPanier_Click(object sender, EventArgs e)
        {
            // Ouvre le panier en cours s'il existe (on le reprend), sinon il sera créé au premier ajout
            Panier fenetrePanier = new Panier(maBase, idPraticien);

            // À chaque ajout / suppression dans la fenêtre Panier, on met à jour ce formulaire tout de suite
            fenetrePanier.PanierModifie += (s, ev) => RafraichirTout();

            fenetrePanier.ShowDialog();
            RafraichirTout();
        }

        // Annule ce qui est choisi dans cblistpanier : le panier en cours ou une commande en attente
        private void BtnannulPanier_Click(object sender, EventArgs e)
        {
            CommandeChoix choix = cblistpanier.SelectedItem as CommandeChoix;
            if (choix == null)
            {
                MessageBox.Show("Choisis un panier ou une commande dans la liste.", "Panier",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ---- Cas 1 : le panier en cours ----
            if (choix.Id == 0)
            {
                DialogResult reponsePanier = MessageBox.Show("Supprimer le panier en cours ?\n"
                    + "Les quantités seront remises en stock.", "Panier",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (reponsePanier != DialogResult.Yes) return;

                if (maBase.AnnulerPanier(idPraticien))
                {
                    MessageBox.Show("Panier supprimé.", "Panier", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RafraichirTout();
                }
                else
                {
                    MessageBox.Show("Aucun panier en cours.", "Panier", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            // ---- Cas 2 : une commande envoyée ----
            // Seule une commande encore en attente peut être annulée
            if (choix.Statut != "en_attente")
            {
                MessageBox.Show("La commande N°" + choix.Id + " est "
                    + BaseDeDonnees.LibelleStatut(choix.Statut).ToLower()
                    + " : elle ne peut plus être annulée.", "Commande",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult reponse = MessageBox.Show("Annuler la commande N°" + choix.Id + " ?\n"
                + "Elle sera supprimée et les quantités remises en stock.", "Commande",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (reponse != DialogResult.Yes) return;

            if (maBase.AnnulerCommande(idPraticien, choix.Id))
            {
                MessageBox.Show("Commande N°" + choix.Id + " annulée.", "Commande",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Un admin l'a acceptée ou refusée entre-temps
                MessageBox.Show("Impossible d'annuler la commande N°" + choix.Id + " : "
                    + "elle vient d'être traitée par un administrateur.", "Commande",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            RafraichirTout(); // la commande supprimée disparaît de la liste, retour sur "Panier en cours"
        }

        private void cblistpanier_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (chargementListe) return; // rechargement en cours : on ne fait rien
            AfficherSelection();
        }

        private void Lvlistepanier_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Affichage seulement.
        }
    }
}