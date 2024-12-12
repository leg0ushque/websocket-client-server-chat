using Microsoft.AspNetCore.Http;
using System;
using System.Net.WebSockets;
using System.Threading;
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
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Запрос не является WebSocket запросом");
                return;
            }

            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

            var tcs = new TaskCompletionSource<bool>();

            ThreadPool.QueueUserWorkItem(async _ =>
            {
                try
                {
                    await messageHandler.HandleMessageAsync(webSocket, manager);
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            try
            {
                await tcs.Task;  // ждем завершения фоновой задачи!
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка во время обработки сообщения в пуле потоков: " + ex.Message);
            }
        }
    }
}
