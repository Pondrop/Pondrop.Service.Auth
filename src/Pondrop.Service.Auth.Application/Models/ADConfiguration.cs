namespace Pondrop.Service.Auth.Application.Models;

public class ADConfiguration
{
    public const string Key = nameof(ADConfiguration);

    public string Authority { get; set; } = string.Empty;

    public string[]? Scope { get; init; }

    public string ClientId { get; set; } = string.Empty;
}
