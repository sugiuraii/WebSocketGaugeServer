using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.Middleware
{
    public interface IWebSocketHandleMiddleware
    {
        Task HandleHttpConnection(HttpContext context, WebSocket webSocket, ILogger logger, CancellationToken ct);
    }
}
