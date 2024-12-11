using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebsocketChat.Library.Entities;

namespace WebsocketChat.Server.Services
{
    public interface IMessageStorageService
    {
        Task<int> GetPagesCount(string userId = null, int? pageSize = null, CancellationToken cancellationToken = default);
        Task<string> CreateAsync(WebSocketMessage message, CancellationToken cancellationToken = default);
        Task<List<WebSocketMessage>> GetAllAsync(int? pageNumber = null, int? pageSize = null, CancellationToken cancellationToken = default);
        Task<List<WebSocketMessage>> GetAllByUserIdAsync(string userId = null, int? pageNumber = null, int? pageSize = null, CancellationToken cancellationToken = default);
    }
}