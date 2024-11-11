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

        public static int MaxAttempts { get; set; } = 10;
        public static TimeSpan MaxTime { get; set; } = TimeSpan.FromMinutes(60);

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


        //
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


        //
        private static void OnEntryWritten(object sender, EntryWrittenEventArgs e)
        {
            if (e.Entry.InstanceId == 4625)
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


        //
        private static void TrackFailedLogon(string ipAddress)
        {
            DateTime currentTime = DateTime.Now;
            if (!failedLogons.ContainsKey(ipAddress))
            {
                failedLogons[ipAddress] = new List<DateTime>();
            }
            failedLogons[ipAddress].Add(currentTime);
            failedLogons[ipAddress] = failedLogons[ipAddress].Where(time => currentTime - time <= MaxTime).ToList();
        }


        //
        private static bool ShouldBlockIP(string ipAddress)
        {
            if (failedLogons.ContainsKey(ipAddress) && failedLogons[ipAddress].Count >= MaxAttempts)
            {
                failedLogons.Remove(ipAddress);
                return true;
            }
            return false;
        }


        //
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
