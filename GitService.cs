using System;
using System.IO;
using System.Diagnostics;
using GitAutoUpdater.Schemas;

namespace GitAutoUpdater
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
        public static bool GitInstalled()
        {
            var versionExec = RunGitCommand("--version", AppContext.BaseDirectory);

            if (versionExec.exitCode != 0)
                return false;

            return versionExec.output.Contains("git version");
        }

        /// <summary>
        /// Clona el repositorio de GitHub el equipo
        /// </summary>
        public static void GitClone(GitSettings settings, string baseDir)
        {
            Logger.Log("[/] Clonando Repositorio...");

            var cloneExec = RunGitCommand($"clone {settings.RepoUrl} \"{settings.LocalPath}\"", baseDir);

            if (cloneExec.exitCode != 0)
            {
                Logger.Log("[!] Error al Clonar.");
                Logger.Log(cloneExec.error);
            }
            Logger.Log("[+] Repositorio Clonado.");
            Logger.Log(cloneExec.output);
        }

        /// <summary>
        /// Verifica si hay actualizaciones en el repositorio Remoto
        /// </summary>
        public static bool UpdatesVerify(GitSettings settings)
        {
            // Verificador carpeta
            if (!Directory.Exists(settings.LocalPath))
            {
                Logger.Log("[!] Carpeta local no existe.");
                return false;
            }

            // Fetch
            RunGitCommand("fetch", settings.LocalPath);

            var localResult = RunGitCommand("rev-parse HEAD", settings.LocalPath);
            var remoteResult = RunGitCommand($"rev-parse origin/{settings.Branch}", settings.LocalPath);

            if (localResult.exitCode != 0 || remoteResult.exitCode != 0)
            {
                Logger.Log("[!] Error al obtener Hashes.");
                Logger.Log(localResult.error);
                Logger.Log(remoteResult.error);
                return false;
            }

            string local = localResult.output.Trim();
            string remote = remoteResult.output.Trim();

            return local != remote;
        }

        /// <summary>
        /// Actualiza el Repositorio Local con los cambios del Repositorio Remoto
        /// </summary>
        public static string GitPull(GitSettings settings)
        {
            var pullExec = RunGitCommand("pull", settings.LocalPath);

            if (pullExec.exitCode != 0)
            {
                Logger.Log("[!] Error al intentar hacer PULL del repositorio.");
                Logger.Log(pullExec.error);
                return pullExec.error;
            }

            Logger.Log(pullExec.output);
            return pullExec.output;
        }
    }
}
