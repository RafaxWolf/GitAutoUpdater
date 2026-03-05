using System;
using System.IO;

namespace GitAutoUpdater
{
    public static class Logger
    {
        private static string _logPath;

        public static void LoggerInit(string folderPath, string logFile)
        {
            // Si la carpeta no existe, la crea.
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Path del Log
            _logPath = Path.Combine(folderPath, logFile);
        }

        /// <summary>
        /// Constructor de los Logs
        /// Ingresa un nuevo registro en los Logs
        /// </summary>
        public static void Log(string msg)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Fecha y hora
            string logLine = $"[{timestamp}] {msg}"; // Linea del Registro

            Console.WriteLine(logLine); // Imprime en Consola

            // Si '_logPath' existe crea y/o edita el archivo de Logs
            if (!string.IsNullOrEmpty(_logPath))
                File.AppendAllText(_logPath, logLine + Environment.NewLine);
        }
    }
}
