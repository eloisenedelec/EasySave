# Manuel de Support Technique - EasySave v3.0

Ce document regroupe les informations techniques nécessaires au support client (Niveau 1 et 2) pour le dépannage, le diagnostic de performance et la configuration avancée du logiciel EasySave v3.0.

---

## 1. Emplacement des fichiers systèmes et paramètres généraux

L'application EasySave stocke l'intégralité de sa configuration et de son suivi localement dans le répertoire utilisateur (AppData).
* **Chemin absolu :** `C:\Users\[NomUtilisateur]\AppData\Roaming\EasySave\`

### Fichiers clés à inspecter :
* **`backups.json` :** Contient la configuration des travaux de sauvegarde créés par l'utilisateur.
* **`state.json` :** Fichier d'état en temps réel au format JSON. En v3.0, il suit dynamiquement l'avancement en pourcentage de chaque thread parallèle actif (`Running`, `Paused`, `Stopped`).
* **`settings.json` :** Fichier de configuration globale. En v3.0, de nouvelles clés critiques ont été ajoutées pour le moteur de règles :
  * `priorityExtensions` : Liste des extensions prioritaires au format JSON (ex: `[".docx",".xlsx"]`).
  * `largeFileThresholdKb` : Seuil en Ko définissant un fichier volumineux (ex: `"1000"`).
  * `businessAppName` : Nom exact du processus logiciel métier surveillé (ex: `"Calculator"`).
  * `logMode` : Mode d'export des logs (`"Local"`, `"Centralized"`, ou `"Both"`).
  * `logServerUrl` : URL pointant vers le conteneur Docker de centralisation (ex: `"http://192.168.1.100:5000"`).

---

## 2. Centralisation et formats des Logs (Service Docker)

EasySave v3.0 permet désormais de déporter et d'agréger les logs journaliers sur un serveur centralisé.

### En cas de logs manquants ou d'erreurs de transfert :
* **Vérification du Mode :** Assurez-vous que le paramètre `logMode` dans `settings.json` est bien positionné sur `Centralized` ou `Both`.
* **Diagnostic Réseau :** Tester l'accessibilité de l'API REST hébergée sous Docker via l'adresse configurée dans `logServerUrl`. Le service utilise le composant `HttpClient` d'EasyLog pour pousser les données.
* **Statut du Conteneur :** Vérifier que l'application ASP.NET Core `LogCentralizationService` tourne correctement sur le serveur Docker. Les requêtes cibles sont :
  * `POST /api/log` : Pour l'envoi en temps réel des entrées de sauvegarde par les clients.
  * `GET /api/log?date=YYYY-MM-DD` : Pour la récupération centralisée des logs d'une date spécifique.

---

## 3. Diagnostic du Parallélisme et Verrous de Flux

Le passage au multi-threading avec `Task.Run()` peut provoquer des comportements d'attente normaux mais confondus par les clients avec des blocages applicatifs.

### Problème : Un travail de sauvegarde semble figé à 0% ou n'avance pas
* **Vérification des Priorités globales :** EasySave intègre un `GlobalPriorityTracker`. Si un autre travail possède des fichiers prioritaires (ex: `.docx`) en attente de copie, **tous les fichiers normaux de tous les autres travaux sont mis en attente** via la méthode `WaitForPriorityIfNeeded()`. C'est un comportement normal. Attendre que les fichiers prioritaires se terminent.
* **Régulation de Bande Passante (Gros fichiers) :** Si deux travaux parallèles contiennent des fichiers dépassant le seuil `largeFileThresholdKb`, le `LargeFileCoordinator` bloque l'un des deux threads. Il utilise un `SemaphoreSlim(1, 1)` pour n'autoriser qu'un seul transfert lourd à la fois. Les fichiers légers, eux, doivent continuer à défiler normalement en tâche de fond.

---

## 4. Diagnostic des Logiciels Métiers (Auto-Pause & Reprise)

La détection d'un logiciel métier a évolué en v3.0 : elle ne bloque plus seulement le lancement, elle suspend les travaux en cours.

* **Mécanisme :** Le `ProcessMonitorService` scanne les processus actifs toutes les 1500 ms.
* **Comportement en cas de détection :** Dès que le processus métier (ex: `Calculator.exe`) est détecté, l'orchestrateur appelle `PauseAll()`. Le fichier en cours de copie se termine pour éviter toute corruption, puis la tâche se met en attente via `PauseEvent.Reset()`.
* **Résolution :** Demander à l'utilisateur de fermer complètement l'application métier. Le système détectera la fermeture et déclenchera automatiquement un `ResumeAll()` en activant `PauseEvent.Set()`, relançant immédiatement la copie.

---

## 5. Dépannage du Chiffrement Évolué (CryptoSoft Mono-Instance)

Le logiciel externe CryptoSoft.exe a été modifié pour devenir strictement **mono-instance système**.

* **Solution technique :** Un `Mutex` global nommé `Global\CryptoSoftMutex` est utilisé pour empêcher deux instances de chiffrer en même temps sur la même machine.
* **Symptôme de panne :** Si deux fichiers volumineux de deux travaux différents demandent un chiffrement simultané, le second thread attendra au niveau du Mutex (Timeout configuré à 5 minutes).
* **Action de support :** Si CryptoSoft reste bloqué indéfiniment, vérifiez via le Gestionnaire des tâches Windows qu'une instance fantôme de `CryptoSoft.exe` n'est pas restée active en tâche de fond suite à un crash précédent, bloquant ainsi le verrou du Mutex global. Si c'est le cas, tuez le processus `CryptoSoft.exe`.
