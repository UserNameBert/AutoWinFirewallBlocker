using System;
using System.Collections.Generic;

namespace FirewallManager.CommandHandlers
{
    public static class HelpCommands
    {
        private static readonly Dictionary<string, string> commands = new Dictionary<string, string>
        {
            { "- active mode", "Actively scans for failed logon attempts and blocks them." },
            { "- dormant mode", "Disables protection and stops active scanning for failed logon attempts." },
            { "- current mode", "Displays whether the system is in active or dormant mode." },
            { "separator1", "" },

            { "- block <ip>", "Blocks an IP address." },
            { "- unblock <ip>", "Unblocks an IP address." },
            { "- show blocked", "Shows all blocked IP addresses." },
            { "separator2", "" },

            { "- whitelist <ip>", "Adds an IP to the whitelist." },
            { "- show whitelisted", "Shows all whitelisted IP addresses." },
            { "- remove whitelisted <ip>", "Removes an IP address from the whitelist." },
            { "separator3", "" },

            { "- reload", "Reloads the .JSON files. Also sets the mode back to dormant." },
            //{ "- changeMA <intger>", "Allows you to specify the max attempts before an IP gets blocked." },
            //{ "- changeMT <minutes>", "Allows you to specify the max amount of time allowed uptill the failed attempts counter is reset."},
            //{ "separator4", "" },

            { "- help", "Shows available commands." },
            { "- cls", "Clears the terminal screen." },
            { "- exit", "Saves data and closes the terminal." },
            { "separator5", "" },
        };


        //
        public static void ShowHelp()
        {
            Console.WriteLine("Available Commands:");
            foreach (var command in commands)
            {
                if (command.Key.StartsWith("separator"))
                {
                    Console.WriteLine("-------------");
                }
                else
                {
                    Console.WriteLine(command.Key.PadRight(35) + " - " + command.Value);
                }
            }
        }


        //
        public static void ShowExit()
        {
            Console.WriteLine("Saving blocked and whitelisted IPs...");
            BlockCommands.SaveBlockedIPs(BlockCommands.LoadBlockedIPs());
            WhitelistCommands.SaveWhitelistedIPs(WhitelistCommands.LoadWhitelistedIPs());
        }
    }
}
