namespace optimizerNXT {
    internal static class Enums {
        internal enum ServiceStartType {
            ENABLE_SERVICE_AUTO = 2,
            ENABLE_SERVICE_MANUAL = 3,
            DISABLE_SERVICE = 4
        }

        internal enum WindowsVersion {
            Unknown = 0,
            Windows7 = 7,
            Windows8 = 8,
            Windows10 = 10,
            Windows11 = 11
        }

        internal enum ExecType {
            Cmd,
            PowerShell,
            Registry
        }
    }
}
