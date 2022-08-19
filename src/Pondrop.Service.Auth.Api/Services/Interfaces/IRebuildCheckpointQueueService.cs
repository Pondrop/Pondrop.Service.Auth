using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Application.Commands;

namespace Pondrop.Service.Auth.Api.Services;

public interface IRebuildCheckpointQueueService
{
    Task<RebuildCheckpointCommand> DequeueAsync(CancellationToken cancellationToken);
    void Queue(RebuildCheckpointCommand command);
}