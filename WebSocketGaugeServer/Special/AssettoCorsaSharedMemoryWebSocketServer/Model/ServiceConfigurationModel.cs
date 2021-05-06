using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace SZ2.WebSocketGaugeServer.Special.AssettoCorsaSharedMemoryWebSocketServer.Model
{
    public class ServiceConfigurationModel
    {
        private readonly IConfiguration ServiceConfiguration;
        public IConfiguration ServiceConfig {get => ServiceConfiguration; }
        public ServiceConfigurationModel(IConfiguration configuration)
        {
            ServiceConfiguration = configuration.GetSection("ServiceConfig");
        }
    }
}