using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Auth.Application.Models;
using Pondrop.Service.Auth.Domain.Models;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.Auth.Application.Commands;

public class CreateUserCommandHandler : DirtyCommandHandler<UserEntity, CreateUserCommand, Result<UserRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateUserCommand> _validator;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IOptions<UserUpdateConfiguration> storeUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateUserCommand> validator,
        ILogger<CreateUserCommandHandler> logger) : base(eventRepository, storeUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<UserRecord>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create user failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<UserRecord>.Error(errorMessage);
        }

        var result = default(Result<UserRecord>);

        try
        {
            var userEntity = new UserEntity(
                command.Email);
            var success = await _eventRepository.AppendEventsAsync(userEntity.StreamId, 0, userEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(userEntity.Id, userEntity.GetEvents()));

            result = success
                ? Result<UserRecord>.Success(_mapper.Map<UserRecord>(userEntity))
                : Result<UserRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<UserRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateUserCommand command) =>
        $"Failed to create store\nCommand: '{JsonConvert.SerializeObject(command)}'";
}