namespace APBD_Cw8.DTOs;

public class PatientDto
{
    public string Pesel { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string Sex { get; set; }
    public IEnumerable<AdmissionDto> Admissions { get; set; }
    public IEnumerable<BedAssignmentDto> BedAssignments { get; set; }
}