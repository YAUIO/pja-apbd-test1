using Test1_s30174.DTOs;

namespace Test1_s30174.Services;

public class AppointmentsService : IAppointmentsService
{
    private readonly string? _connectionString;

    public AppointmentsService(IConfiguration cfg)
    {
        _connectionString = cfg.GetConnectionString("Default");
    }

    public async Task<AppointmentGet> GetAppointmentById(int appId)
    {
        
    }
}