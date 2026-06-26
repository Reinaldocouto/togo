using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Togo.Domain.Entities;
using Togo.Infrastructure.Persistence;
using Togo.Infrastructure.Tests.Support;

namespace Togo.Infrastructure.Tests.Persistence;

public class ClinicalScopePersistenceTests
{
    [Fact]
    public void AppDbContext_ShouldExposeClinicalScopeDbSets()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        using var _ = connection;

        Assert.NotNull(context.Organizations);
        Assert.NotNull(context.Clinics);
        Assert.NotNull(context.ClinicUnits);
    }

    [Fact]
    public void Model_ShouldMapClinicalScopeTablesAndConstraints()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        using var _ = connection;

        AssertTable<Organization>(context, "Organizations");
        AssertTable<Clinic>(context, "Clinics");
        AssertTable<ClinicUnit>(context, "ClinicUnits");

        AssertRequiredProperty<Organization>(context, nameof(Organization.Name), 120);
        AssertRequiredProperty<Organization>(context, nameof(Organization.IsActive));
        AssertRequiredProperty<Organization>(context, nameof(Organization.CreatedAt));
        AssertRequiredProperty<Clinic>(context, nameof(Clinic.OrganizationId));
        AssertRequiredProperty<Clinic>(context, nameof(Clinic.Name), 120);
        AssertRequiredProperty<ClinicUnit>(context, nameof(ClinicUnit.ClinicId));
        AssertRequiredProperty<ClinicUnit>(context, nameof(ClinicUnit.Name), 120);
    }

    [Fact]
    public void Model_ShouldExposeClinicalScopeIndexesAndSafeDeleteBehaviors()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        using var _ = connection;

        AssertIndex<Organization>(context, nameof(Organization.Name));
        AssertIndex<Clinic>(context, nameof(Clinic.OrganizationId));
        AssertUniqueIndex<Clinic>(context, nameof(Clinic.OrganizationId), nameof(Clinic.Name));
        AssertIndex<ClinicUnit>(context, nameof(ClinicUnit.ClinicId));
        AssertUniqueIndex<ClinicUnit>(context, nameof(ClinicUnit.ClinicId), nameof(ClinicUnit.Name));
        AssertDeleteBehavior<Clinic>(context, nameof(Clinic.OrganizationId), DeleteBehavior.Restrict);
        AssertDeleteBehavior<ClinicUnit>(context, nameof(ClinicUnit.ClinicId), DeleteBehavior.Restrict);
    }

    [Fact]
    public async Task ClinicalScopeRelationships_ShouldPersistOrganizationClinicAndClinicUnit()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;
        var createdAt = new DateTime(2026, 6, 25, 10, 0, 0, DateTimeKind.Utc);

        var organization = Organization.Create("TOGO Saúde Animal", createdAt);
        context.Organizations.Add(organization);
        await context.SaveChangesAsync();

        var clinic = Clinic.Create(organization.Id, "Clínica Centro", createdAt);
        context.Clinics.Add(clinic);
        await context.SaveChangesAsync();

        var unit = ClinicUnit.Create(clinic.Id, "Unidade Cirúrgica", createdAt);
        context.ClinicUnits.Add(unit);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var persistedUnit = await context.ClinicUnits.AsNoTracking().SingleAsync();
        var persistedClinic = await context.Clinics.AsNoTracking().SingleAsync();

        Assert.Equal(clinic.Id, persistedUnit.ClinicId);
        Assert.Equal(organization.Id, persistedClinic.OrganizationId);
    }

    [Fact]
    public async Task RemovingOrganizationWithClinic_ShouldBeBlockedByForeignKeyAndKeepClinicUnit()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        await using var _ = connection;
        var createdAt = new DateTime(2026, 6, 25, 10, 0, 0, DateTimeKind.Utc);

        var organization = Organization.Create("Protected organization", createdAt);
        context.Organizations.Add(organization);
        await context.SaveChangesAsync();

        var clinic = Clinic.Create(organization.Id, "Protected clinic", createdAt);
        context.Clinics.Add(clinic);
        await context.SaveChangesAsync();

        var unit = ClinicUnit.Create(clinic.Id, "Protected unit", createdAt);
        context.ClinicUnits.Add(unit);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        context.Organizations.Remove(organization);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        Assert.Equal(1, await context.Clinics.AsNoTracking().CountAsync(item => item.Id == clinic.Id));
        Assert.Equal(1, await context.ClinicUnits.AsNoTracking().CountAsync(item => item.Id == unit.Id));
    }

    private static void AssertTable<TEntity>(AppDbContext context, string tableName)
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        Assert.NotNull(entityType);
        Assert.Equal(tableName, entityType!.GetTableName());
    }

    private static void AssertRequiredProperty<TEntity>(AppDbContext context, string propertyName, int? maxLength = null)
    {
        var property = GetProperty<TEntity>(context, propertyName);
        Assert.False(property.IsNullable);
        if (maxLength.HasValue)
        {
            Assert.Equal(maxLength.Value, property.GetMaxLength());
        }
    }

    private static void AssertIndex<TEntity>(AppDbContext context, params string[] propertyNames)
    {
        Assert.Contains(GetEntityType<TEntity>(context).GetIndexes(), index => HasProperties(index, propertyNames));
    }

    private static void AssertUniqueIndex<TEntity>(AppDbContext context, params string[] propertyNames)
    {
        Assert.Contains(GetEntityType<TEntity>(context).GetIndexes(), index => index.IsUnique && HasProperties(index, propertyNames));
    }

    private static void AssertDeleteBehavior<TEntity>(AppDbContext context, string foreignKeyPropertyName, DeleteBehavior expected)
    {
        var foreignKey = GetEntityType<TEntity>(context).GetForeignKeys().Single(key => key.Properties.Any(property => property.Name == foreignKeyPropertyName));
        Assert.Equal(expected, foreignKey.DeleteBehavior);
    }

    private static IReadOnlyProperty GetProperty<TEntity>(AppDbContext context, string propertyName)
        => GetEntityType<TEntity>(context).FindProperty(propertyName) ?? throw new InvalidOperationException($"Property {propertyName} was not found.");

    private static IReadOnlyEntityType GetEntityType<TEntity>(AppDbContext context)
        => context.Model.FindEntityType(typeof(TEntity)) ?? throw new InvalidOperationException($"Entity {typeof(TEntity).Name} was not mapped.");

    private static bool HasProperties(IReadOnlyIndex index, string[] propertyNames)
        => index.Properties.Select(property => property.Name).SequenceEqual(propertyNames);
}
