using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Auth.Application.Interfaces;
using Pondrop.Service.Auth.Application.Models;
using Pondrop.Service.Auth.Domain.Models;

namespace Pondrop.Service.User.Application.Queries;

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, Result<UserViewRecord?>>
{
    private readonly ICheckpointRepository<UserEntity> _viewRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetUserByEmailQuery> _validator;
    private readonly ILogger<GetUserByEmailQueryHandler> _logger;

    public GetUserByEmailQueryHandler(
        ICheckpointRepository<UserEntity> viewRepository,
        IMapper mapper,
        IValidator<GetUserByEmailQuery> validator,
        ILogger<GetUserByEmailQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _validator = validator;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<UserViewRecord?>> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get User by email failed {validation}";
            _logger.LogError(errorMessage);
            return Result<UserViewRecord?>.Error(errorMessage);
        }

        var result = default(Result<UserViewRecord?>);

        try
        {
            var queryResult = await _viewRepository.QueryAsync($"SELECT * FROM c WHERE c.normalizedEmail = '{query.Email.ToUpper()}'");
            var record = queryResult?.FirstOrDefault();

            result = record is not null
                ? Result<UserViewRecord?>.Success(_mapper.Map<UserViewRecord>(record))
                : Result<UserViewRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<UserViewRecord?>.Error(ex);
        }

        return result;
    }
}