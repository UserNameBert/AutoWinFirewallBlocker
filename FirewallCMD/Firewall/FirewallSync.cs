using System;
using System.Threading.Tasks;
using FirewallManager.CommandHandlers;

namespace FirewallManager.CommandHandlers
{
    public static class FirewallSync
    {
        public static void SyncFirewallWithBlockedIPs()
        {
            var blockedIPs = BlockCommands.LoadBlockedIPs();
            Parallel.ForEach(blockedIPs, blockedEntry =>
            {
                try
                {
                    string ruleName = $"{blockedEntry.Reason}_{blockedEntry.IPAddress}";
                    if (!FWModifier.FirewallRuleExistsForIP(blockedEntry.IPAddress))
                    {
                        string action = "block";
                        FWModifier.AddFirewallRule(ruleName, blockedEntry.IPAddress, action);
                        Console.WriteLine($"Firewall rule added to block IP: {blockedEntry.IPAddress}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing blocked IP {blockedEntry.IPAddress}: {ex.Message}");
                }
            });
        }


        //
        public static void SyncFirewallWithWhitelistedIPs()
        {
            var whitelistedIPs = WhitelistCommands.LoadWhitelistedIPs();
            Parallel.ForEach(whitelistedIPs, whitelistedEntry =>
            {
                try
                {
                    string ruleName = $"@Whitelist_{whitelistedEntry.IPAddress}";
                    if (!FWModifier.FirewallRuleExistsForIP(whitelistedEntry.IPAddress))
                    {
                        string action = "allow";
                        FWModifier.AddFirewallRule(ruleName, whitelistedEntry.IPAddress, action);
                        Console.WriteLine($"Firewall rule added to allow IP: {whitelistedEntry.IPAddress}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing whitelisted IP {whitelistedEntry.IPAddress}: {ex.Message}");
                }
            });
        }
    }
}
