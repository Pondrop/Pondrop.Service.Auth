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
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Auth.Application.Tests.Commands.AuthType.CreateAuthType;

public class CreateAuthTypeCommandHandlerTests
{
    private readonly Mock<IOptions<AuthTypeUpdateConfiguration>> _AuthTypeUpdateConfigMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IDaprService> _daprServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateAuthTypeCommand>> _validatorMock;
    private readonly Mock<ILogger<CreateAuthTypeCommandHandler>> _loggerMock;
    
    public CreateAuthTypeCommandHandlerTests()
    {
        _AuthTypeUpdateConfigMock = new Mock<IOptions<AuthTypeUpdateConfiguration>>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _daprServiceMock = new Mock<IDaprService>();
        _userServiceMock = new Mock<IUserService>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<CreateAuthTypeCommand>>();
        _loggerMock = new Mock<ILogger<CreateAuthTypeCommandHandler>>();

        _AuthTypeUpdateConfigMock
            .Setup(x => x.Value)
            .Returns(new AuthTypeUpdateConfiguration());
        _userServiceMock
            .Setup(x => x.CurrentUserName())
            .Returns("test/user");
    }
    
    [Fact]
    public async void CreateAuthTypeCommand_ShouldSucceed()
    {
        // arrange
        var cmd = AuthTypeFaker.GetCreateAuthTypeCommand();
        var item = AuthTypeFaker.GetAuthTypeRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<AuthTypeRecord>(It.IsAny<AuthTypeEntity>()))
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
            x => x.Map<AuthTypeRecord>(It.IsAny<AuthTypeEntity>()),
            Times.Once);
    }
    
    [Fact]
    public async void CreateAuthTypeCommand_WhenInvalid_ShouldFail()
    {
        // arrange
        var cmd = AuthTypeFaker.GetCreateAuthTypeCommand();
        var item = AuthTypeFaker.GetAuthTypeRecord(cmd);
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
            x => x.Map<AuthTypeRecord>(It.IsAny<AuthTypeEntity>()),
            Times.Never);
    }

    [Fact]
    public async void CreateAuthTypeCommand_WhenAppendEventsFail_ShouldFail()
    {
        // arrange
        var cmd = AuthTypeFaker.GetCreateAuthTypeCommand();
        var item = AuthTypeFaker.GetAuthTypeRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(false));
        _mapperMock
            .Setup(x => x.Map<AuthTypeRecord>(It.IsAny<AuthTypeEntity>()))
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
            x => x.Map<AuthTypeRecord>(It.IsAny<AuthTypeEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateAuthTypeCommand_WhenException_ShouldFail()
    {
        // arrange
        var cmd = AuthTypeFaker.GetCreateAuthTypeCommand();
        var item = AuthTypeFaker.GetAuthTypeRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<AuthTypeRecord>(It.IsAny<AuthTypeEntity>()))
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
            x => x.Map<AuthTypeRecord>(It.IsAny<AuthTypeEntity>()),
            Times.Never);
    }
    
    private CreateAuthTypeCommandHandler GetCommandHandler() =>
        new CreateAuthTypeCommandHandler(
            _AuthTypeUpdateConfigMock.Object,
            _eventRepositoryMock.Object,
            _daprServiceMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}