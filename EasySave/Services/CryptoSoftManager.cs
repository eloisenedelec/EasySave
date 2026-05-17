using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace EasySave.Services
{
    public class CryptoSoftManager
    {
        // Mutex interne : sérialise les appels venant des jobs parallèles d'EasySave.
        // CryptoSoft.exe est mono-instance côté machine entière via son propre Named Mutex.
        private static readonly Mutex _internalMutex = new Mutex(false, @"Global\EasySave_CryptoSoft_Mutex");

        private const int ExitCodeBusy    = 2;
        private const int MaxRetries      = 10;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

        public void EncryptFile(string sourceFile, string destinationFile)
        {
            bool acquired = _internalMutex.WaitOne(TimeSpan.FromMinutes(5));
            if (!acquired)
                throw new TimeoutException("CryptoSoft : délai d'attente dépassé (5 min).");

            try
            {
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CryptoSoft.exe");

                if (!File.Exists(exePath))
                    throw new FileNotFoundException($"CryptoSoft.exe introuvable : {exePath}");

                for (int attempt = 1; attempt <= MaxRetries; attempt++)
                {
                    var (exitCode, stderr) = RunCryptoSoft(exePath, sourceFile, destinationFile);

                    if (exitCode == 0)
                        return;

                    if (exitCode == ExitCodeBusy)
                    {
                        Thread.Sleep(RetryDelay);
                        continue;
                    }

                    string detail = string.IsNullOrWhiteSpace(stderr) ? "" : $" — {stderr.Trim()}";
                    throw new InvalidOperationException(
                        $"CryptoSoft a échoué (code {exitCode}){detail} sur : {sourceFile}");
                }

                throw new TimeoutException(
                    $"CryptoSoft occupé après {MaxRetries} tentatives : {sourceFile}");
            }
            finally
            {
                _internalMutex.ReleaseMutex();
            }
        }

        private static (int exitCode, string stderr) RunCryptoSoft(string exePath, string source, string destination)
        {
            using var process = new Process();
            process.StartInfo.FileName               = exePath;
            process.StartInfo.Arguments              = $"\"{source}\" \"{destination}\"";
            process.StartInfo.CreateNoWindow         = true;
            process.StartInfo.UseShellExecute        = false;
            process.StartInfo.RedirectStandardError  = true;

            process.Start();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return (process.ExitCode, stderr);
        }
    }
}
