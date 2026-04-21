using EasySave.Controllers;
using EasySave.Views;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            Vue vue = new Vue();
            Controller controller = new Controller(vue);

            // Si lancé sans arguments, on passe en mode interactif
            if (args.Length == 0)
            {
                while (true)
                {
                    vue.DisplayHeader();
                    vue.DisplayMenu();
                    string input = Console.ReadLine();

                    if (input?.ToUpper() == "Q") break;

                    controller.ExecuteSelection(input);

                    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                    Console.ReadKey();
                }
            }
            else
            {
                // Mode ligne de commande direct
                controller.ExecuteSelection(args[0]);
            }
        }
    }
}