using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.ContractsServices.Models;
using PropertyPilot.Services.PropertiesServices;
using PropertyPilot.Services.TenantsServices;

namespace PropertyPilot.Services.ContractsServices;

public class ContractsService(PpDbContext ppDbContext, PropertiesService propertiesServices, TenantsService tenantsServices)
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
        var associatedProperty = await propertiesServices.GetPropertyByIdAsync(createContractRequest.PropertyId);

        if (associatedProperty == null)
        {
            return null;
        }

        var associatedTenant = await tenantsServices.GetTenantByIdAsync(createContractRequest.TenantId);

        if (associatedTenant == null)
        {
            return null;
        }

        var newContract = new Contract
        {
            Tenant = associatedTenant,
            Property = associatedProperty,
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
}
