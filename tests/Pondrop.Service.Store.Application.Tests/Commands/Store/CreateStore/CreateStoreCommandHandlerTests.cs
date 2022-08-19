using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Application.Interfaces;
using Pondrop.Service.Auth.Application.Interfaces.Services;
using Pondrop.Service.Auth.Application.Models;
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

public class CreateAuthCommandHandlerTests
{
    private readonly Mock<IOptions<AuthUpdateConfiguration>> _AuthUpdateConfigMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<ICheckpointRepository<RetailerEntity>> _retailerViewRepositoryMock;
    private readonly Mock<ICheckpointRepository<AuthTypeEntity>> _storeTypeViewRepositoryMock;
    private readonly Mock<IDaprService> _daprServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateAuthCommand>> _validatorMock;
    private readonly Mock<ILogger<CreateAuthCommandHandler>> _loggerMock;
    
    public CreateAuthCommandHandlerTests()
    {
        _AuthUpdateConfigMock = new Mock<IOptions<AuthUpdateConfiguration>>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _retailerViewRepositoryMock = new Mock<ICheckpointRepository<RetailerEntity>>();
        _storeTypeViewRepositoryMock = new Mock<ICheckpointRepository<AuthTypeEntity>>();
        _daprServiceMock = new Mock<IDaprService>();
        _userServiceMock = new Mock<IUserService>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<CreateAuthCommand>>();
        _loggerMock = new Mock<ILogger<CreateAuthCommandHandler>>();

        _AuthUpdateConfigMock
            .Setup(x => x.Value)
            .Returns(new AuthUpdateConfiguration());
        _userServiceMock
            .Setup(x => x.CurrentUserName())
            .Returns("test/user");
    }
    
    [Fact]
    public async void CreateAuthCommand_ShouldSucceed()
    {
        // arrange
        var cmd = AuthFaker.GetCreateAuthCommand();
        var item = AuthFaker.GetAuthRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _retailerViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.RetailerId))
            .Returns(Task.FromResult<RetailerEntity?>(new RetailerEntity()));
        _storeTypeViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.AuthTypeId))
            .Returns(Task.FromResult<AuthTypeEntity?>(new AuthTypeEntity()));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<AuthRecord>(It.IsAny<UserEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(item, result.Value);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<AuthRecord>(It.IsAny<UserEntity>()),
            Times.Once);
    }
    
    [Fact]
    public async void CreateAuthCommand_WhenInvalid_ShouldFail()
    {
        // arrange
        var cmd = AuthFaker.GetCreateAuthCommand();
        var item = AuthFaker.GetAuthRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<AuthRecord>(It.IsAny<UserEntity>()),
            Times.Never);
    }

    [Fact]
    public async void CreateAuthCommand_WhenAppendEventsFail_ShouldFail()
    {
        // arrange
        var cmd = AuthFaker.GetCreateAuthCommand();
        var item = AuthFaker.GetAuthRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _retailerViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.RetailerId))
            .Returns(Task.FromResult<RetailerEntity?>(new RetailerEntity()));
        _storeTypeViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.AuthTypeId))
            .Returns(Task.FromResult<AuthTypeEntity?>(new AuthTypeEntity()));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(false));
        _mapperMock
            .Setup(x => x.Map<AuthRecord>(It.IsAny<UserEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<AuthRecord>(It.IsAny<UserEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateAuthCommand_WhenException_ShouldFail()
    {
        // arrange
        var cmd = AuthFaker.GetCreateAuthCommand();
        var item = AuthFaker.GetAuthRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _retailerViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.RetailerId))
            .Returns(Task.FromResult<RetailerEntity?>(new RetailerEntity()));
        _storeTypeViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.AuthTypeId))
            .Returns(Task.FromResult<AuthTypeEntity?>(new AuthTypeEntity()));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<AuthRecord>(It.IsAny<UserEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<AuthRecord>(It.IsAny<UserEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateAuthCommand_WhenRetailerNotFound_ShouldFail()
    {
        // arrange
        var cmd = AuthFaker.GetCreateAuthCommand();
        var item = AuthFaker.GetAuthRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _retailerViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.RetailerId))
            .Returns(Task.FromResult<RetailerEntity?>(null));
        _storeTypeViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.AuthTypeId))
            .Returns(Task.FromResult<AuthTypeEntity?>(new AuthTypeEntity()));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<AuthRecord>(It.IsAny<UserEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _retailerViewRepositoryMock.Verify(
            x =>x.GetByIdAsync(cmd.RetailerId),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<AuthRecord>(It.IsAny<UserEntity>()),
            Times.Never());
    }
    
    [Fact]
    public async void CreateAuthCommand_WhenAuthTypeNotFound_ShouldFail()
    {
        // arrange
        var cmd = AuthFaker.GetCreateAuthCommand();
        var item = AuthFaker.GetAuthRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _retailerViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.RetailerId))
            .Returns(Task.FromResult<RetailerEntity?>(new RetailerEntity()));
        _storeTypeViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.AuthTypeId))
            .Returns(Task.FromResult<AuthTypeEntity?>(null));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<AuthRecord>(It.IsAny<UserEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _storeTypeViewRepositoryMock.Verify(
            x =>x.GetByIdAsync(cmd.AuthTypeId),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<AuthRecord>(It.IsAny<UserEntity>()),
            Times.Never());
    }
    
    private CreateAuthCommandHandler GetCommandHandler() =>
        new CreateAuthCommandHandler(
            _AuthUpdateConfigMock.Object,
            _eventRepositoryMock.Object,
            _retailerViewRepositoryMock.Object,
            _storeTypeViewRepositoryMock.Object,
            _daprServiceMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}