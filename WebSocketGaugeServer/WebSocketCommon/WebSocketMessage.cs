using System;
using System.Net.WebSockets;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon
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
                if (messageType != WebSocketMessageType.Binary)
                    throw new InvalidOperationException("Binary content is requested, however the mesasge type is not binary.");
                else
                    return binaryContent;
            }
        }

        public string TextContent
        {
            get
            {
                if (messageType != WebSocketMessageType.Text)
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

        public static WebSocketMessage CreateTextMessage(string textContent)
        {
            return new WebSocketMessage(WebSocketMessageType.Text, new byte[0], textContent);
        }

        public static WebSocketMessage CreateBinaryMessage(byte[] binaryContent)
        {
            return new WebSocketMessage(WebSocketMessageType.Binary, binaryContent, "");
        }

        public static WebSocketMessage CreateCloseMessage()
        {
            return new WebSocketMessage(WebSocketMessageType.Close, new byte[0], "");
        }
    }
}