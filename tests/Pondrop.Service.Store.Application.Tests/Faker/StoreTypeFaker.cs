using Bogus;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pondrop.Service.Auth.Tests.Faker;

public static class AuthTypeFaker
{
    private static readonly string[] Names = new[] { "Supermarket", "Servo", "Kwik-E-Mart", "Test" };
    private static readonly string[] UserNames = new[] { "test/faker", "test/jsmith", "test/bsmith", "test/asmith", "test/account" };
    
    public static List<AuthTypeRecord> GetAuthTypeRecords(int count = 5)
    {
        var faker = new Faker<AuthTypeRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }

    public static CreateAuthTypeCommand GetCreateAuthTypeCommand()
    {
        var faker = new Faker<CreateAuthTypeCommand>()
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString());

        return faker.Generate();
    }
    
    public static UpdateAuthTypeCommand GetUpdateAuthTypeCommand()
    {
        var faker = new Faker<UpdateAuthTypeCommand>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names));

        return faker.Generate();
    }
    
    public static AuthTypeRecord GetAuthTypeRecord(CreateAuthTypeCommand command)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<AuthTypeRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => command.Name)
            .RuleFor(x => x.ExternalReferenceId, f => command.ExternalReferenceId)
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
    
    public static AuthTypeRecord GetAuthTypeRecord(UpdateAuthTypeCommand command)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<AuthTypeRecord>()
            .RuleFor(x => x.Id, f => command.Id)
            .RuleFor(x => x.Name, f => command.Name)
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
}