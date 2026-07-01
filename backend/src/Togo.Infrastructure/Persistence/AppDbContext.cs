using Microsoft.EntityFrameworkCore;
using Togo.Domain.Entities;

namespace Togo.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Tutor> Tutors => Set<Tutor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<ClinicalAuditLog> ClinicalAuditLogs => Set<ClinicalAuditLog>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<ClinicalEvolution> ClinicalEvolutions => Set<ClinicalEvolution>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<ClinicUnit> ClinicUnits => Set<ClinicUnit>();
    public DbSet<UserClinicAccess> UserClinicAccesses => Set<UserClinicAccess>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
