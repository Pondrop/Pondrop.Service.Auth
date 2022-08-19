using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Application.Interfaces;
using Pondrop.Service.Auth.Application.Queries;
using Pondrop.Service.Auth.Domain.Events;
using Pondrop.Service.Auth.Domain.Models;
using Pondrop.Service.Auth.Tests.Faker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Auth.Application.Tests.Commands.AuthType.CreateAuthType;

public class GetAuthTypeByIdHandlerTests
{
    private readonly Mock<ICheckpointRepository<AuthTypeEntity>> _storeTypeCheckpointRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<GetAuthTypeByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetAuthTypeByIdQueryHandler>> _loggerMock;
    
    public GetAuthTypeByIdHandlerTests()
    {
        _storeTypeCheckpointRepositoryMock = new Mock<ICheckpointRepository<AuthTypeEntity>>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<GetAuthTypeByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetAuthTypeByIdQueryHandler>>();
    }
    
    [Fact]
    public async void GetAuthTypeByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetAuthTypeByIdQuery() { Id = Guid.NewGuid() };
        var item = AuthTypeFaker.GetAuthTypeRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeTypeCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<AuthTypeEntity?>(new AuthTypeEntity()));
        _mapperMock
            .Setup(x => x.Map<AuthTypeRecord>(It.IsAny<AuthTypeEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(item, result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeTypeCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<AuthTypeRecord>(It.IsAny<AuthTypeEntity>()),
            Times.Once());
    }
    
    [Fact]
    public async void GetAuthTypeByIdQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetAuthTypeByIdQuery() { Id = Guid.NewGuid() };
        var item = AuthTypeFaker.GetAuthTypeRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        _storeTypeCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<AuthTypeEntity?>(new AuthTypeEntity()));
        _mapperMock
            .Setup(x => x.Map<AuthTypeRecord>(It.IsAny<AuthTypeEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeTypeCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<AuthTypeRecord>(It.IsAny<AuthTypeEntity>()),
            Times.Never());
    }
    
    [Fact]
    public async void GetAuthTypeByIdQuery_WhenNotFound_ShouldSucceedWithNull()
    {
        // arrange
        var query = new GetAuthTypeByIdQuery() { Id = Guid.NewGuid() };
        var item = AuthTypeFaker.GetAuthTypeRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeTypeCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<AuthTypeEntity?>(null));
        _mapperMock
            .Setup(x => x.Map<AuthTypeRecord>(It.IsAny<AuthTypeEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeTypeCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<AuthTypeRecord>(It.IsAny<AuthTypeEntity>()),
            Times.Never());
    }
    
    [Fact]
    public async void GetAuthTypeByIdQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetAuthTypeByIdQuery() { Id = Guid.NewGuid() };
        var item = AuthTypeFaker.GetAuthTypeRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeTypeCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<AuthTypeRecord>(It.IsAny<AuthTypeEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeTypeCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<AuthTypeRecord>(It.IsAny<AuthTypeEntity>()),
            Times.Never());
    }
    
    private GetAuthTypeByIdQueryHandler GetQueryHandler() =>
        new GetAuthTypeByIdQueryHandler(
            _storeTypeCheckpointRepositoryMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}