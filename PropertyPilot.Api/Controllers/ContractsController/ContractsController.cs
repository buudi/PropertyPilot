using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.ContractsServices;

namespace PropertyPilot.Api.Controllers.ContractsController;

/// <summary>
/// 
/// </summary>
[Route("api/contracts")]
[ApiController]
public class ContractsController(ContractsService contractsService) : ControllerBase
{

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<List<Contract>> GetAllContracts()
    {
        var contracts = await contractsService.GetAllContracts();
        return contracts;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Contract>> GetContractById(Guid id)
    {
        var contract = await contractsService.GetContractById(id);

        if (contract == null)
        {
            return NotFound();
        }

        return Ok(contract);
    }
}
