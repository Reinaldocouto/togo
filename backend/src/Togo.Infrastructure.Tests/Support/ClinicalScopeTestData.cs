using Microsoft.EntityFrameworkCore;
using Togo.Domain.Entities;
using Togo.Infrastructure.Persistence;

namespace Togo.Infrastructure.Tests.Support;

internal static class ClinicalScopeTestData
{
    public static async Task<Clinic> EnsureClinicAsync(AppDbContext context)
    {
        var existingClinic = await context.Clinics
            .OrderBy(clinic => clinic.Id)
            .FirstOrDefaultAsync();

        if (existingClinic is not null)
        {
            return existingClinic;
        }

        var now = DateTime.UtcNow;
        var organization = Organization.Create("Test Organization", now);
        context.Organizations.Add(organization);
        await context.SaveChangesAsync();

        var clinic = Clinic.Create(organization.Id, "Test Clinic", now);
        context.Clinics.Add(clinic);
        await context.SaveChangesAsync();

        return clinic;
    }
}
