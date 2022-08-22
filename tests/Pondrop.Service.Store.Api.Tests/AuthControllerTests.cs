using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Moq;
using Pondrop.Service.Auth.Api.Models;
using Pondrop.Service.Auth.Api.Services.Interfaces;
using Pondrop.Service.Auth.ApiControllers;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Application.Interfaces;
using Pondrop.Service.Auth.Application.Models;
using Pondrop.Service.Auth.Domain.Models;
using Pondrop.Service.Auth.Tests.Faker;
using Pondrop.Service.User.Application.Queries;
using System;
using System.Security.Claims;
using System.Threading;
using Xunit;

namespace Pondrop.Service.Auth.Api.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IJWTTokenProvider> _jwtTokenProviderMock;
        private readonly Mock<IServiceBusService> _serviceBusServiceMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;

        public AuthControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _jwtTokenProviderMock = new Mock<IJWTTokenProvider>();
            _serviceBusServiceMock = new Mock<IServiceBusService>();
            _loggerMock = new Mock<ILogger<AuthController>>();
        }

        private AuthController GetController() =>
         new AuthController(
             _mediatorMock.Object,
             _jwtTokenProviderMock.Object,
             _serviceBusServiceMock.Object,
             _loggerMock.Object
         );

        [Fact]
        public async void ShopperSignin_WhenUserIsExisting_ShouldReturnOkResultWithAccessToken()
        {
            // arrange
            var accessToken = "Test";
            var userId = Guid.NewGuid();
            var signinRequest = AuthFaker.GetSigninRequest();
            var userViewRecord = AuthFaker.GetUserViewRecord(userId, signinRequest.Email);
            var userRecord = AuthFaker.GetUserRecord(userId, signinRequest.Email);


            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<UserViewRecord>.Success(userViewRecord));

            _mediatorMock
            .Setup(x => x.Send(It.IsAny<UserLoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserRecord>.Success(userRecord));

            _jwtTokenProviderMock
              .Setup(x => x.AuthenticateShopper(It.Is<TokenRequest>(x => x.Email == userRecord.Email && x.Id == userRecord.Id)))
              .Returns(accessToken);

            var controller = GetController();

            // act
            var response = await controller.ShopperSignin(signinRequest);

            // assert
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, StatusCodes.Status200OK);
            var signupResponse = ((ObjectResult)response).Value as SigninResponse;
            Assert.Equal(signupResponse.AccessToken, accessToken);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()), Times.Once());
            _mediatorMock.Verify(x => x.Send(It.IsAny<UserLoginCommand>(), It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == userViewRecord.Id)));
        }


        [Fact]
        public async void ShopperSignin_WhenUserIsNotExisting_ShouldCreateUserAndReturnOkResultWithAccessToken()
        {
            // arrange
            var accessToken = "Test";
            var userId = Guid.NewGuid();
            var signinRequest = AuthFaker.GetSigninRequest();
            var userViewRecord = AuthFaker.GetUserViewRecord(userId, signinRequest.Email);
            var userRecord = AuthFaker.GetUserRecord(userId, signinRequest.Email);


            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<UserViewRecord>.Success(null));

            _mediatorMock
              .Setup(x => x.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(Result<UserRecord>.Success(userRecord));

            _jwtTokenProviderMock
              .Setup(x => x.AuthenticateShopper(It.Is<TokenRequest>(x => x.Email == userRecord.Email && x.Id == userRecord.Id)))
              .Returns(accessToken);

            var controller = GetController();

            // act
            var response = await controller.ShopperSignin(signinRequest);

            // assert
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, StatusCodes.Status200OK);
            var signupResponse = ((ObjectResult)response).Value as SigninResponse;
            Assert.Equal(signupResponse.AccessToken, accessToken);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()), Times.Once());
            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == userViewRecord.Id)));
        }

        [Fact]
        public async void ShopperSignin_WhenEmailIsNotProvided_ShouldReturnBadRequestResult()
        {
            // arrange
            var accessToken = "Test";
            var userId = Guid.NewGuid();
            var signinRequest = new SigninRequest() { Email = string.Empty };
            var userViewRecord = AuthFaker.GetUserViewRecord(userId, signinRequest.Email);
            var userRecord = AuthFaker.GetUserRecord(userId, signinRequest.Email);

            var controller = GetController();

            // act
            var response = await controller.ShopperSignin(signinRequest);

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()), Times.Never());
            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()), Times.Never());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == userViewRecord.Id)), Times.Never());
        }

        [Fact]
        public async void ShopperSignout_WhenAccessTokenIsProvided_ShouldReturnOKResult()
        {
            // arrange
            var userId = Guid.NewGuid();
            var email = "test@test.com";
            var accessToken = "Test";
            var userViewRecord = AuthFaker.GetUserViewRecord(userId, email);
            var userRecord = AuthFaker.GetUserRecord(userId, email);
            var claimsPrincipal = AuthFaker.GetClaimsPrincipal(userId, email);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderNames.Authorization] = $"Bearer {accessToken}";

            _jwtTokenProviderMock
              .Setup(x => x.ValidateToken(It.IsAny<string>()))
              .Returns(claimsPrincipal);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<UserViewRecord>.Success(userViewRecord));

            _mediatorMock
            .Setup(x => x.Send(It.IsAny<UserLogoutCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserRecord>.Success(userRecord));

            var controller = GetController();

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // act
            var response = await controller.ShopperSignout();

            // assert
            Assert.IsType<StatusCodeResult>(response);
            Assert.Equal(((StatusCodeResult)response).StatusCode, StatusCodes.Status200OK);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()), Times.Once());
            _mediatorMock.Verify(x => x.Send(It.IsAny<UserLogoutCommand>(), It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == userViewRecord.Id)));
        }

        [Fact]
        public async void ShopperSignout_WhenUserDoesNotExist_ShouldReturnBadRequestResult()
        {
            // arrange
            var userId = Guid.NewGuid();
            var email = "test@test.com";
            var accessToken = "Test";
            var userViewRecord = AuthFaker.GetUserViewRecord(userId, email);
            var userRecord = AuthFaker.GetUserRecord(userId, email);
            var claimsPrincipal = AuthFaker.GetClaimsPrincipal(userId, email);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderNames.Authorization] = $"Bearer {accessToken}";

            _jwtTokenProviderMock
              .Setup(x => x.ValidateToken(It.IsAny<string>()))
              .Returns(claimsPrincipal);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<UserViewRecord>.Success(null));

            _mediatorMock
            .Setup(x => x.Send(It.IsAny<UserLogoutCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserRecord>.Success(userRecord));

            var controller = GetController();

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // act
            var response = await controller.ShopperSignout();

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((BadRequestObjectResult)response).StatusCode, StatusCodes.Status400BadRequest);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()), Times.Once());
            _mediatorMock.Verify(x => x.Send(It.IsAny<UserLogoutCommand>(), It.IsAny<CancellationToken>()), Times.Never());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == userViewRecord.Id)), Times.Never());
        }

        [Fact]
        public async void ShopperSignout_WhenAccessTokenIsNotProvided_ShouldReturnBadRequestResult()
        {
            // arrange
            var userId = Guid.NewGuid();
            var email = "test@test.com";
            var accessToken = "Test";
            var userViewRecord = AuthFaker.GetUserViewRecord(userId, email);
            var userRecord = AuthFaker.GetUserRecord(userId, email);
            var claimsPrincipal = AuthFaker.GetClaimsPrincipal(userId, email);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderNames.Authorization] = string.Empty;

            _jwtTokenProviderMock
              .Setup(x => x.ValidateToken(string.Empty))
              .Returns<ClaimsPrincipal>(null);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<UserViewRecord>.Success(null));

            _mediatorMock
            .Setup(x => x.Send(It.IsAny<UserLogoutCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserRecord>.Success(userRecord));

            var controller = GetController();

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // act
            var response = await controller.ShopperSignout();

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((BadRequestObjectResult)response).StatusCode, StatusCodes.Status400BadRequest);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()), Times.Never());
            _mediatorMock.Verify(x => x.Send(It.IsAny<UserLogoutCommand>(), It.IsAny<CancellationToken>()), Times.Never());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == userViewRecord.Id)), Times.Never());
        }
    }
}
