# Release Notes - EasySave
__Éditeur :__ ProSoft

__Date :__ 29 Avril 2026

__Produit :__ EasySave

---

## EasySave Version 3.0 (Mise à jour majeure)

Cette nouvelle version majeure introduit une refonte complète du moteur d'exécution en y intégrant le parallélisme, un contrôle en temps réel et une orchestration intelligente des ressources.

* __Sauvegardes en parallèle (Multi-threading) :__ Abandon du mode séquentiel au profit d'une exécution simultanée des travaux via `Task.Run()`, optimisant l'utilisation des processeurs multi-cœurs.
* __Contrôle en temps réel (Play / Pause / Stop) :__ Intégration de commandes interactives permettant de suspendre, reprendre ou arrêter définitivement n'importe quelle tâche de sauvegarde à chaud, avec un suivi de progression en pourcentage.
* __Gestion globale des priorités :__ Déploiement du `GlobalPriorityTracker` qui bloque automatiquement la copie des fichiers normaux tant qu'il reste des extensions critiques en attente dans n'importe quel autre travail.
* __Régulation des fichiers volumineux :__ Protection de la bande passante grâce à un sémaphore limitant à un seul slot le transfert simultané des fichiers supérieurs à un seuil paramétrable (_n_ Ko), sans bloquer les fichiers légers.
* __Auto-Pause Métier intelligente :__ Amélioration du module de surveillance qui met désormais en pause les transferts dès la détection d'un logiciel métier (après finalisation du fichier en cours) et redémarre automatiquement les jobs dès sa fermeture.
* __CryptoSoft Mono-instance :__ Sécurisation de l'outil de chiffrement externe via un Mutex système global pour interdire toute exécution simultanée conflictuelle.
* __Centralisation des logs (Docker) :__ Création d'un service d'agrégation de logs en temps réel sous Docker (ASP.NET Core), offrant à l'utilisateur le choix entre un stockage local, centralisé, ou hybride.

---

## EasySave Version 2.0 (Mise à jour majeure)

Cette nouvelle version majeure transforme l'expérience utilisateur et renforce la sécurité et la flexibilité de vos sauvegardes.

* __Nouvelle interface graphique :__ Remplacement complet de l'ancienne interface console par une interface graphique moderne et intuitive (WPF).
* __Travaux de sauvegarde illimités :__ Levée de l'ancienne restriction limitant le système à 5 travaux. Vous pouvez désormais créer et gérer un nombre illimité de tâches.
* __Chiffrement intégré (CryptoSoft) :__ Ajout du chiffrement à la volée. Vous pouvez définir une liste d'extensions spécifiques (ex: .pdf, .txt) qui seront automatiquement chiffrées lors de la copie pour sécuriser vos données sensibles. Le temps de chiffrement est également ajouté aux logs.
* __Surveillance des logiciels métiers :__ Détection en temps réel des logiciels critiques définis par l'utilisateur. Si le logiciel métier est en cours d'exécution, le système met automatiquement les sauvegardes en pause pour éviter les conflits.

---

## EasySave Version 1.1 (Mise à jour mineure)

Cette mise à jour apporte plus de flexibilité dans l'analyse de vos données de sauvegarde.

* __Nouveau format de log (XML) :__ En plus du format JSON natif, le système permet désormais de générer les fichiers journaux (logs) au format XML, selon le choix de l'utilisateur dans les paramètres.
---
L'équipe technique ProSoft reste à votre disposition pour tout accompagnement lors du passage à ces nouvelles versions.
