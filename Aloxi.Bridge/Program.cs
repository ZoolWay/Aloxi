using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ZoolWay.Aloxi.Bridge
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<ActorSystemProvider>();
                    services.AddHostedService<Worker>();
                    var mc = Mediation.MediationConfigBuilder.From(hostContext.Configuration.GetSection("Mediation"));
                    if (mc.ActiveClients.Contains(Mediation.MediationClientType.SignalR))
                    {
                        var sr = services.AddSignalR();
                        sr.AddHubOptions<Mediation.SignalR.AloxiHub>(c =>
                        {
                        });
                        sr.AddAzureSignalR(mc.SignalR.ConnectionString);
                    }
                })
                .ConfigureWebHostDefaults((webBuilder) =>
                {
                    webBuilder.UseStartup<Api.Startup>();
                })
                .UseSystemd();
        }
    }
}
