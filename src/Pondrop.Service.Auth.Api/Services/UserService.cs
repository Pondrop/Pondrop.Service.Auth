using Pondrop.Service.Auth.Application.Interfaces.Services;

namespace Pondrop.Service.Auth.Api.Services;

public class UserService : IUserService
{
    public string CurrentUserName() => "admin";
    public string GetMaterializedViewUserName() => "materialized_view";
}