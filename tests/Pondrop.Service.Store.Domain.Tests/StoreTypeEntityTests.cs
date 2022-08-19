using Pondrop.Service.Auth.Domain.Events.AuthType;
using Pondrop.Service.Auth.Domain.Models;
using System;
using System.Linq;
using Xunit;

namespace Pondrop.Service.Auth.Domain.Tests;

public class AuthTypeEntityTests
{
    private const string Name = "My AuthType";
    private const string ExternalReferenceId = "dc9145d2-b108-482e-ba6e-a141e2fba16f";
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";
    
    [Fact]
    public void AuthType_Ctor_ShouldCreateEmpty()
    {
        // arrange
        
        // act
        var entity = new AuthTypeEntity();
        
        // assert
        Assert.NotNull(entity);
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(0, entity.EventsCount);
    }
    
    [Fact]
    public void AuthType_Ctor_ShouldCreateEvent()
    {
        // arrange
        
        // act
        var entity = GetNewAuthType();
        
        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(Name, entity.Name);
        Assert.Equal(ExternalReferenceId, entity.ExternalReferenceId);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }
    
    [Fact]
    public void AuthType_UpdateAuthType_ShouldUpdate()
    {
        // arrange
        var updateEvent = new UpdateAuthType("New Name");
        var entity = GetNewAuthType();
        
        // act
        entity.Apply(updateEvent, UpdatedBy);
        
        // assert
        Assert.NotNull(entity);
        Assert.Equal(updateEvent.Name, updateEvent.Name);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }
    
    private AuthTypeEntity GetNewAuthType() => new AuthTypeEntity(Name, ExternalReferenceId, CreatedBy);
}