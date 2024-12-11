using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using System;
using WebsocketChat.Library.Entities;
using WebsocketChat.Server.Contexts;
using System.Collections.Generic;
using WebsocketChat.Server.Helpers;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using WebsocketChat.Server.Identity;

namespace WebsocketChat.Server.Services
{
    public class MessageStorageService : BaseDbService<WebSocketMessage>, IMessageStorageService
    {
        public MessageStorageService(AppIdentityDbContext context, UserManager<User> userManager)
        {
            _context = context;
        }

        public Task<string> CreateAsync(WebSocketMessage message,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);

            return CreateEntityAsync(message, cancellationToken);
        }

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

        public Task<int> GetPagesCount(string userId = null,
            int? pageSize = null,
            CancellationToken cancellationToken = default)
        {
            var allItems = GetAll();

            return PaginationHelper<WebSocketMessage>.GetTotalPagesCountAsync(
                        string.IsNullOrEmpty(userId) ?
                            allItems
                            : allItems.Where(x => x.UserId == userId), pageSize,
                        cancellationToken);
        }
    }
}
