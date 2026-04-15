using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using СourseworkApp.Services;

namespace СourseworkApp.Helpers
{
    public class Logger : ILogger
    {
        private readonly string _logFilePath;

        public Logger()
        {
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"app_{DateTime.Now:yyyyMMdd}.log");
            var logDir = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
        }

        public void Info(string message)
        {
            Log("INFO", message);
        }

        public void Warning(string message)
        {
            Log("WARNING", message);
        }

        public void Error(string message)
        {
            Log("ERROR", message);
        }

        private void Log(string level, string message)
        {
            var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
            File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
    }
}
