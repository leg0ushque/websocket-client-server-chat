using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebsocketChat.Server
{
    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

        public WebSocket GetSocketById(string id) => _sockets.FirstOrDefault(p => p.Key == id).Value;

        public ConcurrentDictionary<string, WebSocket> GetAllClients() => _sockets;

        public string GetId(WebSocket socket) => _sockets.FirstOrDefault(p => p.Value == socket).Key;

        public void AddSocket(WebSocket socket)
        {
            string connId = Guid.NewGuid().ToString();
            _sockets.TryAdd(connId, socket);
        }

        public async Task RemoveSocketAsync(string id, CancellationToken ct = default)
        {
            if(!_sockets.ContainsKey(id))
            {
                return;
            }

            _sockets.TryRemove(id, out WebSocket? socket);

            await socket!.CloseAsync(WebSocketCloseStatus.NormalClosure,
                "Closed by the WebSocketConnectionManager", ct);
        }
    }
}
