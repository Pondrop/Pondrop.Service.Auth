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

    public JWTTokenProvider(IConfiguration configuration,
        ILogger<JWTTokenProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;

    }

    public string AuthenticateShopper(TokenRequest request)
    {
        string accessToken = string.Empty;
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                  {
                 new Claim(JwtRegisteredClaimNames.Sub, request.Id.ToString()),
                 new Claim(JwtRegisteredClaimNames.Email, request.Email)
                  }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
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
}
