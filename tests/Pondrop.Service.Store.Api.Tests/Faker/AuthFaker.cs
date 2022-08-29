using Bogus;
using Pondrop.Service.Auth.Api.Models;
using Pondrop.Service.Auth.Domain.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Pondrop.Service.Auth.Tests.Faker;

public static class AuthFaker
{
    private static readonly string[] Emails = new[] { "test@test.com", "test2@test.com", "test3@test.com" };

    public static ShopperSigninRequest GetSigninRequest()
    {
        var faker = new Faker<ShopperSigninRequest>()
            .RuleFor(x => x.Email, f => f.PickRandom(Emails));

        return faker.Generate();
    }

    public static UserViewRecord GetUserViewRecord(Guid? id = null, string email = "")
    {
        var faker = new Faker<UserViewRecord>()
            .RuleFor(x => x.Id, f => id is not null ? id : Guid.NewGuid())
            .RuleFor(x => x.Email, f => !string.IsNullOrEmpty(email) ? email : f.PickRandom(Emails));

        return faker.Generate();
    }

    public static UserRecord GetUserRecord(Guid? id = null, string email = "")
    {
        var faker = new Faker<UserRecord>()
            .RuleFor(x => x.Id, f => id is not null ? id : Guid.NewGuid())
            .RuleFor(x => x.Email, f => !string.IsNullOrEmpty(email) ? email : f.PickRandom(Emails));

        return faker.Generate();
    }

    public static ClaimsPrincipal GetClaimsPrincipal(Guid? id = null, string email = "")
    {
        var claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Sub, id?.ToString() ?? string.Empty),
            new Claim(ClaimTypes.Email, email),
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }
}