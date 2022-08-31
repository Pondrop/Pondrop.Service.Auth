using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Rest;
using Pondrop.Service.Auth.Api.Models;
using Pondrop.Service.Auth.Api.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pondrop.Service.Auth.Api.Services;

public class JWTTokenProvider : IJWTTokenProvider
{

    private readonly IConfiguration _configuration;
    private readonly ILogger<JWTTokenProvider> _logger;
    private readonly byte[] _tokenKey;

    public JWTTokenProvider(IConfiguration configuration,
        ILogger<JWTTokenProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _tokenKey = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);

    }
    public string AuthenticateShopper(TokenRequest request)
    {
        string accessToken = string.Empty;
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.TokenLifetimeInMinutes = (int)TimeSpan.FromDays(10000).TotalMinutes;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                  {
                 new Claim(JwtRegisteredClaimNames.Sub, request.Id.ToString()),
                 new Claim(JwtRegisteredClaimNames.Email, request.Email)
                  }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            accessToken = token is not null ? tokenHandler.WriteToken(token) : string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError("Authentication failed: ", ex);
        }

        return accessToken;
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        IdentityModelEventSource.ShowPII = true;
        token = token.Replace("Bearer ", string.Empty);

        TokenValidationParameters validationParameters = new TokenValidationParameters();
        validationParameters.IssuerSigningKey = new SymmetricSecurityKey(_tokenKey);
        validationParameters.ValidateAudience = false;
        validationParameters.ValidateIssuer = false;
        validationParameters.ClockSkew = TimeSpan.FromDays(10000);

        ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out var validatedToken);

        return principal;
    }

    public string GetClaim(ClaimsPrincipal principal, string claimName)
    {
        var claimsValue = principal.FindFirst(claimName)?.Value;

        return claimsValue ?? string.Empty;
    }
}
