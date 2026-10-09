using optimizerNXT.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace optimizerNXT {
    internal static class Engine {
        internal static bool ShouldExecute(Condition condition)
        {
            if (condition == null)
                return true;

            var bitnessPass = true;
            var windowsVersionPass = true;

            if (!string.IsNullOrEmpty(condition.Bitness))
            {
                var requiredBitness = condition.Bitness.ToLowerInvariant();
                if ((requiredBitness == "x64" && !Environment.Is64BitOperatingSystem) ||
                    (requiredBitness == "x86" && Environment.Is64BitOperatingSystem))
                {
                    bitnessPass = false;
                }
            }

            if (condition.Windows?.Count > 0)
            {
                windowsVersionPass = false;
                foreach (var version in condition.Windows)
                {
                    if ((int)Utilities.CurrentWindowsVersion == version)
                    {
                        windowsVersionPass = true;
                        break;
                    }
                }
            }

            return windowsVersionPass && bitnessPass;
        }

        public static void ExecuteStep(Step step)
        {
            Logger.Info($"Executing step: {step.Name}");
            try
            {
                if (!step.ShouldExecute())
                {
                    Logger.Info($"Skipping step '{step.Name}' due to conditions not met.");
                    return;
                }

                if (step.Registry != null)
                {
                    RegistryHandler.ApplyRegistry(step.Registry);
                }

                if (step.Service != null)
                {
                    ServiceHandler.ApplyService(step.Service);
                }

                if (step.Exec != null)
                {
                    ExecHandler.ExecuteCommand(step.Exec);
                }

                if (step.Hosts != null)
                {
                    HostsHandler.ApplyHosts(step.Hosts);
                }

                if (step.Startup != null)
                {
                    StartupHandler.ApplyStartupItem(step.Startup);
                }

                if (step.Network != null)
                {
                    NetworkHandler.ApplyNetwork(step.Network);
                }

                if (step.UwpApps != null)
                {
                    UwpAppsHandler.ApplyUWPApps(step.UwpApps);
                }

                if (step.ProcessControl != null)
                {
                    ProcessHandler.ApplyProcessControl(step.ProcessControl);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Exception executing step: {step.Name}", ex);
            }
        }

        internal static void ExecuteStage(Stage stage)
        {
            var jobs = stage.Jobs;

            if (jobs == null || jobs.Count < 1)
            {
                Logger.Info($"YAML has no jobs defined.");
                return;
            }

            if (jobs != null)
            {
                foreach (var job in jobs)
                {
                    if (!job.ShouldExecute())
                    {
                        Logger.Info($"Skipping job '{job.Name}' due to conditions not met.");
                        continue;
                    }
                    if (job.Steps != null)
                    {
                        Logger.Info($"Executing job: {job.Name} with {job.Steps.Count} steps.");
                        foreach (var step in job.Steps)
                        {
                            Engine.ExecuteStep(step);
                        }
                    }
                }
            }
        }

        internal static ValidationResult<Stage> ValidateStage(IEnumerable<string> yamlPaths)
        {
            var builder = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var stages = new List<Stage>();
            var errors = new List<ValidationError>();

            foreach (var yamlPath in yamlPaths)
            {
                Stage stage = null;

                try
                {
                    using (var reader = new StreamReader(yamlPath))
                    {
                        stage = builder.Deserialize<Stage>(reader);
                    }
                }
                catch
                {
                    errors.Add(new ValidationError(yamlPath, "Failed to deserialize YAML."));
                    continue;
                }

                if (stage == null || stage.Jobs == null)
                {
                    errors.Add(new ValidationError(yamlPath, "Stage has no jobs defined."));
                    continue;
                }

                foreach (var job in stage.Jobs)
                {
                    if (string.IsNullOrWhiteSpace(job.Name) ||
                        string.IsNullOrWhiteSpace(job.Description))
                    {
                        errors.Add(new ValidationError(yamlPath, "Job is missing name or description."));
                        continue;
                    }

                    if (job.Steps == null || job.Steps.Count == 0)
                    {
                        errors.Add(new ValidationError(yamlPath, "Job has no steps defined."));
                        continue;
                    }

                    foreach (var step in job.Steps)
                    {
                        if (string.IsNullOrWhiteSpace(step.Name))
                        {
                            errors.Add(new ValidationError(yamlPath, "Job has a step missing a name."));
                            continue;
                        }
                    }
                }

                stages.Add(stage);
            }

            return new ValidationResult<Stage>(
                errors.Count == 0,
                stages,
                errors);
        }
    }

    public sealed class ValidationError {
        public string File { get; }
        public string Message { get; }

        public ValidationError(string file, string message)
        {
            File = file;
            Message = message;
        }
    }

    public sealed class ValidationResult<T> {
        public bool IsValid { get; }
        public IReadOnlyList<T> Items { get; }
        public IReadOnlyList<ValidationError> Errors { get; }

        public ValidationResult(
            bool isValid,
            IReadOnlyList<T> items,
            IReadOnlyList<ValidationError> errors)
        {
            IsValid = isValid;
            Items = items;
            Errors = errors;
        }
    }
}
