using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsocketChat.Client.Helpers;
using WebsocketChat.Client.HttpClients;
using WebsocketChat.Client.Models;
using WebsocketChat.Library.Entities;

namespace WebsocketChat.Client.Controllers
{
    [Route("[controller]")]
    public class ChatController(IApiHttpClient api) : Controller
    {
        private readonly IApiHttpClient _api = api;

        [Authorize]
        [HttpGet("")]
        public IActionResult Index()
        {
            var userIdClaimType = "UserId";
            var isSignedIn = User?.Identity?.IsAuthenticated ?? false;
            var userId = !isSignedIn ? null : User?.Claims.FirstOrDefault(claim => claim.Type == userIdClaimType)?.Value;

            HttpContext.Request.Cookies.TryGetValue(Library.Constants.WebSocketSessionTokenKey, out var websocketToken);

            var websocketChatModel = new WebsocketChatModel
            {
                UserId = userId,
                WebsocketToken = websocketToken,
            };

            return View(websocketChatModel);
        }

        [Authorize]
        [HttpGet("messages")]
        public async Task<IActionResult> Messages([FromQuery] string userId,
            [FromQuery] int? pageNumber = Library.Constants.MinPageNumber,
            [FromQuery] int? pageSize = Library.Constants.MinPageSize)
        {
            var messagesResponse = await _api.GetChatMessages(userId, pageNumber, pageSize);

            var messages = await JsonHelper.DeserializeContentAsync<List<WebSocketMessage>>(messagesResponse);

            return View(messages);
        }
    }
}
