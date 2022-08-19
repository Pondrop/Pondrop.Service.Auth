using Microsoft.IdentityModel.Tokens;
using Microsoft.Rest;
using Pondrop.Service.Auth.Api.Services.Interfaces;
using Pondrop.Service.Auth.Application.Models.Signin;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pondrop.Service.Auth.Api.Services;

public class JWTTokenProvider : IJWTTokenProvider
{

    private readonly IConfiguration _configuration;
    public JWTTokenProvider(IConfiguration configuration)
    {
        this._configuration = configuration;
    }

    public SigninResponse Authenticate(SigninRequest request)
    {
        // Else we generate JSON Web Token
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenKey = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
          {
             new Claim(ClaimTypes.Email, request.Email)
          }),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return new SigninResponse { AccessToken = tokenHandler.WriteToken(token) };

    }
}