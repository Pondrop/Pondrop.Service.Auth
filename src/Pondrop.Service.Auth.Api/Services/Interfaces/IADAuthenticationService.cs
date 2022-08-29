using Pondrop.Service.Auth.Api.Enums;
using Pondrop.Service.Auth.Api.Models;

namespace Pondrop.Service.Auth.Api.Services.Interfaces;

public interface IADAuthenticationService
{
    Task<string> IsADUserAsync(AdminSigninRequest adminSigninRequest);
}
