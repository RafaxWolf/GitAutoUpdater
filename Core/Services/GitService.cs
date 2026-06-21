using System.Diagnostics;
using GitAutoUpdater.Schemas;

namespace GitAutoUpdater.Core.Services
{
    public class GitCommandResult
    {
        public string Output { get; set; }
        public string Error { get; set; }
        public int ExitCode { get; set; }
        public bool Success => ExitCode == 0;
    }

    public static class GitService
    {

        private static readonly List<Process> ActiveGitProcesses = [];

        /// <summary>
        /// Handler para la ejecución de comandos de Git y devuelve:
        /// - Manejo del Output
        /// - Manejo de Errores
        /// - Código de Salida
        /// </summary>
        private static (string output, string error, int exitCode) RunGitCommand(string args, string workDir)
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
                    //Logger.Log(e.Data);
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
            ActiveGitProcesses.Add(process);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            ActiveGitProcesses.Remove(process);

            // Devuelve el output, error y código de salida del proceso
            return (output, error, process.ExitCode);
        }

        /// <summary>
        /// Verifica si Git se encuentra instalado y/o en el PATH
        /// </summary>
        public static bool Installed()
        {
            var versionExec = RunGitCommand("--version", AppContext.BaseDirectory);

            if (versionExec.exitCode != 0)
                return false;

            return versionExec.output.Contains("git version");
        }

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
        /// Clona el repositorio de GitHub el equipo
        /// </summary>
        public static void Clone(GitSettings settings, string baseDir)
        {
            var timer = new TimeWaiter();

            timer.Start("Clonando Repositorio...");
            Logger.Log("Clonando Repositorio...", Logger.LogLevel.Process, true);

            var cloneExec = RunGitCommand(
                $"clone -b {settings.Branch} --single-branch {settings.RepoUrl} \"{settings.LocalPath}\"",
                baseDir
            );

            if (cloneExec.exitCode != 0)
            {
                timer.Stop("Error al Clonar.", Logger.LogLevel.Error);
                Logger.Log(cloneExec.error, Logger.LogLevel.Error);
            }
            else
            {
                timer.Stop(cloneExec.error.Trim());
                Logger.Log("Repositorio Clonado.", Logger.LogLevel.Success);
            }
        }

        /// <summary>
        /// Actualiza el Repositorio Local con los cambios del Repositorio Remoto
        /// </summary>
        public static string Pull(GitSettings settings)
        {
            var pullExec = RunGitCommand(
                $"pull origin {settings.Branch}",
                settings.LocalPath
            );

            if (pullExec.exitCode != 0)
            {
                Logger.Log("Error al intentar hacer PULL del repositorio.", Logger.LogLevel.Error);
                Logger.Log(pullExec.error);
                return pullExec.error;
            }

            Logger.Log(pullExec.output);
            return pullExec.output;
        }

        /// <summary>
        /// Verifica si hay actualizaciones en el repositorio Remoto
        /// </summary>
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
            if (fetchResult.exitCode != 0)
            {
                Logger.Log("Error al Fetchear repositorio.", Logger.LogLevel.Error);
                return false;
            }

            var localResult = RunGitCommand("rev-parse HEAD", settings.LocalPath);
            var remoteResult = RunGitCommand($"rev-parse origin/{settings.Branch}", settings.LocalPath);

            if (localResult.exitCode != 0)
            {
                Console.WriteLine();
                Logger.Log("Error al obtener los Hashes.", Logger.LogLevel.Warning);
                Logger.Log(localResult.error, Logger.LogLevel.Error);
                Logger.Log(remoteResult.output.Trim(), Logger.LogLevel.Process);
                return false;

            }

            if (remoteResult.exitCode != 0)
            {
                Console.WriteLine();
                Logger.Log("Error al obtener los Hashes.", Logger.LogLevel.Warning);
                Logger.Log(localResult.output.Trim(), Logger.LogLevel.Process);
                Logger.Log(remoteResult.error, Logger.LogLevel.Error);
                return false;
            }

            string local = localResult.output.Trim();
            string remote = remoteResult.output.Trim();

            return local != remote;
        }

        public static string GetRepoName(string repoUrl)
        {
            if (string.IsNullOrEmpty(repoUrl))
            {
                Logger.Log("La URL del repositorio no puede estar vacía.", Logger.LogLevel.Error);
                return null;
            }

            string name = repoUrl.Split('/').Last();
            if (name.EndsWith(".git"))
                name = name.Substring(0, name.Length - 4);

            return name;
        }

        public static bool IsRepoValid(string path)
        {
            if (!Directory.Exists(path))
                return false;

            var result = RunGitCommand("rev-parse --is-inside-work-tree", path);

            return result.exitCode == 0;
        }
    }
}
