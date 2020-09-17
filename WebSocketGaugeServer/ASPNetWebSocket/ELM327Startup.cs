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
using DefiSSMCOM.OBDII;
using System.Text;
using System.IO;
using System.Net;
using log4net;

namespace ASPNetWebSocket
{
    public class ELM327Startup
    {
        static ILog logger = LogManager.GetLogger(typeof(Program));
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ELM327COMService>(_ => new ELM327COMService("/dev/ttyUSB1"));
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
                    await HandleHttpConnection(context, webSocket);
                }
                else
                {
                    await next();
                }
            });
        }

        private async Task HandleHttpConnection(HttpContext context, WebSocket webSocket)
        {
            var service = (ELM327COMService)context.RequestServices.GetRequiredService(typeof(ELM327COMService));
            var connectionID = Guid.NewGuid();
            var destAddress = context.Connection.RemoteIpAddress;

            service.AddWebSocket(connectionID, webSocket);
            var sessionParam = service.GetSessionParam(connectionID);
            logger.Info("Session is connected from : " + destAddress.ToString());

            while (webSocket.State == WebSocketState.Open)
            {
                await processReceivedMessage(webSocket, service, sessionParam, destAddress);
            }
            service.RemoveWebSocket(connectionID);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed normally", CancellationToken.None);
            logger.Info("Session is disconnected from : " + destAddress.ToString());
        }

        private async Task processReceivedMessage(WebSocket ws, ELM327COMService service, ELM327WebsocketSessionParam sessionParam, IPAddress destAddress)
        {
            // Get mode code
            try
            {
                var wsmessage = await ReceiveWebSocketMessageAsync(ws);
                
                // Do nothing on closing message.
                if(wsmessage.MessageType == WebSocketMessageType.Close)
                    return;
                // Throw exception on non text message.
                if(wsmessage.MessageType != WebSocketMessageType.Text)
                    throw new InvalidDataException("Received websocket message type is not Text.");

                string message = wsmessage.TextContent;
                var msg_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                string receivedJSONmode = msg_dict["mode"];   
                
                switch (receivedJSONmode)
                {
                    //SSM COM all reset
                    case (ResetJSONFormat.ModeCode):
                        sessionParam.reset();
                        await send_response_msg(ws, "ELM327COM session RESET. All send parameters are disabled.", destAddress);
                        break;
                    case (ELM327COMReadJSONFormat.ModeCode):
                        var msg_obj_ssmread = JsonConvert.DeserializeObject<ELM327COMReadJSONFormat>(message);
                        msg_obj_ssmread.Validate();

                        var target_code = (OBDIIParameterCode)Enum.Parse(typeof(OBDIIParameterCode), msg_obj_ssmread.code);
                        bool flag = msg_obj_ssmread.flag;

                        if (msg_obj_ssmread.read_mode == SSMCOMReadJSONFormat.FastReadModeCode)
                        {
                            sessionParam.FastSendlist[target_code] = flag;
                        }
                        else
                        {
                            sessionParam.SlowSendlist[target_code] = flag;
                        }
                        await send_response_msg(ws, "ELM327COM session read flag for : " + target_code.ToString() + " read_mode :" + msg_obj_ssmread.read_mode + " set to : " + flag.ToString(), destAddress);
                        break;

                    case (ELM327SLOWREADIntervalJSONFormat.ModeCode):
                        var msg_obj_interval = JsonConvert.DeserializeObject<ELM327SLOWREADIntervalJSONFormat>(message);
                        msg_obj_interval.Validate();
                        service.ELM327COM.SlowReadInterval = msg_obj_interval.interval;

                        await send_response_msg(ws, "ELM327COM slowread interval to : " + msg_obj_interval.interval.ToString(), destAddress);
                        break;
                    default:
                        throw new JSONFormatsException("Unsuppoted mode property.");
                }
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is JsonException || ex is JSONFormatsException || ex is NotSupportedException)
            {
                await send_error_msg(ws, ex.GetType().ToString() + " " + ex.Message, destAddress);
            }
            catch(InvalidDataException ex)
            {
                logger.Warn(ex.Message);                
            }
            catch(OperationCanceledException ex)
            {
                logger.Info(ex.Message);
            }
        }

        protected async Task send_error_msg(WebSocket ws, string message, IPAddress destAddress)
        {
            ErrorJSONFormat json_error_msg = new ErrorJSONFormat();
            json_error_msg.msg = message;

            logger.Error("Send Error message to " + destAddress.ToString() + " : " + message);            
            await SendWebSocketTextAsync(ws, json_error_msg.Serialize());           
        }

        protected async Task send_response_msg(WebSocket ws, string message, IPAddress destAddress)
        {
            ResponseJSONFormat json_response_msg = new ResponseJSONFormat();
            json_response_msg.msg = message;
            
            logger.Info("Send Response message to " + destAddress.ToString() + " : " + message);
            await SendWebSocketTextAsync(ws, json_response_msg.Serialize());
        }

        private async Task SendWebSocketTextAsync(WebSocket webSocket, string text)
        {
            byte[] sendBuf = Encoding.UTF8.GetBytes(text);
            await webSocket.SendAsync(new ArraySegment<byte>(sendBuf, 0, sendBuf.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task<WebSocketMessage> ReceiveWebSocketMessageAsync(WebSocket webSocket)
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
                switch(result.MessageType)
                {
                    case WebSocketMessageType.Text:
                        string returnStr;
                        using (var reader = new StreamReader(ms, Encoding.UTF8))
                        {
                            returnStr = reader.ReadToEnd();
                        }
                        return WebSocketMessage.CreateTextMessage(returnStr);
                    case WebSocketMessageType.Binary:
                        throw new NotSupportedException("Binary mode websocketmessage is curently not supprted.");
                    case WebSocketMessageType.Close:
                        return WebSocketMessage.CreateCloseMessage();
                    default:
                        throw new InvalidProgramException();
                }
            }
        }
    }
}
