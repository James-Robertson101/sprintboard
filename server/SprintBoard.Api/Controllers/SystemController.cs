using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SprintBoard.Api.Data;

namespace SprintBoard.Api.Controllers;

[ApiController]
[Route("api/system")]
public class SystemController : ControllerBase
{
    private readonly AppDbContext _db;

    public SystemController(AppDbContext db)
    {
        _db = db;
    }

    [AllowAnonymous]
    [HttpPost("reseed")]
    public async Task<IActionResult> ReseedIfDue()
    {
        await DataSeeder.SeedIfDueAsync(_db);
        return NoContent();
    }
}