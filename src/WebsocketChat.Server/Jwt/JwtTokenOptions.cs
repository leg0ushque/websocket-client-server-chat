namespace WebsocketChat.Server.Services
{
    public class JwtTokenOptions
    {
        public const string JwtConfigSectionKey = "JwtTokenSettings";

        public const string JwtIssuerConfigKey = "JwtIssuer";
        public const string JwtAudienceConfigKey = "JwtAudience";
        public const string JwtSecretKeyConfigKey = "JwtSecretKey";

        public string JwtIssuer { get; set; }

        public string JwtAudience { get; set; }

        public string JwtSecretKey { get; set; }
    }
}
