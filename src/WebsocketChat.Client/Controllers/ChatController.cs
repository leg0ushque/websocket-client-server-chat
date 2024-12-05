using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebsocketChat.Client.Models;

namespace WebsocketChat.Client.Controllers
{
    [Route("[controller]")]
    public class ChatController : Controller
    {
        public ChatController() { }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
