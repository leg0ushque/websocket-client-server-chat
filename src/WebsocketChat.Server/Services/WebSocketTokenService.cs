using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WebsocketChat.Library.Entities;
using WebsocketChat.Server.Contexts;

namespace WebsocketChat.Server.Services
{
    public class WebSocketTokenService : IWebSocketTokenService
    {
        private readonly AppIdentityDbContext _context;

        public WebSocketTokenService(AppIdentityDbContext context)
        {
            _context = context;
        }

        public Task<string> CreateAsync(string userId)
        {
            if (userId is null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return CreateAsynchronous(new WebSocketToken
            {
                ExpirationDate = DateTime.Now.AddDays(Constants.TokenDaysExpirationTerm),
                UserId = userId,
            });
        }

        public async Task RemoveAsync(string token, string userId)
        {
            var foundToken = await _context
                .Set<WebSocketToken>().AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == token && t.UserId == userId);

            if (foundToken != null)
            {
                _context.Set<WebSocketToken>().Remove(foundToken);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ValidateAsync(string token, string userId)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);
            ArgumentException.ThrowIfNullOrEmpty(userId);

            var foundToken = await _context
                .Set<WebSocketToken>().AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == token && t.UserId == userId);

            return foundToken != null;
        }

        private async Task<string> CreateAsynchronous(WebSocketToken item)
        {
            _context.Set<WebSocketToken>().Add(item);
            await _context.SaveChangesAsync();

            return item.Id;
        }
    }
}
