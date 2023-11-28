using AuthenticationService.Dtos;
using AuthenticationService.Models;
using AutoMapper;

namespace AuthenticationService.Profiles;

public class UsersProfile:Profile
{
    public UsersProfile()
    {
        CreateMap<UserSignupDto, User>();
        CreateMap<User, UserReadDto>();
        CreateMap<UserLoginDto, User>();
    }
}