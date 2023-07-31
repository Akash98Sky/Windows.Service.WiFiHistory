using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiFiHistory.Service.Models
{
    public class ConnectionHistory
    {
        public int Id { get; set; }
        public Guid InterfaceGuid { get; set; }
        public string InterfaceName { get; set; } = string.Empty;
        public string SSID { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; } = DateTime.Now;
        public DateTime ConnectedUntil { get; set; } = DateTime.Now;
    }
}
