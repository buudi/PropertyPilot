using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.Generics;
using PropertyPilot.Services.UserServices.Models;
using System.Security.Cryptography;
using System.Text;

namespace PropertyPilot.Services.UserServices;

public class UserService(PmsDbContext pmsDbContext)
{

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
    }

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

    public async Task<PropertyPilotUser> CreateUser(CreateUserRequest request)
    {
        var newUser = new PropertyPilotUser
        {
            Name = request.Name,
            Email = request.Email,
            Role = request.Role,
            HasAccess = request.Access,
            HashedPassword = HashPassword(request.Password)
        };

        var user = pmsDbContext.PropertyPilotUsers.Add(newUser);
        await pmsDbContext.SaveChangesAsync();

        var userId = user.Entity.Id;

        var monetaryAccount = new MonetaryAccount
        {
            AccountName = $"{request.Name} (حساب مالي)",
            UserId = userId,
            Balance = 0,
        };

        pmsDbContext.MonetaryAccounts.Add(monetaryAccount);
        await pmsDbContext.SaveChangesAsync();

        return user.Entity;
    }

    public void UpdateUser(Guid id, UpdateUserRequest request)
    {
        var userToUpdate = pmsDbContext.PropertyPilotUsers
            .FirstOrDefault(x => x.Id == id);

        if (userToUpdate == null) return;

        userToUpdate.Name = request.Name;
        userToUpdate.Email = request.Email;
    }
}
