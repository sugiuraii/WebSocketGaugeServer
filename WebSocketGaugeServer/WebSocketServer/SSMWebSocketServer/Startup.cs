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
using log4net;
using SZ2.WebSocketGaugeServer.WebSocketServer.SSMWebSocketServer.Service;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.JSONFormat;
using SZ2.WebSocketGaugeServer.WebSocketServer.SSMWebSocketServer.JSONFormat;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.SSM;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon;
using SZ2.WebSocketGaugeServer.WebSocketServer.SSMWebSocketServer.SessionItems;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.SSMWebSocketServer
{
    public class Startup
    {
        static ILog logger = LogManager.GetLogger(typeof(Program));
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<SSMCOMService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120)
            };
            app.UseWebSockets(webSocketOptions);

            app.UseRouting();

            app.Use(async (context, next) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var cancellationToken = lifetime.ApplicationStopping;
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await HandleHttpConnection(context, webSocket, cancellationToken);
                }
                else
                {
                    await next();
                }
            });
        }

        private async Task HandleHttpConnection(HttpContext context, WebSocket webSocket, CancellationToken ct)
        {
            var service = (SSMCOMService)context.RequestServices.GetRequiredService(typeof(SSMCOMService));
            var connectionID = Guid.NewGuid();
            var destAddress = context.Connection.RemoteIpAddress;

            service.AddWebSocket(connectionID, webSocket);
            var sessionParam = service.GetSessionParam(connectionID);
            logger.Info("Session is connected from : " + destAddress.ToString());

            while (webSocket.State == WebSocketState.Open)
            {
                await processReceivedMessage(webSocket, service, sessionParam, destAddress, ct);
            }
            service.RemoveWebSocket(connectionID);
            if (webSocket.State == WebSocketState.CloseReceived || webSocket.State == WebSocketState.CloseSent)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed normally", ct);
                logger.Info("Session is disconnected from : " + destAddress.ToString());
            }
            else
            {
                logger.Info("Session is aborted. " + destAddress.ToString());
            }
        }

        private async Task processReceivedMessage(WebSocket ws, SSMCOMService service, SSMCOMWebsocketSessionParam sessionParam, IPAddress destAddress, CancellationToken ct)
        {
            // Get mode code
            try
            {
                var wsmessage = await ReceiveWebSocketMessageAsync(ws, ct);

                // Do nothing on closing message.
                if (wsmessage.MessageType == WebSocketMessageType.Close)
                    return;
                // Throw exception on non text message.
                if (wsmessage.MessageType != WebSocketMessageType.Text)
                    throw new InvalidDataException("Received websocket message type is not Text.");

                string message = wsmessage.TextContent;
                var msg_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                string receivedJSONmode = msg_dict["mode"];

                switch (receivedJSONmode)
                {
                    //SSM COM all reset
                    case (ResetJSONFormat.ModeCode):
                        sessionParam.reset();
                        await send_response_msg(ws, "SSMCOM session RESET. All send parameters are disabled.", destAddress);
                        break;
                    case (SSMCOMReadJSONFormat.ModeCode):
                        SSMCOMReadJSONFormat msg_obj_ssmread = JsonConvert.DeserializeObject<SSMCOMReadJSONFormat>(message);
                        msg_obj_ssmread.Validate();

                        SSMParameterCode target_code = (SSMParameterCode)Enum.Parse(typeof(SSMParameterCode), msg_obj_ssmread.code);
                        bool flag = msg_obj_ssmread.flag;

                        if (msg_obj_ssmread.read_mode == SSMCOMReadJSONFormat.FastReadModeCode)
                        {
                            sessionParam.FastSendlist[target_code] = flag;
                        }
                        else
                        {
                            sessionParam.SlowSendlist[target_code] = flag;
                        }
                        await send_response_msg(ws, "SSMCOM session read flag for : " + target_code.ToString() + " read_mode :" + msg_obj_ssmread.read_mode + " set to : " + flag.ToString(), destAddress);
                        break;

                    case (SSMSLOWREADIntervalJSONFormat.ModeCode):
                        SSMSLOWREADIntervalJSONFormat msg_obj_interval = JsonConvert.DeserializeObject<SSMSLOWREADIntervalJSONFormat>(message);
                        msg_obj_interval.Validate();
                        service.SSMCOM.SlowReadInterval = msg_obj_interval.interval;

                        await send_response_msg(ws, "SSMCOM slowread interval to : " + msg_obj_interval.interval.ToString(), destAddress);
                        break;
                    default:
                        throw new JSONFormatsException("Unsuppoted mode property.");
                }
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is JsonException || ex is JSONFormatsException || ex is NotSupportedException)
            {
                await send_error_msg(ws, ex.GetType().ToString() + " " + ex.Message, destAddress);
                logger.Warn(ex.Message);
                logger.Warn(ex.StackTrace);
            }
            catch (WebSocketException ex)
            {
                logger.Warn(ex.Message);
                logger.Warn(ex.StackTrace);
            }
            catch (InvalidDataException ex)
            {
                logger.Warn(ex.Message);
                logger.Warn(ex.StackTrace);
            }
            catch (OperationCanceledException ex)
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

        private async Task<WebSocketMessage> ReceiveWebSocketMessageAsync(WebSocket webSocket, CancellationToken ct)
        {
            var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
            WebSocketReceiveResult result = null;

            using (var ms = new MemoryStream())
            {
                do
                {
                    result = await webSocket.ReceiveAsync(buffer, ct);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                switch (result.MessageType)
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
