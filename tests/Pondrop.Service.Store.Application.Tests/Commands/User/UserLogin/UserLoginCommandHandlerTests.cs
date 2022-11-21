using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Application.Models;
using Pondrop.Service.Auth.Domain.Events;
using Pondrop.Service.Auth.Domain.Models;
using Pondrop.Service.Auth.Tests.Faker;
using Pondrop.Service.Events;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.User.Application.Tests.Commands.User.CreateUser;

public class UserLoginCommandHandlerTests
{
    private readonly Mock<IOptions<UserUpdateConfiguration>> _UserUpdateConfigMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IDaprService> _daprServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<UserLoginCommand>> _validatorMock;
    private readonly Mock<ILogger<UserLoginCommandHandler>> _loggerMock;
    private readonly Mock<ICheckpointRepository<UserEntity>> _checkpointRepositoryMock;

    public UserLoginCommandHandlerTests()
    {
        _UserUpdateConfigMock = new Mock<IOptions<UserUpdateConfiguration>>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _daprServiceMock = new Mock<IDaprService>();
        _checkpointRepositoryMock = new Mock<ICheckpointRepository<UserEntity>>();
        _userServiceMock = new Mock<IUserService>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<UserLoginCommand>>();
        _loggerMock = new Mock<ILogger<UserLoginCommandHandler>>();

        _UserUpdateConfigMock
            .Setup(x => x.Value)
            .Returns(new UserUpdateConfiguration());
    }

    [Fact]
    public async void UserLoginCommand_ShouldSucceed()
    {
        // arrange
        var cmd = UserFaker.GetUserLoginCommand();
        var item = UserFaker.GetUserRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _checkpointRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .Returns(Task.FromResult(new UserEntity()));
        _eventRepositoryMock
       .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
       .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<UserRecord>(It.IsAny<UserEntity>()))
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
        _checkpointRepositoryMock
            .Verify(x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.Once);
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<UserRecord>(It.IsAny<UserEntity>()),
            Times.Once);
    }

    [Fact]
    public async void UserLoginCommand_WhenInvalid_ShouldFail()
    {
        // arrange
        var cmd = UserFaker.GetUserLoginCommand();
        var item = UserFaker.GetUserRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult(new[] { new ValidationFailure() }));
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _checkpointRepositoryMock
            .Verify(x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<UserRecord>(It.IsAny<UserEntity>()),
            Times.Never);
    }

    [Fact]
    public async void UserLoginCommand_WhenAppendEventsFail_ShouldFail()
    {
        // arrange
        var cmd = UserFaker.GetUserLoginCommand();
        var item = UserFaker.GetUserRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _checkpointRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .Returns(Task.FromResult(new UserEntity()));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(false));
        _mapperMock
            .Setup(x => x.Map<UserRecord>(It.IsAny<UserEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _checkpointRepositoryMock
            .Verify(x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.Once);
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.AtLeastOnce());
        _mapperMock.Verify(
            x => x.Map<UserRecord>(It.IsAny<UserEntity>()),
            Times.Never);
    }

    [Fact]
    public async void UserLoginCommand_WhenException_ShouldFail()
    {
        // arrange
        var cmd = UserFaker.GetUserLoginCommand();
        var item = UserFaker.GetUserRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _checkpointRepositoryMock
          .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
          .Returns(Task.FromResult(new UserEntity()));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<UserRecord>(It.IsAny<UserEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _checkpointRepositoryMock
            .Verify(x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.Once);
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.AtLeastOnce());
        _mapperMock.Verify(
            x => x.Map<UserRecord>(It.IsAny<UserEntity>()),
            Times.Never);
    }

    private UserLoginCommandHandler GetCommandHandler() =>
        new UserLoginCommandHandler(
            _UserUpdateConfigMock.Object,
            _eventRepositoryMock.Object,
            _checkpointRepositoryMock.Object,
            _daprServiceMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}