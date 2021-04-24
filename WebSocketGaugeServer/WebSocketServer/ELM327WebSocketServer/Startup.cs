using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SZ2.WebSocketGaugeServer.WebSocketServer.ELM327WebSocketServer.Service;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using SZ2.WebSocketGaugeServer.WebSocketServer.ELM327WebSocketServer.Middleware;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.ELM327WebSocketServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<ELM327COMService>();
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

            loggerFactory.AddMemory();

            // Handle WebSokect connection
            app.UseWebSockets(webSocketOptions);
            app.UseRouting();
            app.Use(async (context, next) =>
            {
                if (context.WebSockets.IsWebSocketRequest && context.Request.Path != "/_blazor") // Ignore blazor signalR access
                {
                    var cancellationToken = lifetime.ApplicationStopping;
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var middleware = new ELM327WebSocketMiddleware(loggerFactory);
                    await middleware.HandleHttpConnection(context, webSocket, cancellationToken);
                }
                else
                {
                    await next();
                }
            });

            app.UseDefaultFiles();
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".fnt"] = "text/xml";
            app.UseStaticFiles(new StaticFileOptions{ ContentTypeProvider = provider });
            app.UseStaticFiles();

            //app.UseHttpsRedirection();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
