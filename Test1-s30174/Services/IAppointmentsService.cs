using Test1_s30174.DTOs;

namespace Test1_s30174.Services;

public interface IAppointmentsService
{
    public Task<AppointmentGet> GetAppointmentById(int appId);
}