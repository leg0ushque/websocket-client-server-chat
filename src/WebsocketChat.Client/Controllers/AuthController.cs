using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebsocketChat.Client.Helpers;
using WebsocketChat.Client.HttpClients;
using WebsocketChat.Library.Models;

namespace WebsocketChat.Client.Controllers
{
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly IApiHttpClient _api;

        public AuthController(IApiHttpClient api)
        {
            _api = api;
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            var model = new RegisterModel();
            return View(model);
        }

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] RegisterModel model)
        {
            using var content = JsonHelper.ObjectToStringContent(model);
            var response = await _api.PostAuthRegister(content);
            if (response.IsSuccessStatusCode)
            {
                var stringResponse = await response.Content.ReadAsStringAsync();
                var responseModel = JsonConvert.DeserializeObject<AuthResultModel>(stringResponse);

                SetTokenCookies(responseModel.JwtToken, responseModel.WebSocketToken);

                return RedirectToAction("Index", "Home");
            }

            var responseContent = await JsonHelper.DeserializeContentAsync<List<CodeDescriptionErrorModel>>(response);
            foreach(var error in responseContent)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginModel user)
        {
            using var content = JsonHelper.ObjectToStringContent(user);
            var response = await _api.PostAuthLogin(content);
            if (response.IsSuccessStatusCode)
            {
                var stringResponse = await response.Content.ReadAsStringAsync();
                var responseModel = JsonConvert.DeserializeObject<AuthResultModel>(stringResponse);

                SetTokenCookies(responseModel.JwtToken, responseModel.WebSocketToken);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", Constants.IncorrectLoginPasswordMessage);
            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff()
        {
            await _api.GetAuthLogout();

            Response.Cookies.Delete(RequestHelper.JwtCookiesKey);
            Response.Cookies.Delete(Library.Constants.WebSocketSessionTokenKey);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet("changePassword")]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost("changePassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid || model == null)
            {
                return View(model);
            }

            using var content = JsonHelper.ObjectToStringContent(model);
            var response = await _api.PostAuthChangePassword(content);
            if (response.IsSuccessStatusCode)
            {
                var stringResponse = await response.Content.ReadAsStringAsync();
                var responseModel = JsonConvert.DeserializeObject<AuthResultModel>(stringResponse);

                Response.Cookies.Delete(RequestHelper.JwtCookiesKey);

                SetTokenCookies(responseModel.JwtToken, responseModel.WebSocketToken);

                return RedirectToAction("", "Profile");
            }

            var responseContent = await JsonHelper.DeserializeContentAsync<ErrorModel>(response);
            ModelState.AddModelError(responseContent.ErrorAttribute, responseContent.ErrorMessage);
            return View(model);
        }

        private void SetTokenCookies(string jwtToken, string webSocketToken)
        {
            HttpContext.Response.Cookies.Append(RequestHelper.JwtCookiesKey, jwtToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                });

            HttpContext.Response.Cookies.Append(Library.Constants.WebSocketSessionTokenKey, webSocketToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                });
        }
    }
}
