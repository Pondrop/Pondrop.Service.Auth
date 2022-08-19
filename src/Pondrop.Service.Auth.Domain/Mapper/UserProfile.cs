using AutoMapper;
using Pondrop.Service.Auth.Domain.Models;

namespace Pondrop.Service.Auth.Domain.Mapper;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserEntity, UserRecord>();
        CreateMap<UserEntity, UserViewRecord>();
    }
}
