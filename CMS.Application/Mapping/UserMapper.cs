using CMS.Application.Features.Users.DTOs;
using CMS.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace CMS.Application.Mapping;

/// <summary>
/// Source-generated mapper for User entities using Mapperly.
/// </summary>
[Mapper]
public static partial class UserMapper
{
    /// <summary>
    /// Maps a User entity to UserDto.
    /// </summary>
    public static partial UserDto ToUserDto(User user);

    /// <summary>
    /// Maps a User entity to UserListDto.
    /// </summary>
    public static partial UserListDto ToUserListDto(User user);

    /// <summary>
    /// Maps a User entity to CurrentUserDto.
    /// </summary>
    public static CurrentUserDto ToCurrentUserDto(User user)
    {
        return new CurrentUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Role = user.Role,
            CanAccessDashboard = user.CanAccessDashboard(),
            CanAccessDesigner = user.CanAccessDesigner(),
            CanManageUsers = user.CanManageUsers()
        };
    }
}