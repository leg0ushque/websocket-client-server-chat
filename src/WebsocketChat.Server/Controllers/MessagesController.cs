using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using WebsocketChat.Library.Models;
using WebsocketChat.Server.Identity;
using WebsocketChat.Server.Services;
using Microsoft.AspNetCore.Authorization;
using WebsocketChat.Server.Helpers;

namespace WebsocketChat.Server.Controllers
{
    [Route("[controller]")]
    public class MessagesController(
        UserManager<User> userManager,
        IMessageStorageService messageStorageService) : ControllerBase
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly IMessageStorageService _messageStorageService = messageStorageService;

        /// <summary>
        /// Retrieves a set of own messages
        /// </summary>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized</response>
        [Authorize()]
        [HttpGet("own")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetOwnMessages(
            [FromQuery] int? pageNumber = Library.Constants.MinPageNumber,
            [FromQuery] int? pageSize = Library.Constants.MinPageSize)
        {
            var userId = HttpContext.User.FindFirst(Identity.IdentityConstants.UserIdClaimType).Value;

            var user = await _userManager.FindByIdAsync(userId);

            var messages = await _messageStorageService.GetAllByUserIdAsync(user.Id,
                pageNumber, pageSize);

            return Ok(messages);
        }

        /// <summary>
        /// Retrieves a set of own messages
        /// </summary>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized</response>
        [Authorize(Roles = Identity.IdentityConstants.AdminRole)]
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllMessages(
            [FromQuery] int? pageNumber = Library.Constants.MinPageNumber,
            [FromQuery] int? pageSize = Library.Constants.MinPageSize)
        {
            var messages = await _messageStorageService.GetAllAsync(pageNumber, pageSize);

            return Ok(messages);
        }
    }
}
