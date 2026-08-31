using System.Security.Cryptography;
using GitAutoUpdater.Core;
using GitAutoUpdater.Core.Services;

namespace GitAutoUpdater
{
    class App
    {
        public static void ErrorHandler(string message)
        {
            Console.WriteLine();
            Logger.Log(message, Logger.LogLevel.Error);

            Console.WriteLine("Presiona ENTER para salir...");
            Console.ReadLine();

            Environment.Exit(1);
        }

        static void Main()
        {
            // Process Exit Handler
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                try
                {
                    GitService.KillActiveGitProcesses();
                }
                catch { }
            };

            // Exit Handler
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Cancelar el evento para evitar que el programa se cierre inmediatamente

                Logger.Log("Saliendo...", Logger.LogLevel.Warning);

                GitService.KillActiveGitProcesses();
                Environment.Exit(0);
                
            };

            // Task para el manejo de eventos de salida
            Task.Run(() =>
            {
                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if(key.Key == ConsoleKey.Q)
                        {
                            Logger.Log("Cancelando...", Logger.LogLevel.Warning);
                            GitService.ReqCancel();
                        }
                    }

                    Thread.Sleep(100);
                }
            });

            // Inicio del Software
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[/] Iniciando Auto Updater...");
            Console.ResetColor();

            // Variables Importantes
            var timer = new TimeWaiter();
            string baseDir = AppContext.BaseDirectory;

            /*
             * Cargador de la configuración
             */
            var appSettings = ConfigService.Load(baseDir);
            if (appSettings == null)
                return;

            /*
             * Iniciador de los Logs.
             */

            string logsDir = Path.Combine(baseDir, "Logs");
            Logger.Init(logsDir, appSettings.LogFile);
            Logger.Log("Iniciando Auto Updater...", Logger.LogLevel.Process, true);
            Logger.Log("Configuración cargada.", Logger.LogLevel.Success);
            Logger.Log($"Ruta de los Logs: {Path.Combine(logsDir, appSettings.LogFile)}");

            var gitSettings = appSettings.GitSettings;

            // Verificar si Git esta instalado
            if (!GitService.Installed())
            {
                // Si git no esta instalado, muestra error y sale del programa
                ErrorHandler("Git no se encuentra instalado o no esta en el PATH.");
            }

            string basePath;

            if (gitSettings.LocalPath == "Default" || gitSettings.LocalPath == "./")
                basePath = gitSettings.LocalPath = Path.Combine(baseDir, "Repository");
            else
                basePath = Path.Combine(baseDir, gitSettings.LocalPath);


            if (gitSettings.UseDedicatedFolder)
            {
                string repoName = GitService.GetRepoName(gitSettings.RepoUrl);
                gitSettings.LocalPath = Path.Combine(basePath, repoName);
            }

            Logger.Log("Ruta del repositorio local: " + gitSettings.LocalPath);

            Logger.Log("Auto Updater Inicializado.", Logger.LogLevel.Success);

            // -----------------------------------------------------------------------------
            
            // Funciones principales

            /*
             * Nota: si falla y no clona me voy a cagar en toda su re puta madre
             */

            //string gitFolder = Path.Combine(gitSettings.LocalPath, ".git");

            if (!GitService.IsRepoValid(gitSettings.LocalPath))
            {
                Logger.Log("El repositorio local no existe o es invalido.", Logger.LogLevel.Warning);
                GitService.Clone(gitSettings, baseDir);
            }
                
            /*
             * Loop principal.
             * Verifica cada <IntSeconds> en "settings.json"
             */
            while (true)
            {
                try
                {
                    timer.Start("Verificando por Actualizaciones...");

                    if (GitService.Updates(gitSettings))
                    {
                        // Detener timer de verificación
                        timer.Stop("Actualización detectada.", Logger.LogLevel.Warning);

                        // Actualizar repo e iniciar timer de actualización
                        timer.Start("Actualizando repositorio local...");

                        string result = GitService.Pull(gitSettings);
                        timer.Stop(result, Logger.LogLevel.Process);

                        Logger.Log("Repositorio local Actualizado.", Logger.LogLevel.Success);
                    }
                    else
                    {
                        timer.Stop("No hay actualizaciónes.");
                    }
                }
                catch (Exception ex) 
                {
                    Logger.Log(ex.Message, Logger.LogLevel.Error);
                }

                // Looper para la re-ejecución
                Thread.Sleep(appSettings.IntSeconds * 1000);
            }
        }
    }
}
