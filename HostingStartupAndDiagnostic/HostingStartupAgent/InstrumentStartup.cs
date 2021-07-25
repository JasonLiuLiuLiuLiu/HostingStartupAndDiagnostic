using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HostingStartupAgent
{
    class InstrumentStartup : IHostedService
    {

        private readonly ILogger _logger;

        public InstrumentStartup(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(InstrumentStartup));
        }
        public Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogInformation("Initializing ...");
            DiagnosticListener.AllListeners.Subscribe(new AspNetCoreTracingDiagnosticObserver());
            DiagnosticListener.AllListeners.Subscribe(new CustomTracingDiagnosticObserver());
            _logger.LogInformation("Started InstrumentStartup Agent.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogInformation("Stopped InstrumentStartup Agent.");
            return Task.CompletedTask;
        }
    }
}
