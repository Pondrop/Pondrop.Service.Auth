using FluentValidation;

namespace Pondrop.Service.Auth.Application.Commands;

public class CreateUserCommandHandlerValidator : AbstractValidator<CreateUserCommand>
{
    
    public CreateUserCommandHandlerValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}