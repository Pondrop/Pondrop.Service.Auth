namespace Pondrop.Service.Auth.Application.Interfaces.Services;

public interface IUserService
{
    string CurrentUserName();
    string GetMaterializedViewUserName();
}