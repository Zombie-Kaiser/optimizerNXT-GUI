using Microsoft.Win32;
using System;

namespace optimizerNXT {
    internal static class RegistryHandler {
        internal static void ApplyRegistry(RegistryStep step)
        {
            if (step?.Entries == null || step.Entries.Count == 0)
                return;

            foreach (var entry in step.Entries)
            {
                var hive = ResolveHive(entry.Hive);

                using (var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64))
                using (var key = baseKey.CreateSubKey(entry.Path, true))
                {
                    if (key == null)
                        throw new InvalidOperationException($"Registry key open failed: {entry.Path}");

                    key.TakeOwnershipOnSubKey("");
                    key.GrantFullControlOnSubKey("");

                    if (entry.Add != null)
                    {
                        foreach (var kv in entry.Add)
                        {
                            var spec = kv.Value;
                            key.SetValue(kv.Key, ConvertValue(spec.Value, spec.Type), ResolveValueKind(spec.Type));
                            Logger.Info($"Set registry value: {entry.Hive}\\{entry.Path}\\{kv.Key}");
                        }
                    }

                    if (entry.Delete?.Values != null)
                    {
                        foreach (var valueName in entry.Delete.Values)
                        {
                            if (key.GetValue(valueName) != null)
                            {
                                key.DeleteValue(valueName, false);
                                Logger.Info($"Deleted registry value: {entry.Hive}\\{entry.Path}\\{valueName}");
                            }
                        }
                    }

                    if (entry.Delete?.Keys != null)
                    {
                        foreach (var subkeyName in entry.Delete.Keys)
                        {
                            key.DeleteSubKeyTree(subkeyName, false);
                            Logger.Info($"Deleted subkey: {entry.Hive}\\{entry.Path}\\{subkeyName}");
                        }
                    }
                }
            }
        }

        internal static RegistryHive ResolveHive(string hive)
        {
            switch (hive.ToUpperInvariant())
            {
                case "HKLM": return RegistryHive.LocalMachine;
                case "HKCU": return RegistryHive.CurrentUser;
                case "HKCR": return RegistryHive.ClassesRoot;
                case "HKEY_CLASSES_ROOT": return RegistryHive.ClassesRoot;
                case "HKEY_CURRENT_USER": return RegistryHive.CurrentUser;
                case "HKEY_LOCAL_MACHINE": return RegistryHive.LocalMachine;
                default: throw new InvalidOperationException($"Unsupported hive: {hive}");
            }
        }

        internal static RegistryValueKind ResolveValueKind(string type)
        {
            switch (type.ToLowerInvariant())
            {
                case "string": return RegistryValueKind.String;
                case "dword": return RegistryValueKind.DWord;
                case "qword": return RegistryValueKind.QWord;
                default: throw new InvalidOperationException($"Unsupported value type: {type}");
            }
        }

        internal static object ConvertValue(object value, string type)
        {
            switch (type.ToLowerInvariant())
            {
                case "string": return Convert.ToString(value);
                case "dword": return Convert.ToInt32(value);
                case "qword": return Convert.ToInt64(value);
                default: throw new InvalidOperationException();
            }
        }
    }
}
