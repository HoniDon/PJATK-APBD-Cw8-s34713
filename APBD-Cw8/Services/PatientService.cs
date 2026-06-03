using APBD_Cw8.DTOs;
using APBD_Cw8.Exceptions;
using APBD_Cw8.Models;
using Microsoft.EntityFrameworkCore;

namespace APBD_Cw8.Services;

public class PatientService(HospitalDbContext context) : IPatientService
{
    public async Task<IEnumerable<PatientDto>> GetPatientsAsync(string? search, CancellationToken cancellationToken)
    {
        var query = context.Patients
            .Include(p => p.Admissions)
                .ThenInclude(a => a.Ward)
            .Include(p => p.BedAssignments)
                .ThenInclude(ba => ba.Bed)
                    .ThenInclude(b => b.BedType)
            .Include(p => p.BedAssignments)
                .ThenInclude(ba => ba.Bed)
                    .ThenInclude(b => b.Room)
                        .ThenInclude(r => r.Ward)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => 
                EF.Functions.Like(p.FirstName, $"%{search}%") || 
                EF.Functions.Like(p.LastName, $"%{search}%"));
        }

        return await query.Select(p => new PatientDto
        {
            Pesel = p.Pesel,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Age = p.Age,
            Sex = p.Sex ? "Male" : "Female",
            Admissions = p.Admissions.Select(a => new AdmissionDto
            {
                Id = a.Id,
                AdmissionDate = a.AdmissionDate,
                DischargeDate = a.DischargeDate,
                Ward = new WardDto
                {
                    Id = a.Ward.Id,
                    Name = a.Ward.Name,
                    Description = a.Ward.Description
                }
            }),
            BedAssignments = p.BedAssignments.Select(ba => new BedAssignmentDto
            {
                Id = ba.Id,
                From = ba.From,
                To = ba.To,
                Bed = new BedDto
                {
                    Id = ba.Bed.Id,
                    BedType = new BedTypeDto
                    {
                        Id = ba.Bed.BedType.Id,
                        Name = ba.Bed.BedType.Name,
                        Description = ba.Bed.BedType.Description
                    },
                    Room = new RoomDto
                    {
                        Id = ba.Bed.Room.Id,
                        HasTv = ba.Bed.Room.HasTv,
                        Ward = new WardDto
                        {
                            Id = ba.Bed.Room.Ward.Id,
                            Name = ba.Bed.Room.Ward.Name,
                            Description = ba.Bed.Room.Ward.Description
                        }
                    }
                }
            })
        }).ToListAsync(cancellationToken);
    }

    public async Task AssignBedAsync(string pesel, AssignBedRequestDto request, CancellationToken cancellationToken)
    {
        var patientExists = await context.Patients.AnyAsync(p => p.Pesel == pesel, cancellationToken);
        if (!patientExists)
        {
            throw new NotFoundException($"Patient with PESEL '{pesel}' does not exist in the system.");
        }

        var availableBed = await context.Beds
            .Where(b => b.BedType.Name == request.BedType && b.Room.Ward.Name == request.Ward)
            .Where(b => !b.BedAssignments.Any(ba => 
                ba.From < request.To && 
                (ba.To == null || ba.To > request.From)))
            .FirstOrDefaultAsync(cancellationToken);

        if (availableBed == null)
        {
            throw new NotFoundException($"Could not find an available bed of type '{request.BedType}' in ward '{request.Ward}' for the requested timeframe.");
        }

        var bedAssignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = availableBed.Id,
            From = request.From,
            To = request.To
        };

        await context.BedAssignments.AddAsync(bedAssignment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}