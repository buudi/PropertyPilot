using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.ContractsServices;
using PropertyPilot.Services.ContractsServices.Models;

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Contract>> CreateContract(CreateContractRequest request)
    {
        var newContract = await contractsService.CreateContractAsync(request);

        if (newContract == null)
        {
            return NotFound("Associated Tenant or Property not found");
        }

        return CreatedAtAction(
            nameof(GetContractById),
            new { id = newContract.Id },
            newContract);

    }
}
