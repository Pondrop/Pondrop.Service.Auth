using FluentValidation;
using Pondrop.Service.Auth.Application.Interfaces.Services;

namespace Pondrop.Service.Auth.Application.Commands;

public class UserLogoutCommandHandlerValidator : AbstractValidator<UserLogoutCommand>
{
    public UserLogoutCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}