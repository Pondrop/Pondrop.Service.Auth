using MediatR;
using Pondrop.Service.Auth.Application.Models;

namespace Pondrop.Service.Auth.Application.Commands;

public abstract class RebuildCheckpointCommand : IRequest<Result<int>> 
{
}