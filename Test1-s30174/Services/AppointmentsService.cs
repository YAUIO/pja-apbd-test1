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

    public async Task<int> AddAppointment(AppointmentPost request)
    {
        await using var connection = new SqlConnection(_connectionString);
        {
            await connection.OpenAsync();
            
            var getPatient = new SqlCommand("select p.patient_id from patient p where p.patient_id = @pwz", connection);
            getPatient.Parameters.AddWithValue("@pwz", request.PWZ);
            var p_id = (int?) await getPatient.ExecuteScalarAsync();
            if (p_id == null) throw new KeyNotFoundException("Patient not found");


            var getDoctor = new SqlCommand("select d.doctor_id from doctor d where d.pwz = @pwz", connection);
            getDoctor.Parameters.AddWithValue("@pwz", request.PWZ);
            var d_id = (int?) await getDoctor.ExecuteScalarAsync();

            if (d_id == null) throw new KeyNotFoundException("Doctor not found");
            
            var insertAppointment = new SqlCommand(
                """
                        INSERT INTO Appointment
                        VALUES 
                        (@a_id, @p_id, @d_id, current_date)
                """
                , connection);

            insertAppointment.Parameters.AddWithValue("@a_id", request.AppointmentId);
            insertAppointment.Parameters.AddWithValue("@p_id", request.PatientId);
            insertAppointment.Parameters.AddWithValue("@d_id", d_id);
                    
            var app = await insertAppointment.ExecuteScalarAsync();

            if (app == null) throw new KeyNotFoundException("Invalid appointment data");

            foreach (var serv in request.Services)
            {
                var getService = new SqlCommand("select s.service_id from service s where s.name = @name", connection);
                var s_id = (int?)await getService.ExecuteScalarAsync();
                if (s_id == null) throw new KeyNotFoundException("No such service");
                
                var insertAppointmentService = new SqlCommand(
                    """
                            INSERT INTO Appointment_Service
                            VALUES 
                            (@a_id, @s_id, @s_fee)
                    """
                    , connection);

                insertAppointmentService.Parameters.AddWithValue("@a_id", request.AppointmentId);
                insertAppointmentService.Parameters.AddWithValue("@s_id", s_id);
                insertAppointmentService.Parameters.AddWithValue("@s_fee",serv.ServiceFee);

                var appServ = await insertAppointment.ExecuteScalarAsync();

                if (appServ == null) throw new KeyNotFoundException("Invalid service data");
            }
        }
        return request.AppointmentId;
    }
}