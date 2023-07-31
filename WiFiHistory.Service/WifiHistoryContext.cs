using Microsoft.EntityFrameworkCore;
using WiFiHistory.Service.Models;

namespace WiFiHistory.Service
{
    public class WifiHistoryContext : DbContext
    {
        public WifiHistoryContext(DbContextOptions<WifiHistoryContext> options) : base(options) { }

        public virtual DbSet<ConnectionHistory> ConnectionHistory { get; set; }
    }
}
