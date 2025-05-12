using System.Data;
using Microsoft.Data.SqlClient;
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
        await using var connection = new SqlConnection(_connectionString);
        {
            await connection.OpenAsync();
            var command = new SqlCommand(
                """
                        SELECT a.date, p.first_name, p.last_name, p.date_of_birth, d.doctor_id, d.pwz, s.name, a_s.service_fee
                               FROM Appointment a
                        join patient p on p.patient_id = a.patient_id
                        join doctor d on d.doctor_id = a.doctor_id
                        join appointment_service a_s on a_s.appointment_id = a.appointment_id
                        join service s on s.service_id = a_s.service_id
                        WHERE a.appointment_id = @id
                """
                , connection);

            command.Parameters.AddWithValue("@id", appId);

            using var reader = await command.ExecuteReaderAsync();

            var i = 0;

            AppointmentGet? ret = null;

            while (reader.Read())
            {
                if (i == 0)
                    ret = new AppointmentGet
                    {
                        Date = reader.GetDateTime("date"),
                        Patient = new PatientGet
                        {
                            FirstName = reader.GetString("first_name"),
                            LastName = reader.GetString("last_name"),
                            DateOfBirth = reader.GetDateTime("date_of_birth")
                        },
                        Doctor = new DoctorGet
                        {
                            DoctorId = reader.GetInt32("doctor_id"),
                            PWZ = reader.GetString("pwz")
                        },
                        AppointmentServices = new List<AppointmentServiceGet>()
                    };
                
                if (ret == null) throw new NullReferenceException();
                
                ret.AppointmentServices.Add(new AppointmentServiceGet
                {
                    Name = reader.GetString("name"),
                    ServiceFee = reader.GetDecimal("service_fee")
                });
                i++;
            }
            if (ret == null) throw new KeyNotFoundException();
            return ret;
        }
    }
}