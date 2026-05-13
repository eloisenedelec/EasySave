using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasySave.Services;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave.Tests
{
    [TestClass]
    public class GlobalPriorityTrackerTests
    {
        [TestInitialize]
        public void Setup()
        {
            GlobalPriorityTracker.Instance.ResetForTests();
        }

        [TestMethod]
        public async Task Test_Priorite_Bloque_Fichier_Normal()
        {
            var tracker = GlobalPriorityTracker.Instance;
            bool normalFileStarted = false;

            tracker.RegisterJobPriorityFiles(1, 1);

            var taskNormal = Task.Run(() =>
            {
                tracker.WaitForPriorityIfNeeded(CancellationToken.None);
                normalFileStarted = true;
            });

            await Task.Delay(500);
            Assert.IsFalse(normalFileStarted, "Le fichier normal devrait être bloqué.");

            tracker.PriorityFileFinished(1);
            await taskNormal;
            Assert.IsTrue(normalFileStarted, "Le fichier normal devrait maintenant être libéré.");
        }
    }
}