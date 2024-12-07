using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;
using WebsocketChat.Server.Handlers;

namespace WebsocketChat.Server.Middlewares
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;

        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,
            WebSocketConnectionManager manager,
            IMessageHandler messageHandler)
        {
            if (context.Request.Path != "/ws")
            {
                await _next(context);
                return;
            }

            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400; // Bad Request
                await context.Response.WriteAsync("Not a WebSocket request");
                return;
            }

            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await messageHandler.HandleMessageAsync(webSocket, manager);
        }
    }
}
