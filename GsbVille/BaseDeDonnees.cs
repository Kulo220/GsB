using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mission1AP3
{
    public class BaseDeDonnees
    {
        //classe métier centrée les échanges avec la base de données.

        //Paramètre de classe sur la connexion
        private MySqlConnection laConnexion;

        public BaseDeDonnees(string nomServeur, string nomBDD, string numeroPort, string nomUtilisateur, string MDP)
        {
            try
            {
                string maChaineConnexion = "server=" + nomServeur + ";database=" + nomBDD + ";port=" + numeroPort
                    + ";uid=" + nomUtilisateur + ";pwd=" + MDP
                    + ";SslMode=Disabled;AllowPublicKeyRetrieval=true";

                laConnexion = new MySqlConnection(maChaineConnexion);
                laConnexion.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur de connexion à la BDD : " + ex.Message);
                laConnexion = null;
            }
        }

        public bool EstConnectee()
        {
            return laConnexion != null && laConnexion.State == System.Data.ConnectionState.Open;
        }

        // Vérifie l'identifiant et le mot de passe.
        // Renvoie l'utilisateur connecté (rôle + id du praticien), ou null si les identifiants sont faux.
        public UtilisateurConnecte Connexion(string identifiant, string motDePasse)
        {
            if (!EstConnectee()) return null;

            try
            {
                string maRequete = "SELECT id, nom, role, id_praticien FROM utilisateur "
                                 + "WHERE nom = @identifiant AND mdp = @motDePasse";
                MySqlCommand commande = new MySqlCommand(maRequete, laConnexion);
                commande.Parameters.AddWithValue("@identifiant", identifiant);
                commande.Parameters.AddWithValue("@motDePasse", motDePasse);

                // "using" ferme le reader même en cas d'erreur (sinon la connexion reste bloquée)
                using (MySqlDataReader lecteur = commande.ExecuteReader())
                {
                    if (!lecteur.Read())
                    {
                        return null; // aucun compte avec ce nom et ce mot de passe
                    }

                    UtilisateurConnecte utilisateur = new UtilisateurConnecte();
                    utilisateur.Id = lecteur.GetInt32("id");
                    utilisateur.Nom = lecteur.GetString("nom");
                    utilisateur.Role = lecteur.GetString("role");

                    // id_praticien est NULL pour l'admin
                    int colonnePraticien = lecteur.GetOrdinal("id_praticien");
                    if (lecteur.IsDBNull(colonnePraticien))
                        utilisateur.IdPraticien = null;
                    else
                        utilisateur.IdPraticien = lecteur.GetInt32(colonnePraticien);

                    return utilisateur;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }



        public void chargerVille(ListBox uneListe, int nbMaxVille)
        {
            //méthode permettant de construire une Liste de Ville à partir des données récupérées depuis la base de données.
            try
            {
                //on utilise les outils MySQL pour effectuer les requêtes.
                string maRequete;
                MySqlCommand maCommande;
                MySqlDataReader mesResultats;
                SqlConnection conn;

                //on commence par netoyer la liste s'il y a des éléments déjà présent.
                uneListe.Items.Clear();

                uneListe.Items.Add("id slug nom code_postal  population_2012 surface");

                maRequete = "SELECT id, slug, nom, code_postal, population_2012, surface FROM ville Limit " + nbMaxVille;

                maCommande = new MySqlCommand(maRequete, laConnexion);

                mesResultats = maCommande.ExecuteReader();
                //pour chaque Ligne retournée par la requete , nous allons remplir la liste des Villes
                while (mesResultats.Read())
                {
                    uneListe.Items.Add(mesResultats.GetInt32("id") + " " + mesResultats.GetString("slug") + " " + mesResultats.GetString("nom") + " " + mesResultats.GetString("code_postal") + " " + mesResultats.GetInt32("population_2012").ToString() + " " + mesResultats.GetInt32("surface").ToString());
                }
                mesResultats.Close();
            }
            catch (System.Exception erreur)
            {
                uneListe.Items.Add("Erreur");
            }
        }

        public void chargerMedicaments(ListBox uneListe, int nbMax)
        {
            // Construit la liste des médicaments à partir de la base de données
            try
            {
                uneListe.Items.Clear();
                uneListe.Items.Add("id | désignation | marque | concentration | type | prix");

                string maRequete = "SELECT id, designation, marque, concentration, type, Prix "
                                 + "FROM medicament LIMIT @nbMax";

                MySqlCommand maCommande = new MySqlCommand(maRequete, laConnexion);
                maCommande.Parameters.AddWithValue("@nbMax", nbMax);

                // "using" ferme automatiquement le reader, même en cas d'erreur
                using (MySqlDataReader mesResultats = maCommande.ExecuteReader())
                {
                    while (mesResultats.Read())
                    {
                        // ToString() évite les plantages sur les cases vides (NULL)
                        uneListe.Items.Add(
                            mesResultats["id"].ToString() + " | " +
                            mesResultats["designation"].ToString() + " | " +
                            mesResultats["marque"].ToString() + " | " +
                            mesResultats["concentration"].ToString() + " | " +
                            mesResultats["type"].ToString() + " | " +
                            mesResultats["Prix"].ToString() + " €");
                    }
                }
            }
            catch (Exception erreur)
            {
                uneListe.Items.Add("Erreur : " + erreur.Message);
            }
        }


        // =====================================================================
        //  PANIER
        //  Un panier = une ligne de la table commande avec statut = 'panier'.
        //  Toutes les requêtes filtrent sur l'id du praticien connecté.
        //
        //  STOCK : il est retiré dès l'ajout au panier (stock - quantité),
        //  et rendu si on retire le médicament du panier ou si on annule le panier.
        //  Chaque opération est une transaction : stock et panier changent ensemble, ou pas du tout.
        // =====================================================================

        // Remplit une liste (ComboBox, ListBox...) avec tous les médicaments :
        // nom, forme, marque, prix et stock. Utilisée pour cbProduit et cblistpanier.
        public void chargerProduits(ListControl uneListe)
        {
            if (!EstConnectee()) return;

            try
            {
                List<MedicamentChoix> produits = new List<MedicamentChoix>();

                // les 4 médicaments sans désignation (id 4000 à 4003) sont exclus
                string maRequete = "SELECT id, designation, marque, concentration, type, Prix, stock "
                                 + "FROM medicament WHERE designation <> '' "
                                 + "ORDER BY designation, concentration";
                MySqlCommand maCommande = new MySqlCommand(maRequete, laConnexion);

                using (MySqlDataReader mesResultats = maCommande.ExecuteReader())
                {
                    while (mesResultats.Read())
                    {
                        MedicamentChoix produit = new MedicamentChoix();
                        produit.Id = Convert.ToInt32(mesResultats["id"]);
                        produit.Prix = Convert.ToDecimal(mesResultats["Prix"]);
                        produit.Stock = Convert.ToInt32(mesResultats["stock"]);
                        produit.Marque = mesResultats["marque"].ToString();

                        // ex : "ABIES CANADENSIS | BOIRON | 64,00 € | stock : 100"
                        string libelle = mesResultats["designation"].ToString();
                        if (mesResultats["concentration"].ToString() != "")
                            libelle += " " + mesResultats["concentration"].ToString();
                        if (mesResultats["type"].ToString() != "")
                            libelle += " - " + mesResultats["type"].ToString();
                        if (produit.Marque != "")
                            libelle += " | " + produit.Marque;
                        libelle += " | " + produit.Prix.ToString("N2") + " € | stock : " + produit.Stock;
                        produit.Libelle = libelle;

                        produits.Add(produit);
                    }
                }

                // DisplayMember = ce qu'on affiche, ValueMember = ce qu'on récupère (l'id)
                uneListe.DataSource = null;
                uneListe.DisplayMember = "Libelle";
                uneListe.ValueMember = "Id";
                uneListe.DataSource = produits;
                uneListe.SelectedIndex = -1;
            }
            catch (Exception erreur)
            {
                MessageBox.Show("Erreur de chargement des médicaments : " + erreur.Message);
            }
        }

        // Prépare une ListView pour afficher un panier (colonnes du tableau).
        // Utilisée pour lvPanier et Lvlistepanier.
        public void configurerListePanier(ListView uneListe)
        {
            uneListe.View = View.Details;
            uneListe.FullRowSelect = true;
            uneListe.MultiSelect = false;
            uneListe.Columns.Clear();
            uneListe.Columns.Add("Médicament", 200);
            uneListe.Columns.Add("Forme", 220);
            uneListe.Columns.Add("Qté", 50, HorizontalAlignment.Right);
            uneListe.Columns.Add("Prix unitaire", 90, HorizontalAlignment.Right);
            uneListe.Columns.Add("Sous-total", 90, HorizontalAlignment.Right);
        }

        // Remplit la ListView avec le panier en cours du praticien, plus une ligne TOTAL en gras.
        // Prix ACTUELS (lus dans medicament) : le panier n'est pas encore envoyé.
        public decimal chargerPanier(ListView uneListe, int idPraticien)
        {
            if (!EstConnectee()) { uneListe.Items.Clear(); return 0; }

            string maRequete = "SELECT m.id, m.designation, m.concentration, m.type, lc.quantite, m.Prix AS prix "
                             + "FROM commande c "
                             + "JOIN ligne_commande lc ON lc.id_commande = c.id "
                             + "JOIN medicament m ON m.id = lc.id_medicament "
                             + "WHERE c.id_praticien = @prat AND c.statut = 'panier' "
                             + "ORDER BY m.designation";
            MySqlCommand maCommande = new MySqlCommand(maRequete, laConnexion);
            maCommande.Parameters.AddWithValue("@prat", idPraticien);
            return remplirLignes(uneListe, maCommande);
        }

        // Remplit la ListView avec le contenu d'une commande envoyée, plus une ligne TOTAL en gras.
        // Prix FIGÉS (lus dans ligne_commande) : ceux du jour où la commande a été envoyée.
        // idPraticien : si renseigné, on vérifie que la commande lui appartient (null pour l'admin).
        public decimal chargerLignesCommande(ListView uneListe, int idCommande, int? idPraticien)
        {
            if (!EstConnectee()) { uneListe.Items.Clear(); return 0; }
            return remplirLignes(uneListe, requeteLignesCommande(idCommande, idPraticien));
        }

        // Même chose pour une ListBox (lvListCom du formulaire Admin) : une ligne de texte par médicament,
        // colonnes alignées grâce à une police à largeur fixe (Consolas) réglée dans le formulaire.
        public decimal chargerLignesCommande(ListBox uneListe, int idCommande, int? idPraticien)
        {
            uneListe.Items.Clear();
            decimal total = 0;
            if (!EstConnectee()) return total;

            try
            {
                uneListe.Items.Add("Médicament".PadRight(40) + "Qté".PadLeft(5)
                                 + "Prix unit.".PadLeft(13) + "Sous-total".PadLeft(14));
                uneListe.Items.Add(new string('-', 72));

                using (MySqlDataReader mesResultats = requeteLignesCommande(idCommande, idPraticien).ExecuteReader())
                {
                    while (mesResultats.Read())
                    {
                        int quantite = Convert.ToInt32(mesResultats["quantite"]);
                        decimal prix = Convert.ToDecimal(mesResultats["prix"]);
                        decimal sousTotal = quantite * prix;
                        total += sousTotal;

                        // nom + concentration, coupé à 38 caractères pour garder les colonnes alignées
                        string nom = (mesResultats["designation"].ToString() + " "
                                    + mesResultats["concentration"].ToString()).Trim();
                        if (nom.Length > 38) nom = nom.Substring(0, 38);

                        uneListe.Items.Add(nom.PadRight(40)
                                         + quantite.ToString().PadLeft(5)
                                         + (prix.ToString("N2") + " €").PadLeft(13)
                                         + (sousTotal.ToString("N2") + " €").PadLeft(14));
                    }
                }

                uneListe.Items.Add(new string('-', 72));
                uneListe.Items.Add("TOTAL".PadRight(58) + (total.ToString("N2") + " €").PadLeft(14));
            }
            catch (Exception erreur)
            {
                MessageBox.Show("Erreur de chargement des lignes : " + erreur.Message);
            }

            return total;
        }

        // Requête commune aux deux versions de chargerLignesCommande.
        // idPraticien : si renseigné, la commande doit lui appartenir (null pour l'admin).
        private MySqlCommand requeteLignesCommande(int idCommande, int? idPraticien)
        {
            string maRequete = "SELECT m.id, m.designation, m.concentration, m.type, lc.quantite, lc.prix_unitaire AS prix "
                             + "FROM commande c "
                             + "JOIN ligne_commande lc ON lc.id_commande = c.id "
                             + "JOIN medicament m ON m.id = lc.id_medicament "
                             + "WHERE c.id = @cmd AND c.statut <> 'panier' "
                             + "AND (@prat IS NULL OR c.id_praticien = @prat) "
                             + "ORDER BY m.designation";
            MySqlCommand maCommande = new MySqlCommand(maRequete, laConnexion);
            maCommande.Parameters.AddWithValue("@cmd", idCommande);
            maCommande.Parameters.AddWithValue("@prat", idPraticien.HasValue ? (object)idPraticien.Value : DBNull.Value);
            return maCommande;
        }

        // Partie commune aux deux méthodes ci-dessus : exécute la requête et remplit la ListView.
        // La requête doit renvoyer les colonnes id, designation, concentration, type, quantite, prix.
        private decimal remplirLignes(ListView uneListe, MySqlCommand maCommande)
        {
            uneListe.Items.Clear();
            decimal total = 0;

            try
            {
                using (MySqlDataReader mesResultats = maCommande.ExecuteReader())
                {
                    while (mesResultats.Read())
                    {
                        int quantite = Convert.ToInt32(mesResultats["quantite"]);
                        decimal prix = Convert.ToDecimal(mesResultats["prix"]);
                        decimal sousTotal = quantite * prix;
                        total += sousTotal;

                        string forme = (mesResultats["concentration"].ToString() + " "
                                      + mesResultats["type"].ToString()).Trim();

                        // 1re colonne = Text, colonnes suivantes = SubItems
                        ListViewItem ligne = new ListViewItem(mesResultats["designation"].ToString());
                        ligne.SubItems.Add(forme);
                        ligne.SubItems.Add(quantite.ToString());
                        ligne.SubItems.Add(prix.ToString("N2") + " €");
                        ligne.SubItems.Add(sousTotal.ToString("N2") + " €");
                        // on garde l'id du médicament "caché" dans la ligne, pour pouvoir la retirer
                        ligne.Tag = Convert.ToInt32(mesResultats["id"]);

                        uneListe.Items.Add(ligne);
                    }
                }

                if (uneListe.Items.Count > 0)
                {
                    ListViewItem ligneTotal = new ListViewItem("TOTAL");
                    ligneTotal.SubItems.Add("");
                    ligneTotal.SubItems.Add("");
                    ligneTotal.SubItems.Add("");
                    ligneTotal.SubItems.Add(total.ToString("N2") + " €");
                    ligneTotal.Font = new Font(uneListe.Font, FontStyle.Bold);
                    // Tag reste null : cette ligne n'est pas un médicament
                    uneListe.Items.Add(ligneTotal);
                }
            }
            catch (Exception erreur)
            {
                MessageBox.Show("Erreur de chargement des lignes : " + erreur.Message);
            }

            return total;
        }

        // Renvoie l'id du panier en cours du praticien, et le crée s'il n'existe pas encore.
        private int OuvrirPanier(int idPraticien, MySqlTransaction transaction)
        {
            MySqlCommand chercher = new MySqlCommand(
                "SELECT id FROM commande WHERE id_praticien = @prat AND statut = 'panier'",
                laConnexion, transaction);
            chercher.Parameters.AddWithValue("@prat", idPraticien);
            object resultat = chercher.ExecuteScalar();
            if (resultat != null)
            {
                return Convert.ToInt32(resultat);
            }

            MySqlCommand creer = new MySqlCommand(
                "INSERT INTO commande (id_praticien) VALUES (@prat)", laConnexion, transaction);
            creer.Parameters.AddWithValue("@prat", idPraticien);
            creer.ExecuteNonQuery();
            return (int)creer.LastInsertedId;
        }

        // Ajoute x unités d'un médicament au panier et retire x du stock.
        // S'il est déjà dans le panier, les quantités s'additionnent.
        public bool AjouterAuPanier(int idPraticien, int idMedicament, int quantite)
        {
            if (!EstConnectee()) return false;
            if (quantite <= 0)
            {
                MessageBox.Show("La quantité doit être supérieure à 0.");
                return false;
            }

            MySqlTransaction transaction = laConnexion.BeginTransaction();
            try
            {
                // 1. stock = stock - quantité, SEULEMENT s'il en reste assez
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE medicament SET stock = stock - @qte WHERE id = @med AND stock >= @qte",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@qte", quantite);
                cmd.Parameters.AddWithValue("@med", idMedicament);

                if (cmd.ExecuteNonQuery() == 0)
                {
                    // aucune ligne modifiée = stock insuffisant : on lit ce qu'il reste pour le dire
                    MySqlCommand lireStock = new MySqlCommand(
                        "SELECT stock FROM medicament WHERE id = @med", laConnexion, transaction);
                    lireStock.Parameters.AddWithValue("@med", idMedicament);
                    object stockRestant = lireStock.ExecuteScalar();

                    transaction.Rollback();
                    MessageBox.Show("Stock insuffisant : il reste " + stockRestant + " unité(s) de ce médicament.",
                        "Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // 2. Ajouter (ou cumuler) la ligne dans le panier
                int idPanier = OuvrirPanier(idPraticien, transaction);

                cmd = new MySqlCommand(
                    "INSERT INTO ligne_commande (id_commande, id_medicament, quantite) "
                    + "VALUES (@panier, @med, @qte) AS nouv "
                    + "ON DUPLICATE KEY UPDATE quantite = ligne_commande.quantite + nouv.quantite",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@panier", idPanier);
                cmd.Parameters.AddWithValue("@med", idMedicament);
                cmd.Parameters.AddWithValue("@qte", quantite);
                cmd.ExecuteNonQuery();

                // 3. Les deux ont marché : on enregistre
                transaction.Commit();
                return true;
            }
            catch (MySqlException erreur) when (erreur.Number == 3819)
            {
                // 3819 = contrainte CHECK violée (quantité entre 1 et 999). Le stock est rendu par le Rollback.
                try { transaction.Rollback(); } catch { }
                MessageBox.Show("La quantité d'un médicament dans le panier ne peut pas dépasser 999.");
                return false;
            }
            catch (Exception erreur)
            {
                try { transaction.Rollback(); } catch { }
                MessageBox.Show("Erreur lors de l'ajout au panier : " + erreur.Message);
                return false;
            }
        }

        // Retire un médicament du panier en cours et rend sa quantité au stock.
        public bool RetirerDuPanier(int idPraticien, int idMedicament)
        {
            if (!EstConnectee()) return false;

            MySqlTransaction transaction = laConnexion.BeginTransaction();
            try
            {
                // 1. Retrouver la ligne (et sa quantité) dans le panier en cours du praticien
                int idPanier = 0;
                int quantite = 0;
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT lc.id_commande, lc.quantite FROM ligne_commande lc "
                    + "JOIN commande c ON c.id = lc.id_commande "
                    + "WHERE c.id_praticien = @prat AND c.statut = 'panier' AND lc.id_medicament = @med "
                    + "FOR UPDATE",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@prat", idPraticien);
                cmd.Parameters.AddWithValue("@med", idMedicament);
                using (MySqlDataReader lecteur = cmd.ExecuteReader())
                {
                    if (lecteur.Read())
                    {
                        idPanier = Convert.ToInt32(lecteur["id_commande"]);
                        quantite = Convert.ToInt32(lecteur["quantite"]);
                    }
                }

                if (idPanier == 0)
                {
                    transaction.Rollback();
                    return false; // ce médicament n'est pas dans le panier
                }

                // 2. Supprimer la ligne
                cmd = new MySqlCommand(
                    "DELETE FROM ligne_commande WHERE id_commande = @panier AND id_medicament = @med",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@panier", idPanier);
                cmd.Parameters.AddWithValue("@med", idMedicament);
                cmd.ExecuteNonQuery();

                // 3. Rendre la quantité au stock
                cmd = new MySqlCommand(
                    "UPDATE medicament SET stock = stock + @qte WHERE id = @med",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@qte", quantite);
                cmd.Parameters.AddWithValue("@med", idMedicament);
                cmd.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch (Exception erreur)
            {
                try { transaction.Rollback(); } catch { }
                MessageBox.Show("Erreur lors du retrait : " + erreur.Message);
                return false;
            }
        }

        // Supprime le panier en cours et rend toutes ses quantités au stock.
        public bool AnnulerPanier(int idPraticien)
        {
            if (!EstConnectee()) return false;

            MySqlTransaction transaction = laConnexion.BeginTransaction();
            try
            {
                // 1. Trouver le panier
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT id FROM commande WHERE id_praticien = @prat AND statut = 'panier' FOR UPDATE",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@prat", idPraticien);
                object resultat = cmd.ExecuteScalar();
                if (resultat == null)
                {
                    transaction.Rollback();
                    return false; // pas de panier en cours
                }
                int idPanier = Convert.ToInt32(resultat);

                // 2. Rendre au stock la quantité de chaque ligne
                cmd = new MySqlCommand(
                    "UPDATE medicament m JOIN ligne_commande lc ON lc.id_medicament = m.id "
                    + "SET m.stock = m.stock + lc.quantite WHERE lc.id_commande = @panier",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@panier", idPanier);
                cmd.ExecuteNonQuery();

                // 3. Supprimer le panier (ses lignes partent avec, grâce au ON DELETE CASCADE)
                cmd = new MySqlCommand("DELETE FROM commande WHERE id = @panier", laConnexion, transaction);
                cmd.Parameters.AddWithValue("@panier", idPanier);
                cmd.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch (Exception erreur)
            {
                try { transaction.Rollback(); } catch { }
                MessageBox.Show("Erreur lors de l'annulation : " + erreur.Message);
                return false;
            }
        }

        // Annule une commande EN ATTENTE du praticien : quantités rendues au stock, puis commande supprimée.
        // Refusé (renvoie false) si la commande n'est pas à lui ou n'est plus en attente (déjà acceptée / refusée).
        public bool AnnulerCommande(int idPraticien, int idCommande)
        {
            if (!EstConnectee()) return false;

            MySqlTransaction transaction = laConnexion.BeginTransaction();
            try
            {
                // 1. Vérifier que la commande est à ce praticien et toujours en attente (et la verrouiller,
                //    pour qu'un admin ne puisse pas l'accepter pendant qu'on l'annule)
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT id FROM commande WHERE id = @cmd AND id_praticien = @prat "
                    + "AND statut = 'en_attente' FOR UPDATE",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@cmd", idCommande);
                cmd.Parameters.AddWithValue("@prat", idPraticien);
                if (cmd.ExecuteScalar() == null)
                {
                    transaction.Rollback();
                    return false;
                }

                // 2. Rendre au stock la quantité de chaque ligne
                cmd = new MySqlCommand(
                    "UPDATE medicament m JOIN ligne_commande lc ON lc.id_medicament = m.id "
                    + "SET m.stock = m.stock + lc.quantite WHERE lc.id_commande = @cmd",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@cmd", idCommande);
                cmd.ExecuteNonQuery();

                // 3. Supprimer la commande (ses lignes partent avec, grâce au ON DELETE CASCADE)
                cmd = new MySqlCommand("DELETE FROM commande WHERE id = @cmd", laConnexion, transaction);
                cmd.Parameters.AddWithValue("@cmd", idCommande);
                cmd.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch (Exception erreur)
            {
                try { transaction.Rollback(); } catch { }
                MessageBox.Show("Erreur lors de l'annulation de la commande : " + erreur.Message);
                return false;
            }
        }

        // Envoie le panier : prix figés, total calculé, statut 'en_attente'.
        // La commande attend ensuite qu'un administrateur l'accepte ou la refuse.
        // Le stock a déjà été retiré à chaque ajout : rien à faire dessus ici.
        // Renvoie le montant de la commande, ou null si rien n'a été envoyé.
        public decimal? ValiderPanier(int idPraticien)
        {
            if (!EstConnectee()) return null;

            MySqlTransaction transaction = laConnexion.BeginTransaction();
            try
            {
                // 1. Trouver le panier et le verrouiller le temps de la validation
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT id FROM commande WHERE id_praticien = @prat AND statut = 'panier' FOR UPDATE",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@prat", idPraticien);
                object resultat = cmd.ExecuteScalar();
                if (resultat == null)
                {
                    transaction.Rollback();
                    return null; // pas de panier
                }
                int idPanier = Convert.ToInt32(resultat);

                // 2. Refuser un panier vide
                cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM ligne_commande WHERE id_commande = @panier",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@panier", idPanier);
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                {
                    transaction.Rollback();
                    return null;
                }

                // 3. Figer les prix : on recopie le prix actuel dans chaque ligne
                cmd = new MySqlCommand(
                    "UPDATE ligne_commande lc JOIN medicament m ON m.id = lc.id_medicament "
                    + "SET lc.prix_unitaire = m.Prix WHERE lc.id_commande = @panier",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@panier", idPanier);
                cmd.ExecuteNonQuery();

                // 4. Calculer le total et passer la commande "en attente" d'un administrateur
                cmd = new MySqlCommand(
                    "UPDATE commande SET statut = 'en_attente', date_validation = NOW(), "
                    + "montant_total = (SELECT SUM(quantite * prix_unitaire) FROM ligne_commande WHERE id_commande = @panier) "
                    + "WHERE id = @panier",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@panier", idPanier);
                cmd.ExecuteNonQuery();

                // 5. Relire le montant pour l'afficher
                cmd = new MySqlCommand(
                    "SELECT montant_total FROM commande WHERE id = @panier",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@panier", idPanier);
                decimal montant = Convert.ToDecimal(cmd.ExecuteScalar());

                transaction.Commit();
                return montant;
            }
            catch (Exception erreur)
            {
                try { transaction.Rollback(); } catch { }
                MessageBox.Show("Erreur lors de la validation : " + erreur.Message);
                return null;
            }
        }


        // =====================================================================
        //  COMMANDES
        // =====================================================================

        // Statut tel qu'il est stocké en base -> texte affiché
        public static string LibelleStatut(string statut)
        {
            switch (statut)
            {
                case "panier": return "Panier en cours";
                case "en_attente": return "En attente";
                case "acceptee": return "Acceptée";
                case "refusee": return "Refusée";
                default: return statut;
            }
        }

        // Remplit la liste (cblistpanier) avec "Panier en cours" puis les commandes du praticien,
        // de la plus récente à la plus ancienne. ex : "N°12 du 16/10/2026 14:02 - 461,00 € - En attente"
        public void chargerCommandesPraticien(ListControl uneListe, int idPraticien)
        {
            if (!EstConnectee()) return;

            try
            {
                List<CommandeChoix> commandes = new List<CommandeChoix>();

                // 1er choix : le panier en cours (Id = 0, ce n'est pas encore une commande)
                CommandeChoix panier = new CommandeChoix();
                panier.Id = 0;
                panier.Statut = "panier";
                panier.Libelle = "Panier en cours";
                commandes.Add(panier);

                string maRequete = "SELECT id, statut, date_validation, montant_total FROM commande "
                                 + "WHERE id_praticien = @prat AND statut <> 'panier' "
                                 + "ORDER BY date_validation DESC";
                MySqlCommand maCommande = new MySqlCommand(maRequete, laConnexion);
                maCommande.Parameters.AddWithValue("@prat", idPraticien);

                using (MySqlDataReader mesResultats = maCommande.ExecuteReader())
                {
                    while (mesResultats.Read())
                    {
                        CommandeChoix commande = new CommandeChoix();
                        commande.Id = Convert.ToInt32(mesResultats["id"]);
                        commande.Statut = mesResultats["statut"].ToString();

                        string date = mesResultats["date_validation"] == DBNull.Value ? ""
                            : Convert.ToDateTime(mesResultats["date_validation"]).ToString("dd/MM/yyyy HH:mm");
                        decimal montant = mesResultats["montant_total"] == DBNull.Value ? 0
                            : Convert.ToDecimal(mesResultats["montant_total"]);

                        commande.Libelle = "N°" + commande.Id + " du " + date + " - "
                                         + montant.ToString("N2") + " € - " + LibelleStatut(commande.Statut);
                        commandes.Add(commande);
                    }
                }

                uneListe.DataSource = null;
                uneListe.DisplayMember = "Libelle";
                uneListe.ValueMember = "Id";
                uneListe.DataSource = commandes;
                uneListe.SelectedIndex = 0; // "Panier en cours" par défaut
            }
            catch (Exception erreur)
            {
                MessageBox.Show("Erreur de chargement des commandes : " + erreur.Message);
            }
        }


        // =====================================================================
        //  ADMINISTRATEUR : accepter / refuser les commandes en attente
        // =====================================================================

        // Remplit la liste (cbCommandes) avec les commandes EN ATTENTE uniquement,
        // de la plus ancienne à la plus récente (la première arrivée est traitée en premier).
        // ex : "N°10 du 16/10/2026 14:01 - Desgranges-Lentz Antoine (Médecine de Ville) - 1 760,00 €"
        public void chargerCommandesEnAttente(ListControl uneListe)
        {
            if (!EstConnectee()) return;

            try
            {
                List<CommandeChoix> commandes = new List<CommandeChoix>();

                string maRequete = "SELECT c.id, c.date_validation, c.montant_total, p.nom, p.prenom, tp.libelle "
                                 + "FROM commande c "
                                 + "JOIN praticien p ON p.id = c.id_praticien "
                                 + "JOIN type_praticien tp ON tp.code = p.code_type_praticien "
                                 + "WHERE c.statut = 'en_attente' "
                                 + "ORDER BY c.date_validation";
                MySqlCommand maCommande = new MySqlCommand(maRequete, laConnexion);

                using (MySqlDataReader mesResultats = maCommande.ExecuteReader())
                {
                    while (mesResultats.Read())
                    {
                        int idCommande = Convert.ToInt32(mesResultats["id"]);
                        string date = mesResultats["date_validation"] == DBNull.Value ? ""
                            : Convert.ToDateTime(mesResultats["date_validation"]).ToString("dd/MM/yyyy HH:mm");
                        decimal montant = mesResultats["montant_total"] == DBNull.Value ? 0
                            : Convert.ToDecimal(mesResultats["montant_total"]);

                        CommandeChoix commande = new CommandeChoix();
                        commande.Id = idCommande;
                        commande.Statut = "en_attente";
                        commande.Libelle = "N°" + idCommande + " du " + date + " - "
                            + mesResultats["nom"].ToString() + " " + mesResultats["prenom"].ToString()
                            + " (" + mesResultats["libelle"].ToString() + ") - "
                            + montant.ToString("N2") + " €";
                        commandes.Add(commande);
                    }
                }

                uneListe.DataSource = null;
                uneListe.DisplayMember = "Libelle";
                uneListe.ValueMember = "Id";
                uneListe.DataSource = commandes;
                uneListe.SelectedIndex = commandes.Count > 0 ? 0 : -1; // -1 : aucune commande en attente
            }
            catch (Exception erreur)
            {
                MessageBox.Show("Erreur de chargement des commandes en attente : " + erreur.Message);
            }
        }

        // Accepte une commande en attente. Le stock a déjà été retiré : rien à faire dessus.
        public bool AccepterCommande(int idCommande)
        {
            if (!EstConnectee()) return false;

            try
            {
                MySqlCommand maCommande = new MySqlCommand(
                    "UPDATE commande SET statut = 'acceptee', date_traitement = NOW() "
                    + "WHERE id = @cmd AND statut = 'en_attente'", laConnexion);
                maCommande.Parameters.AddWithValue("@cmd", idCommande);
                // 0 ligne modifiée = la commande n'était plus en attente (déjà traitée)
                return maCommande.ExecuteNonQuery() > 0;
            }
            catch (Exception erreur)
            {
                MessageBox.Show("Erreur lors de l'acceptation : " + erreur.Message);
                return false;
            }
        }

        // Refuse une commande en attente et rend toutes ses quantités au stock.
        public bool RefuserCommande(int idCommande)
        {
            if (!EstConnectee()) return false;

            MySqlTransaction transaction = laConnexion.BeginTransaction();
            try
            {
                // 1. Vérifier que la commande est toujours en attente (et la verrouiller)
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT id FROM commande WHERE id = @cmd AND statut = 'en_attente' FOR UPDATE",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@cmd", idCommande);
                if (cmd.ExecuteScalar() == null)
                {
                    transaction.Rollback();
                    return false; // déjà traitée
                }

                // 2. Rendre au stock la quantité de chaque ligne
                cmd = new MySqlCommand(
                    "UPDATE medicament m JOIN ligne_commande lc ON lc.id_medicament = m.id "
                    + "SET m.stock = m.stock + lc.quantite WHERE lc.id_commande = @cmd",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@cmd", idCommande);
                cmd.ExecuteNonQuery();

                // 3. Passer la commande au statut 'refusee' (on la garde pour l'historique)
                cmd = new MySqlCommand(
                    "UPDATE commande SET statut = 'refusee', date_traitement = NOW() WHERE id = @cmd",
                    laConnexion, transaction);
                cmd.Parameters.AddWithValue("@cmd", idCommande);
                cmd.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch (Exception erreur)
            {
                try { transaction.Rollback(); } catch { }
                MessageBox.Show("Erreur lors du refus : " + erreur.Message);
                return false;
            }
        }
    }

    public class UtilisateurConnecte
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Role { get; set; }
        public int? IdPraticien { get; set; }
    }

    // Un élément des listes de médicaments (cbProduit, cblistpanier)
    public class MedicamentChoix
    {
        public int Id { get; set; }
        public string Libelle { get; set; }
        public decimal Prix { get; set; }
        public int Stock { get; set; }
        public string Marque { get; set; }

        public override string ToString()
        {
            return Libelle;
        }
    }

    // Un élément de la liste des commandes du praticien (cblistpanier). Id = 0 : panier en cours.
    public class CommandeChoix
    {
        public int Id { get; set; }
        public string Libelle { get; set; }
        public string Statut { get; set; }

        public override string ToString()
        {
            return Libelle;
        }
    }
}