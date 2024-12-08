using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsocketChat.Client.Models;

namespace WebsocketChat.Client.Controllers
{
    [Route("[controller]")]
    public class ChatController(IHttpContextAccessor httpContextAccessor) : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

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
    }
}
