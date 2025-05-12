using Microsoft.AspNetCore.Mvc;
using Test1_s30174.Services;

namespace Test1_s30174.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentsService _service;

    public AppointmentsController(IAppointmentsService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointmentById(int id)
    {
        try
        {
            return Ok(_service.GetAppointmentById(id));
        }
        catch (KeyNotFoundException e)
        {
        }
        catch (Exception e)
        {
        }
    }
}