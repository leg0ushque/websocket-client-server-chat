using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Principal;
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
            HttpContext.Request.Cookies.TryGetValue(
                Library.Constants.WebSocketSessionTokenKey,
                out var websocketToken);

            var userId = GetCurrentUserId();

            var websocketChatModel = new WebsocketChatModel
            {
                UserId = userId,
                WebsocketToken = websocketToken,
            };

            return View(websocketChatModel);
        }

        [Authorize]
        [HttpGet("messages")]
        public async Task<IActionResult> Messages([FromQuery] string userId)
        {
            if (!Guid.TryParse(userId, out _))
            {
                userId = "";
            }

            var allUsersResponse = await _api.GetChatUsers();
            var allUsers = await JsonHelper.DeserializeContentAsync<Dictionary<string, string>>(allUsersResponse);

            return View((User.IsInRole("Admin"), userId, allUsers));
        }

        [Authorize]
        [HttpGet("messages/countPages/{userId?}")]
        public async Task<IActionResult> GetPagesCount(string userId = null,
                    [FromQuery] int? pageNumber = Library.Constants.MinPageNumber,
                    [FromQuery] int? pageSize = Library.Constants.MinPageSize)
        {
            if (!User.IsInRole("Admin"))
            {
                userId = GetCurrentUserId();
            }

            var messages = await _api.GetChatMessages(userId, pageNumber, pageSize);

            return Ok(
                await JsonHelper.DeserializeContentAsync<List<WebSocketMessage>>(
                    messages));
        }

        [Authorize]
        [HttpGet("getMessages")]
        public async Task<IActionResult> GetMessages([FromQuery] string userId = null,
            [FromQuery] int? pageNumber = Library.Constants.MinPageNumber,
            [FromQuery] int? pageSize = Library.Constants.MinPageSize)
        {
            if (!User.IsInRole("Admin"))
            {
                userId = GetCurrentUserId();
            }

            var messages = await _api.GetChatMessages(userId, pageNumber, pageSize);
            var messagesList = await JsonHelper.DeserializeContentAsync<List<WebSocketMessage>>(
                    messages);

            return Ok(messagesList);
        }

        [Authorize]
        [HttpGet("getMessagesPages")]
        public async Task<IActionResult> GetMessagesMessagesPgesCount([FromQuery] string userId = null,
            [FromQuery] int? pageSize = Library.Constants.MinPageSize)
        {
            if (!User.IsInRole("Admin"))
            {
                userId = GetCurrentUserId();
            }

            var countResult = await _api.GetChatMessagesPagesCount(userId, pageSize);
            var count = await JsonHelper.DeserializeContentAsync<int>(countResult);

            return Ok(new { count });
        }

        private string GetCurrentUserId()
        {
            var userIdClaimType = "UserId";
            var isSignedIn = User?.Identity?.IsAuthenticated ?? false;
            return isSignedIn ?
                User?.Claims.FirstOrDefault(claim => claim.Type == userIdClaimType)?.Value
                : null;
        }
    }
}
