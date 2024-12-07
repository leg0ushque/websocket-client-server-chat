using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebsocketChat.Server.Handlers
{
    public class MessageHandler : IMessageHandler
    {
        //private readonly JwtTokenService _jwtTokenService;
        //public MessageHandler(JwtTokenService jwtTokenService)
        //{
        //    _jwtTokenService = jwtTokenService;
        //}

        public async Task HandleMessageAsync(WebSocket webSocket, WebSocketConnectionManager manager,
            CancellationToken ct = default)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

            manager.AddSocket(webSocket);

            while (!result.CloseStatus.HasValue)
            {
                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                foreach (var connectedClient in manager.GetAllClients())
                {
                    if (connectedClient.Value.State == WebSocketState.Open)
                    {
                        await connectedClient.Value.SendAsync(
                            new ArraySegment<byte>(buffer, 0, result.Count),
                            result.MessageType,
                            result.EndOfMessage,
                            ct);
                    }
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
            }

            string id = manager.GetId(webSocket);

            if (id != null)
            {
                await manager.RemoveSocketAsync(id, ct);
            }
        }
    }
}
