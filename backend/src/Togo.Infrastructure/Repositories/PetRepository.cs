using Microsoft.EntityFrameworkCore;
using Togo.Application.Pets;
using Togo.Application.Pets.Contracts;
using Togo.Domain.Entities;
using Togo.Infrastructure.Persistence;

namespace Togo.Infrastructure.Repositories;

public class PetRepository : IPetRepository
{
    private readonly AppDbContext _context;

    public PetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PetListItemProjection>> ListAsync(CancellationToken cancellationToken)
    {
        return await (
            from pet in _context.Pets.AsNoTracking()
            join patient in _context.Patients.AsNoTracking()
                on pet.PatientId equals patient.Id
            orderby patient.Name
            select new PetListItemProjection(
                patient.Id,
                pet.TutorId,
                patient.Name,
                pet.Species,
                pet.Breed,
                pet.Sex,
                patient.Status,
                pet.Microchip))
            .ToListAsync(cancellationToken);
    }

    public async Task<PetDetailsProjection?> GetByPatientIdAsync(long patientId, CancellationToken cancellationToken)
    {
        return await (
            from pet in _context.Pets.AsNoTracking()
            join patient in _context.Patients.AsNoTracking()
                on pet.PatientId equals patient.Id
            where patient.Id == patientId
            select new PetDetailsProjection(
                patient.Id,
                patient.ClinicId,
                pet.TutorId,
                patient.Name,
                patient.BirthDate,
                patient.Status,
                pet.Species,
                pet.Breed,
                pet.Sex,
                pet.WeightKg,
                pet.Microchip,
                patient.CreatedAt,
                patient.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> TutorExistsAsync(long tutorId, CancellationToken cancellationToken)
    {
        return await _context.Tutors
            .AsNoTracking()
            .AnyAsync(tutor => tutor.Id == tutorId, cancellationToken);
    }

    public async Task<bool> TutorBelongsToClinicAsync(long tutorId, long clinicId, CancellationToken cancellationToken)
    {
        return await _context.Tutors
            .AsNoTracking()
            .AnyAsync(tutor => tutor.Id == tutorId && tutor.ClinicId == clinicId, cancellationToken);
    }

    public async Task<bool> MicrochipExistsAsync(string microchip, long? ignorePatientId, CancellationToken cancellationToken)
    {
        return await _context.Pets
            .AsNoTracking()
            .AnyAsync(
                pet => pet.Microchip == microchip
                    && (!ignorePatientId.HasValue || pet.PatientId != ignorePatientId.Value),
                cancellationToken);
    }

    public async Task<PetDetailsProjection> CreateAsync(CreatePetRepositoryData data, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var patient = Patient.Create(data.ClinicId, data.PatientType, data.Name, data.BirthDate, data.Status, data.CreatedAt);

            await _context.Patients.AddAsync(patient, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var pet = Pet.Create(patient.Id, data.TutorId, data.Species, data.Breed, data.Sex, data.WeightKg, data.Microchip);

            await _context.Pets.AddAsync(pet, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return ToDetailsProjection(patient, pet);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<PetDetailsProjection?> UpdateAsync(UpdatePetRepositoryData data, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(patient => patient.Id == data.PatientId, cancellationToken);

            var pet = await _context.Pets
                .FirstOrDefaultAsync(pet => pet.PatientId == data.PatientId, cancellationToken);

            if (patient is null || pet is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return null;
            }

            patient.Update(data.Name, data.BirthDate, data.Status, data.UpdatedAt);
            pet.UpdateProfile(data.Species, data.Breed, data.Sex, data.WeightKg, data.Microchip);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ToDetailsProjection(patient, pet);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(long patientId, CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(patient => patient.Id == patientId, cancellationToken);

        if (patient is null)
        {
            return false;
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private IQueryable<PetDetailsProjection> GetDetailsQuery()
    {
        return _context.Pets
            .AsNoTracking()
            .Join(
                _context.Patients.AsNoTracking(),
                pet => pet.PatientId,
                patient => patient.Id,
                (pet, patient) => new PetDetailsProjection(
                    patient.Id,
                    patient.ClinicId,
                    pet.TutorId,
                    patient.Name,
                    patient.BirthDate,
                    patient.Status,
                    pet.Species,
                    pet.Breed,
                    pet.Sex,
                    pet.WeightKg,
                    pet.Microchip,
                    patient.CreatedAt,
                    patient.UpdatedAt));
    }

    private static PetDetailsProjection ToDetailsProjection(Patient patient, Pet pet)
    {
        return new PetDetailsProjection(
            patient.Id,
            patient.ClinicId,
            pet.TutorId,
            patient.Name,
            patient.BirthDate,
            patient.Status,
            pet.Species,
            pet.Breed,
            pet.Sex,
            pet.WeightKg,
            pet.Microchip,
            patient.CreatedAt,
            patient.UpdatedAt);
    }
}
