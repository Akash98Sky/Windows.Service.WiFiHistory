using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using Quartz;
using Quartz.Impl;
using System.Collections.Specialized;
using System.Configuration;
using WiFiHistory.Service.Helpers;
using WiFiHistory.Service.Interfaces;

namespace WiFiHistory.Service
{
    public class HistoryService : IHostedService
    {
        private static ILogger _logger;
        private IScheduler _scheduler = null!;
        public HistoryService()
        {
            SetUpNLog();
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _scheduler = await GetScheduler();
                var serviceProvider = GetConfiguredServiceProvider();
                _scheduler.JobFactory = new CustomJobFactory(serviceProvider);
                await _scheduler.Start();
                await ConfigureMinutelyJob(_scheduler);
            }
            catch (Exception ex)
            {
                _logger.Error(new CustomConfigurationException(ex.Message));
            }
        }

        private async Task ConfigureMinutelyJob(IScheduler scheduler)
        {
            var minutelyJob = GetMinutelyJob();
            if (await scheduler.CheckExists(minutelyJob.Key))
            {
                await scheduler.ResumeJob(minutelyJob.Key);
                _logger.Info($"The job key {minutelyJob.Key} was already existed, thus resuming the same");
            }
            else
            {
                await scheduler.ScheduleJob(minutelyJob, GetMinutelyJobTrigger());
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _scheduler.Shutdown(cancellationToken);
            await _scheduler.Clear(cancellationToken);
        }

        #region "Private Functions"
        private IServiceProvider GetConfiguredServiceProvider()
        {
            var services = new ServiceCollection()
                .AddScoped<IMinutelyJob, MinutelyJob>()
                .AddScoped<IHelperService, HelperService>();
            services.AddDbContext<WifiHistoryContext>(
                (option) => option.UseSqlite(Convert.ToString(ConfigurationManager.ConnectionStrings["DbConnectionString"])));
            return services.BuildServiceProvider();
        }
        private IJobDetail GetMinutelyJob()
        {
            return JobBuilder.Create<IMinutelyJob>()
                .WithIdentity("minutelyjob", "minutelygroup")
                .Build();
        }
        private ITrigger GetMinutelyJobTrigger()
        {
            return TriggerBuilder.Create()
                 .WithIdentity("minutelytrigger", "minutelygroup")
                 .StartNow()
                 .WithSimpleSchedule(x => x
                     .WithIntervalInSeconds(60)
                     .RepeatForever())
                 .Build();
        }
        private static async Task<IScheduler> GetScheduler()
        {
            // Uncomment this if you want to use RAM instead of database start
            var props = new NameValueCollection { { "quartz.serializer.type", "binary" } };
            var factory = new StdSchedulerFactory(props);
            // Uncomment this if you want to use RAM instead of database end
            var scheduler = await factory.GetScheduler();
            return scheduler;
        }
        private void SetUpNLog()
        {
            var config = new NLog.Config.LoggingConfiguration();
            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "wifihistoryclientlogfile_historyservice.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            // Rules for mapping loggers to targets            
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logfile);
            // Apply config           
            LogManager.Configuration = config;
            _logger = LogManager.GetCurrentClassLogger();
        }
        #endregion
    }
}
