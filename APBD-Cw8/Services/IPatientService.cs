using APBD_Cw8.DTOs;

namespace APBD_Cw8.Services;

public interface IPatientService
{
    Task<IEnumerable<PatientDto>> GetPatientsAsync(string? search, CancellationToken cancellationToken);
    Task AssignBedAsync(string pesel, AssignBedRequestDto request, CancellationToken cancellationToken);
}