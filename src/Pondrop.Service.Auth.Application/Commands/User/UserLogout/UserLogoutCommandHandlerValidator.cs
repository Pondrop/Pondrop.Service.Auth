using FluentValidation;

namespace Pondrop.Service.Auth.Application.Commands;

public class UserLogoutCommandHandlerValidator : AbstractValidator<UserLogoutCommand>
{
    public UserLogoutCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}