using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pondrop.Service.Auth.Api.Services;
using Pondrop.Service.Auth.Api.Services.Interfaces;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Application.Interfaces;
using Pondrop.Service.Auth.Application.Models.Signin;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.User.Application.Queries;

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
        SigninResponse token;

        var getUserByEmailQuery = new GetUserByEmailQuery() { Email = request.Email };
        var getUserByEmailResult = await _mediator.Send(getUserByEmailQuery);

        if (getUserByEmailResult.IsSuccess && getUserByEmailResult.Value is not null)
        {
            userId = getUserByEmailResult.Value.Id;
            var userLoginCommand = new UserLoginCommand() { Id = userId };
            var loginResult = await _mediator.Send(userLoginCommand);
            await loginResult.MatchAsync(
                       async i =>
                       {
                           await _serviceBusService.SendMessageAsync(new UpdateUserCheckpointByIdCommand() { Id = i!.Id });
                           userId = i.Id;
                       },
                       (ex, msg) => throw new Exception());

            token = _jWTTokenProvider.Authenticate(new SigninRequest()
            {
                Email = request.Email,
            });
        }
        else
        {
            var createUserCommand = new CreateUserCommand() { Email = request.Email };
            var createUserResult = await _mediator.Send(createUserCommand);

            if (!createUserResult.IsSuccess)
                return BadRequest();

            await createUserResult.MatchAsync(
                        async i =>
                        {
                            await _serviceBusService.SendMessageAsync(new UpdateUserCheckpointByIdCommand() { Id = i!.Id });
                            userId = i.Id;
                        },
                        (ex, msg) => throw new Exception());

            token = _jWTTokenProvider.Authenticate(new SigninRequest()
            {
                Email = request.Email,
            });
        }

        if(token is null || string.IsNullOrEmpty(token.AccessToken)) return BadRequest();

        return StatusCode(StatusCodes.Status200OK, token);
    }
}