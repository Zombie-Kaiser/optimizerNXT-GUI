using System.Collections.Generic;
using static optimizerNXT.Enums;

namespace optimizerNXT {
    internal sealed class Stage {
        public List<Job> Jobs { get; set; }
    }

    internal sealed class Job {
        public string Name { get; set; }
        public string Description { get; set; }
        public Condition Condition { get; set; }
        public List<Step> Steps { get; set; }

        public bool ShouldExecute() => Engine.ShouldExecute(this.Condition);
    }

    internal sealed class Step {
        public string Name { get; set; }
        public Condition Condition { get; set; }
        public RegistryStep Registry { get; set; }
        public ServiceStep Service { get; set; }
        public ExecStep Exec { get; set; }
        public HostsStep Hosts { get; set; }
        public StartupStep Startup { get; set; }
        public NetworkStep Network { get; set; }
        public UWPAppsStep UwpApps { get; set; }
        public ProcessControlStep ProcessControl { get; set; }

        public bool ShouldExecute() => Engine.ShouldExecute(this.Condition);
    }

    internal sealed class Condition {
        public string Bitness { get; set; } // x86 | x64
        public List<int> Windows { get; set; } // 7 | 8 | 10 | 11
    }

    internal sealed class RegistryStep {
        public string Hive { get; set; }
        public string Path { get; set; }
        public List<RegistryEntry> Entries { get; set; }
    }

    internal sealed class ProcessControlStep {
        public List<string> Deny { get; set; }
        public List<string> Allow { get; set; }
    }

    internal sealed class UWPAppsStep {
        public List<string> Uninstall { get; set; }
    }

    internal sealed class RegistryEntry {
        public string Hive { get; set; }
        public string Path { get; set; }
        public Dictionary<string, RegistryValueSpec> Add { get; set; }
        public RegistryDeleteSpec Delete { get; set; }
    }

    internal sealed class RegistryValueSpec {
        public string Type { get; set; }
        public object Value { get; set; }
    }

    internal sealed class RegistryDeleteSpec {
        public List<string> Keys { get; set; }
        public List<string> Values { get; set; }
    }

    internal sealed class ServiceStep {
        public List<string> Stop { get; set; }
        public List<string> Start { get; set; }
        public List<string> Disable { get; set; }
        public List<string> Enable { get; set; }
        public List<string> EnableAuto { get; set; }
    }

    internal sealed class ExecStep {
        public ExecType Type { get; set; }
        public List<string> Commands { get; set; }
        public string File { get; set; }
    }

    internal sealed class HostsStep {
        public List<string> Add { get; set; }
        public List<string> Delete { get; set; }
    }

    internal sealed class StartupStep {
        public List<RegistryStartupItem> Registry { get; set; }
        public List<FolderStartupItem> Folder { get; set; }
    }

    internal sealed class NetworkStep {
        // Supported values:
        // AUTO, GOOGLE, OPENDNS, CLOUDFLARE, QUAD9,
        // ALTERNATEDNS, ADGUARD, CLEANBROWSING, CLEANBROWSING_ADULT
        public string Dns { get; set; }
        public bool FlushDns { get; set; } = false;
    }

    internal sealed class RegistryStartupItem {
        public string Hive { get; set; } // HKLM | HKCU      
        public string RunType { get; set; } // Run | RunOnce
        public string Name { get; set; }
        public string File { get; set; }
        public string Action { get; set; } // Add | Delete  
    }

    internal sealed class FolderStartupItem {
        public string Location { get; set; } // User | System    
        public string File { get; set; }
        public string Action { get; set; } // Add | Delete
    }
}
