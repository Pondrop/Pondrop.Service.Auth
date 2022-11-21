using FluentValidation;

namespace Pondrop.Service.Auth.Application.Commands;

public class UserLoginCommandHandlerValidator : AbstractValidator<UserLoginCommand>
{
    public UserLoginCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}