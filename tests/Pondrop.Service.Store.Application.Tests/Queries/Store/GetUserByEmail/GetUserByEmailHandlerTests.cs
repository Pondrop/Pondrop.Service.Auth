using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Application.Interfaces;
using Pondrop.Service.Auth.Domain.Events;
using Pondrop.Service.Auth.Domain.Models;
using Pondrop.Service.Auth.Tests.Faker;
using Pondrop.Service.User.Application.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Auth.Application.Tests.Commands.User.GetUserByEmail;

public class GetUserByEmailHandlerTests
{
    private readonly Mock<ICheckpointRepository<UserEntity>> _viewRepositoryMock;
    private readonly Mock<IValidator<GetUserByEmailQuery>> _validatorMock;
    private readonly Mock<ILogger<GetUserByEmailQueryHandler>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly string _email = "test@test.com";

    public GetUserByEmailHandlerTests()
    {
        _viewRepositoryMock = new Mock<ICheckpointRepository<UserEntity>>();
        _validatorMock = new Mock<IValidator<GetUserByEmailQuery>>();
        _loggerMock = new Mock<ILogger<GetUserByEmailQueryHandler>>();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async void GetUserByEmailQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetUserByEmailQuery() { Email = _email };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _viewRepositoryMock
            .Setup(x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.FromResult(new List<UserEntity?>()));
        var handler = GetQueryHandler();

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        Assert.True(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _viewRepositoryMock.Verify(
            x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
            Times.Once());
    }

    [Fact]
    public async void GetUserByEmailQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetUserByEmailQuery() { Email = _email };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new[] { new ValidationFailure() }));
        _viewRepositoryMock
          .Setup(x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
          .Returns(Task.FromResult(new List<UserEntity?>()));
        var handler = GetQueryHandler();

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _viewRepositoryMock.Verify(
            x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never());
    }

    [Fact]
    public async void GetUserByEmailQuery_WhenNotFound_ShouldSucceedWithNull()
    {
        // arrange
        var query = new GetUserByEmailQuery() { Email = _email };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _viewRepositoryMock
        .Setup(x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
        .Returns(Task.FromResult(new List<UserEntity?>()));
        var handler = GetQueryHandler();

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _viewRepositoryMock.Verify(
             x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
             Times.Once());
    }

    [Fact]
    public async void GetUserByEmailQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetUserByEmailQuery() { Email = _email };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _viewRepositoryMock
       .Setup(x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
           .Throws(new Exception());
        var handler = GetQueryHandler();

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _viewRepositoryMock.Verify(
              x => x.QueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
              Times.Once());
    }

    private GetUserByEmailQueryHandler GetQueryHandler() =>
        new GetUserByEmailQueryHandler(
            _viewRepositoryMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}