using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebsocketChat.Server.Identity;
using WebsocketChat.Server.Models;
using WebsocketChat.Server.Services;

namespace WebsocketChat.Server.Controllers
{
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly JwtTokenService _jwtTokenService;

        public AuthController(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            JwtTokenService jwtTokenService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <response code="200">Registration was completed successfully.</response>
        /// <response code="400">Registration wasn't completed with errors thrown.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
            };

            return CreateUser(user, model.Password);
        }

        /// <summary>
        /// Executes log in into account for user.
        /// </summary>
        /// <response code="200">Login was successful.</response>
        /// <response code="403">Login was forbidden with errors thrown.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return LoginUser(model.Email, model.Password);
        }

        /// <summary>
        /// Changes current user's password.
        /// </summary>
        /// <response code="200">Password was changed successfully.</response>
        /// <response code="400">Password wasn't changed due to an error occured.</response>
        /// <response code="401">Password change is available only for authorized users.</response>
        [HttpPost("changePassword")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return ChangeCurrentUserPassword(model.OldPassword, model.NewPassword);
        }

        /// <summary>
        /// Executes log out for user.
        /// </summary>
        /// <response code="200">Logout was successful.</response>
        /// <response code="401">Logout is available only for authorized users.</response>
        /// <response code="404">Logout wasn't successful with errors thrown.</response>
        [Authorize]
        [HttpGet("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return Ok();
        }

        /// <summary>
        /// Validates a token.
        /// </summary>
        /// <response code="200">Token is valid.</response>
        /// <response code="403">Token is not valid.</response>
        [HttpPost("validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult Validate([FromBody] string token)
        {
            return _jwtTokenService.ValidateToken(token) ? Ok() : (IActionResult)Forbid();
        }

        /// <summary>
        /// Sends a response if API works well.
        /// </summary>
        /// <response code="200"> Pong.</response>
        [HttpGet("ping")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            return Ok();
        }

        /// <summary>
        /// Sends a response if the email is free.
        /// </summary>
        /// <response code="200"> Pong.</response>
        [HttpGet("checkIfEmailFree/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckIfEmailFree([FromRoute] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            return (user is null) ? Ok() : BadRequest();
        }

        private async Task<IActionResult> ChangeCurrentUserPassword(string oldPassword, string newPassword)
        {
            var userId = HttpContext.User.FindFirst(Identity.IdentityConstants.UserIdClaimType).Value;

            var user = await _userManager.FindByIdAsync(userId);

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(_jwtTokenService.GetToken(user, roles));
        }

        private async Task<IActionResult> LoginUser(string login, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(login, password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(login);
                var roles = await _userManager.GetRolesAsync(user);
                return Ok(_jwtTokenService.GetToken(user, roles));
            }

            return Forbid();
        }

        private async Task<IActionResult> CreateUser(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Identity.IdentityConstants.UserRole);

                // TODO FIX ME
                // Create other entities if needed
                // await _usersService.CreateCustomerAsync(new BusinessLogic.Dtos.CustomerDto { CustomerCategoryId = null, UserId = user.Id });

                var roles = await _userManager.GetRolesAsync(user);
                return Ok(_jwtTokenService.GetToken(user, roles));
            }

            return BadRequest(result.Errors);
        }
    }
}
