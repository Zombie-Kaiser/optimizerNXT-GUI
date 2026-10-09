using Microsoft.Win32;
using System;
using System.ServiceProcess;
using static optimizerNXT.Enums;

namespace optimizerNXT {
    internal static class ServiceHandler {
        internal static void ApplyService(ServiceStep step)
        {
            if (step.Stop != null)
                foreach (var s in step.Stop)
                    StopService(s);

            if (step.Disable != null)
                foreach (var s in step.Disable)
                    ManageService(s, ServiceStartType.DISABLE_SERVICE);

            if (step.Enable != null)
                foreach (var s in step.Enable)
                    ManageService(s, ServiceStartType.ENABLE_SERVICE_MANUAL);

            if (step.EnableAuto != null)
                foreach (var s in step.EnableAuto)
                    ManageService(s, ServiceStartType.ENABLE_SERVICE_AUTO);

            if (step.Start != null)
                foreach (var s in step.Start)
                    StartService(s);
        }

        internal static void StopService(string name)
        {
            using (var sc = new ServiceController(name))
            {
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    Logger.Info($"Service is already stopped: {name}");
                    return;
                }

                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                Logger.Info($"Service stopped: {name}");
            }
        }

        internal static void StartService(string name)
        {
            using (var sc = new ServiceController(name))
            {
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    Logger.Info($"Service is already running: {name}");
                    return;
                }

                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                Logger.Info($"Service started: {name}");
            }
        }

        internal static void ManageService(string name, ServiceStartType manageCode)
        {
            using (TokenPrivilegeHelper.TakeOwnership)
            {
                using (RegistryKey allServicesKey = Registry.LocalMachine.OpenSubKeyWritable(@"SYSTEM\CurrentControlSet\Services"))
                {
                    allServicesKey.GrantFullControlOnSubKey(name);
                    using (RegistryKey serviceKey = allServicesKey.OpenSubKeyWritable(name))
                    {
                        if (serviceKey == null) return;

                        foreach (string subkeyName in serviceKey.GetSubKeyNames())
                        {
                            serviceKey.TakeOwnershipOnSubKey(subkeyName);
                            serviceKey.GrantFullControlOnSubKey(subkeyName);
                        }
                        serviceKey.SetValue("Start", manageCode, RegistryValueKind.DWord);
                        switch (manageCode)
                        {
                            case ServiceStartType.ENABLE_SERVICE_AUTO:
                                Logger.Info($"Service set to start automatically: {name}");
                                return;

                            case ServiceStartType.ENABLE_SERVICE_MANUAL:
                                Logger.Info($"Service set to start on demand: {name}");
                                return;

                            case ServiceStartType.DISABLE_SERVICE:
                                Logger.Info($"Service set to disabled: {name}");
                                return;

                            default:
                                throw new InvalidOperationException($"Unsupported service start code: {manageCode} for service {name}");
                        }
                    }
                }
            }
        }
    }
}
