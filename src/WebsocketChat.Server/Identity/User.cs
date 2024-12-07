using Microsoft.AspNetCore.Identity;

namespace WebsocketChat.Server.Identity
{
    public class User : IdentityUser
    {
        public string Nickname { get; set; }

        public override string NormalizedEmail => Email.ToUpperInvariant();

        public override string NormalizedUserName => UserName.ToUpperInvariant();
    }
}
