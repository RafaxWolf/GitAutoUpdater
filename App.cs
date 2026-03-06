using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using GitAutoUpdater.Core;
using GitAutoUpdater.Schemas;
using Microsoft.Extensions.Configuration;

namespace GitAutoUpdater
{
    class App
    {
        static void Main()
        {
            // Inicio del Software
            Console.CancelKeyPress += (sender, e) =>
            {
                Logger.Log("[!] Saliendo...");
                Environment.Exit(0);
            };

            Console.WriteLine("[/] Iniciando Auto Updater...");

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                .Build();

            var appSettings = config.GetSection("AppSettings").Get<AppSettings>();
            Console.WriteLine("[+] Configuración cargada.");

            var gitSettings = appSettings.GitSettings;
            if (!GitService.Installed())
            {
                Console.WriteLine("[!] Error: Git no se encuentra instalado");
                return;
            }

            string baseDir = AppContext.BaseDirectory;
            if (gitSettings.LocalPath == "Default" || gitSettings.LocalPath == "./")
            {
                gitSettings.LocalPath = Path.Combine(baseDir, "Repository");
            }

            Logger.Init(gitSettings.LocalPath, appSettings.LogFile);
            Logger.Log("[+] Auto Updater Inicializado.");
            // -----------------------------------------------------------------------------
            
            // Funciones principales
            if (!Directory.Exists(gitSettings.LocalPath))
            {
                GitService.Clone(gitSettings, baseDir);
            }

            while (true)
            {
                try
                {
                    Logger.Log("[/] Verificando por Actualizaciones...");

                    if (GitService.Updates(gitSettings))
                    {
                        Logger.Log("[!] Actualización detectada.");

                        string result = GitService.Pull(gitSettings);
                        Logger.Log(result);
                        Logger.Log("[+] Repositorio local Actualizado.");
                    }
                    else
                    {
                        Logger.Log("[+] No hay actualizaciónes.");
                    }
                }
                catch (Exception ex) 
                {
                    Logger.Log("[!] Error: " + ex.Message);
                }

                Thread.Sleep(appSettings.IntSeconds * 1000);
            }
        }
    }
}
