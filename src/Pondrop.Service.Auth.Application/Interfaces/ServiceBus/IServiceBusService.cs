using Pondrop.Service.Auth.Domain.Events;

namespace Pondrop.Service.Auth.Application.Interfaces;

public interface IServiceBusService
{
    Task SendMessageAsync(object payload);

    Task SendMessageAsync(string subject, object payload);
}