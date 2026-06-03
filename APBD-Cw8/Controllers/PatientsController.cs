using APBD_Cw8.DTOs;
using APBD_Cw8.Exceptions;
using APBD_Cw8.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_Cw8.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController(IPatientService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var patients = await service.GetPatientsAsync(search, cancellationToken);
        return Ok(patients);
    }
    
    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AssignBed(string pesel, [FromBody] AssignBedRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            await service.AssignBedAsync(pesel, request, cancellationToken);
            return StatusCode(201);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}