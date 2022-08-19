using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Auth.Application.Models;
using Pondrop.Service.Auth.Domain.Models;
using Moq;
using Pondrop.Service.Auth.Api.Services;
using Pondrop.Service.Auth.ApiControllers;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Application.Interfaces;
using Pondrop.Service.Auth.Application.Queries;
using Pondrop.Service.Auth.Tests.Faker;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pondrop.Service.Auth.Api.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IServiceBusService> _serviceBusServiceMock;
        private readonly Mock<IRebuildCheckpointQueueService> _rebuildMaterializeViewQueueServiceMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        
        public AuthControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _serviceBusServiceMock = new Mock<IServiceBusService>();
            _rebuildMaterializeViewQueueServiceMock = new Mock<IRebuildCheckpointQueueService>();
            _loggerMock = new Mock<ILogger<AuthController>>();
        }

        [Fact]
        public async void GetAllAuths_ShouldReturnOkResult()
        {
            // arrange
            var items = AuthFaker.GetAuthViewRecords();
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllAuthsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<AuthViewRecord>>.Success(items));
            var controller = GetController();

            // act
            var response = await controller.GetAllAuths();

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, items);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllAuthsQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetAllAuths_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<List<AuthViewRecord>>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllAuthsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetAllAuths();

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllAuthsQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetAuthById_ShouldReturnOkResult()
        {
            // arrange
            var item = AuthFaker.GetAuthViewRecords(1).Single();
            _mediatorMock
                .Setup(x => x.Send(It.Is<GetAuthByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<AuthViewRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.GetAuthById(item.Id);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(It.Is<GetAuthByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetAuthById_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<AuthViewRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAuthByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.GetAuthById(Guid.NewGuid());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAuthByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void CreateAuthCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = AuthFaker.GetCreateAuthCommand();
            var item = AuthFaker.GetAuthRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<AuthRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.CreateAuth(cmd);
        
            // assert
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, StatusCodes.Status201Created);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void CreateAuthCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<AuthRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateAuthCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.CreateAuth(new CreateAuthCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateAuthCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateAuthCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = AuthFaker.GetUpdateAuthCommand();
            var item = AuthFaker.GetAuthRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<AuthRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.UpdateAuth(cmd);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((OkObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void UpdateAuthCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<AuthRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateAuthCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.UpdateAuth(new UpdateAuthCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateAuthCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void AddAddressToAuthCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = AuthFaker.GetAddAddressToAuthCommand();
            var item = AuthFaker.GetAuthRecords(1).Single() with { Id = cmd.AuthId };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<AuthRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.AddAddressToAuth(cmd);
        
            // assert
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, StatusCodes.Status201Created);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void AddAddressToAuthCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<AuthRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<AddAddressToAuthCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.AddAddressToAuth(new AddAddressToAuthCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<AddAddressToAuthCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void RemoveAddressToAuthCommand_ShouldReturnOkResult()
        {
            // arrange
            var item = AuthFaker.GetAuthRecords(1).Single();
            var cmd = new RemoveAddressFromAuthCommand()
            {
                Id = item.Addresses.First().Id,
                AuthId = item.Id
            };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<AuthRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.RemoveAddressFromAuth(cmd);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((OkObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void RemoveAddressToAuthCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<AuthRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<RemoveAddressFromAuthCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.RemoveAddressFromAuth(new RemoveAddressFromAuthCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<RemoveAddressFromAuthCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateAuthAddressCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = AuthFaker.GetUpdateAuthAddressCommand();
            var item = AuthFaker.GetAuthRecords(1).Single() with { Id = cmd.AuthId };
            item.Addresses[0] = item.Addresses[0] with { Id = cmd.Id };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<AuthRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.UpdateAuthAddressAuth(cmd);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((OkObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void UpdateAuthAddressCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<AuthRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateAuthAddressCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.UpdateAuthAddressAuth(new UpdateAuthAddressCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateAuthAddressCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateCheckpoint_ShouldReturnOkResult()
        {
            // arrange
            var cmd = new UpdateAuthCheckpointByIdCommand() { Id = Guid.NewGuid() };
            var item = AuthFaker.GetAuthRecords(1).Single() with { Id = cmd.Id };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<AuthRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.UpdateCheckpoint(cmd);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateCheckpoint_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<AuthRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateAuthCheckpointByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.UpdateCheckpoint(new UpdateAuthCheckpointByIdCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateAuthCheckpointByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void RebuildCheckpoint_ShouldReturnAcceptedResult()
        {
            // arrange
            var controller = GetController();
        
            // act
            var response = controller.RebuildCheckpoint();
        
            // assert
            Assert.IsType<AcceptedResult>(response);
            _rebuildMaterializeViewQueueServiceMock.Verify(x => x.Queue(It.IsAny<RebuildAuthCheckpointCommand>()), Times.Once());
        }
        
        private AuthController GetController() =>
            new AuthController(
                _mediatorMock.Object,
                _serviceBusServiceMock.Object,
                _rebuildMaterializeViewQueueServiceMock.Object,
                _loggerMock.Object
            );
    }
}
