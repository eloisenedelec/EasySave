using System;
using System.IO;
using System.Threading;

namespace EasySave.Models
{
    public class Model
    {
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }

        // Événement pour envoyer l'avancement à la Vue
        public delegate void ProgressHandler(string fileName, int current, int total);
        public event ProgressHandler OnProgress;

        public void Execute()
        {
            // 1. Vérifications de sécurité
            if (!Directory.Exists(SourcePath))
                throw new DirectoryNotFoundException($"La source n'existe pas : {SourcePath}");

            if (!Directory.Exists(TargetPath))
                Directory.CreateDirectory(TargetPath);

            // 2. Récupération de la liste des fichiers
            string[] files = Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories);
            int totalFiles = files.Length;

            if (totalFiles == 0) throw new Exception("Le dossier source est vide !");

            // 3. Boucle de copie
            for (int i = 0; i < totalFiles; i++)
            {
                string sourceFile = files[i];
                // On recrée la structure dans le dossier cible
                string relativePath = Path.GetRelativePath(SourcePath, sourceFile);
                string destFile = Path.Combine(TargetPath, relativePath);

                // Créer le sous-dossier si nécessaire
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));

                // COPIE RÉELLE
                File.Copy(sourceFile, destFile, true);

                // 4. Notification à la Vue (pour la barre de progression)
                OnProgress?.Invoke(Path.GetFileName(sourceFile), i + 1, totalFiles);

                // Petit ralentissement pour avoir le temps de voir la barre (optionnel)
                Thread.Sleep(200);
            }
        }
    }
}