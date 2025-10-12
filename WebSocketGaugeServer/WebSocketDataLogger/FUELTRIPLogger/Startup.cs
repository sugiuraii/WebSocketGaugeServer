using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.Service;
using SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.Middleware;
using SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.Model;

namespace SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddSingleton<FUELTRIPService>();
            services.AddTransient<MemoryLoggerModel>();
            services.AddTransient<ServiceConfigurationModel>();
            services.AddTransient<FUELTRIPDataViewModel>();
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

            app.UseWebSockets(webSocketOptions);

            app.UseRouting();
            app.UseStaticFiles();

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
                        case ("/fueltrip"):
                            var cancellationToken = lifetime.ApplicationStopping;
                            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            var middleware = new FUELTRIPLoggerWebSocketMiddleware(loggerFactory);
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
