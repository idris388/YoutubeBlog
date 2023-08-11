using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeBlog.Entity.DTOs.Categories;
using YoutubeBlog.Entity.DTOs.Users;
using YoutubeBlog.Entity.Entities;

namespace YoutubeBlog.Service.AutoMapper.Users
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDto,AppUser>().ReverseMap();
            CreateMap<UserAddDto,AppUser>().ReverseMap();
            CreateMap<UserUpdateDto,AppUser>().ReverseMap();
            CreateMap<UserProfile,AppUser>().ReverseMap();
        }
    }
}
