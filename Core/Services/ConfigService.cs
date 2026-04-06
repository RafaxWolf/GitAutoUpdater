using GitAutoUpdater.Schemas;
using Microsoft.Extensions.Configuration;

namespace GitAutoUpdater.Core.Services
{
    public static class ConfigService
    {
        private static string ConfigPath => Path.Combine(AppContext.BaseDirectory, "settings.json");

        /// <summary>
        /// Estructura del archivo de configuración por defecto.
        /// </summary>
        private static readonly string defaultConfig = @"{
    ""AppSettings"": {
        ""GitSettings"": {
            ""RepoUrl"": """",
            ""Branch"": ""main"",
            ""LocalPath"": ""Default"",
            ""UseDedicatedFolder"": false,
        },
        ""IntSeconds"": 60,
        ""LogFile"": ""logs.log""
    }
}";

        /// <summary>
        /// Verifica que el archivo de configuración exista, si no existe, se crea uno nuevo con valores por defecto y se le indica al usuario que lo edite antes de ejecutar el programa.
        /// </summary>
        private static void EnsureConfigExists()
        {
            // Si el archivo de configuración ya existe, no hace nada.
            if (File.Exists(ConfigPath))
            {
                string fileContent = File.ReadAllText(ConfigPath);

                if(!string.IsNullOrWhiteSpace(fileContent))
                    return;

                Console.WriteLine("[!] settings.json está vacío. Creando uno nuevo...");
                File.WriteAllText(ConfigPath, defaultConfig);
            }
            else
            {
                Console.WriteLine("[!] settings.json no se pudo encontrar. Creando uno nuevo...");

                // Escribe el archivo de configuración con los valores por defecto.
                File.WriteAllText(ConfigPath, defaultConfig);
                Console.WriteLine("[!] settings.json creado. Por favor edite el archivo con la configuración correcta antes de ejecutar.");
            }

        }

        /// <summary>
        /// Verifica que todas las configuraciones sean válidas, si no lo son, se le indica al usuario que arregle el archivo de configuración antes de ejecutar el programa.
        /// </summary>
        /// <returns>Settings Validas o null si son invalidas</returns>
        public static AppSettings Load(string baseDir)
        {
            // Se asegura de que el archivo de configuración exista, si no existe, se crea uno nuevo con valores por defecto y se le indica al usuario que lo edite antes de ejecutar el programa.
            EnsureConfigExists();
            IConfiguration config;

            try
            {
                config = new ConfigurationBuilder()
                    .SetBasePath(baseDir)
                    .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                    .Build();
            }
            catch (Exception ex) // Captura cualquier error relacionado con la lectura del archivo de configuración, como problemas de formato JSON o permisos de archivo.
            {
                Console.WriteLine($"[!] Error al cargar la configuración: {ex.Message}");
                return null;
            }

            var settings = config.GetSection("AppSettings").Get<AppSettings>();
            if (!ConfigValidator.Validate(settings)) // Si la configuración no es válida, se le indica al usuario que arregle el archivo de configuración antes de ejecutar el programa.
            {
                Console.WriteLine("[!] Configuración inválida. Por favor arregle settings.json.");
                return null;
            }

            return settings;
        }
    }
}
