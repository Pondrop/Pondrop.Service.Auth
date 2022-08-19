using MediatR;
using Pondrop.Service.Auth.Application.Models;

namespace Pondrop.Service.User.Application.Commands;

public class UpdateUserViewCommand : IRequest<Result<int>>
{
    public Guid? UserId { get; init; } = null;
}