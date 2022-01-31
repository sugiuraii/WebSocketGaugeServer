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

        public bool ELM327VirtualCOMEnabled {get => Boolean.Parse(ELM327ServiceConfig["usevirtual"]);}
        public bool SSMVirtualCOMEnabled {get => Boolean.Parse(SSMServiceConfig["usevirtual"]);}
        public bool ArduinoVirtualCOMEnabled {get => Boolean.Parse(ArduinoServiceConfig["usevirtual"]);}
        public bool DefiVirtualCOMEnabled {get => Boolean.Parse(DefiServiceConfig["usevirtual"]);}

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