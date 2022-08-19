using Bogus;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Domain.Events.Auth;
using Pondrop.Service.Auth.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pondrop.Service.Auth.Tests.Faker;

public static class AuthFaker
{
    private static readonly string[] Names = new[] { "The Local", "The Far Away", "The Just Right", "Test" };
    private static readonly string[] Statues = new[] { "Online", "Offline", "Unknown", };
    private static readonly string[] AddressLine1 = new[] { "123 Street", "123 Lane", "123 Court", };
    private static readonly string[] AddressLine2 = new[] { "" };
    private static readonly string[] Suburbs = new[] { "Lakes", "Rivers", "Seaside" };
    private static readonly string[] States = new[] { "WA", "NT", "SA", "QLD", "NSW", "ACT", "VIC", "TAS" };
    private static readonly string[] Postcodes = new[] { "6000", "5000", "4000", "2000", "3000", "7000" };
    private static readonly string[] Countries = new[] { "Australia" };
    private static readonly double[] Lats = new[] { 25.6091 };
    private static readonly double[] Lngs = new[] { 134.3619 };
    private static readonly string[] UserNames = new[] { "test/faker", "test/jsmith", "test/bsmith", "test/asmith", "test/account" };
    
    public static List<AuthRecord> GetAuthRecords(int count = 5)
    {
        var faker = new Faker<AuthRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.Addresses, f => GetAuthAddressRecords(1))
            .RuleFor(x => x.Status, f => f.PickRandom(Statues))
            .RuleFor(x => x.RetailerId, f => Guid.NewGuid())
            .RuleFor(x => x.AuthTypeId, f => Guid.NewGuid())
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }
    
    public static List<AuthViewRecord> GetAuthViewRecords(int count = 5)
    {
        var retailer = RetailerFaker.GetRetailerRecords(1).Single();
        var storeType = AuthTypeFaker.GetAuthTypeRecords(1).Single();
        
        var faker = new Faker<AuthViewRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.Addresses, f => GetAuthAddressRecords(1))
            .RuleFor(x => x.Status, f => f.PickRandom(Statues))
            .RuleFor(x => x.RetailerId, f => retailer.Id)
            .RuleFor(x => x.Retailer, f => retailer)
            .RuleFor(x => x.AuthTypeId, f => storeType.Id)
            .RuleFor(x => x.AuthType, f => storeType)
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }
    
    public static List<AuthAddressRecord> GetAuthAddressRecords(int count = 1)
    {
        var faker = new Faker<AuthAddressRecord>()
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.AddressLine1, f => f.PickRandom(AddressLine1))
            .RuleFor(x => x.AddressLine2, f => f.PickRandom(AddressLine2))
            .RuleFor(x => x.Suburb, f => f.PickRandom(Suburbs))
            .RuleFor(x => x.State, f => f.PickRandom(States))
            .RuleFor(x => x.Postcode, f => f.PickRandom(Postcodes))
            .RuleFor(x => x.Country, f => f.PickRandom(Countries))
            .RuleFor(x => x.Latitude, f => f.PickRandom(Lats))
            .RuleFor(x => x.Longitude, f => f.PickRandom(Lngs))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }

    public static CreateAuthCommand GetCreateAuthCommand()
    {
        var faker = new Faker<CreateAuthCommand>()
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.Address, f => GetAddressRecord())
            .RuleFor(x => x.Status, f => f.PickRandom(Statues))
            .RuleFor(x => x.RetailerId, f => Guid.NewGuid())
            .RuleFor(x => x.AuthTypeId, f => Guid.NewGuid());

        return faker.Generate();
    }
    
    public static UpdateAuthCommand GetUpdateAuthCommand()
    {
        var faker = new Faker<UpdateAuthCommand>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.Status, f => f.PickRandom(Statues))
            .RuleFor(x => x.RetailerId, f => Guid.NewGuid())
            .RuleFor(x => x.AuthTypeId, f => Guid.NewGuid());

        return faker.Generate();
    }
    
    public static AddAddressToAuthCommand GetAddAddressToAuthCommand()
    {
        var faker = new Faker<AddAddressToAuthCommand>()
            .RuleFor(x => x.AuthId, f => Guid.NewGuid())
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.AddressLine1, f => f.PickRandom(AddressLine1))
            .RuleFor(x => x.AddressLine2, f => f.PickRandom(AddressLine2))
            .RuleFor(x => x.Suburb, f => f.PickRandom(Suburbs))
            .RuleFor(x => x.State, f => f.PickRandom(States))
            .RuleFor(x => x.Postcode, f => f.PickRandom(Postcodes))
            .RuleFor(x => x.Country, f => f.PickRandom(Countries))
            .RuleFor(x => x.Latitude, f => f.PickRandom(Lats))
            .RuleFor(x => x.Longitude, f => f.PickRandom(Lngs));

        return faker.Generate();
    }
    
    public static UpdateAuthAddressCommand GetUpdateAuthAddressCommand()
    {
        var faker = new Faker<UpdateAuthAddressCommand>()
            .RuleFor(x => x.AuthId, f => Guid.NewGuid())
            .RuleFor(x => x.AddressLine1, f => f.PickRandom(AddressLine1))
            .RuleFor(x => x.AddressLine2, f => f.PickRandom(AddressLine2))
            .RuleFor(x => x.Suburb, f => f.PickRandom(Suburbs))
            .RuleFor(x => x.State, f => f.PickRandom(States))
            .RuleFor(x => x.Postcode, f => f.PickRandom(Postcodes))
            .RuleFor(x => x.Country, f => f.PickRandom(Countries))
            .RuleFor(x => x.Latitude, f => f.PickRandom(Lats))
            .RuleFor(x => x.Longitude, f => f.PickRandom(Lngs));

        return faker.Generate();
    }
    
    public static AuthRecord GetAuthRecord(CreateAuthCommand command)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<AuthRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => command.Name)
            .RuleFor(x => x.ExternalReferenceId, f => command.ExternalReferenceId)
            .RuleFor(x => x.Addresses, f => command.Address is not null
                ? new List<AuthAddressRecord>(1) { GetAuthAddressRecord(command.Address!) }
                : GetAuthAddressRecords(1))
            .RuleFor(x => x.Status, f => command.Status)
            .RuleFor(x => x.RetailerId, f => command.RetailerId)
            .RuleFor(x => x.AuthTypeId, f => command.AuthTypeId)
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
    
    public static AuthRecord GetAuthRecord(UpdateAuthCommand command)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<AuthRecord>()
            .RuleFor(x => x.Id, f => command.Id)
            .RuleFor(x => x.Name, f => command.Name ?? f.PickRandom(Names))
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.Addresses, f => GetAuthAddressRecords(1))
            .RuleFor(x => x.Status, f => command.Status ?? f.PickRandom(Statues))
            .RuleFor(x => x.RetailerId, f => command.RetailerId ?? Guid.NewGuid())
            .RuleFor(x => x.AuthTypeId, f => command.AuthTypeId ?? Guid.NewGuid())
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
    
    private static AuthAddressRecord GetAuthAddressRecord(AddressRecord record)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<AuthAddressRecord>()
            .RuleFor(x => x.AddressLine1, f => record.AddressLine1)
            .RuleFor(x => x.AddressLine2, f => record.AddressLine2)
            .RuleFor(x => x.Suburb, f => record.Suburb)
            .RuleFor(x => x.State, f => record.State)
            .RuleFor(x => x.Postcode, f => record.Postcode)
            .RuleFor(x => x.Country, f => record.Country)
            .RuleFor(x => x.Latitude, f => record.Latitude)
            .RuleFor(x => x.Longitude, f => record.Longitude)
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
    
    private static AddressRecord GetAddressRecord()
    {
        var faker = new Faker<AddressRecord>()
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.AddressLine1, f => f.PickRandom(AddressLine1))
            .RuleFor(x => x.AddressLine2, f => f.PickRandom(AddressLine2))
            .RuleFor(x => x.Suburb, f => f.PickRandom(Suburbs))
            .RuleFor(x => x.State, f => f.PickRandom(States))
            .RuleFor(x => x.Postcode, f => f.PickRandom(Postcodes))
            .RuleFor(x => x.Country, f => f.PickRandom(Countries))
            .RuleFor(x => x.Latitude, f => f.PickRandom(Lats))
            .RuleFor(x => x.Longitude, f => f.PickRandom(Lngs));

        return faker.Generate();
    }
}