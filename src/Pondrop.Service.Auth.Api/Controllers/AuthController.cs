using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pondrop.Service.Auth.Api.Services.Interfaces;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Application.Interfaces;
using Pondrop.Service.User.Application.Queries;
using Pondrop.Service.Auth.Api.Models;

namespace Pondrop.Service.Auth.ApiControllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IServiceBusService _serviceBusService;
    private readonly IJWTTokenProvider _jWTTokenProvider;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator,
        IJWTTokenProvider jWTTokenProvider,
        IServiceBusService serviceBusService,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _serviceBusService = serviceBusService;
        _jWTTokenProvider = jWTTokenProvider;
        _logger = logger;
    }

    [HttpPost]
    [Route("shopper/signin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAuth([FromBody] SigninRequest request)
    {
        Guid userId = default;
        string accessToken;

        try
        {
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

            accessToken = _jWTTokenProvider.AuthenticateShopper(new TokenRequest()
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
}