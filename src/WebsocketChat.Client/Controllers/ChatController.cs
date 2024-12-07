using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebsocketChat.Client.Controllers
{
    [Route("[controller]")]
    public class ChatController : Controller
    {
        public ChatController() { }

        [Authorize]
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
