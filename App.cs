using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace GitAutoUpdater
{
    class App
    {
        static void Main()
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                Logger.Log("[!] Saliendo...");
                Environment.Exit(0);
            };

            Console.WriteLine("[/] Iniciando GitAutoUpdater...");

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                .Build();
        }
    }
}
