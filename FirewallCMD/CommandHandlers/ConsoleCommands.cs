using System;
using System.Collections.Generic;
using FirewallManager.CommandHandlers;

namespace FirewallManager
{
    public static class CommandHandler
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
            { "dormant mode", _ => Mode.DormantMode(false) },
            { "current mode", _ => Mode.CurrentMode() },
            { "whitelist", WhitelistCommands.WhitelistIP },
            { "show whitelisted", _ => WhitelistCommands.ShowWhitelisted() },
            { "remove whitelisted", ip => WhitelistCommands.RemoveWhitelisted(ip) },
            { "reload", _ => ReloadAll() },
            { "changeMA", input => ChangeMaxAttempts(input) },
            { "changeMT", input => ChangeMaxTime(input) },
        };


        //
        public static bool ExecuteCommand(string input)
        {
            string command = GetCommand(input, out string parameter);
            if (commandActions.ContainsKey(command))
            {
                try
                {
                    commandActions[command](parameter);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing command '{command}': {ex.Message}");
                }
                return command.Equals("exit", StringComparison.OrdinalIgnoreCase);
            }
            Console.WriteLine("Unknown command. Type 'help' for a list of commands.");
            return false;
        }


        //
        private static string GetCommand(string input, out string parameter)
        {
            parameter = string.Empty;
            foreach (var command in commandActions.Keys)
            {
                if (input.StartsWith(command, StringComparison.OrdinalIgnoreCase))
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


        //
        private static void ReloadAll() 
        {
            Console.WriteLine("Reloading all .JSONs.");
            Mode.DormantMode(true);
            BlockCommands.SaveBlockedIPs(BlockCommands.LoadBlockedIPs());
            WhitelistCommands.SaveWhitelistedIPs(WhitelistCommands.LoadWhitelistedIPs());

            BlockCommands.InitializeDatabase();
            FirewallSync.SyncFirewallWithBlockedIPs();

            WhitelistCommands.InitializeDatabase();
            FirewallSync.SyncFirewallWithWhitelistedIPs();
        }


        //
        public static void ChangeMaxAttempts(string input)
        {
            if (int.TryParse(input, out int maxAttempts) && maxAttempts > 1)
            {
                FWMonitor.MaxAttempts = maxAttempts;
                Console.WriteLine($"Max logon attempts has been updated to: {maxAttempts}.");
            }
            else
            {
                Console.WriteLine("Invaild input, use a number greater than 1.");
            }
        }


        //
        public static void ChangeMaxTime(string input)
        {
            if(int.TryParse(input,out int maxMinutes) && maxMinutes > 1)
            {
                FWMonitor.MaxTime = TimeSpan.FromMinutes(maxMinutes);
                Console.WriteLine($"Max time allowed from first log on attempt has been updated to: {maxMinutes}");
            }
            else
            {
                Console.WriteLine("Invailid input, use a number greater than 1");
            }
        }
    }
}
