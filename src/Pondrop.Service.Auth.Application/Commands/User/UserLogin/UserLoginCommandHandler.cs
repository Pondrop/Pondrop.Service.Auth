﻿using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Auth.Application.Models;
using Pondrop.Service.Auth.Domain.Events.User;
using Pondrop.Service.Auth.Domain.Models;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.Auth.Application.Commands;

public class UserLoginCommandHandler : DirtyCommandHandler<UserEntity, UserLoginCommand, Result<UserRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<UserEntity> _userCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UserLoginCommand> _validator;
    private readonly ILogger<UserLoginCommandHandler> _logger;

    public UserLoginCommandHandler(
        IOptions<UserUpdateConfiguration> userUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<UserEntity> userCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UserLoginCommand> validator,
        ILogger<UserLoginCommandHandler> logger) : base(eventRepository, userUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _userCheckpointRepository = userCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<UserRecord>> Handle(UserLoginCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update user login failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<UserRecord>.Error(errorMessage);
        }

        var result = default(Result<UserRecord>);

        try
        {
            var userEntity = await _userCheckpointRepository.GetByIdAsync(command.Id);
            userEntity ??= await GetFromStreamAsync(command.Id);

            if (userEntity is not null)
            {
                var loginDateTime = DateTime.UtcNow;
                var evtPayload = new UserLogin(loginDateTime);
                var createdBy = _userService.CurrentUserName();

                var success = await UpdateStreamAsync(userEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _userCheckpointRepository.FastForwardAsync(userEntity);
                    success = await UpdateStreamAsync(userEntity, evtPayload, createdBy);
                }

                await Task.WhenAll(
                    InvokeDaprMethods(userEntity.Id, userEntity.GetEvents(userEntity.AtSequence)));

                result = success
                    ? Result<UserRecord>.Success(_mapper.Map<UserRecord>(userEntity))
                    : Result<UserRecord>.Error(FailedToCreateMessage(command));
            }
            else
            {
                result = Result<UserRecord>.Error($"Auth does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<UserRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(UserLoginCommand command) =>
        $"Failed to update user\nCommand: '{JsonConvert.SerializeObject(command)}'";
}