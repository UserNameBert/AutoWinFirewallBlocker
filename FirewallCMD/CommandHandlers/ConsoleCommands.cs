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
        };


        //
        public static bool ExecuteCommand(string input)
        {
            string command = GetCommand(input, out string parameter);
            if (commandActions.ContainsKey(command))
            {
                commandActions[command](parameter);
                return command == "exit";
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


        private static void ReloadAll() 
        {
            Console.WriteLine("Reloading all .JSONs.");
            Mode.DormantMode(true);
            BlockCommands.SaveBlockedIPs(BlockCommands.LoadBlockedIPs());
            WhitelistCommands.SaveWhitelistedIPs(WhitelistCommands.LoadWhitelistedIPs());

            BlockCommands.InitializeDatabase();
            WhitelistCommands.InitializeDatabase();
        }
    }
}
