using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketChat.Library.Entities;
using WebsocketChat.Server.Identity;
using WebsocketChat.Server.Services;

namespace WebsocketChat.Server.Controllers
{
    [Route("[controller]")]
    public class MessagesController(
        UserManager<User> userManager,
        IMessageStorageService messageStorageService) : ControllerBase
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly IMessageStorageService _messageStorageService = messageStorageService;

        private bool IsAdmin => User.IsInRole(Identity.IdentityConstants.AdminRole);

        [Authorize()]
        [HttpGet("get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMessages(
            [FromQuery] int? pageNumber = Library.Constants.MinPageNumber,
            [FromQuery] int? pageSize = Library.Constants.MinPageSize)
        {
            var userId = HttpContext.User.FindFirst(Identity.IdentityConstants.UserIdClaimType).Value;
            var user = await _userManager.FindByIdAsync(userId);

            var messagesUserId = IsAdmin ? null : user.Id; // admin sees *all users* messages

            var messages =  await _messageStorageService.GetAllByUserIdAsync(messagesUserId,
                pageNumber, pageSize);

            if (IsAdmin)
            {
                await LoadNicknamesAsync(messages);
            }

            return Ok(messages);
        }

        [Authorize()]
        [HttpGet("get/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUserMessages(string userId,
            [FromQuery] int? pageNumber = Library.Constants.MinPageNumber,
            [FromQuery] int? pageSize = Library.Constants.MinPageSize)
        {
            if(!IsAdmin)
            {
                userId = HttpContext.User.FindFirst(Identity.IdentityConstants.UserIdClaimType).Value;
            }

            var messages = await _messageStorageService.GetAllByUserIdAsync(userId,
                pageNumber, pageSize);

            if (IsAdmin)
            {
                await LoadNicknamesAsync(messages);
            }

            return Ok(messages);
        }

        [Authorize()]
        [HttpGet("getPages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMessagesPagesCount([FromQuery] string userId,
            [FromQuery] int? pageSize = Library.Constants.MinPageSize)
        {
            if (!IsAdmin)
            {
                userId = HttpContext.User.FindFirst(Identity.IdentityConstants.UserIdClaimType).Value;
            }

            var pagesCount = await _messageStorageService.GetPagesCount(userId,
                pageSize);

            return Ok(pagesCount);
        }

        [Authorize(Roles = Identity.IdentityConstants.AdminRole)]
        [HttpGet("getUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUsersList([FromQuery] string userId,
            [FromQuery] int? pageSize = Library.Constants.MinPageSize)
        {
            var allUsers = _userManager.Users
                .OrderBy(x => x.Nickname)
                .ToDictionary(x => x.Id, x => x.Nickname);

            return Ok(allUsers);
        }

        private async Task<Dictionary<string,string>> GetNicknamesAsync(IEnumerable<string> userIds)
        {
            var tasks = userIds.Select(id => _userManager.FindByIdAsync(id)).ToList();
            var userResults = await Task.WhenAll(tasks);

            return userResults.ToDictionary(x => x.Id, x => x.Nickname);
        }

        private async Task LoadNicknamesAsync(List<WebSocketMessage> messages)
        {
            var usersIds = messages.Select(x => x.UserId).Distinct().ToList();
            var userNicknames = await GetNicknamesAsync(usersIds);

            foreach (var message in messages)
            {
                if (userNicknames.ContainsKey(message.UserId))
                {
                    message.UserNickname = userNicknames[message.UserId];
                }
            }
        }
    }
}
