using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.Generics;
using PropertyPilot.Services.UserServices.Models;

namespace PropertyPilot.Services.UserServices;

public class UserService(PmsDbContext pmsDbContext)
{
    public async Task<PaginatedResult<UserResponse>> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        var totalUsers = await pmsDbContext.PropertyPilotUsers.CountAsync();

        var users = await pmsDbContext.PropertyPilotUsers
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var usersResponses = users.Select(user => new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            HasAccess = user.HasAccess,
            LastLogin = user.LastLogin,
            CreatedOn = user.CreatedOn
        }).ToList();

        return new PaginatedResult<UserResponse>
        {
            Items = usersResponses,
            TotalItems = totalUsers,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize)
        };
    }


    public async Task<PropertyPilotUser?> GetUserById(Guid id)
    {
        return await pmsDbContext.PropertyPilotUsers
            .Where(x => x.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public PropertyPilotUser CreateUser(CreateUserRequest request)
    {
        var newUser = new PropertyPilotUser
        {
            Name = request.Name,
            Email = request.Email,
            Role = request.Role,
            HasAccess = request.Access,
            HashedPassword = request.Password
        };

        var user = pmsDbContext.PropertyPilotUsers.Add(newUser);
        pmsDbContext.SaveChanges();

        return user.Entity;
    }

    public void UpdateUser(Guid id, UpdateUserRequest request)
    {
        var userToUpdate = pmsDbContext.PropertyPilotUsers
            .Where(x => x.Id == id)
            .FirstOrDefault();

        if (userToUpdate != null)
        {
            userToUpdate.Name = request.Name;
            userToUpdate.Email = request.Email;
        }
    }
}
