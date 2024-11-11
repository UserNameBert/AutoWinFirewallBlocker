using System;
using System.Threading;
using FirewallManager.CommandHandlers;

namespace FirewallManager
{
    class Program
    {
        static void Main()
        {
            BlockCommands.InitializeDatabase();
            WhitelistCommands.InitializeDatabase();

            Console.WriteLine("\nChecking firewall for missing blocked IPs stored in Blocked.json.");
            FirewallSync.SyncFirewallWithBlockedIPs();
            FirewallSync.SyncFirewallWithWhitelistedIPs();

            Thread.Sleep(2000);
            BlockCommands.ShowBlockedIPs();
            WhitelistCommands.ShowWhitelisted();

            Console.WriteLine("\nFirewall Manager Terminal:");
            Console.WriteLine("Try 'help' for list of commands.");


            //
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine()?.Trim().ToLower();
                if (string.IsNullOrWhiteSpace(input)) continue;

                if (CommandHandler.ExecuteCommand(input))
                {
                    Thread.Sleep(5000);
                    break;
                }
            }
        }
    }
}
