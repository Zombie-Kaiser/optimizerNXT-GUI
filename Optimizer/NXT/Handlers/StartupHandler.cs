using Microsoft.Win32;
using System;
using System.IO;

namespace optimizerNXT {
    internal static class StartupHandler {
        internal static readonly string LocalMachineRun = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        internal static readonly string LocalMachineRunOnce = "Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce";
        internal static readonly string LocalMachineRunWoW = "Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run";
        internal static readonly string LocalMachineRunOnceWow = "Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce";
        internal static readonly string CurrentUserRun = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        internal static readonly string CurrentUserRunOnce = "Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce";
        internal static readonly string ProfileAppDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        internal static readonly string ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        internal static readonly string LocalMachineStartupFolder = ProgramData + "\\Microsoft\\Windows\\Start Menu\\Programs\\Startup";
        internal static readonly string CurrentUserStartupFolder = ProfileAppDataRoaming + "\\Microsoft\\Windows\\Start Menu\\Programs\\Startup";

        internal static string GetRunPath(RegistryKey hive, string hiveName)
        {
            if (hive == Registry.LocalMachine && Environment.Is64BitOperatingSystem && hiveName.Equals("HKLM", StringComparison.OrdinalIgnoreCase))
                return LocalMachineRunWoW;
            return hive == Registry.LocalMachine ? LocalMachineRun : CurrentUserRun;
        }

        internal static string GetRunOncePath(RegistryKey hive, string hiveName)
        {
            if (hive == Registry.LocalMachine && Environment.Is64BitOperatingSystem && hiveName.Equals("HKLM", StringComparison.OrdinalIgnoreCase))
                return LocalMachineRunOnceWow;
            return hive == Registry.LocalMachine ? LocalMachineRunOnce : CurrentUserRunOnce;
        }

        internal static RegistryKey ResolveHive(string hive)
        {
            switch (hive.ToUpperInvariant())
            {
                case "HKLM": return Registry.LocalMachine;
                case "HKCU": return Registry.CurrentUser;
                case "HKCR": return Registry.ClassesRoot;
                case "HKEY_CLASSES_ROOT": return Registry.ClassesRoot;
                case "HKEY_CURRENT_USER": return Registry.CurrentUser;
                case "HKEY_LOCAL_MACHINE": return Registry.LocalMachine;
                default: throw new InvalidOperationException($"Unsupported hive: {hive}");
            }
        }

        internal static void ApplyStartupItem(StartupStep step)
        {
            if (step.Registry != null)
            {
                foreach (var r in step.Registry)
                    ApplyRegistryStartup(r);
            }

            if (step.Folder != null)
            {
                foreach (var f in step.Folder)
                    ApplyFolderStartup(f);
            }
        }

        internal static void ApplyRegistryStartup(RegistryStartupItem action)
        {
            RegistryKey hive = ResolveHive(action.Hive);
            string keyPath = action.RunType.Equals("RunOnce", StringComparison.OrdinalIgnoreCase)
                ? GetRunOncePath(hive, action.Hive)
                : GetRunPath(hive, action.Hive);

            using (var key = hive.OpenSubKeyWritable(keyPath))
            {
                if (key == null) return;

                if (action.Action.Equals("Add", StringComparison.OrdinalIgnoreCase))
                {
                    key.SetValue(action.Name, action.File);
                    Logger.Info($"Added registry startup: {action.Hive}\\{keyPath}\\{action.Name} = {action.File}");
                }
                else if (action.Action.Equals("Delete", StringComparison.OrdinalIgnoreCase))
                {
                    if (key.GetValue(action.Name) != null)
                    {
                        key.DeleteValue(action.Name, false);
                        Logger.Info($"Deleted registry startup: {action.Hive}\\{keyPath}\\{action.Name}");
                    }
                }
            }
        }

        internal static void ApplyFolderStartup(FolderStartupItem action)
        {
            if (!Path.IsPathRooted(action.File))
                throw new InvalidOperationException($"Startup file must be absolute: {action.File}");

            string folderPath = action.Location.Equals("User", StringComparison.OrdinalIgnoreCase)
                ? CurrentUserStartupFolder
                : LocalMachineStartupFolder;

            string targetPath = Path.Combine(folderPath, Path.GetFileName(action.File));

            if (action.Action.Equals("Add", StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(action.File, targetPath, overwrite: true);
                Logger.Info($"Added startup file: {action.File} -> {folderPath}");
            }
            else if (action.Action.Equals("Delete", StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                    Logger.Info($"Deleted startup file: {targetPath}");
                }
            }
        }
    }
}
