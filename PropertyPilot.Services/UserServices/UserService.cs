using PropertyPilot.Api.Controllers.UsersController.Models;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;

namespace PropertyPilot.Services.UserServices;

public class UserService(PmsDbContext pmsDbContext)
{
    public PropertyPilotUser CreateUser(CreateUserRequest request)
    {
        var user = PmsDbContext
    }
}
