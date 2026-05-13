using Microsoft.AspNetCore.Mvc;
using EasyLog.Contracts;
using System.Text.Json;

namespace LogCentralizationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private static readonly object _fileLock = new object();
        private readonly string _logPath = "/app/logs/centralized_logs.json";

        [HttpPost]
        public IActionResult PostLog([FromBody] LogEntry entry)
        {
            try
            {
                lock (_fileLock)
                {
                    if (!Directory.Exists("/app/logs")) Directory.CreateDirectory("/app/logs");

                    // Ajoute ça pour que le terminal Docker parle :
                    Console.WriteLine($"[DOCKER] Nouveau log reçu pour le travail : {entry.JobName}");

                    string json = JsonSerializer.Serialize(entry);
                    System.IO.File.AppendAllText(_logPath, json + Environment.NewLine);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}