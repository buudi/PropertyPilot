using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.UserServices.Models;

namespace PropertyPilot.Services.UserServices;

public class UserService(PmsDbContext pmsDbContext)
{
    public PropertyPilotUser CreateUser(CreateUserRequest request)
    {
        var newUser = new PropertyPilotUser
        {
            Name = request.Name,
            Email = request.Email,
            Role = request.Role,
            HashedPassword = request.HashedPassword
        };

        var user = pmsDbContext.PropertyPilotUsers.Add(newUser);

        return user.Entity;
    }
}
