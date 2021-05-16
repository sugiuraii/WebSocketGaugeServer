using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SZ2.WebSocketGaugeServer.WebSocketServer.Service;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SZ2.WebSocketGaugeServer.WebSocketServer.Middleware;
using SZ2.WebSocketGaugeServer.WebSocketServer.Model;
using Microsoft.Extensions.FileProviders;
using System.IO;
using SZ2.WebSocketGaugeServer.WebSocketServer.Model.VirtualCOMControl;

namespace SZ2.WebSocketGaugeServer.WebSocketServer
{
    public class Startup
    {
        private readonly IConfiguration Configuration;
        private readonly IConfiguration ServiceConfiguration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ServiceConfiguration = Configuration.GetSection("ServiceConfig");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddTransient<MemoryLoggerModel>();

            if (bool.Parse(ServiceConfiguration.GetSection("ELM327")["enabled"]))
                services.AddSingleton<ELM327COMService>();
            if (bool.Parse(ServiceConfiguration.GetSection("Defi")["enabled"]))
                services.AddSingleton<DefiCOMService>();
            if (bool.Parse(ServiceConfiguration.GetSection("Arduino")["enabled"]))
                services.AddSingleton<ArduinoCOMService>();
            if (bool.Parse(ServiceConfiguration.GetSection("SSM")["enabled"]))
                services.AddSingleton<SSMCOMService>();

            services.AddTransient<VirtualArduinoCOMControlModel>();

            services.AddTransient<ServiceConfigurationModel>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime, ILoggerFactory loggerFactory, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120)
            };

            loggerFactory.AddMemory(LogLevel.Debug);

            // Set urlpath for listening websocket connection
            string elm327urlpath = GetELM327URLPath(logger);
            string arduinourlpath = GetArduinoURLPath(logger);
            string defiurlpath = GetDefiURLPath(logger);
            string ssmurlpath = GetSSMURLPath(logger);
            // Handle WebSokect connection
            app.UseWebSockets(webSocketOptions);
            app.UseRouting();
            app.Use(async (context, next) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    if (context.Request.Path.Equals("/_blazor"))
                        // Pass through blazor signalR access
                        await next();
                    else if (context.Request.Path.Equals(elm327urlpath))
                    {
                        if (bool.Parse(ServiceConfiguration.GetSection("ELM327")["enabled"]))
                        {
                            var cancellationToken = lifetime.ApplicationStopping;
                            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            var middleware = new ELM327WebSocketMiddleware(loggerFactory);
                            await middleware.HandleHttpConnection(context, webSocket, cancellationToken);
                        }
                        else
                            logger.LogError("ELM327 websocket connection is requested. However, ELM327 service is disabled.");
                    }
                    else if (context.Request.Path.Equals(defiurlpath))
                    {
                        if (bool.Parse(ServiceConfiguration.GetSection("Defi")["enabled"]))
                        {
                            var cancellationToken = lifetime.ApplicationStopping;
                            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            var middleware = new DefiWebSocketMiddleware(loggerFactory);
                            await middleware.HandleHttpConnection(context, webSocket, cancellationToken);
                        }
                        else
                            logger.LogError("Defi websocket connection is requested. However, Defi service is disabled.");
                    }
                    else if (context.Request.Path.Equals(ssmurlpath))
                    {
                        if (bool.Parse(ServiceConfiguration.GetSection("SSM")["enabled"]))
                        {
                            var cancellationToken = lifetime.ApplicationStopping;
                            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            var middleware = new SSMWebSocketMiddleware(loggerFactory);
                            await middleware.HandleHttpConnection(context, webSocket, cancellationToken);
                        }
                        else
                            logger.LogError("SSM websocket connection is requested. However, SSM service is disabled.");
                    }
                    else if (context.Request.Path.Equals(arduinourlpath))
                    {
                        if (bool.Parse(ServiceConfiguration.GetSection("Arduino")["enabled"]))
                        {
                            var cancellationToken = lifetime.ApplicationStopping;
                            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            var middleware = new ArduinoWebSocketMiddleware(loggerFactory);
                            await middleware.HandleHttpConnection(context, webSocket, cancellationToken);
                        }
                        else
                            logger.LogError("Arduino websocket connection is requested. However, Arduino service is disabled.");
                    }
                    else
                        await next();
                }
                else
                {
                    await next();
                }
            });

            app.UseDefaultFiles();
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".fnt"] = "text/xml";
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider,
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "clientfiles")),
                RequestPath = "/clientfiles"
            });

            app.UseStaticFiles();

            //app.UseHttpsRedirection();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        private string GetELM327URLPath(ILogger logger)
        {
            string path = ServiceConfiguration.GetSection("ELM327")["urlpath"];
            if (path == null)
            {
                logger.LogWarning("URL path of ELM327 service is not found in appsettings json file. use /elm327 instead.");
                path = "/elm327";
            }
            return path;
        }

        private string GetArduinoURLPath(ILogger logger)
        {
            string path = ServiceConfiguration.GetSection("Arduino")["urlpath"];
            if (path == null)
            {
                logger.LogWarning("URL path of Arduino service is not found in appsettings json file. use /arduino instead.");
                path = "/arduino";
            }
            return path;
        }
        private string GetDefiURLPath(ILogger logger)
        {
            string path = ServiceConfiguration.GetSection("Defi")["urlpath"];
            if (path == null)
            {
                logger.LogWarning("URL path of Defi service is not found in appsettings json file. use /defi instead.");
                path = "/defi";
            }
            return path;
        }
        private string GetSSMURLPath(ILogger logger)
        {
            string path = ServiceConfiguration.GetSection("SSM")["urlpath"];
            if (path == null)
            {
                logger.LogWarning("URL path of SSM service is not found in appsettings json file. use /ssm instead.");
                path = "/ssm";
            }
            return path;
        }
    }
}
