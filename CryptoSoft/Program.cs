using System;
using System.IO;
using System.Threading;

if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: CryptoSoft.exe <source> <destination>");
    return 1;
}

string source      = args[0];
string destination = args[1];

const string MutexName = "CryptoSoft_MonoInstance";

Mutex? mutex = null;
try
{
    mutex = new Mutex(false, MutexName);

    bool acquired;
    try
    {
        acquired = mutex.WaitOne(0);
    }
    catch (AbandonedMutexException)
    {
        // Mutex abandonné par un run précédent : on en prend possession
        acquired = true;
    }

    if (!acquired)
    {
        Console.Error.WriteLine("CryptoSoft est déjà en cours d'exécution sur cet ordinateur.");
        return 2;
    }

    try
    {
        if (!File.Exists(source))
        {
            Console.Error.WriteLine($"Fichier source introuvable : {source}");
            return 3;
        }

        byte[] key  = System.Text.Encoding.UTF8.GetBytes("EasySave");
        byte[] data = File.ReadAllBytes(source);

        for (int i = 0; i < data.Length; i++)
            data[i] ^= key[i % key.Length];

        string? dir = Path.GetDirectoryName(destination);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllBytes(destination, data);
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Erreur chiffrement : {ex.Message}");
        return 3;
    }
    finally
    {
        mutex.ReleaseMutex();
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Erreur initialisation : {ex.Message}");
    return 3;
}
finally
{
    mutex?.Dispose();
}
