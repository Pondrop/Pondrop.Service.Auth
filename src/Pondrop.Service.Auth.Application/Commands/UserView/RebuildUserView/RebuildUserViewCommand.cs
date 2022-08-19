using MediatR;
using Pondrop.Service.Auth.Application.Models;

namespace Pondrop.Service.Auth.Application.Commands;

public class RebuildUserViewCommand : IRequest<Result<int>>
{
}