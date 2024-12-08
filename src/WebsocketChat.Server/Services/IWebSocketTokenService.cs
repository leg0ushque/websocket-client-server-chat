using System.Threading.Tasks;

namespace WebsocketChat.Server.Services
{
    public interface IWebSocketTokenService : IWebSocketTokenValidationService
    {
        Task<string> CreateAsync(string userId);
        Task RemoveAsync(string token, string userId);
    }
}