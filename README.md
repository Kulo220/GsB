# GSB – Gestion des commandes de médicaments

Application de bureau **C# / WinForms** développée dans le cadre du BTS SIO (mission AP3, contexte *Galaxy Swiss Bourdin*).
Elle permet aux **praticiens** de commander des médicaments via un panier, et à un **administrateur** de valider ou refuser ces commandes. Les données sont stockées dans une base **MySQL**.

---

## Fonctionnalités

### Connexion
- Authentification par identifiant / mot de passe (table `utilisateur`).
- Redirection automatique selon le rôle : **admin** ou **praticien**.

### Espace praticien
- Créer un panier et y ajouter des médicaments (désignation, forme, marque, prix, stock affichés).
- Les quantités s'additionnent si un médicament est ajouté plusieurs fois (max. 999 par ligne).
- Retirer un médicament du panier ou annuler le panier entier.
- Valider le panier : les prix sont figés et la commande passe **en attente**.
- Consulter l'historique de ses commandes et leur statut.
- Annuler une commande tant qu'elle est encore en attente.

### Espace administrateur
- Lister les commandes en attente de tous les praticiens et voir leur détail.
- **Accepter** une commande.
- **Refuser** une commande : les quantités sont rendues au stock, la commande est conservée pour l'historique.

### Gestion du stock
- Le stock est décrémenté **dès l'ajout au panier**, et seulement s'il est suffisant.
- Il est restitué en cas de retrait, d'annulation du panier, d'annulation ou de refus de commande.
- Chaque opération s'exécute dans une **transaction MySQL** (stock et panier changent ensemble, ou pas du tout), avec verrouillage `SELECT … FOR UPDATE` pour éviter les conflits entre praticien et admin.

---

## Cycle de vie d'une commande

```
panier ──(Valider)──► en_attente ──(Admin : Accepter)──► acceptee
   │                      │
   │                      ├──(Admin : Refuser)──► refusee   (stock restitué)
   │                      └──(Praticien : Annuler)──► supprimée (stock restitué)
   └──(Annuler panier)──► supprimé (stock restitué)
```

---

## Technologies

| Élément | Version |
|---|---|
| Langage | C# |
| Interface | Windows Forms |
| Framework | .NET Framework 4.7.2 |
| Base de données | MySQL (8.0.19+ requis pour la syntaxe `INSERT … AS … ON DUPLICATE KEY UPDATE`) |
| Connecteur | MySql.Data (NuGet) |
| IDE | Visual Studio |

---

## Structure du projet

```
GsB/
├── GsbVille.sln                 # Solution Visual Studio
└── GsbVille/
    ├── Program.cs               # Point d'entrée (lance FormConnexion)
    ├── BaseDeDonnees.cs         # Classe d'accès aux données (requêtes, transactions)
    ├── FormConnexion.cs         # Écran de connexion
    ├── Praticien.cs             # Espace praticien (commandes, historique)
    ├── Panier.cs                # Gestion du panier
    ├── Admin.cs                 # Espace administrateur (validation des commandes)
    ├── App.config
    └── packages.config          # Dépendances NuGet
```

---

## Base de données

Base attendue : `gsb_praticien`. Tables utilisées :

| Table | Rôle |
|---|---|
| `utilisateur` | Comptes (`nom`, `mdp`, `role`, `id_praticien` – NULL pour l'admin) |
| `medicament` | Catalogue (`designation`, `marque`, `concentration`, `type`, `Prix`, `stock`) |
| `commande` | Paniers et commandes (`id_praticien`, `statut`, `date_validation`, `date_traitement`, `montant_total`) |
| `ligne_commande` | Lignes (`id_commande`, `id_medicament`, `quantite`, `prix_unitaire`) |

Statuts possibles d'une commande : `panier`, `en_attente`, `acceptee`, `refusee`.

> ℹ️ Contraintes attendues : clé unique (`id_commande`, `id_medicament`) sur `ligne_commande`, `CHECK` sur la quantité (1 à 999), et `ON DELETE CASCADE` de `ligne_commande` vers `commande`.

---

## Installation

1. **Cloner le dépôt**
   ```bash
   git clone https://github.com/Kulo220/GsB.git
   ```
2. **Ouvrir** `GsbVille.sln` dans Visual Studio.
3. **Restaurer les paquets NuGet** (clic droit sur la solution → *Restaurer les packages NuGet*).
4. **Préparer la base MySQL** `gsb_praticien` avec les tables ci-dessus.
5. **Configurer la connexion** dans `GsbVille/FormConnexion.cs` :
   ```csharp
   maBase = new BaseDeDonnees("ADRESSE_SERVEUR", "gsb_praticien", "3306", "UTILISATEUR", "MOT_DE_PASSE");
   ```
6. **Lancer** le projet (F5).

---

## Pistes d'amélioration

- Externaliser la chaîne de connexion dans `App.config` au lieu de la coder en dur.
- Hacher les mots de passe des utilisateurs (actuellement comparés en clair).
- Activer SSL pour la connexion MySQL.
- Ajouter le script SQL de création de la base au dépôt.

---

## Auteur

Projet réalisé par **Kulo220** dans le cadre du BTS SIO.
