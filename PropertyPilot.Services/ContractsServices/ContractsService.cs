using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;

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

}
