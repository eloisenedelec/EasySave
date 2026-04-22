namespace EasySave.Models
{
    public class BackupJob
    {
        private int _id;
        private string _name;
        private string _sourcePath;
        private string _targetPath;
        private BackupType _type;

        public BackupJob(int id, string name, string source, string target, BackupType type) { // TODO }

        public int GetId() { throw new NotImplementedException(); } // TODO
        public string GetName() { throw new NotImplementedException(); } // TODO
        public string GetSourcePath() { throw new NotImplementedException(); } // TODO
        public string GetTargetPath() { throw new NotImplementedException(); } // TODO
        public BackupType GetType() { throw new NotImplementedException(); } // TODO
    }
}
