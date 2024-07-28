using CommonElasticConfiguration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using Serilog.Events;

namespace Address.API
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseKestrel((builderContext, options) =>
                        {
                            //                            var restPort = builderContext.Configuration.GetSection("config").GetValue<int>("RESTPort");
                            //                            options.Listen(IPAddress.Any, restPort); // it can be defined in json configuration
                            options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(60);

                            options.Configure(builderContext.Configuration.GetSection("Kestrel"));
                        })
                        .UseLibuv(options =>
                        {
                            options.ThreadCount = Environment.ProcessorCount;
                        })
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .ConfigureServices((hostingContext, services) =>
                        {

                        })
                        .ConfigureLogging((hostingContext, logging) =>
                        {
                            logging.AddSerilog(dispose: true);
                        });
                })
                .UseSerilog((hostingContext, serviceProvider, loggerConfiguration) => 
                    loggerConfiguration
                        .ReadFrom.Configuration(hostingContext.Configuration, "Serilog")
                        .AddElkConfiguration(hostingContext, serviceProvider)
                    );

    }
}
