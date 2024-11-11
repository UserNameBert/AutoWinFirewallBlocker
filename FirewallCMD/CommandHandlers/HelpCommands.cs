using System;

namespace FirewallManager.CommandHandlers
{
    public static class HelpCommands
    {
        public static void ShowHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("  active mode                  - Actively scans for failed logon attempts and blocks them.");
            Console.WriteLine("  dormant mode                 - Protection is disabled and will not actively scan for failed logon attempts.");
            Console.WriteLine("  current mode                 - Checks if active or dormant.");
            Console.WriteLine("  block <ip>                   - Block an IP address.");
            Console.WriteLine("  unblock <ip>                 - Unblock an IP address.");
            Console.WriteLine("  show blocked                 - Show all blocked IPs.");
            Console.WriteLine("  whitelist <ip>               - Adds an IP to the whitelist.");
            Console.WriteLine("  show whitelisted             - Shows all whitelisted IPs.");
            Console.WriteLine("  remove whitelisted <ip>      - Removes a whitelisted IP.");
            Console.WriteLine("  help                         - Show available commands.");
            Console.WriteLine("  cls                          - Clears the terminal");
            Console.WriteLine("  exit                         - Closes Terminal.");
        }

        public static void ShowExit()
        {
            Console.WriteLine("Saving blocked & whitelisted IPs...");
        }
    }
}
