using MediatR;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Application.Models;
using Pondrop.Service.Auth.Domain.Models;

namespace Pondrop.Service.Auth.Application.Commands;

public class UpdateUserCheckpointByIdCommand : UpdateCheckpointByIdCommand<Result<UserRecord>>
{
}