using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Auth.Application.Interfaces;
using Pondrop.Service.Auth.Application.Interfaces.Services;
using Pondrop.Service.Auth.Application.Models;
using Pondrop.Service.Auth.Domain.Models;

namespace Pondrop.Service.User.Application.Commands;

public class UpdateUserViewCommandHandler : IRequestHandler<UpdateUserViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<UserEntity> _userCheckpointRepository;
    private readonly IContainerRepository<UserViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateUserViewCommandHandler> _logger;

    public UpdateUserViewCommandHandler(
        ICheckpointRepository<UserEntity> userCheckpointRepository,
        IContainerRepository<UserViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateUserViewCommandHandler> logger) : base()
    {
        _userCheckpointRepository = userCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateUserViewCommand command, CancellationToken cancellationToken)
    {
        if (!command.UserId.HasValue)
            return Result<int>.Success(0);

        var result = default(Result<int>);

        try
        {
            var affectedUsersTask = GetAffectedUsersAsync( command.UserId);

            await Task.WhenAll(affectedUsersTask);


            var tasks = affectedUsersTask.Result.Select(async i =>
            {
                var success = false;

                try
                {
                    var userView = _mapper.Map<UserViewRecord>(i);

                    var result = await _containerRepository.UpsertAsync(userView);
                    success = result != null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update user view for '{i.Id}'");
                }

                return success;
            }).ToList();

            await Task.WhenAll(tasks);

            result = Result<int>.Success(tasks.Count(t => t.Result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage(command));
            result = Result<int>.Error(ex);
        }

        return result;
    }

    private async Task<List<UserEntity>> GetAffectedUsersAsync(Guid? userId)
    {
        const string userIdKey = "@userId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        if (userId.HasValue)
        {
            conditions.Add($"c.id = {userIdKey}");
            parameters.Add(userIdKey, userId.Value.ToString());
        }

        if (!conditions.Any())
            return new List<UserEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedUsers = await _userCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedUsers;
    }

    private static string FailedToMessage(UpdateUserViewCommand command) =>
        $"Failed to update user view '{JsonConvert.SerializeObject(command)}'";
}