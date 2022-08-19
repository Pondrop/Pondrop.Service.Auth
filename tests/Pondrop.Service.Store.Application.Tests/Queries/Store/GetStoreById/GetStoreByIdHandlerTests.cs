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

namespace Pondrop.Service.Auth.Application.Tests.Commands.Auth.CreateAuth;

public class GetAuthByIdHandlerTests
{
    private readonly Mock<IContainerRepository<AuthViewRecord>> _storeContainerRepositoryMock;
    private readonly Mock<IValidator<GetAuthByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetAuthByIdQueryHandler>> _loggerMock;
    
    public GetAuthByIdHandlerTests()
    {
        _storeContainerRepositoryMock = new Mock<IContainerRepository<AuthViewRecord>>();
        _validatorMock = new Mock<IValidator<GetAuthByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetAuthByIdQueryHandler>>();
    }
    
    [Fact]
    public async void GetAuthByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetAuthByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<AuthViewRecord?>(new AuthViewRecord()));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    [Fact]
    public async void GetAuthByIdQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetAuthByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        _storeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<AuthViewRecord?>(new AuthViewRecord()));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Never());
    }
    
    [Fact]
    public async void GetAuthByIdQuery_WhenNotFound_ShouldSucceedWithNull()
    {
        // arrange
        var query = new GetAuthByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<AuthViewRecord?>(null));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    [Fact]
    public async void GetAuthByIdQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetAuthByIdQuery() { Id = Guid.NewGuid() };
        var item = AuthFaker.GetAuthRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Throws(new Exception());
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    private GetAuthByIdQueryHandler GetQueryHandler() =>
        new GetAuthByIdQueryHandler(
            _storeContainerRepositoryMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}