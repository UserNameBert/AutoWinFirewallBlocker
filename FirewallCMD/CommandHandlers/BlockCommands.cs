using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FirewallManager.IPsJSON;

namespace FirewallManager.CommandHandlers
{
    public static class BlockCommands
    {
        // Try initialize database (create JSON file if it doesn't exist)
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
        public static void ManualBlockIP(string blockedIP)
        {
            // Cross-check if the IP is already whitelisted
            if (WhitelistCommands.IsIPWhitelisted(blockedIP))
            {
                Console.WriteLine($"Cannot block IP: {blockedIP} because it is whitelisted.");
                return;
            }

            // Check if the IP is already blocked
            var blockedIPs = LoadBlockedIPs();
            if (blockedIPs.Exists(ip => ip.IPAddress == blockedIP))
            {
                Console.WriteLine($"IP: {blockedIP} is already blocked.");
                return;
            }
            blockedIPs.Add(new BlockedIP
            {
                IPAddress = blockedIP,
                BlockedAt = DateTime.Now,
                Reason = "@Manual block"
            });
            SaveBlockedIPs(blockedIPs);
            FWModifier.AddFirewallRule($"@Manual block_{blockedIP}", blockedIP);
            Console.WriteLine($"IP: {blockedIP} has been blocked");
        }
        public static void AutoBlockIP(string blockedIP)
        {
            // Cross-check if the IP is already whitelisted
            if (WhitelistCommands.IsIPWhitelisted(blockedIP))
            {
                Console.WriteLine($"Cannot block IP: {blockedIP} because it is whitelisted.");
                return;
            }

            // Check if the IP is already blocked
            var blockedIPs = LoadBlockedIPs();
            if (blockedIPs.Exists(ip => ip.IPAddress == blockedIP))
            {
                Console.WriteLine($"IP: {blockedIP} is already blocked.");
                return;
            }
            blockedIPs.Add(new BlockedIP
            {
                IPAddress = blockedIP,
                BlockedAt = DateTime.Now,
                Reason = "@Auto block"
            });
            FWModifier.AddFirewallRule($"@Auto block_{blockedIP}", blockedIP);
            SaveBlockedIPs(blockedIPs);
        }
        public static void UnblockIP(string blockedIP)
        {
            var blockedIPs = LoadBlockedIPs();
            int removedCount = blockedIPs.RemoveAll(ip => ip.IPAddress == blockedIP);
            SaveBlockedIPs(blockedIPs);

            if (removedCount > 0)
            {
                FWModifier.RemoveFirewallRuleByIP(blockedIP);
            }
            else
            {
                Console.WriteLine($"IP: {blockedIP} was not found in the blocked list.");
            }
        }

        public static void ShowBlockedIPs()
        {
            var blockedIPs = LoadBlockedIPs();
            Console.WriteLine("Blocked IPs:");
            foreach (var ip in blockedIPs)
            {
                Console.WriteLine($"- {ip.IPAddress} (Blocked at {ip.BlockedAt}, Reason: {ip.Reason})");
            }
        }
        public static bool IsIPBlocked(string ipAddress)
        {
            var blockedIPs = LoadBlockedIPs();
            return blockedIPs.Exists(ip => ip.IPAddress == ipAddress);
        }
        public static List<BlockedIP> LoadBlockedIPs()
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<BlockedIP>>(json);
        }
        private static void SaveBlockedIPs(List<BlockedIP> blockedIPs)
        {
            var json = JsonSerializer.Serialize(blockedIPs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);

        }
    }
}
