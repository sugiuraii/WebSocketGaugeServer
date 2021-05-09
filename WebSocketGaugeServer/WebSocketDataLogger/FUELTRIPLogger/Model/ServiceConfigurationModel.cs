using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.Service;
using SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.Settings;

namespace SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.Model
{
    public class ServiceConfigurationModel
    {
        private readonly IConfiguration appSettings;
        private readonly FUELTRIPLoggerSettings fUELTRIPLoggerSettings;
        public IConfiguration AppSettings {get => appSettings; }
        public FUELTRIPLoggerSettings FUELTRIPLoggerSettings {get => fUELTRIPLoggerSettings; }
        public ServiceConfigurationModel(IConfiguration configuration, FUELTRIPService service)
        {
            appSettings = configuration;
            fUELTRIPLoggerSettings = service.AppSettings;
        }
    }
}