using Bogus;
using Pondrop.Service.Auth.Application.Commands;
using Pondrop.Service.Auth.Domain.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Pondrop.Service.Auth.Tests.Faker;

public static class UserFaker
{
    private static readonly string[] Emails = new[] { "test@test.com", "test2@test.com", "test3@test.com" };

    public static CreateUserCommand GetCreateUserCommand()
    {
        var faker = new Faker<CreateUserCommand>()
            .RuleFor(x => x.Email, f => f.PickRandom(Emails));

        return faker.Generate();
    }

    public static UserLoginCommand GetUserLoginCommand()
    {
        var faker = new Faker<UserLoginCommand>()
            .RuleFor(x => x.Id, f => f.Random.Guid());

        return faker.Generate();
    }

    public static UserLogoutCommand GetUserLogoutCommand()
    {
        var faker = new Faker<UserLogoutCommand>()
            .RuleFor(x => x.Id, f => f.Random.Guid());

        return faker.Generate();
    }

    public static UserViewRecord GetUserViewRecord(Guid? id = null, string email = "")
    {
        var faker = new Faker<UserViewRecord>()
            .RuleFor(x => x.Id, f => id is not null ? id : Guid.NewGuid())
            .RuleFor(x => x.Email, f => !string.IsNullOrEmpty(email) ? email : f.PickRandom(Emails));

        return faker.Generate();
    }

    public static UserRecord GetUserRecord(CreateUserCommand cmd)
    {
        var faker = new Faker<UserRecord>()
            .RuleFor(x => x.Email, f => cmd.Email);

        return faker.Generate();
    }

    public static UserRecord GetUserRecord(UserLoginCommand cmd)
    {
        var faker = new Faker<UserRecord>()
            .RuleFor(x => x.Id, f => cmd.Id)
            .RuleFor(x => x.Email, f => f.PickRandom(Emails));

        return faker.Generate();
    }

    public static UserRecord GetUserRecord(UserLogoutCommand cmd)
    {
        var faker = new Faker<UserRecord>()
            .RuleFor(x => x.Id, f => cmd.Id)
            .RuleFor(x => x.Email, f => f.PickRandom(Emails));

        return faker.Generate();
    }
}