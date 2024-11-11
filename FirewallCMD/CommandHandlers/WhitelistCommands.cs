using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text.Json;
using FirewallManager.IPsJSON;

namespace FirewallManager.CommandHandlers
{
    public static class WhitelistCommands
    {
        private static readonly string FilePath = Path.Combine("IPsJSON", "Whitelist.json");
        public static void InitializeDatabase()
        {
            try
            {
                Directory.CreateDirectory("IPsJSON");
                if (!File.Exists(FilePath))
                {
                    File.WriteAllText(FilePath, "[]");
                }
                Console.WriteLine("JSON storage initialized for whitelisted IPs.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while initializing the whitelist database:\n {ex.Message}");
            }
        }


        //
    public static void WhitelistIP(string ipAddress)
        {
            if (BlockCommands.IsIPBlocked(ipAddress))
            {
                Console.WriteLine($"Cannot whitelist IP {ipAddress} because it is already blocked.");
                return;
            }
            var whitelistedIPs = LoadWhitelistedIPs();
            if (whitelistedIPs.Exists(ip => ip.IPAddress == ipAddress))
            {
                Console.WriteLine($"IP {ipAddress} is already whitelisted.");
                return;
            }
            whitelistedIPs.Add(new WhitelistIP
            {
                IPAddress = ipAddress,
                WhitelistedAt = DateTime.Now
            });
            string action = "allow";
            FWModifier.AddFirewallRule($"@White Listed{ipAddress}", ipAddress, action);
            SaveWhitelistedIPs(whitelistedIPs);
        }


        //
        public static void RemoveWhitelisted(string ipAddress)
        {
            var whitelistedIPs = LoadWhitelistedIPs();
            int removedCount = whitelistedIPs.RemoveAll(ip => ip.IPAddress == ipAddress);
            SaveWhitelistedIPs(whitelistedIPs);

            if (removedCount > 0)
                FWModifier.RemoveFirewallRuleByIP(ipAddress);
            else
                Console.WriteLine($"IP: {ipAddress} was not found in the whitelist.");
        }


        //
        public static void ShowWhitelisted()
        {
            var whitelistedIPs = LoadWhitelistedIPs();
            Console.WriteLine("Whitelisted IPs:");
            foreach (var ip in whitelistedIPs)
            {
                Console.WriteLine($"- {ip.IPAddress} (Whitelisted at {ip.WhitelistedAt})");
            }
        }


        //
        public static bool IsIPWhitelisted(string ipAddress)
        {
            var whitelistedIPs = LoadWhitelistedIPs();
            return whitelistedIPs.Exists(ip => ip.IPAddress == ipAddress);
        }


        //
        public static List<WhitelistIP> LoadWhitelistedIPs()
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<WhitelistIP>>(json);
        }


        //
        public static void SaveWhitelistedIPs(List<WhitelistIP> whitelistedIPs)
        {
            var json = JsonSerializer.Serialize(whitelistedIPs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
    }
}
