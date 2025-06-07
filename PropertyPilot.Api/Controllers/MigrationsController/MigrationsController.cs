using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Services.MigrationServices;

namespace PropertyPilot.Api.Controllers.MigrationsController;

/// <summary>
/// Migrations Controller, CSV Uploads
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MigrationController(MigrationService migrationService) : ControllerBase
{
    /// <summary>
    /// Upload CSV
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is missing.");

        var result = await migrationService.ParseCsvAsync(file);
        return Ok(result);
    }
}