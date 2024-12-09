using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using System;
using WebsocketChat.Library.Entities;
using WebsocketChat.Server.Contexts;
using System.Collections.Generic;
using WebsocketChat.Server.Helpers;
using System.Linq;

namespace WebsocketChat.Server.Services
{
    public class MessageStorageService : BaseDbService<WebSocketMessage>, IMessageStorageService
    {
        public MessageStorageService(AppIdentityDbContext context) => _context = context;

        public Task<List<WebSocketMessage>> GetAllAsync(
            int? pageNumber = null,
            int? pageSize = null,
            CancellationToken cancellationToken = default)
        {
            var allItems = GetAll();

            return PaginationHelper<WebSocketMessage>.GetPageItems(
                    allItems, pageNumber, pageSize,
                    cancellationToken);
        }

        public Task<List<WebSocketMessage>> GetAllByUserIdAsync(string userId = null,
            int? pageNumber = null,
            int? pageSize = null,
            CancellationToken cancellationToken = default)
        {
            var allItems = GetAll();

            return PaginationHelper<WebSocketMessage>.GetPageItems(
                        string.IsNullOrEmpty(userId) ?
                            allItems
                            : allItems.Where(x => x.UserId == userId),
                        pageNumber, pageSize,
                        cancellationToken);
        }
    }
}
