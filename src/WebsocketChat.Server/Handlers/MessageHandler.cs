using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebsocketChat.Library.Entities;
using WebsocketChat.Library.Models;
using WebsocketChat.Server.Identity;
using WebsocketChat.Server.Services;

namespace WebsocketChat.Server.Handlers
{
    public class MessageHandler : IMessageHandler
    {
        private readonly UserManager<User> _userManager;
        private readonly IWebSocketTokenValidationService _webSocketTokenValidationService;
        private readonly IMessageStorageService _messageStorageService;
        private readonly ILogger<MessageHandler> _logger;

        public MessageHandler(
            UserManager<User> userManager,
            IWebSocketTokenValidationService webSocketTokenValidationService,
            IMessageStorageService messageStorageService,
            ILogger<MessageHandler> logger)
        {
            _userManager = userManager;
            _webSocketTokenValidationService = webSocketTokenValidationService;
            _messageStorageService = messageStorageService;
            _logger = logger;
        }

        public async Task HandleMessageAsync(WebSocket webSocket, WebSocketConnectionManager manager, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, cancellationToken);
                        break;
                    }
                    await ProcessReceivedMessage(webSocket, buffer, result, manager, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(message: "Message handler encountered an error:", exception: ex);
                await TryCloseWebSocket(webSocket, WebSocketCloseStatus.InternalServerError, "Internal error occurred");
            }
            finally
            {
                await FinalizeWebSocketConnection(webSocket, manager, cancellationToken);
            }
        }

        private async Task ProcessReceivedMessage(WebSocket webSocket, byte[] buffer, WebSocketReceiveResult result, WebSocketConnectionManager manager, CancellationToken cancellationToken)
        {
            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var deserializedMessage = JsonConvert.DeserializeObject<WebSocketMessage>(receivedMessage);

            if (!await _webSocketTokenValidationService.ValidateAsync(
                    deserializedMessage.Token,
                    deserializedMessage.UserId))
            {
                _logger.LogWarning("Invalid token received from UserID {0}", deserializedMessage.UserId);
                return;
            }

            if (deserializedMessage.IsSystemMessage)
            {
                var existingSocket = manager.GetSocketByUserId(deserializedMessage.UserId);
                if (existingSocket == null)
                {
                    // Adding WebSocket to manager, associated with a user ID
                    manager.AddSocket(deserializedMessage.UserId, webSocket);
                    _logger.LogInformation("New system connection established for UserId: {UserId}", deserializedMessage.UserId);
                }
                return;
            }

            deserializedMessage.Date = DateTime.Now;
            var storedId = await _messageStorageService.CreateAsync(deserializedMessage, cancellationToken);

            _logger.LogWarning("Message was created in DB, created id='{Id}'", storedId);


            await BroadcastMessageToClients(deserializedMessage, manager, cancellationToken);
        }

        private async Task BroadcastMessageToClients(WebSocketMessage message, WebSocketConnectionManager manager, CancellationToken cancellationToken)
        {
            var sender = await _userManager.FindByIdAsync(message.UserId);
            var messageToSend = new SentMessageModel
            {
                Message = message.MessageText,
                Date = DateTime.UtcNow,
                SenderNickname = sender.Nickname,
            };
            var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageToSend));

            foreach (var connectedWebSocket in manager.GetAllSockets())
            {
                if (connectedWebSocket.Value.State == WebSocketState.Open
                    && connectedWebSocket.Key != sender.Id)
                {
                    await connectedWebSocket.Value.SendAsync(
                        new ArraySegment<byte>(messageBytes, 0, messageBytes.Length),
                        WebSocketMessageType.Text,
                        true,
                        cancellationToken);
                }
            }
        }

        private static async Task TryCloseWebSocket(WebSocket webSocket, WebSocketCloseStatus closeStatus, string statusDescription)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
            }
        }

        private async Task FinalizeWebSocketConnection(WebSocket webSocket, WebSocketConnectionManager manager, CancellationToken cancellationToken)
        {
            var userId = manager.GetSocketUserId(webSocket);
            if (userId != null)
            {
                await manager.RemoveSocketAsync(userId, cancellationToken);
                _logger.LogInformation("WebSocket connection closed for user {0}", userId);
            }
        }
    }
}