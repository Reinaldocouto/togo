using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Togo.Domain.Entities;
using Togo.Infrastructure.Persistence;
using Togo.Infrastructure.Tests.Support;

namespace Togo.Infrastructure.Tests.Persistence;

public class UserClinicAccessPersistenceTests
{
    [Fact]
    public void AppDbContext_ShouldExposeUserClinicAccessDbSet()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        using var _ = connection;

        Assert.NotNull(context.UserClinicAccesses);
    }

    [Fact]
    public void Model_ShouldMapUserClinicAccessTableConstraintsIndexesAndDeleteBehavior()
    {
        using var context = SqliteAppDbContextFactory.CreateContext(out var connection);
        using var _ = connection;

        AssertTable<UserClinicAccess>(context, "UserClinicAccesses");
        AssertRequiredProperty<UserClinicAccess>(context, nameof(UserClinicAccess.UserId));
        AssertRequiredProperty<UserClinicAccess>(context, nameof(UserClinicAccess.ClinicId));
        AssertRequiredProperty<UserClinicAccess>(context, nameof(UserClinicAccess.IsActive));
        AssertRequiredProperty<UserClinicAccess>(context, nameof(UserClinicAccess.CreatedAt));
        AssertIndex<UserClinicAccess>(context, nameof(UserClinicAccess.UserId));
        AssertIndex<UserClinicAccess>(context, nameof(UserClinicAccess.ClinicId));
        AssertUniqueIndex<UserClinicAccess>(context, nameof(UserClinicAccess.UserId), nameof(UserClinicAccess.ClinicId));
        AssertDeleteBehavior<UserClinicAccess>(context, nameof(UserClinicAccess.UserId), DeleteBehavior.Restrict);
        AssertDeleteBehavior<UserClinicAccess>(context, nameof(UserClinicAccess.ClinicId), DeleteBehavior.Restrict);
    }

    private static void AssertTable<TEntity>(AppDbContext context, string tableName)
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        Assert.NotNull(entityType);
        Assert.Equal(tableName, entityType!.GetTableName());
    }

    private static void AssertRequiredProperty<TEntity>(AppDbContext context, string propertyName)
    {
        var property = GetProperty<TEntity>(context, propertyName);
        Assert.False(property.IsNullable);
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
