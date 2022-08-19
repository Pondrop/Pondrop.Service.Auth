using Pondrop.Service.Auth.Api.Models;

namespace Pondrop.Service.Auth.Api.Services.Interfaces;
public interface IJWTTokenProvider
{
    string AuthenticateShopper(TokenRequest request);
}

