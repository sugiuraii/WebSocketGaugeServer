using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace SZ2.WebSocketGaugeServer.Special.AssettoCorsaSharedMemoryWebSocketServer.Model
{
    public class ServiceConfigurationModel
    {
        private readonly IConfiguration Configuration;
        public IConfiguration Config {get => Configuration; }
        public ServiceConfigurationModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}