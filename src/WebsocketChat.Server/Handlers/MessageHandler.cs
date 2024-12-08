using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebsocketChat.Library.Entities;
using WebsocketChat.Server.Services;

namespace WebsocketChat.Server.Handlers
{
    public class MessageHandler : IMessageHandler
    {
        private readonly JwtTokenService _jwtTokenService;
        private readonly UserService _userService;
        private readonly IWebSocketTokenValidationService _webSocketTokenValidationService;

        public MessageHandler(
            JwtTokenService jwtTokenService,
            UserService userService,
            IWebSocketTokenValidationService webSocketTokenValidationService)
        {
            _jwtTokenService = jwtTokenService;
            _userService = userService;
            _webSocketTokenValidationService = webSocketTokenValidationService;
        }

        public async Task HandleMessageAsync(WebSocket webSocket, WebSocketConnectionManager manager,
            CancellationToken ct = default)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result;
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, ct);
                        break;
                    }

                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var deserializedMessage = JsonConvert.DeserializeObject<WebSocketMessage>(receivedMessage);

                    var wsTokenIsValid = await _webSocketTokenValidationService.ValidateAsync(
                        deserializedMessage.Token,
                        deserializedMessage.UserId);

                    if(!wsTokenIsValid)
                    {
                        return;
                    }

                    if (deserializedMessage.IsSystemMessage)
                    {

                    }

                    foreach (var connectedClient in manager.GetAllClients())
                    {
                        if (connectedClient.Value.State == WebSocketState.Open)
                        {
                            await connectedClient.Value.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count),
                                result.MessageType, result.EndOfMessage, ct);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket handling error: {ex.Message}");
            }
            finally
            {
                // Cleanup code
                var id = manager.GetId(webSocket);
                if (id != null)
                {
                    await manager.RemoveSocketAsync(id, ct);
                }
            }
        }
    }
}
