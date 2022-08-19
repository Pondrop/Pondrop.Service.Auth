using Pondrop.Service.Auth.Domain.Events.Auth;
using Pondrop.Service.Auth.Domain.Models;
using System;
using System.Linq;
using Xunit;

namespace Pondrop.Service.Auth.Domain.Tests;

public class AuthEntityTests
{
    private const string Name = "My Auth";
    private const string Status = "Online";
    private const string ExternalReferenceId = "dc9145d2-b108-482e-ba6e-a141e2fba16f";
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";
    
    private const string RetailerName = "My Retailer";
    private const string AuthTypeName = "My AuthType";
    
    [Fact]
    public void Auth_Ctor_ShouldCreateEmpty()
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
    public void Auth_Ctor_ShouldCreateEvent()
    {
        // arrange
        
        // act
        var entity = GetNewAuth();
        
        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(Name, entity.Name);
        Assert.Equal(Status, entity.Status);
        Assert.Equal(ExternalReferenceId, entity.ExternalReferenceId);
        Assert.NotEqual(Guid.Empty, entity.RetailerId);
        Assert.NotEqual(Guid.Empty, entity.AuthTypeId);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }
    
    [Fact]
    public void Auth_UpdateAuth_ShouldUpdate()
    {
        // arrange
        var updateEvent = new UpdateAuth("New Name", null, null, null);
        var entity = GetNewAuth();
        
        // act
        entity.Apply(updateEvent, UpdatedBy);
        
        // assert
        Assert.NotNull(entity);
        Assert.Equal(updateEvent.Name, updateEvent.Name);
        Assert.Equal(Status, entity.Status);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }
    
    [Fact]
    public void Auth_AddAuthAddress_ShouldAddAddress()
    {
        // arrange
        var entity = GetNewAuth();
        var addEvent = GetAddAuthAddress(entity.Id);
        
        // act
        entity.Apply(addEvent, UpdatedBy);
        
        // assert
        Assert.NotNull(entity);
        Assert.Equal(addEvent.Id, entity.Addresses.Single().Id);
        Assert.Equal(addEvent.AddressLine1, entity.Addresses.Single().AddressLine1);
        Assert.Equal(addEvent.AddressLine2, entity.Addresses.Single().AddressLine2);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }
    
    [Fact]
    public void Auth_UpdateAuthAddress_ShouldUpdateAddress()
    {
        // arrange
        var entity = GetNewAuth();
        var addEvent = GetAddAuthAddress(entity.Id);
        var updateEvent = new UpdateAuthAddress(
            addEvent.Id,
            entity.Id,
            addEvent.AddressLine1 + " Updated",
            null,
            null,
            null,
            null,
            null,
            null,
            null);
        entity.Apply(addEvent, UpdatedBy);
        
        // act
        entity.Apply(updateEvent, UpdatedBy);
        
        // assert
        Assert.NotNull(entity);
        Assert.Equal(addEvent.Id, entity.Addresses.Single().Id);
        Assert.Equal(updateEvent.AddressLine1, entity.Addresses.Single().AddressLine1);
        Assert.Equal(addEvent.AddressLine2, entity.Addresses.Single().AddressLine2);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(3, entity.EventsCount);
    }
    
    [Fact]
    public void Auth_RemoveAddressFromAuth_ShouldRemoveAddress()
    {
        // arrange
        var entity = GetNewAuth();
        var addEvent = GetAddAuthAddress(entity.Id);
        var removeEvent = new RemoveAddressFromAuth(addEvent.Id, entity.Id);
        entity.Apply(addEvent, UpdatedBy);
        
        // act
        entity.Apply(removeEvent, UpdatedBy);
        
        // assert
        Assert.NotNull(entity);
        Assert.Empty(entity.Addresses);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(3, entity.EventsCount);
    }
    
    private UserEntity GetNewAuth() => new AuthEntity(
        Name,
        Status,
        ExternalReferenceId,
        GetRetailerRecord().Id,
        GetAuthTypeRecord().Id,
        CreatedBy);
    private AddAuthAddress GetAddAuthAddress(Guid storeId) => new AddAuthAddress(
        Guid.NewGuid(), 
        storeId,
        Guid.NewGuid().ToString(), 
        nameof(AddAuthAddress.AddressLine1), 
        nameof(AddAuthAddress.AddressLine2), 
        nameof(AddAuthAddress.Suburb), 
        nameof(AddAuthAddress.State), 
        nameof(AddAuthAddress.Postcode),
        nameof(AddAuthAddress.Country),
        0,
        0);
    private RetailerRecord GetRetailerRecord() => new RetailerRecord(
        Guid.NewGuid(), 
        Guid.NewGuid().ToString(), 
        RetailerName, 
        CreatedBy, 
        UpdatedBy, 
        DateTime.UtcNow.AddDays(-1), 
        DateTime.UtcNow);
    private AuthTypeRecord GetAuthTypeRecord() => new AuthTypeRecord(
        Guid.NewGuid(), 
        Guid.NewGuid().ToString(), 
        AuthTypeName, 
        CreatedBy, 
        UpdatedBy, 
        DateTime.UtcNow.AddDays(-1), 
        DateTime.UtcNow);
}