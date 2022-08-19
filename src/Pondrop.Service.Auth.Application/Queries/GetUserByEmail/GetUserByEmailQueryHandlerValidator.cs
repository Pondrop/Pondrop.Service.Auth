using FluentValidation;

namespace Pondrop.Service.User.Application.Queries;

public class GetUserByEmailQueryHandlerValidator : AbstractValidator<GetUserByEmailQuery>
{
    public GetUserByEmailQueryHandlerValidator()
    {
        RuleFor(x => x.Email).NotEmpty();
    }
}