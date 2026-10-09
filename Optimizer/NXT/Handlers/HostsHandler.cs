using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace optimizerNXT {
    internal static class HostsHandler {
        private static readonly string HostsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");

        private static string NormalizeHostsLine(string line) => Regex.Replace(line.Trim(), @"\s+", " ");

        public static void ApplyHosts(HostsStep step)
        {
            if (!File.Exists(HostsFile))
                throw new FileNotFoundException($"Hosts file not found: {HostsFile}");

            var lines = File.ReadAllLines(HostsFile).ToList();

            if (step.Delete != null)
            {
                foreach (var line in step.Delete)
                {
                    string normalized = NormalizeHostsLine(line);
                    lines.RemoveAll(l => NormalizeHostsLine(l).Equals(normalized, StringComparison.OrdinalIgnoreCase));
                    Logger.Info($"Removed hosts line: {line}");
                }
            }

            if (step.Add != null)
            {
                foreach (var line in step.Add)
                {
                    string normalized = NormalizeHostsLine(line);
                    if (!lines.Any(l => NormalizeHostsLine(l).Equals(normalized, StringComparison.OrdinalIgnoreCase)))
                    {
                        lines.Add(line);
                        Logger.Info($"Added hosts line: {line}");
                    }
                }
            }

            File.WriteAllLines(HostsFile, lines, Encoding.UTF8);
        }
    }
}
