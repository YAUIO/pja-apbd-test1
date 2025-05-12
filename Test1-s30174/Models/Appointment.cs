namespace Test1_s30174.Models;

public class Appointment
{
    public int AppointmentId { set; get; }
    public int PatientId { set; get; }
    public int DoctorId { set; get; }
    public DateTime Date { set; get; }
}