using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasySave.Models;
using EasySave.Observers;

namespace EasySave.Services
{
    public class StateManager : IBackupObserver
    {
        private static StateManager? _instance;
        private static readonly object _lock = new object();
        private readonly object _fileLock = new object();

        private readonly string _stateFilePath;
        private Dictionary<string, BackupState> _states;
        private string? _currentJobName;

        private StateManager()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string easySavePath = Path.Combine(appDataPath, "EasySave");

            Directory.CreateDirectory(easySavePath);

            _stateFilePath = Path.Combine(easySavePath, "state.json");
            _states = new Dictionary<string, BackupState>();
        }

        public static StateManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new StateManager();
                    }
                }
            }
            return _instance;
        }

        public void UpdateState(BackupState state)
        {
            _states[state.JobName] = state;
            SaveToFile();
        }

        public BackupState? GetState(string jobName)
        {
            if (_states.ContainsKey(jobName))
                return _states[jobName];
            return null;
        }

        public void OnBackupStarted(string jobName, int totalFiles, long totalSize)
        {
            _currentJobName = jobName;

            var state = new BackupState(jobName)
            {
                Status = JobStatus.Active,
                TotalFiles = totalFiles,
                TotalSize = totalSize,
                FilesProcessed = 0,
                FilesRemaining = totalFiles,
                SizeRemaining = totalSize,
                CurrentSourceFile = string.Empty,
                CurrentTargetFile = string.Empty
            };

            _states[jobName] = state;
            SaveToFile();
        }

        public void OnFileProcessed(string sourceFile, string targetFile, long fileSize, long transferTime, long encryptionTimeMs)
        {
            if (_currentJobName != null && _states.ContainsKey(_currentJobName))
            {
                var state = _states[_currentJobName];

                state.FilesProcessed++;
                state.FilesRemaining--;
                state.SizeRemaining -= fileSize;
                state.CurrentSourceFile = sourceFile;
                state.CurrentTargetFile = targetFile;
                state.Timestamp = DateTime.Now;

                SaveToFile();
            }
        }

        public void OnBackupCompleted(string jobName)
        {
            if (_states.ContainsKey(jobName))
            {
                var state = _states[jobName];
                state.Status = JobStatus.Completed;
                state.CurrentSourceFile = string.Empty;
                state.CurrentTargetFile = string.Empty;
                state.Timestamp = DateTime.Now;

                SaveToFile();
            }

            _currentJobName = null;
        }

        public void OnBackupError(string jobName, string error)
        {
            if (_states.ContainsKey(jobName))
            {
                var state = _states[jobName];
                state.Status = JobStatus.Error;
                state.Timestamp = DateTime.Now;

                SaveToFile();
            }
        }

        private void SaveToFile()
        {
            lock (_fileLock)
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(_states, options);
                    File.WriteAllText(_stateFilePath, json);
                }
                catch { }
            }
        }
    }
}
