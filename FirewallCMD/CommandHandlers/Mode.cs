using System;
using System.Threading;

namespace FirewallManager.CommandHandlers
{
    public static class Mode
    {
        private static bool isActive = false;

        public static void ActiveMode()
        {
            if (!isActive)
            {
                isActive = true;
                Console.WriteLine("Switching to Active Mode. Monitoring for failed logon attempts...");
                FWMonitor.StartMonitoring();
            }
            else
            {
                Console.WriteLine("Already in Active Mode.");
            }
        }


        //
        public static void DormantMode(bool reload)
        {
            if (isActive)
            {
                isActive = false;
                Console.WriteLine("Switching to Dormant Mode. Stopping monitoring.");
                FWMonitor.StopMonitoring();
            }
            else if (!reload)
            {
                Console.WriteLine("Already in Dormant Mode.");
            }
        }


        //
        public static void CurrentMode()
        {
            Console.WriteLine($"Current mode: {(isActive ? "Active Mode" : "Dormant Mode")}");
        }
    }
}
