using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Services
{
    public class CryptoSoftManager
    {
        // Le préfixe "Global\" rend le Mutex visible pour toutes les sessions utilisateurs
        private static readonly Mutex _cryptoMutex = new Mutex(false, @"Global\EasySave_CryptoSoft_Mutex");

        public void EncryptFile(string sourceFile, string destinationFile)
        {
            // 1. On demande l'accès (on attend max 5 min si déjà utilisé)
            bool isAcquired = _cryptoMutex.WaitOne(TimeSpan.FromMinutes(5));

            try
            {
                if (!isAcquired)
                {
                    throw new TimeoutException("CryptoSoft est déjà utilisé par une autre instance.");
                }

                // 2. Lancement du processus externe
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "CryptoSoft.exe";
                    // On passe les arguments (source et destination) entre guillemets pour gérer les espaces
                    process.StartInfo.Arguments = $"\"{sourceFile}\" \"{destinationFile}\"";
                    process.StartInfo.CreateNoWindow = true; // Cache la console noire
                    process.StartInfo.UseShellExecute = false;

                    process.Start();
                    process.WaitForExit(); // On attend la fin du chiffrement avant de libérer le verrou
                }
            }
            finally
            {
                // 3. Quoi qu'il arrive, on libère le Mutex pour les autres jobs
                if (isAcquired)
                {
                    _cryptoMutex.ReleaseMutex();
                }
            }
        }
    }
}
