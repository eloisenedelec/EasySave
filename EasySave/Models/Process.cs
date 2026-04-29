using System.Text.Json.Serialization;

namespace EasySave.Models
{
    public class Process
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public Process()
        {
        }

        public Process(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
