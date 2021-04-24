using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.WebSockets;
using System.Threading;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Net;
using SZ2.WebSocketGaugeServer.WebSocketServer.ArduinoWebSocketServer.Service;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.JSONFormat;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino;
using SZ2.WebSocketGaugeServer.WebSocketServer.ArduinoWebSocketServer.SessionItems;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.JSONFormat.Arduino;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using SZ2.WebSocketGaugeServer.WebSocketServer.ArduinoWebSocketServer.Middleware;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.ArduinoWebSocketServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ArduinoCOMService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime, ILoggerFactory loggerFactory, ILogger<Startup> logger)
        {
            loggerFactory.AddMemory();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120)
            };

            // Handle WebSokect connection   
            app.UseWebSockets(webSocketOptions);
            app.UseRouting();
            app.Use(async (context, next) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var cancellationToken = lifetime.ApplicationStopping;
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var middleware = new ArduinoWebSocketMiddleware(loggerFactory);
                    await middleware.HandleHttpConnection(context, webSocket, cancellationToken);
                }
                else
                {
                    await next();
                }
            });

            // Add static file with new extension mappings for bitmaptext fnt file
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".fnt"] = "text/xml";
            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });
        }
    }
}
