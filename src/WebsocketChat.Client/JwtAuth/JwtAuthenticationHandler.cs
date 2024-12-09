using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using WebsocketChat.Client.Helpers;
using WebsocketChat.Client.HttpClients;

namespace WebsocketChat.Client.JwtAuth
{
    internal class JwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IApiHttpClient _apiHttpClient;

        public JwtAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            IApiHttpClient apiHttpClient,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
            _apiHttpClient = apiHttpClient;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Cookies.ContainsKey(RequestHelper.JwtCookiesKey))
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            var token = Request.Cookies[RequestHelper.JwtCookiesKey].ToString(null);

            var response = await _apiHttpClient.GetAuthValidateToken(token);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var identity = new ClaimsIdentity(jwtToken.Claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }

            return AuthenticateResult.Fail("Unauthorized");
        }
    }
}
