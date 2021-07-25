using HostingStartupAgent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

[assembly: HostingStartup(typeof(StartupAgent))]
namespace HostingStartupAgent
{
    public class StartupAgent : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            Console.WriteLine("In Startup Agent");
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IHostedService, InstrumentStartup>();
            });
        }
    }
}
