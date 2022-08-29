namespace Pondrop.Service.Auth.Api.Enums;

public enum ADAuthenticationResult
{
    Failed,
    Success,
    PermissionError,
    InvalidRequest,
    UnauthorizedClient,
    UnknownUserType,
    NotRecognized
}
