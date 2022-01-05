using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SZ2.WebSocketGaugeServer.WebSocketCommon.Middleware
{
    public interface IWebSocketHandleMiddleware
    {
        Task HandleHttpConnectionAsync(HttpContext context, WebSocket webSocket, CancellationToken ct);
    }
}
