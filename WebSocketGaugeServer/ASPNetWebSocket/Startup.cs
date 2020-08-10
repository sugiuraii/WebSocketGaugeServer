using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ASPNetWebSocket.Service;
using System.Net.WebSockets;
using System.Threading;
using DefiSSMCOM.WebSocket;
using DefiSSMCOM.WebSocket.JSON;

namespace ASPNetWebSocket
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<DefiCOMService>(new DefiCOMService("COM3"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };
            app.UseWebSockets(webSocketOptions);

            app.UseRouting();

            app.Use(async (context, next) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await Handle(context, webSocket);
                }
                else
                {
                    await next();
                }
            });
        }

        private async Task Handle(HttpContext context, WebSocket webSocket)
        {
            var service = (DefiCOMService)context.RequestServices.GetRequiredService(typeof(DefiCOMService));

            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var connectionID = Guid.NewGuid();
            service.AddWebSocket(connectionID, webSocket);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            service.RemoveWebSocket(connectionID);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            await tickTask;
        }

        private void processReceivedJSONMessage(string receivedJSONmode, string message, DefiCOMWebsocketSessionParam sessionParam, WebSocket ws)
        {
            switch (receivedJSONmode)
            {
                case (ResetJSONFormat.ModeCode):
                    sessionParam.reset();
                    send_response_msg(session, "Defi Websocket all parameter reset.");
                    break;
                case (DefiWSSendJSONFormat.ModeCode):
                    DefiWSSendJSONFormat msg_obj_wssend = JsonConvert.DeserializeObject<DefiWSSendJSONFormat>(message);
                    msg_obj_wssend.Validate();
                    sessionParam.Sendlist[(DefiParameterCode)Enum.Parse(typeof(DefiParameterCode), msg_obj_wssend.code)] = msg_obj_wssend.flag;

                    send_response_msg(session, "Defi Websocket send_flag for : " + msg_obj_wssend.code.ToString() + " set to : " + msg_obj_wssend.flag.ToString());
                    break;

                case (DefiWSIntervalJSONFormat.ModeCode):
                    DefiWSIntervalJSONFormat msg_obj_interval = JsonConvert.DeserializeObject<DefiWSIntervalJSONFormat>(message);
                    msg_obj_interval.Validate();
                    sessionparam.SendInterval = msg_obj_interval.interval;

                    send_response_msg(session, "Defi Websocket send_interval to : " + msg_obj_interval.interval.ToString());
                    break;
                default:
                    throw new JSONFormatsException("Unsuppoted mode property.");
            }
        }

        protected void send_error_msg(WebSocket ws, string message)
        {
            ErrorJSONFormat json_error_msg = new ErrorJSONFormat();
            json_error_msg.msg = message;
            
            session.Send(json_error_msg.Serialize());
            IPAddress destinationAddress = session.RemoteEndPoint.Address;
            logger.Error("Send Error message to " + destinationAddress.ToString() + " : " + message);
        }

        protected void send_response_msg(WebSocket Ws, string message)
        {
            ResponseJSONFormat json_response_msg = new ResponseJSONFormat();
            json_response_msg.msg = message;
            session.Send(json_response_msg.Serialize());

            IPAddress destinationAddress = session.RemoteEndPoint.Address;
            logger.Info("Send Response message to " + destinationAddress.ToString() + " : " + message);
        }
    }
}
