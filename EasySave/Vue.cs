using System;

namespace EasySave.Views
{
    public class Vue
    {
        public bool IsFrench { get; set; } = true; // Par défaut en Français

        public void DisplayHeader()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=====================================================");
            Console.WriteLine("                PROSOFT - EASYSAVE V1.0              ");
            Console.WriteLine("=====================================================");
            Console.ResetColor();
        }

        public void DisplayMenu()
        {
            if (IsFrench)
            {
                Console.WriteLine("\n[INSTRUCTIONS]");
                Console.WriteLine("- Pour lancer des travaux : Entrez les numéros (ex: 1-3 ou 1;3)");
                Console.WriteLine("- Pour quitter : Tapez 'Q'");
            }
            else
            {
                Console.WriteLine("\n[INSTRUCTIONS]");
                Console.WriteLine("- To run jobs: Enter numbers (e.g., 1-3 or 1;3)");
                Console.WriteLine("- To quit: Type 'Q'");
            }
            Console.Write("\n>> ");
        }

        public void DisplayProgress(string fileName, int current, int total)
        {
            // Simple barre de progression : [##########----------] 50%
            int percentage = (int)((double)current / total * 100);
            int progress = percentage / 5;
            string bar = new string('█', progress) + new string('-', 20 - progress);

            Console.Write($"\rProgress: [{bar}] {percentage}% | Current: {fileName}");
        }

        public void DisplaySuccess(string jobName)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n\n✓ {(IsFrench ? "Succès" : "Success")}: {jobName}");
            Console.ResetColor();
        }

        public void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[!] ERROR: {message}");
            Console.ResetColor();
        }

        public void DisplayJobAction(string jobName) => Console.WriteLine($"\nRunning Job: {jobName}...");
    }
}