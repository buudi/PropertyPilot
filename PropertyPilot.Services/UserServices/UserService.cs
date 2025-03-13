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

        var userResponses = new List<UserResponse>();

        foreach (var user in users)
        {
            var monetaryAccountName = await pmsDbContext
                .MonetaryAccounts
                .Where(x => x.UserId == user.Id)
                .Select(x => x.AccountName)
                .FirstOrDefaultAsync();

            var userResponse = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                HasAccess = user.HasAccess,
                MonetaryAccountName = monetaryAccountName!,
                LastLogin = user.LastLogin,
                CreatedOn = user.CreatedOn
            };

            userResponses.Add(userResponse);
        }

        return new PaginatedResult<UserResponse>
        {
            Items = userResponses,
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

    public async Task UpdateUser(Guid id, UpdateUserRequest request)
    {
        var userToUpdate = await pmsDbContext.PropertyPilotUsers
            .FirstOrDefaultAsync(x => x.Id == id);

        if (userToUpdate == null) return;

        var monetaryAccount = await pmsDbContext.MonetaryAccounts
            .FirstOrDefaultAsync(x => x.UserId == userToUpdate.Id);

        if (monetaryAccount == null) return;

        userToUpdate.Name = request.Name;
        userToUpdate.Email = request.Email;
        userToUpdate.HasAccess = request.HasAccess;

        monetaryAccount.AccountName = request.MonetaryAccountName;

        // Update caretaker properties if the user is a caretaker and caretaker properties are provided
        if (userToUpdate.Role == PropertyPilotUser.UserRoles.Caretaker && request.CaretakerProperties != null)
        {
            // Remove all previously assigned caretaker properties
            var assignedProperties = await pmsDbContext.AssignedCaretakerProperties
                .Where(x => x.UserId == userToUpdate.Id)
                .ToListAsync();
            pmsDbContext.AssignedCaretakerProperties.RemoveRange(assignedProperties);

            // Add new caretaker properties from the request
            foreach (var caretakerProperty in request.CaretakerProperties)
            {
                var newAssignedProperty = new AssignedCaretakerProperty
                {
                    // You can also use caretakerProperty.UserId if that is needed,
                    // but here we ensure that the assignment is for the current user
                    UserId = userToUpdate.Id,
                    PropertyListingId = caretakerProperty.PropertyId
                };

                await pmsDbContext.AssignedCaretakerProperties.AddAsync(newAssignedProperty);
            }
        }

        await pmsDbContext.SaveChangesAsync();
    }

}
