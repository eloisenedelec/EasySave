using System.Diagnostics;
using System.IO;
using System.Text;

namespace EasySave.Services
{
    public class EncryptionService
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("EasySave");

        public long EncryptFile(string sourceFile, string targetFile)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                byte[] data = File.ReadAllBytes(sourceFile);
                for (int i = 0; i < data.Length; i++)
                    data[i] ^= Key[i % Key.Length];
                File.WriteAllBytes(targetFile, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EncryptionService] Erreur : {ex.Message}");
                return -1;
            }
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
    }
}
