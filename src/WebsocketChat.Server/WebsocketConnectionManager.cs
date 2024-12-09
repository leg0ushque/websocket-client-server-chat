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

        public WebSocket GetSocketByUserId(string userId) => _sockets.FirstOrDefault(p => p.Key == userId).Value;

        public ConcurrentDictionary<string, WebSocket> GetAllSockets() => _sockets;

        public string GetSocketUserId(WebSocket socket) => _sockets.FirstOrDefault(p => p.Value == socket).Key;

        public void AddSocket(string guid, WebSocket socket)
        {
            _sockets.TryAdd(guid, socket);
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
