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
using Newtonsoft.Json;
using DefiSSMCOM.Defi;
using System.Text;
using System.IO;

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

        private async Task processReceivedJSONMessage(string receivedJSONmode, string message, DefiCOMWebsocketSessionParam sessionParam, WebSocket ws)
        {
            switch (receivedJSONmode)
            {
                case (ResetJSONFormat.ModeCode):
                    sessionParam.reset();
                    await send_response_msg(ws, "Defi Websocket all parameter reset.");
                    break;
                case (DefiWSSendJSONFormat.ModeCode):
                    DefiWSSendJSONFormat msg_obj_wssend = JsonConvert.DeserializeObject<DefiWSSendJSONFormat>(message);
                    msg_obj_wssend.Validate();
                    sessionParam.Sendlist[(DefiParameterCode)Enum.Parse(typeof(DefiParameterCode), msg_obj_wssend.code)] = msg_obj_wssend.flag;

                    await send_response_msg(ws, "Defi Websocket send_flag for : " + msg_obj_wssend.code.ToString() + " set to : " + msg_obj_wssend.flag.ToString());
                    break;

                case (DefiWSIntervalJSONFormat.ModeCode):
                    DefiWSIntervalJSONFormat msg_obj_interval = JsonConvert.DeserializeObject<DefiWSIntervalJSONFormat>(message);
                    msg_obj_interval.Validate();
                    sessionParam.SendInterval = msg_obj_interval.interval;

                    await send_response_msg(ws, "Defi Websocket send_interval to : " + msg_obj_interval.interval.ToString());
                    break;
                default:
                    throw new JSONFormatsException("Unsuppoted mode property.");
            }
        }

        protected async Task send_error_msg(WebSocket ws, string message)
        {
            ErrorJSONFormat json_error_msg = new ErrorJSONFormat();
            json_error_msg.msg = message;
            
            await SendWebSocketTextAsync(ws, json_error_msg.Serialize());
            /*
            IPAddress destinationAddress = session.RemoteEndPoint.Address;
            logger.Error("Send Error message to " + destinationAddress.ToString() + " : " + message);
            */
        }

        protected async Task send_response_msg(WebSocket ws, string message)
        {
            ResponseJSONFormat json_response_msg = new ResponseJSONFormat();
            json_response_msg.msg = message;
            
            /*
            IPAddress destinationAddress = session.RemoteEndPoint.Address;
            logger.Info("Send Response message to " + destinationAddress.ToString() + " : " + message);
            */
            await SendWebSocketTextAsync(ws, json_response_msg.Serialize());
        }

        private async Task SendWebSocketTextAsync(WebSocket webSocket, string text)
        {
            byte[] sendBuf = Encoding.UTF8.GetBytes(text);
            await webSocket.SendAsync(new ArraySegment<byte>(sendBuf, 0, sendBuf.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task<string> ReceiveWebSocketTextAsync(WebSocket webSocket)
        {
            var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
            WebSocketReceiveResult result= null;
            
            using (var ms = new MemoryStream())
            {
                do
                {
                    result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while(!result.EndOfMessage);
                
                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
                else
                    throw new InvalidOperationException("WebSocket received message is not text.");

            }
        }
    }
}
