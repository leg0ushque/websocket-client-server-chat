using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using WebsocketChat.Server.Identity;

namespace WebsocketChat.Server.Services
{
    public class UserService(JwtTokenService jwtTokenService, UserManager<User> identityUserManager)
    {
        private readonly JwtTokenService _jwtTokenService = jwtTokenService;
        private readonly UserManager<User> _identityUserManager = identityUserManager;

        public Task<User> GetUserInfoFromTokenAsync(string token)
        {
            var userId = _jwtTokenService.GetUserIdFromValidatedToken(token);

            if (userId == null)
            {
                return Task.FromResult<User>(null);
            }

            return _identityUserManager.FindByIdAsync(userId);
        }
    }
}
