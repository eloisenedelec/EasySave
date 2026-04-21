using System;
using System.Collections.Generic;
using EasySave.Models;
using EasySave.Views;

namespace EasySave.Controllers
{
    public class Controller
    {
        private readonly Vue _vue;
        private List<Model> _jobs;

        public Controller(Vue vue)
        {
            _vue = vue;
            // On initialise une liste de test
            // Dans le constructeur de Controller.cs
            _jobs = new List<Model> {
                new Model {
                    Name = "Backup_Test",
                    SourcePath = @"C:\Users\ruben\Desktop\Source",
                    TargetPath = @"C:\Users\ruben\Desktop\Target"
                }
            };
        }

        public void ExecuteSelection(string input)
        {
            // Simulation : on prend tous les jobs pour le test
            foreach (var job in _jobs)
            {
                // On s'abonne à l'événement de progression de la vue
                job.OnProgress += _vue.DisplayProgress;

                // --- CORRECTION ICI ---
                // On utilise DisplayHeader au lieu de PrintMessage
                _vue.DisplayHeader();
                _vue.DisplayJobAction(job.Name);

                try
                {
                    job.Execute();
                    _vue.DisplaySuccess(job.Name);
                }
                catch (Exception ex)
                {
                    _vue.DisplayError(ex.Message);
                }

                // On se désabonne pour le prochain job
                job.OnProgress -= _vue.DisplayProgress;
            }
        }
    }
}