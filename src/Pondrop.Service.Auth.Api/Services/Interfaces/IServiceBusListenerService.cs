using System.Collections.Concurrent;

namespace Pondrop.Service.Auth.Api.Services;

public interface IServiceBusListenerService
{
    Task StartListener();

    Task StopListener();

    ValueTask DisposeAsync();
}