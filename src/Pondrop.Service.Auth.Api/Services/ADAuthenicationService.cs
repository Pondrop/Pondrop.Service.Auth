
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Pondrop.Service.Auth.Api.Enums;
using Pondrop.Service.Auth.Api.Models;
using Pondrop.Service.Auth.Api.Services.Interfaces;
using Pondrop.Service.Auth.Application.Models;

namespace Pondrop.Service.Auth.Api.Services;

public class ADAuthenicationService : IADAuthenticationService
{
    private readonly ILogger<JWTTokenProvider> _logger;
    private readonly ADConfiguration _config;

    private readonly string _authority;
    private readonly string _clientId;
    private readonly string[]? _scopes;

    public ADAuthenicationService(
        IOptions<ADConfiguration> config,
        ILogger<JWTTokenProvider> logger)
    {
        _logger = logger;

        if (string.IsNullOrEmpty(config.Value?.Authority))
            throw new ArgumentException("AD Config 'Authority' cannot be null or empty");
        if (string.IsNullOrEmpty(config.Value?.ClientId))
            throw new ArgumentException("AD Config 'ClientId' cannot be null or empty"); ;
        if (config.Value?.Scope is null)
            throw new ArgumentException("AD Config 'Scope' cannot be null or empty"); ;

        _config = config.Value;

        _authority = _config.Authority;
        _scopes = _config.Scope;
        _clientId = _config.ClientId;

    }

    public async Task<string> IsADUserAsync(AdminSigninRequest adminSigninRequest)
    {
        IPublicClientApplication app;
        app = PublicClientApplicationBuilder.Create(_clientId)
                                          .WithAuthority(_authority)
                                          .Build();
        string result = string.Empty;

        try
        {
            var authResult = await app.AcquireTokenByUsernamePassword(_scopes,
                    adminSigninRequest.Email,
                    adminSigninRequest.Password)
                .ExecuteAsync();
            if (authResult.AccessToken is not null)
                result = ADAuthenticationResult.Success.ToString();
        }
        catch (MsalUiRequiredException ex)
        {
            return ex.Message;
        }

        return result;
    }
}
