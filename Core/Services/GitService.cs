using GitAutoUpdater.Schemas;
using System.Diagnostics;

namespace GitAutoUpdater.Core.Services
{
    /// <summary>
    /// Representa el resultado de la ejecución de un comando de Git,
    /// incluyendo la salida estándar, el error estándar, el código de salida y el tipo de error detectado.
    /// </summary>
    public class GitCommandResult
    {
        public string Output { get; set; } = "";
        public string Error { get; set; } = "";
        public int ExitCode { get; set; }

        public bool Success => ExitCode == 0;
        
        public GitErrorType ErrorType { get; set; } = GitErrorType.None;
        
        public string FullOutput => $"{Output}{Environment.NewLine}{Error}";

    }

    /// <summary>
    /// Enum para categorizar los tipos de errores que pueden ocurrir al ejecutar comandos de Git.
    /// </summary>
    public enum GitErrorType
    {
        None,
        RepositoryNotFound,
        AuthenticationFailed,
        DmcaRemoved,
        NetworkError,
        InvalidBranch,
        RepositoryCorrupted,
        Cancelled,
        Unknown
    }

    public static class GitService
    {
        private static readonly object ProcessLock = new();
        private static readonly List<Process> ActiveGitProcesses = [];
        private static bool CancelReq = false;

        /// <summary>
        /// Ejecuta un comando de Git en un directorio de trabajo específico y captura su salida, error y código de salida.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="workDir"></param>
        /// <returns></returns>
        private static GitCommandResult RunGitCommand(string args, string workDir)
        {
            // Configuración del proceso para ejecutar el comando de Git
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                WorkingDirectory = workDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            string output = "";
            string error = "";

            // Captura de Output
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine(e.Data);
                    output += e.Data + Environment.NewLine;
                }
            };

            // Manejo de errores
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine(e.Data);
                    error += e.Data + Environment.NewLine;
                }
            };

            // Iniciar process y que reciba los datos de salida y error
            process.Start();

            lock (ProcessLock)
            {
                ActiveGitProcesses.Add(process);
            }

            // Espera a que el proceso termine y captura el output y error
            try
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
            }
            finally // Asegura que el proceso se elimine de la lista de procesos activos incluso si ocurre una excepción
            {
                lock (ProcessLock)
                {
                    ActiveGitProcesses.Remove(process);
                }
            }


            // Devuelve el output, error, código de salida del proceso y el tipo de error detectado
            return new GitCommandResult
            {
                Output = output,
                Error = error,
                ExitCode = process.ExitCode,
                ErrorType = DetectGitError(output, error, process.ExitCode)
            };
        }

        /// <summary>
        /// Detecta el tipo de error de Git basado en la salida, error y código de salida del proceso.
        /// Esto permite categorizar los errores comunes y proporcionar retroalimentación más específica al usuario.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="error"></param>
        /// <param name="exitCode"></param>
        /// <returns></returns>
        private static GitErrorType DetectGitError(string output, string error, int exitCode)
        {
            string text = (output + "\n" + error).ToLower();

            if (exitCode == 0)
                return GitErrorType.None;

            if (text.Contains("repository not found"))
                return GitErrorType.RepositoryNotFound;

            if (text.Contains("authentication failed"))
                return GitErrorType.AuthenticationFailed;

            if (text.Contains("access denied"))
                return GitErrorType.AuthenticationFailed;

            if (text.Contains("dmca"))
                return GitErrorType.DmcaRemoved;

            if (text.Contains("could not resolve host"))
                return GitErrorType.NetworkError;

            if (text.Contains("unable to access"))
                return GitErrorType.NetworkError;

            if (text.Contains("remote branch"))
                return GitErrorType.InvalidBranch;

            return GitErrorType.Unknown;
        }

        /// <summary>
        /// Muestra un mensaje de error en la consola y en el log dependiendo del tipo de error detectado al ejecutar un comando de Git.
        /// Esto proporciona retroalimentación clara al usuario sobre lo que salió mal durante la operación de Git.
        /// </summary>
        /// <param name="result"></param>
        private static void LogGitError(GitCommandResult result)
        {
            switch (result.ErrorType)
            {
                case GitErrorType.RepositoryNotFound:
                    Logger.Log("El repositorio no existe o no se pudo encontrar.", Logger.LogLevel.Error);
                    break;

                case GitErrorType.AuthenticationFailed:
                    Logger.Log("Error de autenticación. Revisa permisos, token o si el repo es privado.", Logger.LogLevel.Error);
                    break;

                case GitErrorType.DmcaRemoved:
                    Logger.Log("El repositorio parece estar deshabilitado o retirado por DMCA.", Logger.LogLevel.Error);
                    break;

                case GitErrorType.NetworkError:
                    Logger.Log("Error de red al contactar el repositorio remoto.", Logger.LogLevel.Error);
                    break;

                case GitErrorType.InvalidBranch:
                    Logger.Log("La rama configurada no existe en el repositorio remoto.", Logger.LogLevel.Error);
                    break;

                case GitErrorType.Cancelled:
                    Logger.Log("Operación Git cancelada.", Logger.LogLevel.Warning);
                    break;

                default:
                    Logger.Log("Error desconocido de Git.", Logger.LogLevel.Error);
                    break;
            }

            if(!string.IsNullOrWhiteSpace(result.Error))
                Logger.Log(result.Error.Trim(), Logger.LogLevel.Error);

            if (!string.IsNullOrWhiteSpace(result.Output))
                Logger.Log(result.Output.Trim(), Logger.LogLevel.Process);
        }

        /// <summary>
        /// Solicita la cancelación de cualquier operación de Git en curso.
        /// Esto establece un flag de cancelación y mata todos los procesos de Git activos,
        /// permitiendo una interrupción inmediata de las operaciones en curso.
        /// </summary>
        public static void ReqCancel()
        {
            CancelReq = true;
            KillActiveGitProcesses();
        }

        // ================================================ Metodos de Git ================================================

        /// <summary>
        /// Mata todos los procesos de Git activos que se están ejecutando actualmente.
        /// Esto es útil para cancelar operaciones de Git en curso, como clonaciones o pulls,
        /// especialmente si se requiere una interrupción inmediata del proceso.
        /// </summary>
        public static void KillActiveGitProcesses()
        {
            foreach (var process in ActiveGitProcesses.ToList())
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(true);

                        Logger.Log($"Git process killed (PID {process.Id})", Logger.LogLevel.Warning);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error al matar proceso: {ex.Message}", Logger.LogLevel.Error);
                }
            }

            ActiveGitProcesses.Clear();
        }

        /// <summary>
        /// Verifica si Git está instalado en el sistema ejecutando el comando 'git --version' y analizando su salida.
        /// Devuelve true si Git está instalado y accesible, o false si no lo está.
        /// </summary>
        /// <returns></returns>
        public static bool Installed()
        {
            var versionExec = RunGitCommand("--version", AppContext.BaseDirectory);

            if (!versionExec.Success)
                return false;

            return versionExec.Output.Contains("git version");
        }

        /// <summary>
        /// Clona el repositorio de GitHub el equipo
        /// </summary>
        public static bool Clone(GitSettings settings, string baseDir)
        {
            var timer = new TimeWaiter();

            timer.Start("Clonando Repositorio...");
            Logger.Log("Clonando Repositorio...", Logger.LogLevel.Process, true);

            var cloneExec = RunGitCommand(
                $"clone -b {settings.Branch} --single-branch {settings.RepoUrl} \"{settings.LocalPath}\"",
                baseDir
            );

            if (!cloneExec.Success)
            {
                timer.Stop("Error al Clonar.", Logger.LogLevel.Error);
                LogGitError(cloneExec);
                return false;
            }
            
            timer.Stop("Repositorio clonado correctamente.", Logger.LogLevel.Success);
            Logger.Log("Repositorio Clonado.", Logger.LogLevel.Success);
            return true;
            
        }

        /// <summary>
        /// Realiza un pull del repositorio remoto para actualizar la rama local con los últimos cambios.
        /// Devuelve el output del comando o un mensaje de error si falla.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string Pull(GitSettings settings)
        {
            var pullExec = RunGitCommand(
                $"pull origin {settings.Branch}",
                settings.LocalPath
            );

            if (!pullExec.Success)
            {
                Logger.Log("Error al intentar hacer PULL del repositorio.", Logger.LogLevel.Error);
                LogGitError(pullExec);
                return pullExec.Error;
            }

            Logger.Log(pullExec.Output);
            return pullExec.Output;
        }

        /// <summary>
        /// Verifica si hay actualizaciones en el repositorio remoto comparando los hashes de la rama local y la rama remota.
        /// Devuelve true si hay diferencias (es decir, si hay actualizaciones disponibles)
        /// y false si no las hay o si ocurre algún error durante el proceso.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static bool Updates(GitSettings settings)
        {
            // Verificador carpeta
            if (!Directory.Exists(settings.LocalPath))
            {
                Logger.Log("Carpeta local no existe.", Logger.LogLevel.Error);
                return false;
            }

            // Fetch
            var fetchResult = RunGitCommand($"fetch", settings.LocalPath);
            if (!fetchResult.Success)
            {
                Logger.Log("Error al Fetchear repositorio.", Logger.LogLevel.Error);
                return false;
            }

            var localResult = RunGitCommand("rev-parse HEAD", settings.LocalPath);
            var remoteResult = RunGitCommand($"rev-parse origin/{settings.Branch}", settings.LocalPath);

            if (!localResult.Success)
            {
                Console.WriteLine();
                Logger.Log("Error al obtener los Hashes.", Logger.LogLevel.Warning);
                LogGitError(localResult);
                return false;

            }

            if (!remoteResult.Success)
            {
                Console.WriteLine();
                Logger.Log("Error al obtener los Hashes.", Logger.LogLevel.Warning);
                LogGitError(remoteResult);
                return false;
            }

            string local = localResult.Output.Trim();
            string remote = remoteResult.Output.Trim();

            return local != remote;
        }

        /// <summary>
        /// Obtiene el nombre del repositorio a partir de la URL proporcionada.
        /// </summary>
        /// <param name="repoUrl"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GetRepoName(string repoUrl)
        {
            if (string.IsNullOrEmpty(repoUrl)) 
                throw new ArgumentException("La URL del repositorio no puede estar vacía.", nameof(repoUrl));

            string name = repoUrl.Split('/').Last();
            
            if (name.EndsWith(".git"))
                name = name.Substring(0, name.Length - 4);

            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("No se pudo obtener el nombre del Repositorio."):

            return name;
        }

        /// <summary>
        /// Verifica si la carpeta local es un repositorio Git válido
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsRepoValid(string path)
        {
            if (!Directory.Exists(path))
                return false;

            var result = RunGitCommand("rev-parse --is-inside-work-tree", path);

            return result.Success;
        }
    }
}
