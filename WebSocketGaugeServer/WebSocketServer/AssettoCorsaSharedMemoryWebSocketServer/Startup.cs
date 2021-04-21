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
using SZ2.WebSocketGaugeServer.WebSocketServer.AssettoCorsaSharedMemoryWebSocketServer.Service;
using SZ2.WebSocketGaugeServer.WebSocketServer.AssettoCorsaSharedMemoryWebSocketServer.SessionItems;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.AssettoCorsaSHM;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.JSONFormat;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.JSONFormat.AssettoCorsaSHM;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.AssettoCorsaSharedMemoryWebSocketServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
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
                    await HandleHttpConnection(context, webSocket, logger, cancellationToken);
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
            app.UseStaticFiles(new StaticFileOptions{
                ContentTypeProvider = provider
            });
        }

        private async Task HandleHttpConnection(HttpContext context, WebSocket webSocket, ILogger logger, CancellationToken ct)
        {
            var service = (AssettoCorsaSHMService)context.RequestServices.GetRequiredService(typeof(AssettoCorsaSHMService));
            var connectionID = Guid.NewGuid();
            var destAddress = context.Connection.RemoteIpAddress;

            service.AddWebSocket(connectionID, webSocket);
            var sessionParam = service.GetSessionParam(connectionID);
            logger.LogInformation("Session is connected from : " + destAddress.ToString());

            while (webSocket.State == WebSocketState.Open)
            {
                await processReceivedMessage(webSocket, sessionParam, destAddress, logger, ct);
            }
            service.RemoveWebSocket(connectionID);
            if (webSocket.State == WebSocketState.CloseReceived || webSocket.State == WebSocketState.CloseSent)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed normally", ct);
                logger.LogInformation("Session is disconnected from : " + destAddress.ToString());
            }
            else
            {
                logger.LogInformation("Session is aborted. " + destAddress.ToString());
            }
        }

        private async Task processReceivedMessage(WebSocket ws, AssettoCorsaWebsocketSessionParam sessionParam, IPAddress destAddress, ILogger logger, CancellationToken ct)
        {
            // Get mode code
            try
            {
                var wsmessage = await ReceiveWebSocketMessageAsync(ws, ct);
                if (wsmessage.MessageType == WebSocketMessageType.Text)
                {
                    string message = wsmessage.TextContent;
                    var msg_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                    string receivedJSONmode = msg_dict["mode"];

                    switch (receivedJSONmode)
                    {
                        case (ResetJSONFormat.ModeCode):
                            {
                                sessionParam.reset();
                                await send_response_msg(ws, "AssettoCorsaSHM Websocket all parameter reset.", destAddress, logger, ct);
                                break;
                            }
                        case (AssettoCorsaPhysicsSendJSONFormat.ModeCode):
                            {

                                var msg_obj_wssend = JsonConvert.DeserializeObject<AssettoCorsaPhysicsSendJSONFormat>(message);
                                msg_obj_wssend.Validate();
                                sessionParam.PhysicsDataSendList[(AssettoCorsaSHMPhysicsParameterCode)Enum.Parse(typeof(AssettoCorsaSHMPhysicsParameterCode), msg_obj_wssend.code)] = msg_obj_wssend.flag;

                                await send_response_msg(ws, "AsstoCorsa Websocket Physics send_flag for : " + msg_obj_wssend.code.ToString() + " set to : " + msg_obj_wssend.flag.ToString(), destAddress, logger, ct);
                                break;
                            }
                        case (AssettoCorsaGraphicsSendJSONFormat.ModeCode):
                            {
                                var msg_obj_wssend = JsonConvert.DeserializeObject<AssettoCorsaGraphicsSendJSONFormat>(message);
                                msg_obj_wssend.Validate();
                                sessionParam.GraphicsDataSendList[(AssettoCorsaSHMGraphicsParameterCode)Enum.Parse(typeof(AssettoCorsaSHMGraphicsParameterCode), msg_obj_wssend.code)] = msg_obj_wssend.flag;

                                await send_response_msg(ws, "AsstoCorsa Websocket Graphics send_flag for : " + msg_obj_wssend.code.ToString() + " set to : " + msg_obj_wssend.flag.ToString(), destAddress, logger, ct);
                                break;
                            }
                        case (AssettoCorsaStaticInfoSendJSONFormat.ModeCode):
                            {
                                var msg_obj_wssend = JsonConvert.DeserializeObject<AssettoCorsaStaticInfoSendJSONFormat>(message);
                                msg_obj_wssend.Validate();
                                sessionParam.StaticInfoDataSendList[(AssettoCorsaSHMStaticInfoParameterCode)Enum.Parse(typeof(AssettoCorsaSHMStaticInfoParameterCode), msg_obj_wssend.code)] = msg_obj_wssend.flag;

                                await send_response_msg(ws, "AsstoCorsa Websocket StaticInfo send_flag for : " + msg_obj_wssend.code.ToString() + " set to : " + msg_obj_wssend.flag.ToString(), destAddress, logger, ct);
                                break;
                            }
                        case (AssettoCorsaPhysicsWSIntervalJSONFormat.ModeCode):
                            {
                                var msg_obj_interval = JsonConvert.DeserializeObject<AssettoCorsaPhysicsWSIntervalJSONFormat>(message);
                                msg_obj_interval.Validate();
                                sessionParam.PhysicsDataSendInterval = msg_obj_interval.interval;

                                await send_response_msg(ws, "AsstoCorsa Websocket Physics send_interval to : " + msg_obj_interval.interval.ToString(), destAddress, logger, ct);
                                break;
                            }
                        case (AssettoCorsaGraphicsWSIntervalJSONFormat.ModeCode):
                            {
                                var msg_obj_interval = JsonConvert.DeserializeObject<AssettoCorsaGraphicsWSIntervalJSONFormat>(message);
                                msg_obj_interval.Validate();
                                sessionParam.GraphicsDataSendInterval = msg_obj_interval.interval;

                                await send_response_msg(ws, "AsstoCorsa Websocket Graphics send_interval to : " + msg_obj_interval.interval.ToString(), destAddress, logger, ct);
                                break;
                            }
                        case (AssettoCorsaStaticInfoWSIntervalJSONFormat.ModeCode):
                            {
                                var msg_obj_interval = JsonConvert.DeserializeObject<AssettoCorsaStaticInfoWSIntervalJSONFormat>(message);
                                msg_obj_interval.Validate();
                                sessionParam.StaticInfoDataSendInterval = msg_obj_interval.interval;

                                await send_response_msg(ws, "AsstoCorsa Websocket StaticInfo send_interval to : " + msg_obj_interval.interval.ToString(), destAddress, logger, ct);
                                break;
                            }
                        default:
                            throw new JSONFormatsException("Unsuppoted mode property.");

                    }
                }
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is JsonException || ex is JSONFormatsException || ex is NotSupportedException)
            {
                await send_error_msg(ws, ex.GetType().ToString() + " " + ex.Message, destAddress, logger, ct);
                logger.LogWarning(ex.Message);
                logger.LogWarning(ex.StackTrace);
            }
            catch (WebSocketException ex)
            {
                logger.LogWarning(ex.Message);
                logger.LogWarning(ex.StackTrace);
            }
            catch (OperationCanceledException ex)
            {
                logger.LogInformation(ex.Message);
            }
        }

        protected async Task send_error_msg(WebSocket ws, string message, IPAddress destAddress, ILogger logger, CancellationToken ct)
        {
            ErrorJSONFormat json_error_msg = new ErrorJSONFormat();
            json_error_msg.msg = message;

            logger.LogError("Send Error message to " + destAddress.ToString() + " : " + message);
            await SendWebSocketTextAsync(ws, json_error_msg.Serialize(), ct);
        }

        protected async Task send_response_msg(WebSocket ws, string message, IPAddress destAddress, ILogger logger, CancellationToken ct)
        {
            ResponseJSONFormat json_response_msg = new ResponseJSONFormat();
            json_response_msg.msg = message;

            logger.LogInformation("Send Response message to " + destAddress.ToString() + " : " + message);
            await SendWebSocketTextAsync(ws, json_response_msg.Serialize(), ct);
        }

        private async Task SendWebSocketTextAsync(WebSocket webSocket, string text, CancellationToken ct)
        {
            byte[] sendBuf = Encoding.UTF8.GetBytes(text);
            await webSocket.SendAsync(new ArraySegment<byte>(sendBuf, 0, sendBuf.Length), WebSocketMessageType.Text, true, ct);
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
