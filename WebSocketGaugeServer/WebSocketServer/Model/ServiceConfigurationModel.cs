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

        public bool ELM327VirtualCOMEnabled {get => Boolean.Parse(ELM327ServiceConfig.GetSection("virtualecu")["enabled"]);}
        public bool SSMVirtualCOMEnabled {get => Boolean.Parse(SSMServiceConfig.GetSection("virtualecu")["enabled"]);}
        public bool ArduinoVirtualCOMEnabled {get => Boolean.Parse(ArduinoServiceConfig.GetSection("virtualecu")["enabled"]);}
        public bool DefiVirtualCOMEnabled {get => Boolean.Parse(DefiServiceConfig.GetSection("virtualecu")["enabled"]);}

        public int ELM327VirtualCOMWait {get => int.Parse(ELM327ServiceConfig.GetSection("virtualecu")["waitmsec"]);}
        public int SSMVirtualCOMWait {get => int.Parse(SSMServiceConfig.GetSection("virtualecu")["waitmsec"]);}
        public int ArduinoVirtualCOMWait {get => int.Parse(ArduinoServiceConfig.GetSection("virtualecu")["waitmsec"]);}
        public int DefiVirtualCOMWait {get => int.Parse(DefiServiceConfig.GetSection("virtualecu")["waitmsec"]);}

        public bool ELM327COMEnabled {get => Boolean.Parse(ELM327ServiceConfig["enabled"]);}
        public bool SSMCOMEnabled {get => Boolean.Parse(SSMServiceConfig["enabled"]);}
        public bool ArduinoCOMEnabled {get => Boolean.Parse(ArduinoServiceConfig["enabled"]);}
        public bool DefiCOMEnabled {get => Boolean.Parse(DefiServiceConfig["enabled"]);}
        public ServiceConfigurationModel(IConfiguration configuration)
        {
            ConfigurationRoot = configuration;
            ServiceConfiguration = configuration.GetSection("ServiceConfig");
        }
        public bool ClientFilesEnabled  { get => bool.Parse(ConfigurationRoot.GetSection("clientFiles")["enabled"]);}
    }
}