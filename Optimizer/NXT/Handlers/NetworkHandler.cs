using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace optimizerNXT {
    internal static class NetworkHandler {
        internal static string[] GoogleDNSv4 = { "8.8.8.8", "8.8.4.4" };
        internal static string[] GoogleDNSv6 = { "2001:4860:4860::8888", "2001:4860:4860::8844" };

        internal static string[] OpenDNSv4 = { "208.67.222.222", "208.67.220.220" };
        internal static string[] OpenDNSv6 = { "2620:0:ccc::2", "2620:0:ccd::2" };

        internal static string[] CloudflareDNSv4 = { "1.1.1.1", "1.0.0.1" };
        internal static string[] CloudflareDNSv6 = { "2606:4700:4700::1111", "2606:4700:4700::1001" };

        internal static string[] Quad9DNSv4 = { "9.9.9.9", "149.112.112.112" };
        internal static string[] Quad9DNSv6 = { "2620:fe::fe", "2620:fe::9" };

        internal static string[] CleanBrowsingDNSv4 = { "185.228.168.168", "185.228.168.169" };
        internal static string[] CleanBrowsingDNSv6 = { "2a0d:2a00:1::", "2a0d:2a00:2::" };

        internal static string[] CleanBrowsingAdultDNSv4 = { "185.228.168.10", "185.228.168.11" };
        internal static string[] CleanBrowsingAdultDNSv6 = { "2a0d:2a00:1::1", "2a0d:2a00:2::1" };

        internal static string[] AlternateDNSv4 = { "76.76.19.19", "76.223.122.150" };
        internal static string[] AlternateDNSv6 = { "2602:fcbc::ad", "2602:fcbc:2::ad" };

        internal static string[] AdguardDNSv4 = { "94.140.14.14", "94.140.15.15" };
        internal static string[] AdguardDNSv6 = { "2a10:50c0::ad1:ff", "2a10:50c0::ad2:ff" };

        internal static void ApplyNetwork(NetworkStep step)
        {
            var dnsV4 = ResolveDNSv4(step.Dns);
            var dnsV6 = ResolveDNSv6(step.Dns);

            var activeNic = GetActivePrimaryNic();

            if (step.Dns.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                ResetDNSToDHCP(activeNic);
            }
            else
            {
                SetDNS(activeNic, dnsV4, dnsV6, step.Dns);
            }

            if (step.FlushDns)
            {
                FlushDNSCache();
            }
        }

        internal static void SetDNS(string nic, string[] dnsv4, string[] dnsv6, string dnsOption)
        {
            var cmdv4Primary = $"netsh interface ipv4 set dnsservers {nic} static {dnsv4[0]} primary";
            var cmdv4Alternate = $"netsh interface ipv4 add dnsservers {nic} {dnsv4[1]} index=2";

            var cmdv6Primary = $"netsh interface ipv6 set dnsservers {nic} static {dnsv6[0]} primary";
            var cmdv6Alternate = $"netsh interface ipv6 add dnsservers {nic} {dnsv6[1]} index=2";

            Utilities.RunCommand(cmdv4Primary);
            Utilities.RunCommand(cmdv4Alternate);
            Utilities.RunCommand(cmdv6Primary);
            Utilities.RunCommand(cmdv6Alternate);

            Logger.Info($"DNS has been set to {dnsOption} for NIC: {nic}");
        }

        internal static void ResetDNSToDHCP(string nic)
        {
            string cmdv4 = $"netsh interface ipv4 set dnsservers {nic} dhcp";
            string cmdv6 = $"netsh interface ipv6 set dnsservers {nic} dhcp";

            Utilities.RunCommand(cmdv4);
            Utilities.RunCommand(cmdv6);

            Logger.Info($"DNS has been reset to defaults from DHCP for NIC: {nic}");
        }

        internal static string GetActivePrimaryNic()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                    continue;

                var props = nic.GetIPProperties();
                if (props == null)
                    continue;

                bool hasIpv4Gateway = false;
                foreach (GatewayIPAddressInformation g in props.GatewayAddresses)
                {
                    if (g.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        hasIpv4Gateway = true;
                        break;
                    }
                }

                if (!hasIpv4Gateway)
                    continue;

                Logger.Info($"Active NIC detected: {nic.Name}");
                Logger.Info($"Current DNS servers: {string.Join(", ", nic.GetIPProperties().DnsAddresses.Select(x => x.ToString()).ToArray())}");
                return nic.Name;
            }

            throw new InvalidOperationException("No active network interface with IPv4 gateway found.");
        }

        internal static void FlushDNSCache()
        {
            Utilities.RunCommand("ipconfig /flushdns");
            Logger.Info("DNS cache flushed.");
        }

        internal static string[] ResolveDNSv4(string dnsOption)
        {
            string key = dnsOption.ToLowerInvariant();
            if (key == "google") return GoogleDNSv4;
            if (key == "opendns") return OpenDNSv4;
            if (key == "cloudflare") return CloudflareDNSv4;
            if (key == "quad9") return Quad9DNSv4;
            if (key == "alternatedns") return AlternateDNSv4;
            if (key == "adguard") return AdguardDNSv4;
            if (key == "cleanbrowsing") return CleanBrowsingDNSv4;
            if (key == "cleanbrowsing_adult") return CleanBrowsingAdultDNSv4;
            if (key == "auto") return new string[0];
            throw new InvalidOperationException("Unsupported DNS option: " + dnsOption);
        }

        internal static string[] ResolveDNSv6(string dnsOption)
        {
            string key = dnsOption.ToLowerInvariant();
            if (key == "google") return GoogleDNSv6;
            if (key == "opendns") return OpenDNSv6;
            if (key == "cloudflare") return CloudflareDNSv6;
            if (key == "quad9") return Quad9DNSv6;
            if (key == "alternatedns") return AlternateDNSv6;
            if (key == "adguard") return AdguardDNSv6;
            if (key == "cleanbrowsing") return CleanBrowsingDNSv6;
            if (key == "cleanbrowsing_adult") return CleanBrowsingAdultDNSv6;
            if (key == "auto") return new string[0];
            throw new InvalidOperationException("Unsupported DNS option: " + dnsOption);
        }
    }
}
