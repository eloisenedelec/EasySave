# Manuel de Support Technique - EasySave v1.1 & v2.0

Ce document regroupe les informations techniques nécessaires au support client (Niveau 1 et 2) pour le dépannage et la configuration avancée du logiciel EasySave.

## 1. Emplacement des fichiers systèmes

L'application EasySave ne requiert pas de base de données. L'intégralité de la configuration et du suivi est stockée localement dans le répertoire utilisateur (AppData). 

**Chemin absolu :** `C:\Users\[NomUtilisateur]\AppData\Roaming\EasySave\`

Ce répertoire contient les éléments suivants :
*   `backups.json` : Contient la configuration des travaux de sauvegarde créés par l'utilisateur.
*   `state.json` : Fichier d'état en temps réel (toujours au format JSON), mis à jour dynamiquement pendant l'exécution d'une sauvegarde.
*   `settings.json` : Fichier de configuration globale (Logiciels métiers, format des logs, extensions à chiffrer).
*   **Dossier `/logs/`** : Contient les journaux journaliers (nommés `YYYY-MM-DD.json` ou `YYYY-MM-DD.xml`).

## 2. Fichiers Logs (v1.1 et v2.0)

Suite à la mise à jour 1.1, l'utilisateur peut choisir d'exporter ses logs journaliers en **JSON** ou en **XML**. Ce paramètre est modifiable depuis l'interface graphique (v2.0).

**Nouveau paramètre de cryptage :**
Dans la version 2.0, les logs incluent le temps de cryptage d'un fichier (via CryptoSoft). Voici comment interpréter cette valeur lors d'un diagnostic :
*   `0` : Aucun cryptage n'a été appliqué sur ce fichier.
*   `> 0` : Le cryptage a réussi. La valeur indique le temps pris en millisecondes (ms).
*   `< 0` : Une erreur s'est produite lors du cryptage (ex: -1).

## 3. Diagnostic des Logiciels Métiers (Process Monitoring)

Si un client signale qu'une sauvegarde refuse de se lancer ou reste bloquée, vérifiez la configuration des "Logiciels Métiers".
*   L'application bloque la sauvegarde si un processus listé dans les paramètres est en cours d'exécution.
*   Pour vérifier le bon fonctionnement de cette sécurité avec le client, vous pouvez ajouter le processus `calculator` (la calculatrice Windows) dans la liste des logiciels métiers.
*   Si un arrêt d'urgence est déclenché par l'ouverture d'un logiciel métier, cet événement est explicitement consigné dans le log journalier.

## 4. Dépannage du Chiffrement (CryptoSoft)

L'intégration de CryptoSoft (v2.0) s'applique uniquement aux fichiers dont l'extension a été explicitement définie par l'utilisateur.
*   Si des fichiers ne sont pas chiffrés : Vérifiez que l'extension est bien présente dans les paramètres (avec le point, ex: `.pdf`, `.docx`).
*   CryptoSoft est un exécutable externe. Assurez-vous que l'antivirus du client ne bloque pas l'appel système (Process.Start) vers ce composant.
