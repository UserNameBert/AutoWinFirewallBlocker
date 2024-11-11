using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FirewallManager.CommandHandlers;

namespace FirewallManager
{
    public static class FWMonitor
    {
        private static EventLog eventLog;
        private static readonly Dictionary<string, List<DateTime>> failedLogons = new Dictionary<string, List<DateTime>>();
        private const int MaxAttempts = 10;
        private static readonly TimeSpan TimeWindow = TimeSpan.FromMinutes(60);

        public static void StartMonitoring()
        {
            if (eventLog != null)
            {
                Console.WriteLine("Firewall monitor is already running.");
                return;
            }
            eventLog = new EventLog("Security");
            eventLog.EntryWritten += OnEntryWritten;
            eventLog.EnableRaisingEvents = true;
        }

        public static void StopMonitoring()
        {
            if (eventLog != null)
            {
                eventLog.EntryWritten -= OnEntryWritten;
                eventLog.EnableRaisingEvents = false;
                eventLog.Dispose();
                eventLog = null;
            }
        }

        private static void OnEntryWritten(object sender, EntryWrittenEventArgs e)
        {
            if (e.Entry.InstanceId == 4625) // Failed logon attempt
            {
                string message = e.Entry.Message;
                string ipAddress = ExtractField(message, "Source Network Address:");
                string userName = ExtractField(message, "Workstation Name:") != "Unknown"
                    ? ExtractField(message, "Workstation Name:")
                    : ExtractField(message, "Account Name:");

                if (BlockCommands.IsIPBlocked(ipAddress))
                {
                    Console.WriteLine($"Blocked IP: {ipAddress} attempted another logon.");
                }
                else
                {
                    Console.WriteLine($"\nFailed logon attempt detected from IP: {ipAddress}, Username: {userName}");
                    if (ipAddress != "Unknown")
                    {
                        TrackFailedLogon(ipAddress);

                        if (ShouldBlockIP(ipAddress) && !BlockCommands.IsIPBlocked(ipAddress))
                        {
                            BlockCommands.AutoBlockIP(ipAddress);
                            Console.WriteLine($"IP: {ipAddress} has been blocked due to repeated failed logon attempts.");
                        }
                    }
                }
            }
        }

        private static void TrackFailedLogon(string ipAddress)
        {
            DateTime currentTime = DateTime.Now;

            // Add a new entry for this IP if it doesn't exist
            if (!failedLogons.ContainsKey(ipAddress))
            {
                failedLogons[ipAddress] = new List<DateTime>();
            }

            // Add the current failed attempt to the list
            failedLogons[ipAddress].Add(currentTime);

            // Remove any attempts that are outside the time window
            failedLogons[ipAddress] = failedLogons[ipAddress].Where(time => currentTime - time <= TimeWindow).ToList();
        }

        private static bool ShouldBlockIP(string ipAddress)
        {
            // Check if the number of attempts exceeds the threshold within the time window
            if (failedLogons.ContainsKey(ipAddress) && failedLogons[ipAddress].Count >= MaxAttempts)
            {
                // Remove the IP from tracking after blocking
                failedLogons.Remove(ipAddress);
                return true;
            }
            return false;
        }

        private static string ExtractField(string message, string fieldLabel)
        {
            int index = message.IndexOf(fieldLabel);
            if (index >= 0)
            {
                string fieldPart = message.Substring(index + fieldLabel.Length).Trim();
                int endIndex = fieldPart.IndexOfAny(new[] { ' ', '\n', '\r' });
                if (endIndex >= 0)
                {
                    return fieldPart.Substring(0, endIndex).Trim();
                }
                return fieldPart;
            }
            return "Unknown";
        }
    }
}
