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
                if (!FWModifier.FirewallRuleExistsForIP(blockedEntry.IPAddress))
                {
                    string action = "block";
                    FWModifier.AddFirewallRule(ruleName, blockedEntry.IPAddress, action);
                }
            }
        }
        
        
        //
        public static void SyncFireWallWithWhitelistedIPs()
        {

        }
    }
}
