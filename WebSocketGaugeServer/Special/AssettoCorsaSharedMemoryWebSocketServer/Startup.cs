using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using SSZ2.WebSocketGaugeServer.Special.AssettoCorsaSharedMemoryWebSocketServer.Service;
using SZ2.WebSocketGaugeServer.Special.AssettoCorsaSharedMemoryWebSocketServer.Middleware;
using SZ2.WebSocketGaugeServer.Special.AssettoCorsaSharedMemoryWebSocketServer.Model;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace SZ2.WebSocketGaugeServer.Special.AssettoCorsaSharedMemoryWebSocketServer
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
            services.AddTransient<ServiceConfigurationModel>();
            services.AddSingleton<AssettoCorsaSHMService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime, ILoggerFactory loggerFactory, ILogger<Startup> logger)
        {
            loggerFactory.AddMemory();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var webSocketOptions = new WebSocketOptions();

            // Handle WebSokect connection 
            app.UseWebSockets(webSocketOptions);
            app.UseRouting();
            int keepAliveDummyMessageInterval = this.GetKeepAliveDummyMessageInterval(logger);

            app.Use(async (context, next) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    switch (context.Request.Path)
                    {
                        case ("/_blazor"):
                            // Pass through blazor signalR access
                            await next();
                            break;
                        case ("/assettocorsa_ws"):
                            var cancellationToken = lifetime.ApplicationStopping;
                            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            var middleware = new AssettoCorsaSHMWebSocketMiddleware(loggerFactory, keepAliveDummyMessageInterval);
                            await middleware.HandleHttpConnectionAsync(context, webSocket, cancellationToken);
                            break;
                        default:
                            await next();
                            break;    
                    }
                }
                else
                {
                    await next();
                }
            });

            app.UseDefaultFiles();
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".fnt"] = "text/xml";
            provider.Mappings[".jsonc"] = "text/xml";
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
        private int GetKeepAliveDummyMessageInterval(ILogger logger) 
        {
            var interval = ServiceConfiguration["KeepAliveDummyMessageInterval"];
            if (interval == null)
            {
                logger.LogWarning("KeepAliveDummyMessageInterval setting is not found in appsettings json file. use 0 to disable this feature. instead.");
                interval = "0";
            }
            return int.Parse(interval);
        }
    }
}
