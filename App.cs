using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using GitAutoUpdater.Core;
using GitAutoUpdater.Schemas;
using Microsoft.Extensions.Configuration;

namespace GitAutoUpdater
{
    class App
    {
        static void Main()
        {
            // Exit Handler
            Console.CancelKeyPress += (sender, e) =>
            {
                Logger.Log("Saliendo...", Logger.LogLevel.Warning);
                Environment.Exit(0);
            };

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
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                .Build();

            var appSettings = config.GetSection("AppSettings").Get<AppSettings>();

            /*
             * Iniciador de los Logs.
             */

            string logsDir = Path.Combine(baseDir, "Logs");
            Logger.Init(logsDir, appSettings.LogFile);
            Logger.Log("Configuración cargada.", Logger.LogLevel.Success);
            

            var gitSettings = appSettings.GitSettings;

            // Verificar si Git esta instalado
            if (!GitService.Installed())
            {
                // Si git no esta instalado, muestra error y sale del programa
                Logger.Log("Error: Git no se encuentra instalado.", Logger.LogLevel.Error);
                Logger.Log("Por favor instale Git antes de usar el Software.", Logger.LogLevel.Warning);
                return;
            }

            if (gitSettings.LocalPath == "Default" || gitSettings.LocalPath == "./")
            {
                gitSettings.LocalPath = Path.Combine(baseDir, "Repository");
            }

            Logger.Log("Auto Updater Inicializado.", Logger.LogLevel.Success);

            // -----------------------------------------------------------------------------
            
            // Funciones principales

            /*
             * Nota: si falla y no clona me voy a cagar en toda su re puta madre
             */

            string gitFolder = Path.Combine(gitSettings.LocalPath, ".git");

            if (!Directory.Exists(gitSettings.LocalPath) || !Directory.Exists(gitFolder))
            {
                Logger.Log("El repositorio local no existe.", Logger.LogLevel.Warning);
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
                        timer.Stop("No hay actualizaciónes.", Logger.LogLevel.Success);
                    }
                }
                catch (Exception ex) 
                {
                    /*
                     * Nota: Algun dia serviras, por ahora solo SIGUE ESPERANDO.
                     */
                    Logger.Log("Error: " + ex.Message, Logger.LogLevel.Error);
                }

                // Looper para la re-ejecución
                Thread.Sleep(appSettings.IntSeconds * 1000);
            }
        }
    }
}
