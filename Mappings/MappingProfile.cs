using AutoMapper;
using BlogApi.DTOs;
using BlogApi.Models;
using Microsoft.Data.SqlClient;

namespace BlogApi.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User maps
        CreateMap<RegisterDto, User>();
        CreateMap<User, AuthResponseDto>();

        // Post maps
        CreateMap<CreatePostDto, Post>();
        CreateMap<Post, PostResponseDto>()
            .ForMember(dest => dest.AuthorUsername, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count));

        CreateMap<CreateCommentDto, Comment>();
        CreateMap<Comment, CommentResponseDto>()
            .ForMember(dest => dest.AuthorUsername, opt => opt.MapFrom(src => src.User.Username));
    }
}
