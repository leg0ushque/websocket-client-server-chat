using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebsocketChat.Server.Handlers
{
    public interface IMessageHandler
    {
        public Task HandleMessageAsync(WebSocket webSocket, WebSocketConnectionManager manager,
            CancellationToken ct = default);
    }
}
