using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using log4net;

[assembly: log4net.Config.XmlConfigurator(ConfigFile="log4net.config",Watch=true)]

namespace FUELTRIP_Logger
{
    public class Program
    {
        static ILog logger = LogManager.GetLogger(typeof(Program));
        public static void Main(string[] args)
        {
            logger.Info("Start");
            var host =  CreateHostBuilder(args).Build();
            try
            {
                host.Run();
            }
            catch(OperationCanceledException ex)
            {
                logger.Info(ex.Message);                
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://*:5000");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
