using MediatR;
using Pondrop.Service.Auth.Application.Models;

namespace Pondrop.Service.User.Application.Queries;

public class GetUserByEmailQuery : IRequest<Result<UserViewRecord?>>
{
    public string Email { get; init; } = string.Empty;
}