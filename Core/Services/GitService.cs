using System;
using System.IO;
using System.Diagnostics;
using GitAutoUpdater.Schemas;

namespace GitAutoUpdater.Core.Services
{
    public static class GitService
    {
        /// <summary>
        /// Handler para la ejecución de comandos de Git y devuelve:
        /// - Manejo del Output
        /// - Manejo de Errores
        /// - Código de Salida
        /// </summary>
        private static (string output, string error, int exitCode) RunGitCommand(string args, string workDir)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                WorkingDirectory = workDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = Process.Start(psi);

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

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

        /// <summary>
        /// Clona el repositorio de GitHub el equipo
        /// </summary>
        public static void Clone(GitSettings settings, string baseDir)
        {
            Logger.Log("Clonando Repositorio...", Logger.LogLevel.Process);

            var cloneExec = RunGitCommand(
                $"clone -b {settings.Branch} --single-branch {settings.RepoUrl} \"{settings.LocalPath}\"",
                baseDir
            );

            if (cloneExec.exitCode != 0)
            {
                Logger.Log("Error al Clonar.", Logger.LogLevel.Error);
                Logger.Log(cloneExec.error, Logger.LogLevel.Error);
                Environment.Exit(cloneExec.exitCode);
            }
            else
            {
                Logger.Log(cloneExec.error.Trim());
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
            RunGitCommand($"fetch", settings.LocalPath);

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

        
    }
}
