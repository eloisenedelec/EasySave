using EasySave.Models;

namespace EasySave.Repositories
{
    public class JsonBackupRepository : IBackupRepository
    {
        private string _filePath;

        public JsonBackupRepository(string filePath) { // TODO }

        public void Save(List<BackupJob> jobs) { // TODO }
        public List<BackupJob> Load() { throw new NotImplementedException(); } // TODO
        public bool Delete(int id) { throw new NotImplementedException(); } // TODO
    }
}
