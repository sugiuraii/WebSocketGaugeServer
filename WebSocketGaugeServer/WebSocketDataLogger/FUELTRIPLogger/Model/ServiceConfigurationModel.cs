using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.Service;
using SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.Settings;

namespace SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.Model
{
    public class ServiceConfigurationModel
    {
        private readonly IConfiguration ServiceConfiguration;
        private readonly FUELTRIPLoggerSettings fUELTRIPLoggerSettings;
        public IConfiguration ServiceConfig {get => ServiceConfiguration; }
        public FUELTRIPLoggerSettings FUELTRIPLoggerSettings {get => fUELTRIPLoggerSettings; }
        public ServiceConfigurationModel(IConfiguration configuration, FUELTRIPService service)
        {
            ServiceConfiguration = configuration.GetSection("ServiceConfig");
            fUELTRIPLoggerSettings = service.AppSettings;
        }
    }
}