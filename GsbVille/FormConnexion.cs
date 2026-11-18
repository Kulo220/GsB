using GsbVille;
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

namespace Mission1AP3
{
    public partial class FormConnexion : Form
    {
        BaseDeDonnees maBase;

        public FormConnexion()
        {
            InitializeComponent();
        }

        private void BtnConnexion_Click(object sender, EventArgs e)
        {
            //à partir des champs identifiant et mot de passe, on se connecte sur la base gsb_praticien en local
            //vous pouvez paramétrer la chaine de connexion ici.
            //appel du construction de la classe BaseDeDonnees.
            // identifiants enregistrés en minuscules : "ADesgranges-Lentz" ou "adesgranges-lentz" marchent tous les deux
            string pseudo = textIdentifiant.Text.Trim().ToLower();
            string mdp = textMotDePasse.Text;
            maBase = new BaseDeDonnees("172.23.48.3", "gsb_praticien", "3306", "adrien", "sio");

            UtilisateurConnecte utilisateur = maBase.Connexion(pseudo, mdp); // null si identifiants faux
            if (utilisateur == null)
            {
                MessageBox.Show("Identifiant ou mot de passe incorrect.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("connexion établie");

            // le rôle vient de la base, plus du pseudo tapé
            if (utilisateur.Role == "admin")
            {
                Admin a = new Admin(maBase); // l'admin a besoin de la connexion pour gérer les commandes
                a.ShowDialog();
            }
            else if (utilisateur.IdPraticien.HasValue)
            {
                // on transmet la connexion et l'id du praticien pour gérer SON panier
                Praticien p = new Praticien(maBase, utilisateur.IdPraticien.Value);
                p.ShowDialog();
            }
            else
            {
                MessageBox.Show("Ce compte n'est relié à aucun praticien.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void FormConnexion_Load(object sender, EventArgs e)
        {

        }
    }
}