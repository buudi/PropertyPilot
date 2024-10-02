using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.UserServices.Models;

namespace PropertyPilot.Services.UserServices;

public class UserService(PmsDbContext pmsDbContext)
{
    public async Task<List<PropertyPilotUser>> GetAllUsersAsync()
    {
        var users = await pmsDbContext.PropertyPilotUsers.AsNoTracking().ToListAsync();
        return users;
    }

    public PropertyPilotUser CreateUser(CreateUserRequest request)
    {
        var newUser = new PropertyPilotUser
        {
            Name = request.Name,
            Email = request.Email,
            Role = request.Role,
            HashedPassword = request.Password
        };

        var user = pmsDbContext.PropertyPilotUsers.Add(newUser);
        pmsDbContext.SaveChanges();

        return user.Entity;
    }
}
