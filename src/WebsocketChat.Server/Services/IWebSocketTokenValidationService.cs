using System.Threading.Tasks;

namespace WebsocketChat.Server.Services
{
    public interface IWebSocketTokenValidationService
    {
        Task<bool> ValidateAsync(string token, string userId);
    }
}