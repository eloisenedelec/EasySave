#  README - EasySave v1.0

## Description du Projet
EasySave est un logiciel de sauvegarde en console développé en C# (.NET 8.0). 
Il permet de créer jusqu'à 5 travaux de sauvegarde (complète ou différentielle) et de les exécuter de manière séquentielle ou individuelle.
A travers ce projet nous souhaitons mettre un pratique des concepts avancés de programmation orientée objet tel que l'utilisation de 
Design pattern et le respect des principes SOLID.  

## Architecture du projet
### Structure des projets 
EasySave/

├── EasyLog.Contracts/       (Bibliothèque de classes - Interfaces)

├── EasyLog/                 (Bibliothèque de classes - DLL de logging)

├── EasySave/                (Application console - Programme principal)

### Dépendances
EasyLog (DDL) dépend de EasyLog.Contracts(Interfaces)

EasySave (Console App) dépend de EasyLog.Contracts(Interfaces)

## Choix Architecturaux
### Séparation en 3 Projets
EasyLog.Contracts (Bibliothèque d'interfaces)

__Rôle :__ Contient uniquement l'interface IBackupObserver

__Raisonnement :__
- Évite les dépendances circulaires (EasySave ↔ EasyLog)
- Permet à EasyLog d'être réutilisable dans d'autres projets sans embarquer EasySave
- Respecte le principe D de SOLID (Dependency Inversion)

EasyLog (DLL de logging)

__Rôle :__ Gestion des logs journaliers au format JSON
Pourquoi une DLL séparée ?

__Raisonnement :__ 
- D'autres projets peuvent utiliser cette DLL
- Maintenabilité : Évolutions du système de log sans toucher EasySave
- Conformité au cahier des charges : "Il vous est demandé de développer cette fonctionnalité dans une Dynamic Link Library nommée EasyLog.dll"

EasySave (Application principale)

__Rôle :__ Interface console, logique métier, orchestration

### Architecture en Couches (Layered Architecture)
Couche Présentation : UI (ConsoleUI)

Couche Logique Métier : Services (BackupManager, BackupExecutor, StateManager)

Couche Algorithmes :  Strategies + Factories 

Couche Persistance : Repositories

Couche Données : Models

__Raisonnement :__
- Séparation des responsabilités : Chaque couche a un rôle précis
- Testabilité : Chaque couche peut être testée indépendamment
- Évolutivité : Facilite la migration vers MVVM (v2.0 avec GUI)

__Prévision du passage à une architecture MVVM :__
Code réutilisable :
- Models
- Strategies
- Factories
- Repositories
- Services (BackupManager, BackupExecutor)
- EasyLog (DLL complète)

A remplacer :
- ConsoleUI → ViewModels + Views (WPF/XAML)

## Description du Projet
### Singleton Pattern
__Appliqué à :__ Logger, StateManager, BackupManager, LanguageManager

__Raisonnement :__
- Garantit une seule instance dans toute l'application
- Évite les conflits d'accès aux fichiers (logs, state.json, backups.json)
- Point d'accès global cohérent

__Avantages :__
- Thread-safe (compatible multi-threading futur)
- Initialisation lazy (créée seulement si nécessaire)
- Contrôle strict de l'instance

### Strategy Pattern
__Appliqué à :__ IBackupStrategy, FullBackupStrategy, DiffBackupStrategy

__Raisonnement :__
- Deux algorithmes différents (sauvegarde complète vs différentielle)
- Permet d'ajouter facilement un 3ème type (ex: sauvegarde incrémentale)
- Respecte le principe Open/Closed de SOLID

__Avantages :__
- Code client (BackupExecutor) ne change jamais
- Algorithmes isolés et testables indépendamment
- Extensibilité garantie

### Factory Pattern
__Appliqué à :__ BackupStrategyFactory

__Raisonnement :__
- Centralise la logique de création des strategies
- Évite la duplication de code if/else partout

__Avantages :__
- Un seul endroit à modifier pour ajouter un nouveau type
- Validation centralisée (exception si type inconnu)

### Repository Pattern
__Appliqué à :__ IBackupRepository, JsonBackupRepository

__Raisonnement :__
- Sépare la logique métier de la persistance des données
- Facilite les tests (création de MockBackupRepository)
- Permet de changer de format de stockage sans toucher BackupManager

__Avantages :__
- BackupManager dépend de l'interface, pas de l'implémentation
- Demain : passer de JSON à SQL en changeant 1 ligne de code

### Observer Pattern
__Appliqué à :__ IBackupObserver, Logger, StateManager

__Raisonnement :__
Pendant une sauvegarde, plusieurs composants doivent être notifiés en temps réel :
- Logger → Écrire dans le fichier log journalier
- StateManager → Mettre à jour state.json
- Future : ProgressBarUI (v2.0 avec GUI)

__Avantages :__
- Découplage total (BackupExecutor ne connaît pas Logger directement)
- Facilité d'ajout d'observers (ex: barre de progression GUI)
- Notifications temps réel sans polling

## Fichiers Générés (AppData)
Le logiciel génère automatiquement des fichiers dans : C:\Users\[User]\AppData\Roaming\EasySave\

backups.json (Configuration des travaux)
[

  {
  
    "id": 1,
  
    "name": "Documents Backup",
    
    "sourcePath": "C:\\Users\\User\\Documents",
    
    "targetPath": "D:\\Backups\\Documents",
    
    "type": "Differential"
  
  }

]

state.json (État temps réel)

{

  "Documents Backup": {
  
    "JobName": "Documents Backup",
    
    "Timestamp": "2024-04-21T14:32:15",
    
    "Status": "Active",
    
    "TotalFiles": 150,
    
    "FilesProcessed": 87,
    
    "FilesRemaining": 63,
    
    "CurrentSourceFile": "C:\\Users\\User\\Documents\\rapport.pdf"
  
  }

}

logs/YYYY-MM-DD.json (Log journalier)

[

  {
  
    "Timestamp": "2024-04-21T14:30:00",
    
    "JobName": "Documents Backup",
    
    "SourceFile": "C:\\Users\\User\\Documents\\notes.txt",
    
    "TargetFile": "D:\\Backups\\Documents\\notes.txt",
    
    "FileSize": 2048,
    
    "TransferTimeMs": 15
  
  }

]

##  Multilangue
### Système de Traduction
Fichiers de langue dans Resources/ :
- lang_fr.json : Français
- lang_en.json : English

Gestion centralisée via LanguageManager (Singleton)

### Avantages :
- Facile d'ajouter une langue (créer lang_es.json)
- Changement de langue à chaud (sans redémarrer)
- Séparation du code et du contenu textuel

## Principes SOLID
### Single Responsibility
Chaque classe a une seule responsabilité (Logger → logs, StateManager → état)
### Open/Closed
Strategy Pattern : ajout de nouveaux types sans modifier le code existant
### Liskov Substitution
Toutes les strategies sont interchangeables via IBackupStrategy
### Interface Segregation
Interfaces petites et spécifiques (IBackupObserver, IBackupRepository)
### Dependency Inversion
Dépendance aux abstractions (interfaces) plutôt qu'aux implémentations concrètes
