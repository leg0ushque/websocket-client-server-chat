using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using WebsocketChat.Server.Identity;

namespace WebsocketChat.Server.Services
{
    public class JwtTokenService
    {
        private readonly JwtTokenOptions _settings;

        public JwtTokenService(IOptions<JwtTokenOptions> options)
        {
            if (options != null)
            {
                _settings = options.Value;
            }
        }

        public string GetToken(User user, IList<string> roles)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // Adding claims to the list of all claims inside the JWT token.
            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role));
            var userClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(Identity.IdentityConstants.UserIdClaimType, user.Id),
                new Claim(JwtRegisteredClaimNames.GivenName, user.Nickname),
            };

            userClaims.AddRange(roleClaims);

            var jwt = new JwtSecurityToken(
                issuer: _settings.JwtIssuer,
                audience: _settings.JwtAudience,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtSecretKey)),
                    SecurityAlgorithms.HmacSha256),
                claims: userClaims,
                expires: DateTime.Now.AddDays(Constants.TokenDaysExpirationTerm));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                _ = HandleTokenValidation(tokenHandler, token);
            }
            catch (SecurityTokenException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        public string GetUserIdFromValidatedToken(string tokenToValidate)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var securityToken = HandleTokenValidation(tokenHandler, tokenToValidate);
                var jwtToken = (JwtSecurityToken)securityToken;
                var userId = jwtToken.Claims.First(c => c.Type == Identity.IdentityConstants.UserIdClaimType).Value;

                return userId;
            }
            catch (SecurityTokenException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public SecurityToken HandleTokenValidation(JwtSecurityTokenHandler tokenHandler, string token)
        {
            tokenHandler.ValidateToken(token,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _settings.JwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = _settings.JwtAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtSecretKey)),
                    ValidateLifetime = true,
                    LifetimeValidator =
                        (before, exprise, localToken, parameters) =>
                        {
                            if (exprise.HasValue)
                            {
                                return exprise.Value > DateTime.UtcNow;
                            }

                            return true;
                        },
                    RequireExpirationTime = true,
                },
                out var claimsPrincipal);

            return claimsPrincipal;
        }
    }
}
