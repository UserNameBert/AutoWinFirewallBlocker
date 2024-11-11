using System;
using System.Threading;
using System.Collections.Generic;
using FirewallManager.CommandHandlers;

namespace FirewallManager
{
    class Program
    {
        private static readonly Dictionary<string, Action<string>> commandActions = new Dictionary<string, Action<string>>
        {
            { "block", BlockCommands.ManualBlockIP },
            { "unblock", BlockCommands.UnblockIP },
            { "show blocked", _ => BlockCommands.ShowBlockedIPs() },
            { "help", _ => HelpCommands.ShowHelp() },
            { "cls", _ => Console.Clear() },
            { "exit", _ => HelpCommands.ShowExit() },
            { "active mode", _ => Mode.ActiveMode() },
            { "dormant mode", _ => Mode.DormantMode() },
            { "current mode", _ => Mode.CurrentMode() },
            { "whitelist", WhitelistCommands.WhitelistIP },
            { "show whitelisted", _ => WhitelistCommands.ShowWhitelisted() },
            { "remove whitelisted", ip => WhitelistCommands.RemoveWhitelisted(ip) }
        };

        static void Main(string[] args)
        {
            BlockCommands.InitializeDatabase();
            WhitelistCommands.InitializeDatabase();
            Console.WriteLine("\nChecking firewall for missing blocked IPs stored in Blocked.json.");
            Thread.Sleep(5000);
            FirewallSync.SyncFirewallWithBlockedIPs();
            Thread.Sleep(2500);
            BlockCommands.ShowBlockedIPs();
            WhitelistCommands.ShowWhitelisted();


            Console.WriteLine("\nFirewall Manager Terminal:\n");
            Console.WriteLine("Type 'help' for commands.");

            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine()?.Trim().ToLower();
                if (string.IsNullOrWhiteSpace(input)) continue;
                string command = GetCommand(input, out string parameter);
                if (commandActions.ContainsKey(command))
                {
                    commandActions[command](parameter);
                    if (command == "exit")
                    {
                        Thread.Sleep(5000); //Adjust for saving time
                        break; // Exit the loop on "exit" command
                    }
                }
                else
                {
                    Console.WriteLine("Unknown command. Type 'help' for a list of commands.");
                }
            }
        }

        static string GetCommand(string input, out string parameter)
        {
            parameter = string.Empty;
            foreach (var command in commandActions.Keys)
            {
                if (input.StartsWith(command))
                {
                    parameter = input.Substring(command.Length).Trim();
                    return command;
                }
            }
            int spaceIndex = input.IndexOf(' ');
            if (spaceIndex == -1) return input;

            string singleWordCommand = input.Substring(0, spaceIndex);
            parameter = input.Substring(spaceIndex + 1).Trim();
            return singleWordCommand;
        }


    }
}