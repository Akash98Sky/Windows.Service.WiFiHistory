using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using WiFiHistory.Service;
using WiFiHistory.Service.Extensions;

var isDebugging = !(Debugger.IsAttached || args.Contains("--console"));
var hostBuilder = new HostBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<HistoryService>();
    });
if (isDebugging)
{
    await hostBuilder.RunTheServiceAsync();
}
else
{
    await hostBuilder.RunConsoleAsync();
}