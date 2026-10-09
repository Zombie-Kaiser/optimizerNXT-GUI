namespace optimizerNXT {
    internal static class UwpAppsHandler {
        public static void ApplyUWPApps(UWPAppsStep step)
        {
            if (step?.Uninstall == null || step.Uninstall.Count == 0)
                return;

            foreach (string appName in step.Uninstall)
            {
                Logger.Info($"Attempting to uninstall UWP app for all users: {appName}");
                string psCommand = $"Get-AppxPackage -AllUsers *{appName}* | Remove-AppxPackage -AllUsers";
                int exitCode = Utilities.RunPowershell(psCommand);
                if (exitCode == 0)
                    Logger.Info($"Successfully uninstalled UWP app for all users: {appName}");
                else
                    Logger.Warn($"Failed to uninstall UWP app: {appName}, exit code {exitCode}");
            }
        }
    }
}
