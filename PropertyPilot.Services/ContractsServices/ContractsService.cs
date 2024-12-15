using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.ContractsServices.Models;

namespace PropertyPilot.Services.ContractsServices;

public class ContractsService(PpDbContext ppDbContext)
{
    public async Task<List<Contract>> GetAllContracts()
    {
        List<Contract> contracts = await ppDbContext
            .Contracts
            .AsNoTracking()
            .ToListAsync();

        return contracts;
    }

    public async Task<Contract?> GetContractById(Guid id)
    {
        Contract? contract = await ppDbContext
            .Contracts
            .AsNoTracking()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        return contract;
    }

    public async Task<Contract?> CreateContractAsync(CreateContractRequest createContractRequest)
    {
        var newContract = new Contract
        {
            TenantId = createContractRequest.TenantId,
            PropertyId = createContractRequest.PropertyId,
            StartDate = createContractRequest.StartDate,
            EndDate = createContractRequest.EndDate,
            Rent = createContractRequest.Rent,
            Notes = createContractRequest.Notes,
            Active = true,
            Renewable = false,
            MoveOut = false
        };

        ppDbContext.Contracts.Add(newContract);
        await ppDbContext.SaveChangesAsync();

        return newContract;
    }

    public async Task<List<Contract>> CreateContractAsync(List<CreateContractRequest> createContractRequests)
    {
        if (createContractRequests == null || !createContractRequests.Any())
        {
            throw new ArgumentException("The list of contract requests cannot be null or empty.", nameof(createContractRequests));
        }

        var newContracts = createContractRequests.Select(request => new Contract
        {
            TenantId = request.TenantId,
            PropertyId = request.PropertyId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Rent = request.Rent,
            Notes = request.Notes,
            Active = true,
            Renewable = false,
            MoveOut = false
        }).ToList();

        ppDbContext.Contracts.AddRange(newContracts);

        await ppDbContext.SaveChangesAsync();

        return newContracts;
    }
}
