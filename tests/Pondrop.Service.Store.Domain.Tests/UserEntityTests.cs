using Pondrop.Service.Auth.Domain.Events.User;
using Pondrop.Service.Auth.Domain.Models;
using System;
using Xunit;

namespace Pondrop.Service.Auth.Domain.Tests;

public class UserEntityTests
{
    private const string Email = "test@test.com";
    private const string NormalizedEmail = "TEST@TEST.COM";
    private DateTime LastLoginDateTime = new DateTime(2022, 2, 1, 1, 1, 1);
    private DateTime LastLogoutDateTime = new DateTime(2022, 1, 1, 1, 1, 1);
    private const string UpdatedBy = "user/admin1";

    [Fact]
    public void User_Ctor_ShouldCreateEmpty()
    {
        // arrange

        // act
        var entity = new UserEntity();

        // assert
        Assert.NotNull(entity);
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(0, entity.EventsCount);
    }

    [Fact]
    public void User_Ctor_ShouldCreateEvent()
    {
        // arrange

        // act
        var entity = GetNewUser();

        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(Email, entity.Email);
        Assert.Equal(NormalizedEmail, entity.NormalizedEmail);
        Assert.NotNull(entity.LastLogin);
        Assert.Null(entity.LastLogout);
        Assert.Equal(1, entity.EventsCount);
    }

    [Fact]
    public void User_UserLogin_ShouldUpdate()
    {
        // arrange
        var userLoginEvent = new UserLogin(LastLoginDateTime);
        var entity = GetNewUser();

        // act
        entity.Apply(userLoginEvent, UpdatedBy);

        // assert
        Assert.NotNull(entity);
        Assert.Equal(LastLoginDateTime, userLoginEvent.LastLoginDateTime);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }

    [Fact]
    public void User_UserLogout_ShouldUpdate()
    {
        // arrange
        var userLogoutEvent = new UserLogout(LastLogoutDateTime);
        var entity = GetNewUser();

        // act
        entity.Apply(userLogoutEvent, UpdatedBy);

        // assert
        Assert.NotNull(entity);
        Assert.Equal(LastLogoutDateTime, userLogoutEvent.LastLogoutDateTime);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }


    private UserEntity GetNewUser() => new UserEntity(Email);
}