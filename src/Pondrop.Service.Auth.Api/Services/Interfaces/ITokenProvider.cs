using Pondrop.Service.Auth.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Pondrop.Service.Auth.Api.Services.Interfaces;
public interface IJWTTokenProvider
{
    string AuthenticateShopper(TokenRequest request);

    ClaimsPrincipal ValidateToken(string token);

    string GetClaim(ClaimsPrincipal principal, string claimName);
}

