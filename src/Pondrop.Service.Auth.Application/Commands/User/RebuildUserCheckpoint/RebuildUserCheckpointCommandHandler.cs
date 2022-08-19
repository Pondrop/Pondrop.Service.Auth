using Microsoft.Extensions.Logging;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Application.Interfaces;
using Pondrop.Service.Auth.Domain.Models;
using Pondrop.Service.User.Application.Commands;

namespace Pondrop.Service.Auth.Application.Commands;

public class RebuildUserCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildUserCheckpointCommand, UserEntity>
{
    public RebuildUserCheckpointCommandHandler(
        ICheckpointRepository<UserEntity> storeCheckpointRepository,
        ILogger<RebuildUserCheckpointCommandHandler> logger) : base(storeCheckpointRepository, logger)
    {
    }
}