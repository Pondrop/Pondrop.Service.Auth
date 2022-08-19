using MediatR;
using Pondrop.Service.Auth.Application.Models;
using Pondrop.Service.Auth.Domain.Models;

namespace Pondrop.Service.Auth.Application.Commands;

public class UserLoginCommand : IRequest<Result<UserRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
}