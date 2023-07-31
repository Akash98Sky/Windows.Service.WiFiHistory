using Microsoft.EntityFrameworkCore;
using NativeWifi;
using NLog;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO.Compression;
using System.Text;
using WiFiHistory.Service.Interfaces;
using WiFiHistory.Service.Models;
using static NativeWifi.WlanClient;

namespace WiFiHistory.Service.Helpers
{
    public class HelperService : IHelperService
    {
        private static ILogger _logger = null!;
        private WlanClient _wlanClient;
        private WifiHistoryContext _historyDbContext;
        public HelperService(WifiHistoryContext historyContext)
        {
            SetUpNLog();
            _wlanClient = new WlanClient();
            _historyDbContext = historyContext;
        }

        public async Task PerformService(string schedule)
        {
            try
            {
                _logger.Info($"{DateTime.Now}: The PerformService() is called with {schedule} schedule");
                if (!string.IsNullOrWhiteSpace(schedule))
                {
                    await ProcessWifiConnections();
                    _logger.Info($"{DateTime.Now}: The PerformService() is finished with {schedule} schedule");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{DateTime.Now}: Exception is occured at PerformService(): {ex.Message}");
                throw new CustomConfigurationException(ex.Message);
            }
        }

        private async Task ProcessWifiConnections()
        {
            foreach (WlanInterface wlanInterface in _wlanClient.Interfaces)
            {
                var interfaceGuid = wlanInterface.InterfaceGuid;
                var interfaceName = wlanInterface.InterfaceName;
                Wlan.Dot11Ssid ssid = wlanInterface.CurrentConnection.wlanAssociationAttributes.dot11Ssid;
                var ssidName = new String(Encoding.ASCII.GetChars(ssid.SSID, 0, (int)ssid.SSIDLength));

                var query = from conHist in _historyDbContext.ConnectionHistory
                            where conHist.InterfaceGuid == interfaceGuid
                            && conHist.SSID == ssidName
                            && conHist.ConnectedUntil >= DateTime.Now.AddMinutes(-2)
                            select conHist;

                if(await query.AnyAsync())
                {
                    var conHist = await query.SingleAsync();
                    conHist.ConnectedUntil = DateTime.Now;
                    _historyDbContext.ConnectionHistory.Update(conHist);
                }
                else
                {
                    var conHist = new ConnectionHistory
                    {
                        InterfaceGuid = interfaceGuid,
                        InterfaceName = interfaceName,
                        SSID = ssidName,
                        ConnectedAt = DateTime.Now,
                        ConnectedUntil = DateTime.Now,
                    };
                    _historyDbContext.ConnectionHistory.Add(conHist);
                }
                await _historyDbContext.SaveChangesAsync();
            }
        }

        private void SetUpNLog()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "wifihistoryclientlogfile_helperservice.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logfile);

            // Apply config           
            LogManager.Configuration = config;

            _logger = LogManager.GetCurrentClassLogger();
        }
    }
}
