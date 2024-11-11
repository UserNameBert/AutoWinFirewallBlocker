using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FirewallManager.IPsJSON;

namespace FirewallManager.CommandHandlers
{
    public static class BlockCommands
    {
        private static readonly string FilePath = Path.Combine("IPsJSON", "Blocked.json");
        public static void InitializeDatabase()
        {
            try
            {
                Directory.CreateDirectory("IPsJSON");
                if (!File.Exists(FilePath))
                {
                    File.WriteAllText(FilePath, "[]");
                }
                Console.WriteLine("JSON storage initialized for blocked IPs.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while initializing the blocked database:\n {ex.Message}");
            }
        }


        //
        public static void BlockIP(string ipAddress, string type)
        {
            if (WhitelistCommands.IsIPWhitelisted(ipAddress))
            {
                Console.WriteLine($"Cannot block IP: {ipAddress} because it is whitelisted.");
                return;
            }
            var alreadyBlockedIPs = LoadBlockedIPs();
            if (type == "@Manual block" && alreadyBlockedIPs.Exists(ip => ip.IPAddress == ipAddress))
            {
                Console.WriteLine($"IP: {ipAddress} is already blocked.");
                return;
            }
            else if (alreadyBlockedIPs.Exists(ip => ip.IPAddress == ipAddress))
            {
                return;
            }
            alreadyBlockedIPs.Add(new BlockedIP
            {
                IPAddress = ipAddress,
                BlockedAt = DateTime.Now,
                Reason = type
            });
            string action = "block";
            FWModifier.AddFirewallRule($"{type} {ipAddress}", ipAddress, action);
            SaveBlockedIPs(alreadyBlockedIPs);
            Console.WriteLine($"IP: {ipAddress} has been blocked with reason: {type}");
        }


        //
        public static void ManualBlockIP(string ipAddress)
        {
            BlockIP(ipAddress, "@Manual block");
        }


        //
        public static void AutoBlockIP(string ipAddress)
        {
            BlockIP(ipAddress, "@Auto block");
        }


        //
        public static void UnblockIP(string ipAddress)
        {
            var alreadyblockedIPs = LoadBlockedIPs();
            int removedCount = alreadyblockedIPs.RemoveAll(ip => ip.IPAddress == ipAddress);
            SaveBlockedIPs(alreadyblockedIPs);

            if (removedCount > 0)
            {
                FWModifier.RemoveFirewallRuleByIP(ipAddress);
                Console.WriteLine($"IP: {ipAddress} has been unblocked and firewall rule removed.");
            }
            else
            {
                Console.WriteLine($"IP: {ipAddress} was not found in the blocked list.");
            }
        }


        //
        public static void ShowBlockedIPs()
        {
            var alreadyblockedIPs = LoadBlockedIPs();
            Console.WriteLine("Blocked IPs:");
            foreach (var ip in alreadyblockedIPs)
            {
                Console.WriteLine($"- {ip.IPAddress} (Blocked on: {ip.BlockedAt}, Reason: {ip.Reason})");
            }
        }


        //
        public static bool IsIPBlocked(string ipAddress)
        {
            var alreadyblockedIPs = LoadBlockedIPs();
            return alreadyblockedIPs.Exists(ip => ip.IPAddress == ipAddress);
        }


        //
        public static List<BlockedIP> LoadBlockedIPs()
        {
            try
            {
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<List<BlockedIP>>(json) ?? new List<BlockedIP>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading blocked IPs: {ex.Message}");
                return new List<BlockedIP>();
            }
        }


        //
        public static void SaveBlockedIPs(List<BlockedIP> alreadyBlockedIPs)
        {
            try
            {
                var json = JsonSerializer.Serialize(alreadyBlockedIPs, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving blocked IPs: {ex.Message}");
            }
        }
    }
}
