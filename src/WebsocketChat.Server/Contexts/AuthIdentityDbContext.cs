using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebsocketChat.Server.Identity;

namespace WebsocketChat.Server.Contexts
{
    public class AuthIdentityDbContext : IdentityDbContext<User>
    {
        public AuthIdentityDbContext(DbContextOptions<AuthIdentityDbContext> options)
            : base(options)
        { }
    }
}
