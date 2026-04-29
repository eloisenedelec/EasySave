using System;
using System.Diagnostics;

namespace EasySave.Services
{
    public class EncryptionService
    {
        public long EncryptFile(string sourceFile, string targetFile)
        {
            Stopwatch stopwatch = new Stopwatch();

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "CryptoSoft.exe",
                    Arguments = $"\"{sourceFile}\" \"{targetFile}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                stopwatch.Start();

                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                    }
                }

                stopwatch.Stop();

                return stopwatch.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EncryptionService] Erreur lors du chiffrement : {ex.Message}");
                return -1;
            }
        }
    }
}