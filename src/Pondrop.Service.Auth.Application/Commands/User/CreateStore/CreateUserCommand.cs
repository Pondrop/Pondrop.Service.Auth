using MediatR;
using Pondrop.Service.Auth.Application.Models;
using Pondrop.Service.Auth.Domain.Models;

namespace Pondrop.Service.Auth.Application.Commands;

public class CreateUserCommand : IRequest<Result<UserRecord>>
{
    public string Email { get; init; } = string.Empty;
}
