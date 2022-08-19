namespace Pondrop.Service.Auth.Application.Models;

public class ServiceBusConfiguration
{
    public const string Key = nameof(ServiceBusConfiguration);

    public string ConnectionString { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
}
