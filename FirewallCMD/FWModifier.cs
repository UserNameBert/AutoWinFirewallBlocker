using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FirewallManager
{
    public static class FWModifier
    {
        public static void AddFirewallRule(string ruleName, string ipAddress)
        {
            string command = $"netsh advfirewall firewall add rule name=\"{ruleName}\" dir=in action=block remoteip={ipAddress}";
            ExecuteCommand(command);
            Console.WriteLine($"Firewall rule added to block IP: {ipAddress}");
        }
        public static void RemoveFirewallRuleByIP(string ipAddress)
        {
            // Command to list firewall rules that include the specified IP address
            string command = $"netsh advfirewall firewall show rule name=all | findstr /i \"{ipAddress}\"";
            string output = ExecuteCommand(command);

            if (string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine($"No firewall rules found for IP: {ipAddress}");
                return;
            }

            // Parse the output to extract rule names containing the IP address
            var ruleNames = ParseRuleNames(output);

            foreach (var ruleName in ruleNames)
            {
                string deleteCommand = $"netsh advfirewall firewall delete rule name=\"{ruleName}\"";
                ExecuteCommand(deleteCommand);
                Console.WriteLine($"Firewall rule removed: {ruleName}");
            }
        }
        private static List<string> ParseRuleNames(string output)
        {
            var ruleNames = new List<string>();
            var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // Assuming each line contains a rule name prefixed by "Rule Name: "
                int nameIndex = line.IndexOf("Rule Name:", StringComparison.OrdinalIgnoreCase);
                if (nameIndex >= 0)
                {
                    string ruleName = line.Substring(nameIndex + "Rule Name:".Length).Trim();
                    ruleNames.Add(ruleName);
                }
            }

            return ruleNames;
        }
        public static bool FirewallRuleExistsForIP(string ipAddress)
        {
            string command = $"netsh advfirewall firewall show rule name=all | findstr /i \"{ipAddress}\"";
            string output = ExecuteCommand(command);

            // If output is not empty, at least one rule with the specified IP exists
            return !string.IsNullOrWhiteSpace(output);
        }

        private static string ExecuteCommand(string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/C " + command;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }
    }
}
