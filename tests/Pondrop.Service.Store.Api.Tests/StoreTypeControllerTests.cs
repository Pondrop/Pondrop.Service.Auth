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
    public class AuthTypeControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IServiceBusService> _serviceBusServiceMock;
        private readonly Mock<IRebuildCheckpointQueueService> _rebuildMaterializeViewQueueServiceMock;
        private readonly Mock<ILogger<AuthTypeController>> _loggerMock;
        
        public AuthTypeControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _serviceBusServiceMock = new Mock<IServiceBusService>();
            _rebuildMaterializeViewQueueServiceMock = new Mock<IRebuildCheckpointQueueService>();
            _loggerMock = new Mock<ILogger<AuthTypeController>>();
        }

        [Fact]
        public async void GetAllAuthTypes_ShouldReturnOkResult()
        {
            // arrange
            var items = AuthTypeFaker.GetAuthTypeRecords();
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllAuthTypesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<AuthTypeRecord>>.Success(items));
            var controller = GetController();

            // act
            var response = await controller.GetAllAuthTypes();

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, items);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllAuthTypesQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetAllAuthTypes_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<List<AuthTypeRecord>>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllAuthTypesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetAllAuthTypes();

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllAuthTypesQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetAuthTypeById_ShouldReturnOkResult()
        {
            // arrange
            var item = AuthTypeFaker.GetAuthTypeRecords(1).Single();
            _mediatorMock
                .Setup(x => x.Send(It.Is<GetAuthTypeByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<AuthTypeRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.GetAuthTypeById(item.Id);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(It.Is<GetAuthTypeByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetAuthTypeById_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<AuthTypeRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAuthTypeByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.GetAuthTypeById(Guid.NewGuid());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAuthTypeByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void CreateAuthTypeCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = AuthTypeFaker.GetCreateAuthTypeCommand();
            var item = AuthTypeFaker.GetAuthTypeRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<AuthTypeRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.CreateAuthType(cmd);
        
            // assert
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, StatusCodes.Status201Created);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void CreateAuthTypeCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<AuthTypeRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateAuthTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.CreateAuthType(new CreateAuthTypeCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateAuthTypeCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateAuthTypeCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = AuthTypeFaker.GetUpdateAuthTypeCommand();
            var item = AuthTypeFaker.GetAuthTypeRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<AuthTypeRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.UpdateAuthType(cmd);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((OkObjectResult)response).StatusCode, StatusCodes.Status200OK);
            Assert.Equal(((OkObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void UpdateAuthTypeCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<AuthTypeRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateAuthTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.UpdateAuthType(new UpdateAuthTypeCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateAuthTypeCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UUpdateCheckpoint_ShouldReturnOkResult()
        {
            // arrange
            var cmd = new UpdateAuthTypeCheckpointByIdCommand() { Id = Guid.NewGuid() };
            var item = AuthTypeFaker.GetAuthTypeRecords(1).Single() with { Id = cmd.Id };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<AuthTypeRecord>.Success(item));
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
            var failedResult = Result<AuthTypeRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateAuthTypeCheckpointByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.UpdateCheckpoint(new UpdateAuthTypeCheckpointByIdCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateAuthTypeCheckpointByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once());
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
            _rebuildMaterializeViewQueueServiceMock.Verify(x => x.Queue(It.IsAny<RebuildAuthTypeCheckpointCommand>()), Times.Once());
        }

        private AuthTypeController GetController() =>
            new AuthTypeController(
                _mediatorMock.Object,
                _serviceBusServiceMock.Object,
                _rebuildMaterializeViewQueueServiceMock.Object,
                _loggerMock.Object
            );
    }
}
