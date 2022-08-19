using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Domain.Models;

namespace Pondrop.Service.Auth.Application.Interfaces;

public interface ICheckpointRepository<T> : IContainerRepository<T> where T : EventEntity
{
    Task<int> RebuildAsync();
    Task<T?> UpsertAsync(long expectedVersion, T item);

    Task FastForwardAsync(T item);
}