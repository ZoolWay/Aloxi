using System;
using Microsoft.AspNetCore.Hosting;
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
                })
                .ConfigureWebHostDefaults((webBuilder) =>
                {
                    webBuilder.UseStartup<Api.Startup>();
                })
                .UseSystemd();
        }
    }
}
