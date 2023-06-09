using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pondrop.Service.Auth.Api.Services.Interfaces;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.User.Application.Queries;
using Pondrop.Service.Auth.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Net.Http.Headers;
using Pondrop.Service.Auth.Api.Enums;
using System.Security.Claims;
using Pondrop.Service.Interfaces;

namespace Pondrop.Service.Auth.ApiControllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IServiceBusService _serviceBusService;
    private readonly IJWTTokenProvider _jwtTokenProvider;
    private readonly IADAuthenticationService _adAuthenticationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator,
        IJWTTokenProvider jWTTokenProvider,
        IADAuthenticationService adAuthenticationService,
        IServiceBusService serviceBusService,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _serviceBusService = serviceBusService;
        _adAuthenticationService = adAuthenticationService;
        _jwtTokenProvider = jWTTokenProvider;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("shopper/signin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ShopperSignin([FromBody] ShopperSigninRequest request)
    {
        Guid userId = default;
        string accessToken;

        try
        {
            if (request is null || string.IsNullOrEmpty(request.Email))
            {
                return new BadRequestObjectResult("No email address provided");
            }

            var getUserByEmailQuery = new GetUserByEmailQuery() { Email = request.Email };
            var getUserByEmailResult = await _mediator.Send(getUserByEmailQuery);

            if (getUserByEmailResult.IsSuccess && getUserByEmailResult.Value is not null)
            {
                userId = getUserByEmailResult.Value.Id;
                var userLoginCommand = new UserLoginCommand() { Id = userId };
                var loginResult = await _mediator.Send(userLoginCommand);

                if (!loginResult.IsSuccess)
                    return new BadRequestObjectResult("User login has failed");

                await loginResult.MatchAsync(
                       async i =>
                       {
                           await _serviceBusService.SendMessageAsync(new UpdateUserCheckpointByIdCommand() { Id = i!.Id });
                           userId = i.Id;
                       },
                       (ex, msg) => throw new Exception());
            }
            else
            {
                var createUserCommand = new CreateUserCommand() { Email = request.Email };
                var createUserResult = await _mediator.Send(createUserCommand);

                if (!createUserResult.IsSuccess)
                    return new BadRequestObjectResult("User creation has failed");

                await createUserResult.MatchAsync(
                        async i =>
                        {
                            await _serviceBusService.SendMessageAsync(new UpdateUserCheckpointByIdCommand() { Id = i!.Id });
                            userId = i.Id;
                        },
                        (ex, msg) => throw new Exception());
            }

            accessToken = _jwtTokenProvider.Authenticate(new TokenRequest()
            {
                Id = userId,
                Email = request.Email,
            });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex);
        }

        if (accessToken is null || string.IsNullOrEmpty(accessToken))
            return new BadRequestObjectResult("No access token provided");
        return StatusCode(StatusCodes.Status200OK, new SigninResponse(accessToken));
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("admin/signin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdminSignin([FromBody] AdminSigninRequest request)
    {
        Guid userId = default;
        string accessToken;

        try
        {
            if (request is null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return new BadRequestObjectResult("No email and/or password provided");
            }

            //var result = await _adAuthenticationService.IsADUserAsync(request);

            //if (result != ADAuthenticationResult.Success.ToString())
            //    return new BadRequestObjectResult($"Authentication Error: {result.ToString()}");

            var getUserByEmailQuery = new GetUserByEmailQuery() { Email = request.Email };
            var getUserByEmailResult = await _mediator.Send(getUserByEmailQuery);

            if (getUserByEmailResult.IsSuccess && getUserByEmailResult.Value is not null)
            {
                userId = getUserByEmailResult.Value.Id;
                var userLoginCommand = new UserLoginCommand() { Id = userId };
                var loginResult = await _mediator.Send(userLoginCommand);

                if (!loginResult.IsSuccess)
                    return new BadRequestObjectResult("User login has failed");

                await loginResult.MatchAsync(
                       async i =>
                       {
                           await _serviceBusService.SendMessageAsync(new UpdateUserCheckpointByIdCommand() { Id = i!.Id });
                           userId = i.Id;
                       },
                       (ex, msg) => throw new Exception());
            }
            else
            {
                var createUserCommand = new CreateUserCommand() { Email = request.Email };
                var createUserResult = await _mediator.Send(createUserCommand);

                if (!createUserResult.IsSuccess)
                    return new BadRequestObjectResult("User creation has failed");

                await createUserResult.MatchAsync(
                        async i =>
                        {
                            await _serviceBusService.SendMessageAsync(new UpdateUserCheckpointByIdCommand() { Id = i!.Id });
                            userId = i.Id;
                        },
                        (ex, msg) => throw new Exception());
            }

            accessToken = _jwtTokenProvider.Authenticate(new TokenRequest()
            {
                Id = userId,
                Email = request.Email,
                IsAdmin = true
            });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex);
        }

        if (accessToken is null || string.IsNullOrEmpty(accessToken))
            return new BadRequestObjectResult("No access token provided");
        return StatusCode(StatusCodes.Status200OK, new SigninResponse(accessToken));
    }

    [HttpPost]
    [Route("shopper/signout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ShopperSignout()
    {
        try
        {
            var token = Request.Headers[HeaderNames.Authorization];

            if (string.IsNullOrEmpty(token))
                return new BadRequestObjectResult("User login has failed");

            var principal = _jwtTokenProvider.ValidateToken(token);
            {
                var email = _jwtTokenProvider.GetClaim(principal, ClaimTypes.Email);

                var getUserByEmailQuery = new GetUserByEmailQuery() { Email = email };
                var getUserByEmailResult = await _mediator.Send(getUserByEmailQuery);

                if (getUserByEmailResult.IsSuccess && getUserByEmailResult.Value is not null)
                {
                    var userId = getUserByEmailResult.Value.Id;
                    var userLogoutCommand = new UserLogoutCommand() { Id = userId };
                    var logoutResult = await _mediator.Send(userLogoutCommand);

                    if (logoutResult.IsSuccess)
                    {
                        await logoutResult.MatchAsync(
                            async i =>
                            {
                                await _serviceBusService.SendMessageAsync(new UpdateUserCheckpointByIdCommand() { Id = i!.Id });
                                userId = i.Id;
                            },
                            (ex, msg) => throw new Exception());

                        return StatusCode(StatusCodes.Status200OK);
                    }
                }
            }
            return new BadRequestObjectResult("User login has failed");

        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex);
        }

    }
}