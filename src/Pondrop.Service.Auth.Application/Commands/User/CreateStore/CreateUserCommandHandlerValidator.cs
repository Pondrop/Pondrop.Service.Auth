using FluentValidation;
using Pondrop.Service.Auth.Application.Interfaces.Services;

namespace Pondrop.Service.Auth.Application.Commands;

public class CreateUserCommandHandlerValidator : AbstractValidator<CreateUserCommand>
{
    
    public CreateUserCommandHandlerValidator()
    {
        RuleFor(x => x.Email).NotEmpty();
    }
}