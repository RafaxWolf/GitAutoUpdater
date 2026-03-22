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
                Logger.Log("[!] Saliendo...");
                Environment.Exit(0);
            };

            // Inicio del Software
            Console.WriteLine("[/] Iniciando Auto Updater...");

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
            Logger.Log("[+] Configuración cargada.");
            

            var gitSettings = appSettings.GitSettings;
            if (!GitService.Installed())
            {
                Logger.Log("[!] Error: Git no se encuentra instalado.");
                Logger.Log("[!] Por favor instale Git antes de usar el Software.");
                return;
            }

            if (gitSettings.LocalPath == "Default" || gitSettings.LocalPath == "./")
            {
                gitSettings.LocalPath = Path.Combine(baseDir, "Repository");
            }

            Logger.Log("[+] Auto Updater Inicializado.");

            // -----------------------------------------------------------------------------
            
            // Funciones principales

            /*
             * Nota: si falla y no clona me voy a cagar en toda su re puta madre
             */

            string gitFolder = Path.Combine(gitSettings.LocalPath, ".git");

            if (!Directory.Exists(gitSettings.LocalPath) || !Directory.Exists(gitFolder))
            {
                Logger.Log("[!] El repositorio local no existe.");
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
                        timer.Stop("[!] Actualización detectada.");

                        // Actualizar repo
                        timer.Start("Actualizando repositorio local...");
                        string result = GitService.Pull(gitSettings);
                        timer.Stop(result);

                        Logger.Log("[+] Repositorio local Actualizado.");
                    }
                    else
                    {
                        timer.Stop("[+] No hay actualizaciónes.");
                    }
                }
                catch (Exception ex) 
                {
                    /*
                     * Nota: Algun dia serviras, por ahora solo SIGUE ESPERANDO.
                     */
                    Logger.Log("[!] Error: " + ex.Message);
                }
                // Looper para la re-ejecución
                Thread.Sleep(appSettings.IntSeconds * 1000);
            }
        }
    }
}
