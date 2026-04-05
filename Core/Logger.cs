using System;
using System.IO;

namespace GitAutoUpdater.Core
{
    public static class Logger
    {
        private static string _logPath;

        public enum LogLevel
        {
            Info,
            Process,
            Warning,
            Success,
            Error
        }

        /// <summary>
        /// Inicializador del sistema de Logs
        /// </summary>
        public static void Init(string folderPath, string logFile)
        {
            // Si la carpeta no existe, la crea.
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Path del Log
            _logPath = Path.Combine(folderPath, logFile);
        }

        /// <summary>
        /// Constructor de Logs
        /// Ingresa un nuevo registro en los Logs
        /// </summary>
        public static void Log(string msg, LogLevel type = LogLevel.Info, bool silent = false)
        {
            string prefix = type switch
            {
                LogLevel.Info => "[*]",
                LogLevel.Process => "[/]",
                LogLevel.Warning => "[!]",
                LogLevel.Success => "[+]",
                LogLevel.Error => "[-]",
                _ => "[*]"
            };

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Fecha y hora
            string logLine = $"[{timestamp}] {prefix} {msg}"; // Linea del Registro

            if (!silent)
            {
                // Colores segun el LogLevel
                ConsoleColor originalColor = Console.ForegroundColor;

                Console.ForegroundColor = type switch
                {
                    LogLevel.Info => ConsoleColor.Gray,
                    LogLevel.Process => ConsoleColor.Blue,
                    LogLevel.Warning => ConsoleColor.Yellow,
                    LogLevel.Success => ConsoleColor.Green,
                    LogLevel.Error => ConsoleColor.Red,
                    _ => ConsoleColor.White
                };

                Console.WriteLine(logLine);
                Console.ForegroundColor = originalColor;
            }
                

            // Si '_logPath' existe crea y/o edita el archivo de Logs
            if (!string.IsNullOrEmpty(_logPath))
                File.AppendAllText(_logPath, logLine + Environment.NewLine);
        }
    }
}
