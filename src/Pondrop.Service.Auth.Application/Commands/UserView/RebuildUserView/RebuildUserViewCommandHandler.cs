using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Auth.Application.Models;
using Pondrop.Service.Auth.Domain.Models;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.user.Application.Commands;

public class RebuildUserViewCommandHandler : IRequestHandler<RebuildUserViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<UserEntity> _userCheckpointRepository;
    private readonly IContainerRepository<UserViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<RebuildUserViewCommandHandler> _logger;

    public RebuildUserViewCommandHandler(
        ICheckpointRepository<UserEntity> userCheckpointRepository,
        IContainerRepository<UserViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildUserViewCommandHandler> logger) : base()
    {
        _userCheckpointRepository = userCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildUserViewCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var usersTask = _userCheckpointRepository.GetAllAsync();

            await Task.WhenAll(usersTask);


            var tasks = usersTask.Result.Select(async i =>
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
            _logger.LogError(ex, $"Failed to rebuild user view");
            result = Result<int>.Error(ex);
        }

        return result;
    }
}