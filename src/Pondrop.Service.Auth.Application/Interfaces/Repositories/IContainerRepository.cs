using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Domain.Models;

namespace Pondrop.Service.Auth.Application.Interfaces;

public interface IContainerRepository<T>
{
    Task<bool> IsConnectedAsync();

    Task<T?> UpsertAsync(T item);

    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);

    Task<List<T>> QueryAsync(string sqlQueryText, Dictionary<string, string>? parameters = null);
}