using Pondrop.Service.Auth.Application.Models.Signin;

namespace Pondrop.Service.Auth.Api.Services.Interfaces;
public interface IJWTTokenProvider
{
    SigninResponse Authenticate(SigninRequest request);
}

