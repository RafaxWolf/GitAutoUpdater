using GitAutoUpdater.Schemas;

namespace GitAutoUpdater.Core
{
    public static class ConfigValidator
    {
        /// <summary>
        /// Válida la configuración de la aplicación, asegurándose de que todas las secciones necesarias estén presentes y que los valores sean válidos.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static bool Validate(AppSettings settings)
        {
            bool isValid = true;

            // Validar que la sección AppSettings esté presente
            if (settings == null)
            {
                Console.WriteLine("[!] Falta la sección 'AppSettings'");
                return false;
            }

            // Validar la sección GitSettings
            if (settings.GitSettings == null)
            {
                Console.WriteLine("[!] Falta la sección 'GitSettings'");
                isValid = false;
            }
            else // Si GitSettings está presente, validar sus campos
            {
                // Validar que los campos necesarios en GitSettings estén presentes y no estén vacíos
                if (string.IsNullOrEmpty(settings.GitSettings.RepoUrl))
                {
                    Console.WriteLine("[!] 'RepoUrl' faltante o vacío");
                    isValid = false;
                }

                // Validar que el RepoUrl sea una URL válida
                if (string.IsNullOrEmpty(settings.GitSettings.Branch))
                {
                    Console.WriteLine("[!] 'Branch' faltante o vacío");
                    isValid = false;
                }

                // Validar que el LocalPath no esté vacío
                if (string.IsNullOrEmpty(settings.GitSettings.LocalPath))
                {
                    Console.WriteLine("[!] 'LocalPath' faltante o vacío");
                    isValid = false;
                }
            }

            // Validar que IntSeconds sea un número mayor a 0
            if (settings.IntSeconds <= 10)
            {
                Console.WriteLine("[!] 'IntSeconds' debe de ser mayor a 10 segundos.");
                isValid = false;
            }

            return isValid;
        }
        
    }
}
