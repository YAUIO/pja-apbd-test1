namespace Test1_s30174.DTOs;

public class AppointmentGet
{
    public DateTime Date { set; get; }
    public PatientGet Patient { set; get; }
    public DoctorGet Doctor { set; get; }
    public List<AppointmentServiceGet> AppointmentServices { set; get; }
}