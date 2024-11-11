using System;

namespace FirewallManager.IPsJSON
{
    public class BlockedIP
    {
        public string IPAddress { get; set; }
        public DateTime BlockedAt { get; set; }
        public string Reason { get; set; }
    }
}
