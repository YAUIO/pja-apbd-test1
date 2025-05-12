namespace Test1_s30174.DTOs;

public class AppointmentPost
{
    public int AppointmentId { set; get; }
    public int PatientId { set; get; }
    public string PWZ { set; get; }
    public List<AppointmentServicePost> Services { set; get; }
}