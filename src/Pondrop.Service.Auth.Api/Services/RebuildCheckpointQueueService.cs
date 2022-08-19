using Pondrop.Service.Auth.Api.Services;
using Pondrop.Service.Auth.Application.Commands;

namespace Pondrop.Service.Auth.Api.Services;

public class RebuildCheckpointQueueService : BaseBackgroundQueueService<RebuildCheckpointCommand>, IRebuildCheckpointQueueService
{
}