using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.Model
{
    public class ServiceConfigurationModel
    {
        private readonly IConfiguration ConfigurationRoot;
        private readonly IConfiguration ServiceConfiguration;
        public IConfiguration ConfigRoot {get => ConfigurationRoot;}
        public IConfiguration ServiceConfig {get => ServiceConfiguration;}
        public IConfiguration ELM327ServiceConfig {get => ServiceConfiguration.GetSection("ELM327");}
        public IConfiguration DefiServiceConfig {get => ServiceConfiguration.GetSection("Defi");}
        public IConfiguration SSMServiceConfig {get => ServiceConfiguration.GetSection("SSM");}
        public IConfiguration ArduinoServiceConfig {get => ServiceConfiguration.GetSection("Arduino");}
        public ServiceConfigurationModel(IConfiguration configuration)
        {
            ConfigurationRoot = configuration;
            ServiceConfiguration = configuration.GetSection("ServiceConfig");
        }
    }
}