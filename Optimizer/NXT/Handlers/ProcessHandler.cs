using Microsoft.Win32;
using System.Security.AccessControl;

namespace optimizerNXT.Handlers {
    internal static class ProcessHandler {
        internal static void ApplyProcessControl(ProcessControlStep step)
        {
            if (step.Allow != null)
            {
                foreach (var processToBeAllowed in step.Allow)
                {
                    AllowProcessToRun(processToBeAllowed);
                    Logger.Info($"Allow process to run: {processToBeAllowed}");
                }
            }

            if (step.Deny != null)
            {
                foreach (var processToBeDenied in step.Deny)
                {
                    DenyProcessFromRunning(processToBeDenied);
                    Logger.Info($"Deny process from running: {processToBeDenied}");
                }
            }
        }

        internal static void DenyProcessFromRunning(string pName)
        {
            using (RegistryKey ifeo = Registry.LocalMachine.OpenSubKeyWritable(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", RegistryRights.FullControl))
            {
                if (ifeo == null) return;

                ifeo.GrantFullControlOnSubKey("Image File Execution Options");

                using (RegistryKey k = ifeo.OpenSubKeyWritable("Image File Execution Options", RegistryRights.FullControl))
                {
                    if (k == null) return;

                    k.CreateSubKey(pName);
                    k.GrantFullControlOnSubKey(pName);

                    using (RegistryKey f = k.OpenSubKeyWritable(pName, RegistryRights.FullControl))
                    {
                        if (f == null) return;

                        f.SetValue("Debugger", @"%windir%\System32\taskkill.exe");
                    }
                }
            }
        }

        internal static void AllowProcessToRun(string pName)
        {
            using (RegistryKey ifeo = Registry.LocalMachine.OpenSubKeyWritable(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", RegistryRights.FullControl))
            {
                if (ifeo == null) return;

                ifeo.GrantFullControlOnSubKey("Image File Execution Options");

                using (RegistryKey k = ifeo.OpenSubKeyWritable("Image File Execution Options", RegistryRights.FullControl))
                {
                    if (k == null) return;

                    k.GrantFullControlOnSubKey(pName);
                    k.DeleteSubKey(pName, false);
                }
            }
        }
    }
}
