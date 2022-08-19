using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Application.Interfaces;
using Pondrop.Service.Auth.Domain.Models;

namespace Pondrop.Service.Auth.Application.Commands;

public class UpdateUserCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateUserCheckpointByIdCommand, UserEntity, UserRecord>
{
    public UpdateUserCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<UserEntity> userCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateUserCheckpointByIdCommandHandler> logger) : base(eventRepository, userCheckpointRepository, mapper, validator, logger)
    {
    }
}