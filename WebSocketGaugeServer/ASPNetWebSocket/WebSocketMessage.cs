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
    public class WebSocketMessage
    {
        private readonly WebSocketMessageType messageType;
        private readonly byte[] binaryContent;
        private readonly string textContent;
        public WebSocketMessageType MessageType
        {
            get
            {
                return messageType;
            }
        }

        public byte[] BinaryContent
        {
            get
            {
                if(messageType != WebSocketMessageType.Binary)
                    throw new InvalidOperationException("Binary content is requested, however the mesasge type is not binary.");
                else
                    return binaryContent;
            }
        }

        public string textContent
        {
            get
            {
                if(messageType != WebSocketMessageType.Text)
                    throw new InvalidOperationException("Texi content is requested, however the mesasge type is not text.");
                else
                    return textContent;
            }
        }
        private WebSocketMessage(WebSocketMessageType messageType, byte[] binaryContent, string textContent)
        {
            this.messageType = messageType;
            this.binaryContent = binaryContent;
            this.textContent = textContent;
        }

        public static CreateTextMessage(string textContent)
        {
            return new WebSocketMessage(WebSocketMessageType.Text, new byte[0], textContent);
        }

        public static CreateBinaryMessage(byte[] binaryContent)
        {
            return new WebSocketMessage(WebSocketMessageType.Binary, binaryContent, "");
        }

        public static CreateCloseMessage()
        {
            return new WebSocketMessage(WebSocketMessageType.Close, new byte[0], "");
        }



    }
}