using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static optimizerNXT.Enums;

namespace optimizerNXT {
    internal static class ExecHandler {
        internal static void ExecuteCommand(ExecStep step)
        {
            var resolvedFile = string.Empty;
            if (!string.IsNullOrWhiteSpace(step.File))
            {
                resolvedFile = ResolveFilePath(step.File);
                ValidateFileType(step.Type, resolvedFile);
            }

            var processStartInfoArray = CreateProcessStartInfo(step, resolvedFile);
            foreach (var process in processStartInfoArray)
            {
                RunProcess(process);
            }
        }

        internal static void RunProcess(ProcessStartInfo psi)
        {
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            using (var proc = Process.Start(psi))
            {
                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                if (!string.IsNullOrWhiteSpace(output))
                {
                    Logger.Info($"Command standard output: {output}");
                }

                if (!string.IsNullOrWhiteSpace(error))
                {
                    Logger.Error($"Command standard error: {error}");
                }
                Logger.Info($"Command executed successfully");
            }
        }

        internal static string ResolveFilePath(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                return string.Empty;

            string fullPath = string.Empty;
            if (Path.IsPathRooted(file))
            {
                fullPath = Path.GetFullPath(file);
                return File.Exists(fullPath) ? fullPath : string.Empty;
            }

            string workDir = AppDomain.CurrentDomain.BaseDirectory;
            string combined = Path.Combine(workDir, file);
            fullPath = Path.GetFullPath(combined);

            if (!File.Exists(fullPath))
                return string.Empty;

            return fullPath;
        }

        internal static void ValidateFileType(ExecType type, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new FileNotFoundException($"Script file not found: {filePath}");

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (type is ExecType.PowerShell && extension != ".ps1")
                throw new InvalidOperationException("PowerShell steps must reference .ps1 files.");
            if (type is ExecType.Cmd && extension != ".bat")
                throw new InvalidOperationException("CMD steps must reference .bat files.");
            if (type is ExecType.Registry && extension != ".reg")
                throw new InvalidOperationException("Registry steps must reference .reg files.");
        }

        internal static ProcessStartInfo[] CreateProcessStartInfo(ExecStep step, string resolvedFile)
        {
            if (!string.IsNullOrWhiteSpace(resolvedFile))
            {
                if (step.Type is ExecType.PowerShell)
                {
                    return new ProcessStartInfo[] { new ProcessStartInfo("powershell.exe", $"-ExecutionPolicy Bypass -File \"{resolvedFile}\"") };
                }

                if (step.Type is ExecType.Registry)
                {
                    return new ProcessStartInfo[] { new ProcessStartInfo("regedit.exe", $"/s \"{resolvedFile}\"") };
                }
                return new ProcessStartInfo[] { new ProcessStartInfo("cmd.exe", $"/c \"{resolvedFile}\"") };
            }

            if (step.Type is ExecType.PowerShell)
            {
                return step.Commands
                    .Select(x => new ProcessStartInfo("powershell.exe", "-NoProfile -Command " + x))
                    .ToArray();
            }

            return step.Commands
                .Select(x => new ProcessStartInfo("cmd.exe", $"/c \"{x}\""))
                .ToArray();
        }
    }
}
