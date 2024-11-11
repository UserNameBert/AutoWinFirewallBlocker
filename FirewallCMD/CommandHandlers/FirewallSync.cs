using System;
using FirewallManager.CommandHandlers;

namespace FirewallManager.CommandHandlers
{
    public static class FirewallSync
    {
        public static void SyncFirewallWithBlockedIPs()
        {
            var blockedIPs = BlockCommands.LoadBlockedIPs();

            foreach (var blockedEntry in blockedIPs)
            {
                string ruleName = $"{blockedEntry.Reason}_{blockedEntry.IPAddress}";

                // Check if a firewall rule exists for this IP
                if (!FWModifier.FirewallRuleExistsForIP(blockedEntry.IPAddress))
                {
                    // If no rule exists, add a new firewall rule for this IP
                    FWModifier.AddFirewallRule(ruleName, blockedEntry.IPAddress);
                }
            }
        }
    }
}
