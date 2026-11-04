# Manuel d'utilisation — Media Organizer
>
> Guide complet de l'application **Media Organizer** : gestion des publications (livres, e-books et documents), des auteurs, des médias (audio, vidéo et YouTube), des galeries, des fonctionnalités d'intelligence artificielle, des tables d'appui, de l'administration, des rapports et des outils.
>
> Ce manuel s'adresse aux **utilisateurs finaux** et aux **administrateurs**. Aucune connaissance en programmation n'est requise.
>
> 💡 Le manuel est disponible **dans l'application** : cliquez sur le bouton d'aide (`?`) dans la **barre supérieure** pour l'ouvrir dans une boîte de dialogue, sans quitter la page où vous vous trouvez.

---

## Table des matières

- [0. Démarrage rapide — commencer en 5 minutes](#0-démarrage-rapide--commencer-en-5-minutes)
- [1. Vue d'ensemble](#1-vue-densemble)
- [2. Premiers pas](#2-premiers-pas)
- [3. Tableau de bord (Accueil)](#3-tableau-de-bord-accueil)
- [4. Publications (livres, e-books et documents)](#4-publications-livres-e-books-et-documents)
- [5. Auteurs](#5-auteurs)
- [6. Médias (audio, vidéo et YouTube)](#6-médias-audio-vidéo-et-youtube)
- [7. Galeries](#7-galerie)
- [8. Intelligence artificielle](#8-intelligence-artificielle)
- [9. Tables d'appui](#9-tables-dappui)
- [10. Administration](#10-administration)
- [11. Rapports](#11-rapports)
- [12. Outils](#12-outils)
- [13. Astuces et raccourcis](#13-astuces-et-raccourcis)
- [14. Résolution de problèmes](#14-résolution-de-problèmes)
- [15. Glossaire](#15-glossaire)
- [16. Scénarios complets (pas à pas)](#16-scenarios-complets-pas-a-pas)
- [Appendice A — Routes principales](#appendice-a--routes-principales)
- [Appendice B — Checklist de maintenance périodique](#appendice-b--checklist-de-maintenance-periodique)
- [Appendice C — Questions fréquentes (FAQ)](#appendice-c--questions-frequentes-faq)

---

## 0. Démarrage rapide — commencer en 5 minutes {#0-démarrage-rapide--commencer-en-5-minutes}

Si vous utilisez l'application pour la première fois, commencez par ces étapes essentielles.

### 0.1 Se connecter à l'application

1. Ouvrez l'adresse de l'application dans votre navigateur (en environnement local, généralement `https://localhost:4110`).
2. Saisissez l'**e-mail** et le **mot de passe** de votre compte.
3. Cliquez sur **Se connecter**.
4. Si vous n'avez pas encore de compte, utilisez le lien d'**inscription** sur l'écran d'authentification et attendez la création de l'utilisateur (dans certaines installations, l'inscription est effectuée par l'administrateur).

### 0.2 Choisir la langue

1. Ouvrez **Paramètres** (pied de page du menu latéral) → **Langue**.
2. Choisissez entre **Français (par défaut)**, **Anglais**, **Portugais** ou **Espagnol**.
3. Le choix est enregistré et appliqué à toute l'application (la page se recharge).

> 💡 La barre supérieure **n'a pas** de sélecteur de langue : elle affiche uniquement la langue actuelle (icône de globe + `FR`/`EN`/`PT`/`ES`), avec le nom d'utilisateur, le bouton d'aide (`?`), le bouton de thème et **Déconnexion**.

### 0.3 Choisir le thème

1. Dans la barre supérieure, cliquez sur l'icône de **thème** (soleil/lune).
2. L'application bascule entre le **mode clair** et le **mode sombre**.
3. La barre supérieure et le menu latéral sont toujours sombres — c'est intentionnel, pour conserver l'identité visuelle.

### 0.4 Créer ou importer votre première publication

1. Dans le menu latéral, ouvrez **Publications → Gestion** (`/publicacoes`).
2. Pour créer manuellement (par exemple un livre physique), cliquez sur **Nouveau** (bouton vert avec `+`), remplissez au moins **Titre**, **Auteur**, **Catégorie**, **Éditeur**, **Format** et **Emplacement du fichier**, puis cliquez sur **Enregistrer**.
3. Pour importer un document de votre PC (PDF, DOCX ou message Outlook `.msg`), utilisez **Importer PDF/DOCX**, sélectionnez le fichier et laissez l'application lire les **métadonnées**.
4. **Vérifiez** toujours les données remplies automatiquement avant d'enregistrer.

> 💡 La section **Publications** ne sert pas uniquement aux livres : elle gère également les **e-books (PDF)**, les **documents texte (DOCX)**, les **messages Outlook** et autres documents. Le formulaire est le même ; ce qui change, c'est surtout l'origine des données (manuelle ou importée).

### 0.5 Créer votre premier auteur

1. Ouvrez **Publications → Auteurs** (`/autores`).
2. Cliquez sur **Nouveau**.
3. Remplissez le **nom** et, si possible, la **biographie**.
4. **Enregistrez**.
5. Facultatif : importez une **photo** et utilisez l'IA pour générer/compléter le **profil de l'auteur**.

### 0.6 Ajouter un média (audio/vidéo)

1. Ouvrez **Médias → Gestion** (`/media`).
2. Cliquez sur **Nouveau** pour un enregistrement manuel ou **Importer** pour envoyer des fichiers.
3. Choisissez le type (audio, vidéo ou YouTube), remplissez les données et **Enregistrez**.

### 0.7 Parler avec l'IA

1. Ouvrez **IA → Chat** (`/ai/openrouter-chat`) pour le chat dans le cloud, ou **IA → Chat (Ollama)** (`/ai/chat`) pour le chat local.
2. Écrivez la question et appuyez sur **Entrée**.
3. La réponse apparaît en streaming ; vous pouvez **copier**, évaluer avec **👍/👎** et consulter l'historique.

### 0.8 Tableau « je veux faire X → je vais à Y »

| Je veux… | Je vais à… | Route |
|---|---|---|
| Voir le résumé général | Accueil | `/` |
| Gérer les publications (livres, e-books, documents) | Publications → Gestion | `/publicacoes` |
| Voir les couvertures en galerie | Publications → Galerie | `/imagegallery` |
| Gérer les auteurs | Publications → Auteurs | `/autores` |
| Gérer les médias | Médias → Gestion | `/media` |
| Voir les vidéos | Médias → Galerie | `/videogallery` |
| Créer des playlists | Médias → Playlists | `/media/playlists` |
| Modifier les prompts IA | IA → Éditeur de prompts | `/edit-prompts` |
| Chat cloud | IA → Chat | `/ai/openrouter-chat` |
| Chat local | IA → Chat (Ollama) | `/ai/chat` |
| Gérer les utilisateurs | Maintenance → Utilisateurs | `/admin/manageusers` |
| Gérer les tables auxiliaires | Maintenance → Tables auxiliaires | `/admin/managelookuptables` |
| Voir les logs | Logs | `/admin/viewapplogs` |
| Gérer les fichiers | Gestionnaire de fichiers | `/admin/filemanager` |
| Sauvegarde de la base de données | Sauvegarde → Base de données | `/backupdatabase` |
| Paramètres | Pied de page du menu → Paramètres | `/admin/appsettings2` |

---
## 1. Vue d'ensemble {#1-vue-densemble}

### 1.1 Qu'est-ce que Media Organizer

Media Organizer est une application de **catalogue personnel/organisationnel** qui rassemble, en un seul endroit :

- **Publications** — collection mixte de **livres imprimés, e-books (PDF), documents texte (DOCX), messages Outlook (`.msg`)** et autres documents, avec couverture, synopsis, ISBN, éditeur, catégorie, format, emplacement du fichier et classement personnel. Vous pouvez créer manuellement ou **importer** le fichier et laisser l'application remplir les données à partir des métadonnées.
- **Auteurs** — fiches complètes, avec biographie, photo et liste de livres.
- **Médias** — audio, vidéo et liens YouTube, organisés en playlists et lus directement dans l'application.
- **Galeries** — vues visuelles d'images, de vidéos et de musique.
- **Intelligence artificielle** — traduction, résumés, métadonnées, profils d'auteur et deux chats (cloud et local).

### 1.2 Comment l'application est structurée (sans détails techniques)

L'application se compose de deux parties qui travaillent ensemble :

1. **Service/API** — stocke et traite les données (base de données, fichiers, IA).
2. **Interface** — ce que vous voyez dans le navigateur.

Pour l'utilisateur, cela se comporte comme un seul site web. En environnement local, les deux composants peuvent utiliser des ports différents ; si une page ne se charge pas, consultez [Résolution de problèmes](#14-résolution-de-problèmes).

### 1.3 Rôles utilisateurs

Il existe deux rôles :

| Rôle | Ce qu'il peut voir | Ce qu'il peut faire |
|---|---|---|
| **Admin** | **Toutes** les données de tous les utilisateurs | Tout : gestion des utilisateurs, tables auxiliaires, paramètres, logs, sauvegardes, supprimer l'historique de tous |
| **GeneralUser** | **Uniquement ses propres** données | Créer/modifier/supprimer ses publications, auteurs, médias et playlists ; utiliser l'IA ; gérer son historique de chat |

> ⚠️ **Attention :** l'isolation des données est appliquée au niveau serveur. Un GeneralUser **ne peut pas** voir les enregistrements d'un autre utilisateur, même en modifiant l'adresse de la page. Si un enregistrement a « disparu », vérifiez que vous êtes authentifié avec le bon compte.

### 1.4 Plan mental de la navigation

```
Accueil (/)
├── Publications
│   ├── Gestion (/publicacoes)
│   ├── Galerie (/imagegallery)
│   └── Auteurs (/autores)
├── Médias
│   ├── Gestion (/media)
│   ├── Galerie (/videogallery)
│   └── Playlists (/media/playlists)
├── IA
│   ├── Éditeur de prompts (/edit-prompts)
│   ├── Chat (Ollama) (/ai/chat)
│   └── Chat (/ai/openrouter-chat)
├── [Admin] Sauvegarde
│   ├── Base de données (/backupdatabase)
│   ├── Uploads Publications (/backupuploads/1)
│   ├── Uploads Médias (/backupuploads/2)
│   └── Uploads Images (/backupuploads/3)
├── [Admin] Maintenance
│   ├── Utilisateurs (/admin/manageusers)
│   └── Tables auxiliaires (/admin/managelookuptables)
├── [Admin] Gestionnaire de fichiers (/admin/filemanager)
├── [Admin] Logs (/admin/viewapplogs)
└── Paramètres (/admin/appsettings2)   ← pied de page du menu
```

### 1.5 Ce qui est stocké où

- **Enregistrements** (publications, auteurs, médias, playlists, conversations, évaluations) — dans la base de données.
- **Fichiers** (PDFs, couvertures, images d'auteurs, audio, vidéo) — dans les dossiers configurés dans Paramètres.
- **Préférences locales** (thème) — dans le navigateur ; la **langue** est définie dans **Paramètres → Langue** et appliquée par cookie de culture.
- **Configuration IA** — dans la base de données, modifiable dans la section **IA** des Paramètres.

---

## 2. Premiers pas {#2-premiers-pas}

### 2.1 Authentification

#### Se connecter

1. Ouvrez l'application.
2. Saisissez **e-mail** et **mot de passe**.
3. Cliquez sur **Se connecter**.
4. Si les identifiants sont corrects, vous accédez directement au **Tableau de bord**.

#### Récupérer le mot de passe

1. Sur l'écran d'authentification, cliquez sur **Mot de passe oublié**.
2. Saisissez l'e-mail associé au compte.
3. Suivez le lien envoyé par e-mail.
4. Définissez le nouveau mot de passe.

> 💡 Si l'e-mail de récupération n'arrive pas, vérifiez le dossier spam ou contactez l'administrateur (l'envoi d'e-mail dépend de la configuration du serveur).

#### Se déconnecter

- Utilisez l'option de **déconnexion** dans la barre supérieure. La session est fermée et vous revenez à l'écran d'authentification.

### 2.2 Langues

L'application est disponible en **quatre langues** :

| Langue | Code | Remarques |
|---|---|---|
| Français | `fr` | Langue **par défaut** |
| Anglais | `en` | — |
| Portugais | `pt` | — |
| Espagnol | `es` | — |

**Comment changer :**

1. Ouvrez **Paramètres** (pied de page du menu latéral) → section **Langue**.
2. Choisissez la langue souhaitée dans la liste.
3. L'application enregistre le choix et recharge l'interface dans la nouvelle langue.

> 💡 La **barre supérieure n'a pas de sélecteur de langue**. Elle affiche simplement un indicateur avec la langue actuelle (globe + `FR`/`EN`/`PT`/`ES`), avec le nom d'utilisateur, le bouton d'aide (`?`), le bouton de thème et **Déconnexion**. Le changement se fait toujours dans **Paramètres**.

**Ce qui est traduit :** menus, boutons, étiquettes, messages de validation et textes de l'interface.

**Ce qui n'est pas traduit :** le **contenu** que vous saisissez (titres, synopsis, biographies) et les noms des modèles IA. Vous pouvez avoir l'interface en anglais et vos publications en français — c'est normal.

> 💡 Le choix est enregistré dans les Paramètres et appliqué à toute l'application (ce n'est pas une préférence isolée par onglet).

### 2.3 Thème clair/sombre

1. Cliquez sur l'icône de **thème** dans la barre supérieure.
2. L'application bascule instantanément, sans rechargement.
3. La préférence est mémorisée dans le navigateur.

**Règles visuelles importantes :**

- La **barre latérale** et la **barre supérieure** sont **toujours sombres**, dans les deux thèmes.
- Les cartes, grilles, formulaires et boîtes de dialogue suivent le thème.
- Les graphiques du Tableau de bord s'adaptent également.

> 💡 Si vous remarquez un artefact visuel sur un graphique après avoir passé la souris, déplacez simplement la souris hors du graphique puis remettez-la ; l'application corrige automatiquement le redessin.

### 2.4 Anatomie de la barre latérale

La barre latérale comporte trois zones :

1. **Haut** — éléments généraux : Accueil, Publications, Médias, IA.
2. **Milieu** — éléments administratifs (uniquement pour Admin) : Sauvegarde, Maintenance, Gestionnaire de fichiers, Logs.
3. **Bas** — **Paramètres**.

**Comportement :**

- Les groupes avec des enfants se développent/réduisent au clic.
- L'élément actif est mis en surbrillance.
- Sur petits écrans, la barre latérale peut être réduite.

### 2.5 Barre supérieure

Elle contient généralement :

- Le **nom/marque** de l'application.
- La **langue actuelle** (icône de globe + `FR`/`EN`/`PT`/`ES`) — c'est uniquement un indicateur ; le changement se fait dans **Paramètres**.
- Le **nom d'utilisateur**.
- Le bouton d'**aide** (`?`), qui ouvre ce manuel dans une boîte de dialogue, sans quitter la page où vous êtes.
- Le **bouton de thème**.
- L'option de **session** (déconnexion).

### 2.6 Premier accès — checklist

- [ ] J'ai réussi à me connecter.
- [ ] J'ai choisi la langue.
- [ ] J'ai choisi le thème.
- [ ] J'ai créé ou importé au moins un livre/document.
- [ ] J'ai créé au moins un auteur.
- [ ] J'ai confirmé que les tables d'appui essentielles contiennent des données (catégories, formats, éditeurs).
- [ ] J'ai testé une traduction ou un résumé avec l'IA.
- [ ] J'ai ouvert le chat et envoyé un message.

---

## 3. Tableau de bord (Accueil) {#3-tableau-de-bord-accueil}

Le Tableau de bord est la page d'accueil (`/`) et donne une vue globale du catalogue et de l'activité.

### 3.1 Cartes de totaux

En haut apparaissent des cartes avec des comptages, par exemple :

- **Publications**.
- **Auteurs**.
- **Éditeurs**.
- **Formats**.
- **Médias**.

Chaque carte est cliquable (lorsque applicable) et mène à la page correspondante.

### 3.2 Graphiques circulaires

Il existe des graphiques de répartition par :

- **Catégories de publications**.
- **Formats de publications**.
- **Éditeurs**.
- **Catégories de médias**.
- **Formats de médias**.

**Comment lire :**

- Chaque tranche représente une catégorie/format/éditeur.
- En passant la souris, le nom et la valeur apparaissent.
- Les très petites tranches peuvent être regroupées visuellement ; consultez les tables d'appui pour le détail.

> 💡 Si un graphique est vide, il n'y a pas assez d'enregistrements (ou les enregistrements n'ont pas cette classification remplie).

### 3.3 Section « Santé » (qualité des données)

Indicateurs d'enregistrements incomplets ou de problèmes :

| Indicateur | Signification | Que faire |
|---|---|---|
| **Publications sans couverture** | Publications sans image de couverture | Ouvrir chaque publication et charger la couverture |
| **Publications sans pages** | Publications sans nombre de pages | Compléter le champ **Pages** |
| **Auteurs sans publications** | Auteurs sans publications associées | Associer des publications ou supprimer les auteurs orphelins |
| **Éditeurs sans publications** | Éditeurs non utilisés | Réutiliser ou supprimer dans les tables d'appui |
| **Catégories sans publications** | Catégories non utilisées | Réutiliser ou supprimer |
| **Formats sans publications** | Formats non utilisés | Réutiliser ou supprimer |
| **Emplacements sans publications** | Emplacements non utilisés | Réutiliser ou supprimer |
| **Titres en double** | Possibles doublons | Confirmer et consolider les enregistrements |

> ⚠️ Ces indicateurs sont des **suggestions**, pas des erreurs. Un enregistrement peut être incomplet volontairement.

### 3.4 Section « Activité »

Métriques d'utilisation de l'IA/chat :

- **Conversations** — nombre de conversations.
- **Total des réponses**.
- **7 derniers jours**.
- **Latence moyenne**.
- **Tokens/seconde** (avec une note expliquant qu'il s'agit d'une mesure approximative).
- **Taille moyenne des tokens**.
- **Pourcentage d'utilisation cloud**.
- **Modèles les plus utilisés** (tableau).

### 3.5 Section « IA en usage interne »

Montre l'utilisation de l'IA **hors du chat** (traductions, résumés, métadonnées, profils, etc.) :

- Total des opérations.
- 7 derniers jours.
- Appels via **OpenRouter**.
- Appels via **Ollama** (local).
- **Fallbacks** (nombre de fois où le cloud a échoué et où le local a été utilisé).
- Tableau **par opération** (traduire, résumé, métadonnées…).
- Tableau des **derniers enregistrements** avec opération, fournisseur, contexte, scénario, durée et date.

**Comment interpréter :**

- De nombreux **fallbacks** = le cloud est instable, sans clé ou sans solde ; l'application utilise Ollama.
- Des durées élevées = modèles locaux sur des machines modestes ou prompts longs.

### 3.6 Section « Audit »

- **Dernières connexions**.
- **Erreurs récentes** (avec niveau et message).
- Nombre total d'erreurs.

> 💡 Pour le détail complet, ouvrez **Logs** (`/admin/viewapplogs`).

### 3.7 Mise à jour des données

Le Tableau de bord charge à l'ouverture de la page. Si vous avez fait des modifications dans un autre onglet, rechargez la page (F5) pour voir les chiffres mis à jour.

---

## 4. Publications {#4-publications-livres-e-books-et-documents}

Page de gestion : **Gestion des publications** (`/publicacoes`).

### 4.1 La grille de publications

La grille liste les publications auxquelles vous avez accès, avec des colonnes comme titre, auteur, éditeur, catégorie, format, année, classement et état de lecture.

**Opérations disponibles :**

| Action | Comment |
|---|---|
| Trier | Cliquer sur l'en-tête de colonne |
| Filtrer | Utiliser la ligne de filtres de l'en-tête |
| Rechercher | Boîte de recherche de la grille |
| Grouper | Glisser l'en-tête vers la zone de groupement |
| Redimensionner les colonnes | Glisser la bordure de l'en-tête |
| Paginer | Contrôles au bas de la grille |

### 4.2 Créer une publication

1. Cliquez sur **Nouveau** (bouton vert `+`).
2. Remplissez le formulaire (voir [4.4 Champs](#44-champs-du-formulaire)).
3. Cliquez sur **Enregistrer**.
4. L'enregistrement apparaît dans la grille.

> 💡 **Titre** et **Auteur** sont les champs les plus importants. Pour **enregistrer** une publication, l'application exige également **Catégorie**, **Éditeur**, **Format** et **Emplacement du fichier** — créez-les d'abord dans les Tables d'appui s'ils manquent.

### 4.3 Modifier et supprimer

**Modifier :**

1. Sur la ligne de la publication, cliquez sur l'icône de **crayon** (Modifier).
2. Modifiez les champs.
3. **Enregistrez**.

**Supprimer :**

1. Sur la ligne, cliquez sur l'icône de **corbeille** (Supprimer).
2. Confirmez dans la boîte de dialogue.

> ⚠️ La suppression est définitive. Si la publication a des fichiers associés, confirmez si vous souhaitez également les supprimer du disque (l'application avertit le cas échéant).

### 4.4 Champs du formulaire {#44-champs-du-formulaire}

| Champ | Obligatoire | Description |
|---|---|---|
| **ID** | — | Généré automatiquement ; lecture seule |
| **ISBN** | Non | Identifiant de la publication ; utile pour la recherche Google Books |
| **Date de mouvement** | Non | Date d'acquisition/enregistrement |
| **Titre** | **Oui** | Nom de la publication (2 à 256 caractères) |
| **Année d'édition** | Non | Si remplie, doit avoir **4 chiffres** et ne peut pas être supérieure à l'année en cours ; peut être suggérée par l'IA |
| **Pages** | Non | Nombre de pages |
| **Prix** | Non | Valeur monétaire |
| **Auteur** | **Oui** | Sélectionné dans la liste des auteurs (recherchable) |
| **Catégorie** | **Oui** | Catégorie de la publication |
| **Éditeur** | **Oui** | Éditeur |
| **Format** | **Oui** | Format physique/numérique |
| **Emplacement du fichier** | **Oui** | Où l'exemplaire/fichier est stocké |
| **Classement (Rating)** | Non | Évaluation personnelle (étoiles) |
| **Lu** | Non | Marque si déjà lu |
| **Synopsis** | Non | Description/résumé ; supporte la traduction par IA |

**Notes sur les sélecteurs :**

- Les champs **Auteur**, **Catégorie**, **Éditeur**, **Format** et **Emplacement du fichier** sont recherchables — tapez une partie du nom pour filtrer.
- S'il manque une option, créez-la d'abord dans les **Tables d'appui** (ou demandez à l'Admin).

### 4.5 Importer des publications à partir de fichiers (PDF/DOCX)

1. Dans la page de gestion, utilisez le bouton **Importer PDF/DOCX**.
2. Sélectionnez un ou plusieurs fichiers.
3. L'application crée/met à jour des enregistrements à partir du contenu.
4. Revoyez les enregistrements créés et corrigez ce qui est nécessaire.

> 💡 L'importation tente d'extraire le titre, l'auteur et d'autres données. **Revoyez toujours** le résultat — l'extraction automatique n'est pas parfaite.

### 4.6 Lire une publication dans l'application

1. Sur la ligne de la publication, cliquez sur la commande **Lire PDF**.
2. Le visualiseur ouvre le fichier associé.
3. Vous pouvez **modifier la publication** depuis le visualiseur ou **fermer**.
4. Pour les documents Word, un visualiseur dédié apparaît, avec les mêmes actions.

> ⚠️ Si le fichier n'existe pas à l'emplacement indiqué, l'application avertit. Utilisez les pages de diagnostic (voir [4.9](#49-fichiers-manquants-et-chemins-incorrects)).

### 4.7 Recherche Google Books

1. Dans le formulaire de la publication (ou dans le flux de recherche), ouvrez **Rechercher en ligne**.
2. Saisissez le **titre** et/ou l'**auteur**.
3. L'application interroge **Google Books**.
4. Choisissez un résultat pour remplir les champs.
5. Si le résultat n'a pas de description, l'application utilise l'**extrait** disponible.
6. S'il n'y a pas de résultats, l'**IA** peut être déclenchée pour générer un résumé.
7. Confirmez et **enregistrez**.

**Bonnes pratiques :**

- Recherchez avec le **titre exact** (sans guillemets) et le **nom de l'auteur**.
- Si la publication est traduite, essayez le titre original.
- Vérifiez toujours l'**année** et l'**éditeur** avant d'enregistrer.

### 4.8 Fonctionnalités IA dans la publication

Dans le formulaire de la publication, vous pouvez utiliser :

| Fonction | Ce qu'elle fait | Où apparaît le résultat |
|---|---|---|
| **Résumé IA** | Génère un synopsis à partir du titre/auteur/données | Champ **Synopsis** |
| **Traduire** | Traduit le synopsis/texte entre langues | Dialogue de traduction → **Confirmer** |
| **Année d'édition IA** | Suggère l'année d'édition | Champ **Année d'édition** |
| **Livres de l'auteur** | Liste/génère les livres de l'auteur sélectionné | Panneau **Livres de l'auteur** |

Le fonctionnement détaillé (y compris la **puce du modèle** et le bouton **Annuler**) se trouve dans [8. Intelligence artificielle](#8-intelligence-artificielle).

### 4.9 Fichiers manquants et chemins incorrects {#49-fichiers-manquants-et-chemins-incorrects}

Deux pages de diagnostic aident à maintenir le catalogue cohérent :

#### Publications non trouvées sur le disque (`/pubsnotfoundondisk`)

- Liste les publications dont le **fichier référencé n'existe pas**.
- Actions typiques : corriger le chemin, recharger le fichier ou supprimer l'enregistrement.

#### Publications avec chemin incorrect (`/pubswithwrongpath`)

- Liste les publications dont le chemin pointe vers un emplacement invalide/ancien.
- Actions typiques : mettre à jour l'**Emplacement du fichier**, déplacer le fichier ou corriger la configuration des dossiers.

> 💡 Exécutez ces vérifications après avoir déplacé des dossiers, changé d'ordinateur ou restauré des sauvegardes.

### 4.10 Statistiques et « livres de l'auteur »

- Le bouton **Statistiques** ouvre un résumé avec comptages et répartitions.
- Le bouton **Livres de l'auteur** ouvre un panneau avec les livres de l'auteur sélectionné, permettant de naviguer rapidement entre les œuvres.

### 4.11 Bonnes pratiques pour les publications

- Remplissez **Auteur**, **Catégorie**, **Éditeur** et **Format** pour améliorer les graphiques du Tableau de bord.
- Utilisez l'**ISBN** pour éviter les doublons.
- Nommez les fichiers de manière cohérente (ex. : `ISBN - Titre.pdf`).
- Gardez les **couvertures** dans le dossier configuré.
- Revoyez périodiquement la section **Santé** du Tableau de bord.

---

## 5. Auteurs {#5-auteurs}

Page : **Publications → Auteurs** (`/autores`).

### 5.1 Liste des auteurs

La grille affiche les auteurs auxquels vous avez accès, avec nom, nombre de livres et photo (lorsqu'elle existe).

**Opérations :**

- Rechercher par nom.
- Trier et filtrer.
- Ouvrir la fiche (**Modifier**).
- Voir en mode **lecture seule**.
- Supprimer (avec confirmation).

### 5.2 Créer un auteur

1. Cliquez sur **Nouveau**.
2. Remplissez le **Nom** (obligatoire, 2 à 128 caractères).
3. Remplissez **À propos** (biographie ; optionnel, mais recommandé).
4. Indiquez la **date de naissance** (optionnelle).
5. Importez une **photo** (optionnelle).
6. **Enregistrez**.

### 5.3 Fiche de l'auteur

La fiche (édition) inclut :

| Champ | Obligatoire | Description |
|---|---|---|
| **Nom** | **Oui** | Nom de l'auteur (2 à 128 caractères) |
| **Date de naissance** | Non | Date de naissance de l'auteur |
| **À propos / Biographie** | Non | Texte libre (éditeur de texte avec formatage) ; supporte la traduction par IA |
| **Photo** | Non | Image de l'auteur |
| **Livres** | — | Liste des œuvres associées |
| **Profil IA** | — | Profil généré/complété par IA |

### 5.4 Photo de l'auteur

1. Dans la fiche, utilisez la zone d'**importation**.
2. Sélectionnez l'image (formats courants : JPG, PNG).
3. Attendez la progression.
4. L'image devient visible dans la fiche et la liste.

> 💡 Utilisez des images carrées ou portrait, avec une bonne résolution, pour une meilleure présentation.

### 5.5 Biographie et traduction

1. Écrivez ou collez la biographie.
2. Utilisez **Traduire** pour convertir dans une autre langue.
3. Revoyez le texte dans la boîte de dialogue.
4. Cliquez sur **Confirmer** pour appliquer.

> ⚠️ La traduction remplace le texte du champ quand vous confirmez. Si vous voulez conserver l'original, copiez-le d'abord dans un bloc-notes.

### 5.6 Livres de l'auteur

- Dans la fiche ou la liste, ouvrez **Livres de l'auteur**.
- Vous pouvez **associer** des livres existants et **créer** de nouveaux livres déjà liés à l'auteur.
- La liste reflète ce qui est dans la base de données pour cet auteur.

### 5.7 Générer un profil avec l'IA

1. Ouvrez la fiche de l'auteur.
2. Cliquez sur **Générer profil IA** (ou équivalent).
3. L'application essaie d'abord le **cloud** (OpenRouter) et, en cas d'échec, **Ollama local**.
4. Le résultat est présenté pour révision.
5. Modifiez si nécessaire et **enregistrez**.

**Ce que l'IA peut utiliser :** nom, biographie existante et œuvres connues.

> 💡 Revoyez toujours le résultat : l'IA peut inventer des dates ou des œuvres. Le profil est un **point de départ**, pas une source de vérité.

### 5.8 Vue lecture seule

La vue **AuthorReadOnly** présente la fiche sans permettre l'édition — utile pour une consultation rapide ou une présentation.

### 5.9 Supprimer des auteurs

1. Cliquez sur **Supprimer** sur la ligne de l'auteur.
2. Confirmez.

> ⚠️ Si l'auteur a des livres associés, l'application peut empêcher la suppression ou avertir. Résolvez d'abord les associations.

---

## 6. Médias (Audio, Vidéo et YouTube) {#6-médias-audio-vidéo-et-youtube}

Page de gestion : **Médias → Gestion** (`/media`).

### 6.1 Types de médias

L'application distingue :

- **Audio** — MP3 et formats similaires.
- **Vidéo** — fichiers vidéo locaux.
- **YouTube** — liens vers des vidéos hébergées sur YouTube (sans fichier local).

Chaque type a ses propres champs et actions, mais partage la même grille et le même schéma d'édition.

### 6.2 La grille de médias

Colonnes typiques : titre, artiste/auteur, catégorie, format, classement, type et actions.

**Actions par ligne :**

| Icône | Action |
|---|---|
| ▶ | **Lire** (lecteur adapté au type) |
| ℹ | **Information/détail** |
| ✏ | **Modifier** |
| 🗑 | **Supprimer** |

### 6.3 Créer un enregistrement média

1. Cliquez sur **Nouveau**.
2. Choisissez le **type** (audio, vidéo ou YouTube).
3. Remplissez les champs (voir tableau ci-dessous).
4. Pour YouTube, saisissez le **lien** de la vidéo.
5. **Enregistrez**.

**Champs et validation :**

| Champ | Obligatoire | Notes |
|---|---|---|
| **Titre** (nom du fichier/enregistrement) | **Oui** | 2 à 256 caractères |
| **Type** | **Oui** | Audio ou vidéo (YouTube est traité comme cas à part) |
| **Auteur/Artiste** | **Oui** | — |
| **Format de média** | **Oui** | Ex. : MP3, MP4 (table d'appui) |
| **Genre/Catégorie** | **Oui** | Catégorie de média (table d'appui) |
| **Année d'édition** | Non | Si remplie, 4 chiffres et pas supérieure à l'année en cours |
| **Date de mouvement** | Non | Ne peut pas être une date future |
| **Classement** | Non | Étoiles |
| **Notes/description** | Non | Supporte l'IA |

### 6.4 Charger des fichiers (Upload)

1. Cliquez sur **Importer**.
2. Sélectionnez un ou plusieurs fichiers.
3. Choisissez la destination/type, si demandé.
4. Attendez la fin.
5. Revoyez les enregistrements créés.

**Recommandations :**

- Évitez les fichiers énormes sans nécessité.
- Utilisez des noms sans caractères spéciaux problématiques.
- Confirmez le **format de média** (catégorie) pour que les graphiques soient corrects.

### 6.5 Métadonnées et notes avec l'IA

Dans le formulaire/détail du média, vous pouvez utiliser :

- **Notes IA** — génère une description/notes sur l'élément.
- **Métadonnées IA** — suggère des données comme genre, année, artiste.
- **Année/édition** — lorsque applicable.

Le résultat est toujours **revérifié par vous** avant d'enregistrer.

### 6.6 Playlists

Page : **Médias → Playlists** (`/media/playlists`).

**Créer une playlist :**

1. Cliquez sur **Nouveau**.
2. Donnez un **nom** et, si vous le souhaitez, une description.
3. **Enregistrez**.
4. Ouvrez la playlist et **ajoutez des éléments** (média existant).

**Gérer les éléments :**

- Ajouter/supprimer.
- Réordonner (lorsque disponible).
- Lire la playlist complète.

**Lire :**

1. Ouvrez la playlist.
2. Cliquez sur **Lire**.
3. Le **Lecteur de Playlist** (`/media/playlists/{id}/player`) s'ouvre avec la file.
4. Utilisez les contrôles pour avancer/reculer.

> 💡 Si un élément ne se lit pas, confirmez que le fichier existe sur le disque ou que le lien YouTube est valide.

### 6.7 Lecteur MP3

Page : **`/mp3player`**.

- Lit des pistes audio.
- Permet de choisir la piste, mettre en pause, avancer et régler le volume.
- Affiche l'information de la piste en cours.

> 💡 Le lecteur fonctionne dans la page ; pour continuer à écouter en naviguant, ouvrez-le dans un onglet séparé du navigateur.

### 6.8 Lecteur YouTube

Page : **`/youtubeplayer`**.

- Lit des vidéos via le lien stocké dans l'enregistrement.
- Si le lien est invalide ou la vidéo a été supprimée, une erreur apparaît — corrigez le lien dans l'enregistrement.

### 6.9 Détail des médias

- **Détail audio** (`Mp3Detail`) — information et lecture.
- **Détail vidéo** (`VideoDetail`) — information et lecture.

### 6.10 YouTube et abonnements

L'application inclut une zone d'**abonnements** (`/subscriptions`) pour suivre des chaînes/liens YouTube, lorsque configurée.

### 6.11 Bonnes pratiques pour les médias

- Cohérence dans les noms d'**artiste** et de **catégorie**.
- Une playlist par contexte (ex. : « Lectures », « Voyages »).
- Pour YouTube, privilégiez les liens « non listés » ou publics stables.
- Vérifiez les **formats de média** dans les tables d'appui avant d'importer en masse.

---

## 7. Galeries {#7-galeries}

Les galeries offrent des vues visuelles alternatives aux grilles.

### 7.1 Galerie d'images (`/imagegallery`)

**Ce qu'elle montre :** couvertures de livres et images associées, dans une grille de miniatures.

**Comment l'utiliser :**

1. Ouvrez **Publications → Galerie**.
2. Parcourez les images.
3. Cliquez sur une image pour l'agrandir/voir le détail.
4. Utilisez le lien pour ouvrir l'enregistrement correspondant.

**Comportement :**

- Les images manquantes montrent un espace réservé (placeholder).
- La galerie respecte l'isolation des données (vous ne voyez que ce qui vous appartient, sauf Admin).

### 7.2 Galerie de vidéos (`/videogallery`)

**Ce qu'elle montre :** miniatures des vidéos, avec lecture intégrée.

**Comment l'utiliser :**

1. Ouvrez **Médias → Galerie**.
2. Cliquez sur une vidéo pour la lire.
3. Utilisez le détail pour modifier les métadonnées.

### 7.3 Galerie de musique (`/musicgallery`)

**Ce qu'elle montre :** vue style **polaroïd** des pistes/albums.

**Interactions :**

- Passer la souris met l'élément en surbrillance (couleur d'accent).
- Cliquer sélectionne/lit.
- La sélection est visuellement distincte.

> 💡 La galerie de musique est idéale pour naviguer par couvertures d'albums ; pour modifier les métadonnées, utilisez la grille de **Médias**.

### 7.4 Quand une galerie est vide

Si aucune image/vidéo/piste n'est présente, le message **« Aucune donnée disponible »** apparaît. Vérifiez :

1. Qu'il existe des enregistrements du bon type.
2. Que les fichiers d'image/vidéo sont associés.
3. Que vous êtes connecté avec le bon compte/utilisateur.

---

## 8. Intelligence artificielle {#8-intelligence-artificielle}

L'IA est l'une des zones les plus riches de l'application. Cette section explique **comment elle fonctionne** et **comment utiliser chaque fonction**.

### 8.1 Architecture en deux couches (l'essentiel)

L'application utilise **deux fournisseurs** :

1. **Cloud (OpenRouter)** — **premier choix**. Rapide et avec des modèles plus grands, mais dépend d'Internet, d'une clé API et de limites.
2. **Local (Ollama)** — **réserve**. Fonctionne sur la machine, sans coût par appel, mais plus lent et dépend du matériel.

**Flux typique :**

```
Demande de l'utilisateur
        │
        ▼
  Cloud (OpenRouter)  ──échec/sans clé/limite──►  Local (Ollama)
        │                                              │
        └──────────── réponse ────────────────────────┘
```

**Ce qui compte comme « échec » du cloud :** pas de connexion, pas de clé, limite quotidienne atteinte, erreur HTTP, réponse invalide ou vide.

> 💡 Ce mécanisme est **automatique**. L'utilisateur n'a pas besoin de choisir le fournisseur à chaque opération — mais il **voit lequel a été utilisé** dans la puce du modèle (voir [8.3](#83-la-puce-du-modele)).

### 8.2 Où l'IA est utilisée

| Opération | Où | Résultat |
|---|---|---|
| **Traduire** | Formulaires publication/auteur, outil de traduction | Texte traduit |
| **Résumé de publication** | Édition publication / recherche Google Books | Synopsis |
| **Année d'édition** | Édition publication | Année suggérée |
| **Notes de média** | Édition média | Notes/description |
| **Métadonnées de média** | Édition média | Champs suggérés |
| **Livres de l'auteur** | Fiche auteur | Liste d'œuvres |
| **Profil de l'auteur** | Fiche auteur | Profil/biographie |
| **Chat cloud** | `/ai/openrouter-chat` | Conversation |
| **Chat local** | `/ai/chat` | Conversation |

### 8.3 La puce du modèle {#83-la-puce-du-modele}

Chaque fois qu'une opération IA se termine, une **puce** (petite étiquette) peut apparaître avec le modèle utilisé, par exemple :

```
⊘ OpenRouter · llama-3.3-70b
```

**Significations :**

- **OpenRouter · nom-du-modèle** — le cloud a répondu avec ce modèle.
- **Ollama · nom-du-modèle** — le modèle local a répondu.
- **Automatique** — le routeur du cloud a choisi le modèle (aucun modèle spécifique n'a été demandé).
- **Modèle en cours d'essai** (dans le `title`) — apparaît quand l'opération a été **annulée** ; indique celui qui était en cours.

**Pourquoi c'est utile :** comme l'application essaie plusieurs modèles et peut passer au local, le modèle réel n'est pas prévisible. La puce donne la **provenance** de la réponse.

> 💡 Dans la traduction, la puce apparaît **sous le bouton Traduire**, pour ne pas concurrencer le message d'état (annulation ou confiance).

### 8.4 Traduire du texte

**Où la trouver :** formulaires de livre (synopsis), fiche auteur (biographie) et outil de traduction.

**Pas à pas :**

1. Ouvrez le dialogue de traduction.
2. Choisissez la langue d'**origine** et de **destination**.
3. Cliquez sur **Traduire**.
4. Pendant le traitement, un message de progression apparaît qui **nomme déjà le modèle** en cours.
5. Si cela prend trop de temps, cliquez sur **Annuler**.
6. En cas de succès, revoyez le texte et cliquez sur **Confirmer**.

**Le bouton Annuler :**

- L'interface s'arrête **immédiatement**.
- Le modèle local peut continuer en arrière-plan, mais le résultat est **rejeté**.
- Le message **« Annulé par l'utilisateur »** apparaît à droite.
- **Confirmer** devient désactivé (il n'y a pas de texte à appliquer).

**Fermer le dialogue :**

- Annule silencieusement toute traduction en cours.
- Une réponse tardive **n'écrase pas** le champ.

**Conseils :**

- Traduisez des textes déjà revus, pour éviter de traduire des erreurs.
- Si la traduction échoue, réessayez — l'application peut choisir un autre modèle.
- Sur des textes longs, l'annulation est plus rapide que d'attendre le timeout.

---

## 9. Tables d'appui {#9-tables-dappui}

Les tables d'appui (ou « auxiliaires ») alimentent les sélecteurs et les graphiques. Sans elles, les formulaires sont pauvres et les statistiques incomplètes.

Page : **Maintenance → Tables auxiliaires** (`/admin/managelookuptables`). Il existe aussi une version nouvelle (`/admin/managelookuptables-new`), avec le même contenu et une mise en page différente.

### 9.1 Catégories de publications

- **À quoi ça sert :** classer les publications (ex. : Roman, Technique, Histoire).
- **Champs :** nom/description.
- **Impact :** utilisé dans le formulaire de publication et dans le graphique des catégories.

### 9.2 Catégories de médias

- **À quoi ça sert :** classer audio/vidéo (ex. : Musique, Documentaire, Podcast).
- **Champs :** nom/description (avec détail propre).
- **Impact :** formulaire de média et graphique des catégories de médias.

### 9.3 Formats de publications

- **À quoi ça sert :** format physique/numérique de la publication (ex. : Couverture rigide, Broché, eBook, PDF).
- **Impact :** formulaire de publication et graphique des formats.

### 9.4 Formats de médias

- **À quoi ça sert :** format du fichier média (ex. : MP3, MP4, FLAC).
- **Impact :** formulaire de média et graphique des formats de médias.

### 9.5 Éditeurs

- **À quoi ça sert :** éditeur de la publication.
- **Champs :** nom et données de contact (lorsque applicable).
- **Impact :** formulaire de publication, recherche Google Books et graphique des éditeurs.

### 9.6 Emplacements de stockage

- **À quoi ça sert :** indiquer où l'exemplaire/fichier est stocké (ex. : Étagère A, Dossier PDFs, Cloud).
- **Impact :** formulaire de publication et diagnostic des chemins.

### 9.7 Modèles IA

- **À quoi ça sert :** catalogue des modèles disponibles pour Ollama local (et référence de configuration).
- **Champs :** nom du modèle, adresse/base, paramètres.
- **Impact :** choix du modèle local dans Paramètres et dans le chat local.

### 9.8 Règles de maintenance des tables

| Règle | Pourquoi |
|---|---|
| Ne pas supprimer des valeurs **en cours d'utilisation** | Peut casser des associations et laisser des enregistrements « orphelins » |
| Préférer **modifier** plutôt que supprimer + recréer | Conserve les associations existantes |
| Éviter les doublons | Confond les graphiques et recherches |
| Noms courts et cohérents | Améliore les filtres et la lecture |
| Vérifier après des importations en masse | Les données importées peuvent créer des doublons |

> 💡 Si une catégorie/éditeur est bloqué pour suppression, c'est parce que des enregistrements l'utilisent. Réassociez d'abord.

### 9.9 Version classique vs version nouvelle

Les deux font la même chose :

- **Classique** (`/admin/managelookuptables`) — onglets.
- **Nouvelle** (`/admin/managelookuptables-new`) — barre latérale avec groupes (Publications, Médias, IA) et cartes.

Utilisez celle que vous préférez ; les données sont les mêmes.

---

## 10. Administration {#10-administration}

> Les sections suivantes s'appliquent aux utilisateurs avec le rôle **Admin**. Un GeneralUser ne voit pas ces éléments dans le menu.

### 10.1 Utilisateurs

Page : **Maintenance → Utilisateurs** (`/admin/manageusers`).

**Liste des utilisateurs :** affiche utilisateur, rôle et actions.

**Créer un utilisateur :**

1. Cliquez sur **Nouveau**.
2. Remplissez nom, e-mail et mot de passe.
3. Choisissez le **rôle** : **Admin** ou **GeneralUser**.
4. **Enregistrez**.

**Modifier :**

1. Cliquez sur **Modifier** dans la ligne.
2. Changez les données/le rôle.
3. **Enregistrez**.

**Supprimer :**

1. Cliquez sur **Supprimer**.
2. Confirmez.

> ⚠️ Supprimer un utilisateur peut supprimer/masquer les données associées. Confirmez l'impact avant. Évitez de supprimer le dernier Admin.

**Bonnes pratiques :**

- Un Admin par personne, pas un compte partagé.
- GeneralUser pour l'usage quotidien.
- Désactiver/supprimer les comptes inactifs.
- Documenter qui a des privilèges.

### 10.2 Paramètres — vue d'ensemble

Page : **Paramètres** (`/admin/appsettings2`, dans le pied de page du menu). Il existe aussi la version classique (`/admin/appsettings`).

La version nouvelle organise tout dans une barre latérale avec des icônes et des cartes à droite. Les sections sont :

| Section | Contenu |
|---|---|
| **Téléchargements** | Dossier de téléchargements des médias |
| **Sauvegardes** | Dossier des sauvegardes d'uploads |
| **Stockage** | Dossiers de listes, images de l'app, couvertures de publications, images d'auteurs |
| **Images** | Configurations d'images |
| **Contacts** | E-mail du support |
| **Autres paramètres** | Temps de notification, organiser les fichiers, supprimer image après modification |
| **Langue** | Langue par défaut |
| **IA** | Paramètres communs, OpenRouter, Ollama (voir 10.3) |
| **WebApi** | Adresse de l'API |
| **Services** | Configuration des services |
| **Base de données** | Connexion/état |

> 💡 Les modifications des dossiers affectent **où les fichiers sont lus/écrits**. Changez avec prudence et confirmez que les dossiers existent.

### 10.3 Zone IA {#103-zone-ia}

La zone **IA** comprend trois sous-sections :

#### Paramètres communs

- **Raisonnement** (activer/désactiver) — commun aux deux chats (cloud et local). Quand activé, le raisonnement des modèles s'affiche dans le bloc « 💭 Raisonnement ».

#### OpenRouter

| Champ | À quoi ça sert |
|---|---|
| **Clé API** | Authentifie dans le cloud |
| **Modèle par défaut** | Modèle à utiliser par défaut |
| **Autoriser les modèles payants** | Si désactivé, n'utilise que des modèles gratuits |
| **Utiliser le cloud pour publications d'auteur** | Active la génération de publications d'auteur dans le cloud |
| **Limite quotidienne gratuite** | Maximum de requêtes gratuites par jour |
| **Cartes d'utilisation** | Consommation actuelle et historique |

**Notes :**

- Sans clé, le cloud ne fonctionne pas et tout passe sur Ollama.
- Avec **modèles payants désactivés**, l'application **n'utilise pas** de modèles payants : elle choisit des équivalents gratuits ou le routeur gratuit.
- La limite est **appliquée au serveur** ; quand elle est atteinte, les opérations cloud retournent « sans réponse » et l'application passe au local.

#### Ollama (local)

| Champ | À quoi ça sert |
|---|---|
| **Clé API** | Si votre Ollama l'exige |
| **Adresse de base** | Où Ollama tourne |
| **Nom du modèle** | Modèle local à utiliser |

**Notes :**

- Ollama doit être installé et en cours d'exécution sur la machine (ou accessible à l'adresse indiquée).
- Les modèles plus grands donnent de meilleurs résultats, mais exigent plus de mémoire et sont plus lents.

## 11. Rapports {#11-rapports}

L'application génère des rapports à partir de modèles (RDLC) et les présente dans son propre interface.

### 11.1 Rapport d'auteurs

Page : **`/AuthorsPreview_Report`**.

**Comment l'utiliser :**

1. Ouvrez la page de rapport.
2. Choisissez/écrivez les paramètres (par exemple, auteur ou intervalle).
3. Cliquez sur **Pré-visualiser**.
4. Le rapport est généré et affiché.
5. Utilisez les options d'**exportation** disponibles (PDF/Excel, selon le modèle).

**Ce qui vous attend :**

- Le rapport utilise les données auxquelles vous avez accès (isolation appliquée).
- Les rapports volumineux peuvent prendre du temps.
- S'il n'y a pas de données, le rapport peut apparaître vide.

### 11.2 Résolution de problèmes de rapports

| Symptôme | Cause probable | Solution |
|---|---|---|
| Rapport vide | Pas de données pour les paramètres | Élargir l'intervalle/limper les filtres |
| Erreur de génération | Modèle manquant ou données invalides | Vérifier les logs ; contacter l'Admin |
| Exportation échouée | Permissions ou format non supporté | Essayer un autre format ; vérifier le dossier |
| Démora excessive | Trop d'enregistrements | Restreindre les paramètres |

> 💡 Les modèles de rapports sont maintenus par l'Admin. Si un rapport est systématiquement incorrect, c'est là que la correction est apportée.

---

## 12. Outils {#12-outils}

### 12.1 Traducteur

Le traducteur apparaît comme **dialogue** dans plusieurs points de l'application et comme outil autonome.

**Fonctionnalités :**

- Choix de langue d'origine/destination.
- Progression avec **nom du modèle**.
- **Annuler** (arrête l'UI immédiatement).
- **Confirmer** (applique le texte).
- **Puce** avec le modèle qui a répondu.

**Bonnes pratiques :**

- Traduisez des phrases complètes, pas des fragments.
- Revoyez les noms propres et les termes techniques.
- Si vous annulez, le texte original reste intact.

### 12.2 Benchmark de l'IA

L'application collecte des métriques de performance des modèles (latence, tokens/seconde, utilisation) et les présente dans le Dashboard (section **Activité**) et dans les Paramètres (cartes d'utilisation).

**À quoi ça sert :**

- Comparer des modèles.
- Détecter la dégradation de performance.
- Aider à choisir le modèle par défaut.

> 💡 Le benchmark est une **outil d'observation** ; il ne modifie pas le comportement de l'application.

### 12.3 Chat comme outil de travail

En plus de la conversation, les chats peuvent aider dans les tâches du quotidien :

- Rédiger des synopses.
- Suggerer des catégories.
- Traduire des extraits.
- Résumer des textes collés.
- Corriger l'orthographe.

> 💡 Pour des tâches structurées, utilisez les fonctions dédiées (Résumé, Traduire, Métadonnées) — les résultats sont appliqués directement dans les champs.

### 12.4 Recettes et contenus auxiliaires

Des pages auxiliaires de recettes (`/recipes`, `/recipe_cards`) existent comme contenu de démonstration/exemple. Elles ne font pas partie du flux principal de catalogage.

---

## 13. Astuces et raccourcis {#13-astuces-et-raccourcis}

### 13.1 Productivité

- **Entrée** dans le chat envoie le message.
- **Ctrl+V** colle du texte/images dans les champs qui le supportent.
- Utilisez la **recherche des sélecteurs** (auteur, éditeur, catégorie) au lieu de parcourir de longues listes.
- Triez/filtrez la grille avant d'exporter ou de faire un rapport.
- Maintenez une **conversation par thème** dans le chat, pour que l'historique soit utile.

### 13.2 Qualité des données

- Remplissez toujours **Titre**, **Auteur** et **Catégorie**.
- Utilisez **ISBN** pour désambigüer.
- Évitez les doublons : recherchez avant de créer.
- Cohérence des noms (ex. : "J. R. R. Tolkien", pas trois variantes).
- Revoyez les indicateurs de **Santé** du Tableau de bord.

### 13.3 Fichiers et dossiers

- Noms sans caractères problématiques (`/ \ : * ? " < > |`).
- Un dossier par type (PDF, couvertures, audio, vidéo).
- Sauvegardes avant de grandes importations.
- Ne déplacez pas de fichiers hors de l'application sans mettre à jour les chemins.

### 13.4 IA

- Revoyez **toujours** ce que l'IA génère.
- Préférez les opérations dédiées pour des résultats structurés.
- Si le cloud est lent/indisponible, l'application utilise automatiquement le local.
- Sur les modèles gratuits, l'artefact « User Safety » peut apparaître — répondez « continue ».
- Si la réponse est vide, reformulez ou choisissez un autre modèle.

### 13.5 Sécurité

- Ne partagez pas la clé OpenRouter.
- Déconnectez-vous sur les ordinateurs partagés.
- Utilisez des mots de passe forts et uniques.
- Ne utilisez pas le compte Admin pour des tâches quotidiennes.
- Méfiez-vous des prompts/fichiers demandant des données sensibles.

### 13.6 Thème et accessibilité

- Le thème clair est meilleur pour les environnements lumineux ; le sombre pour les environnements sombres.
- La barre latérale et la barre supérieure sombres sont intentionnelles et ne changent pas avec le thème.
- Utilisez le zoom du navigateur (Ctrl + / Ctrl -) si le texte est trop petit.
- Les contrastes principaux respectent les normes d'accessibilité (WCAG AA).

### 13.7 Tableau des actions rapides

| Je veux… | Action |
|---|---|
| Ouvrir une publication rapidement | Rechercher dans la grille de publications |
| Ouvrir ce manuel | Bouton d'aide (`?`) dans la barre supérieure |
| Changer de langue | Paramètres → Langue (la barre supérieure ne montre que la langue actuelle) |
| Changer de thème | Icône soleil/lune |
| Annuler une traduction | Bouton **Annuler** dans le dialogue |
| Copier une réponse IA | Icône de copie dans le message |
| Évaluer une réponse | 👍 / 👎 dans le message |
| Supprimer l'historique | Historique → Supprimer l'historique |
| Voir les erreurs récentes | Dashboard → Audits / Logs |
| Faire une sauvegarde | Sauvegarde → Base de données + Uploads |

---

## 14. Résolution de problèmes {#14-résolution-de-problèmes}

Cette section couvre les problèmes les plus courants. Les symptômes sont décrits comme l'utilisateur les voit.

### 14.1 Authentification et session

| Symptom | Cause probable | Solution |
|---|---|---|
| "Identifiants invalides" | Email/mot de passe erronés | Confirmer majuscules et clavier ; récupérer le mot de passe |
| Session expirée rapidement | Politique de sécurité | Reconnectez-vous ; contactez l'Admin |
| Redirection vers le login sur toutes les pages | Cookie/session invalide | Nettoyer les cookies du site ; reconnectez-vous |
| Pas d'email de récupération | SMTP non configuré/spam | Vérifier le dossier spam ; contacter l'Admin |

### 14.2 Données manquantes

| Symptom | Cause probable | Solution |
|---|---|---|
| Publications/auteurs « disparus » | Compte erroné (isolation par utilisateur) | Confirmer l'utilisateur ; demander à un Admin de vérifier |
| Grille vide mais enregistrements existants | Filtre/recherche activé | Nettoyer les filtres et la recherche |
| Graphiques vides | Catégories de classification non remplies | Remplir catégorie/format/éditeur |
| Enregistrements d'un autre utilisateur invisibles | Comportement attendu pour GeneralUser | Demander un accès Admin, si nécessaire |

### 14.3 IA sans réponse

| Symptom | Cause probable | Solution |
|---|---|---|
| Traduction/résumé ne renvoie rien | Cloud indisponible et Ollama arrêté | Vérifier Ollama ; réessayer |
| "Limite quotidienne atteinte" | Cap des modèles gratuits | Attendre le lendemain ou demander une augmentation/modèles payants à l'Admin |
| Chat cloud avec erreur | Sans clé/limite/instabilité | Vérifier la clé dans Paramètres ; réessayer plus tard ; utiliser le chat local |
| Réponse vide | Modèle sans contenu | Reformuler ; choisir un autre modèle |
| "User Safety: safe;" | Artefact du modèle | Répondre « continue » ou reformuler |
| Traduction très lente | Modèle local grand | Annuler ; utiliser le cloud ou un modèle plus petit |
| Raisonnement absent | Option désactivée | Activer « Raisonnement » dans Paramètres communs |
| Modèle inattendu | Sweep/fallback automatique | Vérifier la puce du modèle ; fixer le modèle par défaut dans Paramètres |

### 14.4 Fichiers

| Symptom | Cause probable | Solution |
|---|---|---|
| "Fichier non trouvé" | Fichier déplacé/supprimé | Page `/pubsnotfoundondisk` ; corriger le chemin ou recharger |
| Chemin incorrect | Dossier changé | Page `/pubswithwrongpath` ; mettre à jour le chemin |
| Couverture absente | Image manquante/invalide | Recharger l'image |
| Échec du téléchargement | Taille/format/permissions | Réduire le fichier ; vérifier le format ; vérifier les logs |
| Lecteur ne lit pas | Codec/fichier invalide | Convertir au format supporté |

### 14.5 Rapports

| Symptom | Cause probable | Solution |
|---|---|---|
| Rapport vide | Pas de données/filtrage | Élargir les critères |
| Erreur de génération | Modèle/données | Vérifier les logs ; contacter l'Admin |
| Exportation échouée | Permissions/dossier | Essayer un autre format/dossier |

### 14.6 Environnement local (technique, pour Admin)

| Symptom | Cause probable | Solution |
|---|---|---|
| Page non chargée / connexion refusée | API ou interface arrêtées | Démarrer les deux processus |
| Conflit de port 5001 | API et interface utilisent le même port par omission | Utiliser des profils/ports différents (ex. : API 4100, interface 4110) |
| Erreur de certificat dans le navigateur | Certificat auto-signé | Accepter le certificat ; ne pas activer HSTS tant qu'un certificat de confiance n'est pas présent |
| `dotnet build` échoue avec MSB3027/MSB3021 | Application en cours d'exécution bloque la DLL | Arrêter l'application et recompiler |
| Restore échoue | Source NuGet/Syncfusion | Vérifier la configuration de la source et la clé |
| Connexion SQL échoue avec `Trusted_Connection` | App pool avec un nom différent du login | Aligner le nom de l'app pool avec le login SQL |

> 💡 Les détails de déploiement, de ports et de SQL sont documentés dans les notes techniques du projet (`AGENTS.md`), pas dans ce manuel d'utilisation.

### 14.7 Quand demander de l'aide

Recueillez et envoyez :

1. **Ce que vous faisiez** (page et action).
2. **Ce que vous attendiez** et **ce qui s'est passé**.
3. **Message d'erreur** (texte ou capture).
4. **Date/heure** approximative.
5. **Utilisateur** (sans envoyer de mots de passe).
6. Si possible, les **logs** correspondants.

> ⚠️ Ne jamais envoyer de mots de passe ni la clé OpenRouter dans des captures d'écran.

---

## 15. Glossaire {#15-glossaire}

| Terme | Signification |
|---|---|
| **Admin** | Utilisateur avec accès total, y compris l'administration |
| **GeneralUser** | Utilisateur normal ; ne voit que ses propres données |
| **Publication** | Enregistrement d'une publication/document |
| **Média** | Enregistrement d'audio, vidéo ou YouTube |
| **Playlist** | Collection ordonnée d'articles média |
| **Table d'appui / auxiliaire** | Liste de valeurs utilisée par les sélecteurs (catégories, formats, éditeurs…) |
| **Lookup** | Synonyme de table d'appui |
| **OpenRouter** | Service cloud qui donne accès à divers modèles IA |
| **Ollama** | Service local qui exécute des modèles IA sur la machine |
| **Fallback** | Changement automatique du fournisseur principal vers un fournisseur alternatif en cas d'échec |
| **Cloud** | Fournisseur distant (OpenRouter) |
| **Local** | Fournisseur sur la machine (Ollama) |
| **Modèle** | Moteur IA (ex. : llama-3.3-70b) |
| **Puce du modèle** | Étiquette montrant quel modèle a répondu |
| **Prompt** | Instruction/texte qui guide l'IA |
| **Template de prompt** | Prompt réutilisable, modifiable dans l'Éditeur de prompts |
| **Streaming** | Réponse qui apparaît progressivement |
| **Raisonnement** | "Pensée" interne à certains modèles, affichée séparément de la réponse |
| **Historique** | Liste des conversations enregistrées |
| **Rating** | Classification personnelle (étoiles) |
| **Synopsis** | Résumé/description d'une publication |
| **ISBN** | Identifiant international de publication |
| **RDLC** | Format de modèle de rapport |
| **Sauvegarde** | Copie de sécurité |
| **Log** | Enregistrement d'événements/erreurs |
| **Isolation des données** | Règle empêchant un utilisateur de voir les données d'un autre |
| **Thème** | Apparence claire ou sombre |
| **Culture** | Langue/région de l'interface (pt, en, fr, es) |
| **Upload** | Chargement de fichiers vers l'application |
| **Download** | Transfert de fichiers de l'application |
| **Cap** | Limite maximale (ex. : requêtes gratuites par jour) |
| **Token** | Unité de texte utilisée par les modèles IA |
| **Latence** | Temps de réponse |
| **TPS / Tokens par seconde** | Vitesse de génération du modèle |

---

## 16. Scénarios complets (pas à pas) {#16-scenarios-complets-pas-a-pas}

Les scénarios suivants combinent plusieurs fonctionnalités et servent de « recette » pour des tâches réelles.

### 16.1 Cataloguer une publication à partir du ISBN

**Objectif :** créer un enregistrement complet, avec données officielles et couverture, en quelques minutes.

1. Ouvrez **Publications → Gestion** (`/publicacoes`).
2. Cliquez sur **Nouveau**.
3. Dans le champ **ISBN**, saisissez le numéro (avec ou sans tiret).
4. Ouvrez **Rechercher en ligne** et recherchez par ISBN ou par titre.
5. Choisissez le résultat correct dans la liste.
6. Confirmez les champs remplis : **Titre**, **Auteur**, **Éditeur**, **Année**, **Pages**, **Synopsis**.
7. Si l'**Auteur** manque, créez-le d'abord dans **Auteurs** et revenez en arrière (les données du formulaire sont perdues si vous sortez sans enregistrer — copiez ce qui est déjà présent).
8. Si **Catégorie**, **Format** ou **Emplacement du fichier** manquent, choisissez une option existante (ou demandez à l'Admin de les créer).
9. Si la **Synopsis** vient d'une autre langue, utilisez **Traduire** et confirmez.
10. Chargez la **couverture** (si la recherche ne l'a pas fournie).
11. Cliquez sur **Enregistrer**.
12. Retournez au Tableau de bord et confirmez que la publication apparaît dans les compteurs et que la section **Santé** ne la liste plus comme « sans couverture/pages ».

**Erreurs courantes :**

| Erreur | Cause | Correction |
|---|---|---|
| "Auteur requis" | Aucun auteur sélectionné | Choisir/créer un auteur |
| "Catégorie requise" | Aucune catégorie sélectionnée | Choisir une catégorie |
| "Année d'édition invalide" | Année avec moins/4 chiffres ou future | Corriger pour 4 chiffres ≤ l'année en cours |
| Enregistrement dupliqué | ISBN déjà existant | Rechercher avant de créer ; consolider |

### 16.2 Importer une colonne de PDFs en lot

**Objectif :** créer des enregistrements à partir de plusieurs fichiers, puis corriger ce qui manque.

1. Avant d'importer, faites un **backup** de la base de données (`/backupdatabase`).
2. Préparez les fichiers dans un dossier avec des noms cohérents (`ISBN - Titre.pdf`).
3. Dans **Publications → Gestion**, utilisez **Importer PDF/DOCX** et sélectionnez les fichiers.
4. Attendez le traitement (peut prendre du temps).
5. Ouvrez la grille et triez par **date** pour voir les nouveaux enregistrements.
6. Pour chaque enregistrement :
   - Confirmez **Titre** et **Auteur**.
   - Complétez **Catégorie**, **Éditeur**, **Format** et **Emplacement du fichier**.
   - Ajoutez **couverture** et **synopsis** (IA si nécessaire).
7. Enfin, ouvrez `/pubsnotfoundondisk` et `/pubswithwrongpath` et corrigez les chemins.
8. Revoyez la section **Santé** : doublons, sans couverture, sans pages.
9. Faites un nouveau backup.

**Conseils :**

- Importez en petits lots (10–20) pour ne pas perdre du travail si quelque chose échoue.
- Si un fichier n'est pas reconnu, ouvrez-le d'abord et confirmez qu'il n'est pas protégé/corrompu.
- Ne supprimez pas les fichiers d'origine avant d'avoir validé les enregistrements.

### 16.3 Préparer une fiche d'auteur complète

**Objectif :** auteur avec photo, biographie révisée et profil généré par IA.

1. Ouvrez **Publications → Auteurs** (`/autores`).
2. **Nouveau** → remplissez **Nom** → **Enregistrer**.
3. Ouvrez la fiche de l'auteur.
4. Chargez la **photo** (portrait, bonne résolution).
5. Écrivez une **biographie de base** courte (2–4 phrases) avec les faits que vous connaissez.
6. Utilisez **Générer profil IA** pour compléter.
7. **Revoyez** le résultat : noms, dates, œuvres et prix.
8. Corrigez ce qui est erroné et **Enregistrez**.
9. Ouvrez **Publications de l'auteur** et associez/créez les œuvres.
10. Si la biographie est dans une autre langue, utilisez **Traduire** et confirmez.

**Attention :** l'IA peut inventer des dates et des œuvres. Le profil doit être validé avant d'être partagé.

### 16.4 Construire une musicothèque et une playlist

**Objectif :** charger des albums, remplir les métadonnées et écouter dans une playlist.

1. Ouvrez **Médias → Gestion** (`/media`).
2. Cliquez sur **Importer** et sélectionnez les fichiers audio.
3. Pour chaque enregistrement créé, ouvrez **Modifier** et confirmez :
   - **Titre**, **Artiste/Autor**, **Format de média**, **Genre/Catégorie**, **Année**.
4. Utilisez **Métadonnées IA** / **Notes IA** pour compléter ce qui manque (revoyez toujours).
5. Ouvrez **Médias → Playlists** (`/media/playlists`).
6. **Nouveau** → donnez un nom (ex. : « Jazz — soirée ») → **Enregistrer**.
7. Ouvrez la playlist → **Ajouter des éléments** → sélectionnez les pistes.
8. Réordonnez si nécessaire.
9. Cliquez sur **Lire** pour ouvrir le **Lecteur de Playlist**.
10. Vérifiez que toutes les pistes jouent ; corrigez celles qui ne fonctionnent pas.

**Conseils :**

- Une playlist par contexte est plus utile qu'une liste gigantesque.
- Si une piste ne joue pas, confirmez le fichier dans le **Gestionnaire de fichiers**.
- Vérifiez le **format de média** correct pour que les graphiques du Dashboard soient exacts.

### 16.5 Ajouter et lire une vidéo YouTube

1. Ouvrez **Médias → Gestion**.
2. **Nouveau** → choisissez le type **YouTube**.
3. Remplissez **Titre**, **Artiste/Artiste**, **Genre/Catégorie**, **Format de média**.
4. Collez le **lien** de la vidéo (URL complète).
5. **Enregistrez**.
6. Dans la grille, cliquez sur **▶** pour lire.
7. Si le lecteur affiche une erreur, confirmez :
   - Le lien est correct et la vidéo n'est pas privée.
   - La vidéo n'a pas été supprimée.
8. Pour organiser, ajoutez l'élément à une **playlist** (les playlists peuvent mélanger audio, vidéo et YouTube selon la configuration).

### 16.6 Traduire et résumer sans perdre l'original

**Objectif :** utiliser l'IA tout en conservant le texte original.

1. Ouvrez la publication/auteur et localisez le texte (sinopse/biographie).
2. **Copiez le texte original** vers un bloc-notes ou le chat (comme référence).
3. Ouvrez **Traduire**, choisissez les langues et cliquez **Traduire**.
4. Si cela prend du temps, cliquez **Annuler** — le champ reste intact.
5. Avec succès, comparez le résultat avec l'original.
6. Cliquez **Confirmer** seulement quand vous êtes satisfait.
7. Pour le résumé, utilisez **Résumé IA** et revoyez avant d'enregistrer.
8. Enregistrez le registre.

> ⚠️ **Confirmer** applique le texte au champ. Si vous voulez conserver les deux versions, sauvegardez l'original ailleurs avant.

### 16.7 Routine de sauvegarde et vérification

**Objectif :** garantir que vous pouvez récupérer le système.

1. Ouvrez `/backupdatabase` et générez le backup de la base de données.
2. Ouvrez `/backupuploads/1`, `/backupuploads/2` et `/backupuploads/3` et générez les backups d'uploads.
3. Copiez les fichiers résultants **hors de la machine** (disque externe/cloud).
4. Enregistrez la date et ce qui a été inclus.
5. Périodiquement, **testez le restauration** sur une copie — un backup non testé n'est pas un backup.
6. Confirmez que les chemins des dossiers dans **Paramètres** restent corrects après tout changement.

### 16.8 Récupérer d'une modification de dossiers

**Objectif :** après avoir déplacé des fichiers, revenir à tout fonctionner.

1. Mettez à jour les dossiers dans **Paramètres** (Stockage, Images, Téléchargements, Sauvegardes).
2. Ouvrez `/pubsnotfoundondisk` et voyez ce qui manque de fichier.
3. Ouvrez `/pubswithwrongpath` et corrigez les chemins.
4. Dans le **Gestionnaire de fichiers**, confirmez que les uploads sont visibles.
5. Ouvrez quelques publications et confirmez que **Lire PDF** fonctionne.
6. Ouvrez les galeries et confirmez que les images apparaissent.
7. Faites un nouveau backup.

---

*Fin du manuel. Pour des détails techniques d'installation, de déploiement et de configuration du serveur, consultez la documentation technique du projet.*
